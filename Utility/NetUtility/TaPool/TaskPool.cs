namespace NetUtility.TaPool;

/// <summary>
///     任务池。
/// </summary>
/// <typeparam name="T">任务类型。</typeparam>
internal sealed class TaskPool<T> where T : TaskBase
{
    private readonly Stack<ITaskAgent<T>> _freeAgents = [];
    private readonly NetUtilityLinkedList<T> _waitingTasks = [];
    private readonly NetUtilityLinkedList<ITaskAgent<T>> _workingAgents = [];

    /// <summary>
    ///     获取或设置任务池是否被暂停。
    /// </summary>
    public bool Paused { get; set; } = false;

    /// <summary>
    ///     获取任务代理总数量。
    /// </summary>
    public int TotalAgentCount => FreeAgentCount + WorkingAgentCount;

    /// <summary>
    ///     获取可用任务代理数量。
    /// </summary>
    public int FreeAgentCount => _freeAgents.Count;

    /// <summary>
    ///     获取工作中任务代理数量。
    /// </summary>
    public int WorkingAgentCount => _workingAgents.Count;

    /// <summary>
    ///     获取等待任务数量。
    /// </summary>
    public int WaitingTaskCount => _waitingTasks.Count;

    /// <summary>
    ///     任务池轮询。
    /// </summary>
    /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
    /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
    public void Update(float elapseSeconds, float realElapseSeconds)
    {
        if (Paused) return;

        ProcessRunningTasks(elapseSeconds, realElapseSeconds);
        ProcessWaitingTasks(elapseSeconds, realElapseSeconds);
    }

    /// <summary>
    ///     关闭并清理任务池。
    /// </summary>
    public void Shutdown()
    {
        RemoveAllTasks();

        while (FreeAgentCount > 0) _freeAgents.Pop().Shutdown();
    }

    /// <summary>
    ///     增加任务代理。
    /// </summary>
    /// <param name="agent">要增加的任务代理。</param>
    public void AddAgent(ITaskAgent<T> agent)
    {
        if (agent == null) throw new NetUtilityException("Task agent is invalid.");

        agent.Initialize();
        _freeAgents.Push(agent);
    }

    /// <summary>
    ///     根据任务的序列编号获取任务的信息。
    /// </summary>
    /// <param name="serialId">要获取信息的任务的序列编号。</param>
    /// <returns>任务的信息。</returns>
    public TaskInfo GetTaskInfo(int serialId)
    {
        return (from workingAgent in _workingAgents
                select workingAgent.Task
                into workingTask
                where workingTask.SerialId == serialId
                select new TaskInfo(workingTask.SerialId, workingTask.Tag, workingTask.Priority, workingTask.UserData,
                    workingTask.Done ? TaskStatus.Done : TaskStatus.Doing, workingTask.Description))
            .Concat(from waitingTask in _waitingTasks
                where waitingTask.SerialId == serialId
                select new TaskInfo(waitingTask.SerialId, waitingTask.Tag, waitingTask.Priority, waitingTask.UserData,
                    TaskStatus.Todo, waitingTask.Description))
            .FirstOrDefault();
    }

    /// <summary>
    ///     根据任务的标签获取任务的信息。
    /// </summary>
    /// <param name="tag">要获取信息的任务的标签。</param>
    /// <returns>任务的信息。</returns>
    public TaskInfo[] GetTaskInfos(string tag)
    {
        return (from workingAgent in _workingAgents
                select workingAgent.Task
                into workingTask
                where workingTask.Tag == tag
                select new TaskInfo(workingTask.SerialId, workingTask.Tag, workingTask.Priority, workingTask.UserData,
                    workingTask.Done ? TaskStatus.Done : TaskStatus.Doing, workingTask.Description))
            .Concat(from waitingTask in _waitingTasks
                where waitingTask.Tag == tag
                select new TaskInfo(waitingTask.SerialId, waitingTask.Tag, waitingTask.Priority, waitingTask.UserData,
                    TaskStatus.Todo, waitingTask.Description)).ToArray();
    }

    /// <summary>
    ///     根据任务的标签获取任务的信息。
    /// </summary>
    /// <param name="tag">要获取信息的任务的标签。</param>
    /// <param name="results">任务的信息。</param>
    public void GetTaskInfos(string tag, out List<TaskInfo> results)
    {
        results = (from workingAgent in _workingAgents
                select workingAgent.Task
                into workingTask
                where workingTask.Tag == tag
                select new TaskInfo(workingTask.SerialId, workingTask.Tag, workingTask.Priority, workingTask.UserData,
                    workingTask.Done ? TaskStatus.Done : TaskStatus.Doing, workingTask.Description))
            .Concat(from waitingTask in _waitingTasks
                where waitingTask.Tag == tag
                select new TaskInfo(waitingTask.SerialId, waitingTask.Tag, waitingTask.Priority, waitingTask.UserData,
                    TaskStatus.Todo, waitingTask.Description)).ToList();
    }

    /// <summary>
    ///     获取所有任务的信息。
    /// </summary>
    /// <returns>所有任务的信息。</returns>
    public TaskInfo[] GetAllTaskInfos()
    {
        return (from workingAgent in _workingAgents
                select workingAgent.Task
                into workingTask
                select new TaskInfo(workingTask.SerialId, workingTask.Tag, workingTask.Priority,
                    workingTask.UserData, workingTask.Done ? TaskStatus.Done : TaskStatus.Doing,
                    workingTask.Description))
            .Concat(from waitingTask in _waitingTasks
                select new TaskInfo(waitingTask.SerialId, waitingTask.Tag, waitingTask.Priority,
                    waitingTask.UserData, TaskStatus.Todo, waitingTask.Description))
            .ToArray();
    }

    /// <summary>
    ///     获取所有任务的信息。
    /// </summary>
    /// <param name="results">所有任务的信息。</param>
    public void GetAllTaskInfos(out List<TaskInfo> results)
    {
        results = (from workingAgent in _workingAgents
                select workingAgent.Task
                into workingTask
                select new TaskInfo(workingTask.SerialId, workingTask.Tag, workingTask.Priority,
                    workingTask.UserData, workingTask.Done ? TaskStatus.Done : TaskStatus.Doing,
                    workingTask.Description))
            .Concat(from waitingTask in _waitingTasks
                select new TaskInfo(waitingTask.SerialId, waitingTask.Tag, waitingTask.Priority,
                    waitingTask.UserData, TaskStatus.Todo, waitingTask.Description))
            .ToList();
    }

    /// <summary>
    ///     增加任务。
    /// </summary>
    /// <param name="task">要增加的任务。</param>
    public void AddTask(T task)
    {
        var current = _waitingTasks.Last;
        while (current != null)
        {
            if (task.Priority <= current.Value.Priority) break;

            current = current.Previous;
        }

        if (current != null)
            _waitingTasks.AddAfter(current, task);
        else
            _waitingTasks.AddFirst(task);
    }

    /// <summary>
    ///     根据任务的序列编号移除任务。
    /// </summary>
    /// <param name="serialId">要移除任务的序列编号。</param>
    /// <returns>是否移除任务成功。</returns>
    public bool RemoveTask(int serialId)
    {
        foreach (var task in _waitingTasks.Where(task => task.SerialId == serialId))
        {
            _waitingTasks.Remove(task);
            ReferencePool.Release(task);
            return true;
        }

        var currentWorkingAgent = _workingAgents.First;
        while (currentWorkingAgent != null)
        {
            var next = currentWorkingAgent.Next;
            var workingAgent = currentWorkingAgent.Value;
            var task = workingAgent.Task;
            if (task.SerialId == serialId)
            {
                workingAgent.Reset();
                _freeAgents.Push(workingAgent);
                _workingAgents.Remove(currentWorkingAgent);
                ReferencePool.Release(task);
                return true;
            }

            currentWorkingAgent = next;
        }

        return false;
    }

    /// <summary>
    ///     根据任务的标签移除任务。
    /// </summary>
    /// <param name="tag">要移除任务的标签。</param>
    /// <returns>移除任务的数量。</returns>
    public int RemoveTasks(string tag)
    {
        var count = 0;

        var currentWaitingTask = _waitingTasks.First;
        while (currentWaitingTask != null)
        {
            var next = currentWaitingTask.Next;
            var task = currentWaitingTask.Value;
            if (task.Tag == tag)
            {
                _waitingTasks.Remove(currentWaitingTask);
                ReferencePool.Release(task);
                count++;
            }

            currentWaitingTask = next;
        }

        var currentWorkingAgent = _workingAgents.First;
        while (currentWorkingAgent != null)
        {
            var next = currentWorkingAgent.Next;
            var workingAgent = currentWorkingAgent.Value;
            var task = workingAgent.Task;
            if (task.Tag == tag)
            {
                workingAgent.Reset();
                _freeAgents.Push(workingAgent);
                _workingAgents.Remove(currentWorkingAgent);
                ReferencePool.Release(task);
                count++;
            }

            currentWorkingAgent = next;
        }

        return count;
    }

    /// <summary>
    ///     移除所有任务。
    /// </summary>
    /// <returns>移除任务的数量。</returns>
    public int RemoveAllTasks()
    {
        var count = _waitingTasks.Count + _workingAgents.Count;

        foreach (var task in _waitingTasks) ReferencePool.Release(task);

        _waitingTasks.Clear();

        foreach (var workingAgent in _workingAgents)
        {
            var task = workingAgent.Task;
            workingAgent.Reset();
            _freeAgents.Push(workingAgent);
            ReferencePool.Release(task);
        }

        _workingAgents.Clear();

        return count;
    }

    private void ProcessRunningTasks(float elapseSeconds, float realElapseSeconds)
    {
        var current = _workingAgents.First;
        while (current != null)
        {
            var task = current.Value.Task;
            if (!task.Done)
            {
                current.Value.Update(elapseSeconds, realElapseSeconds);
                current = current.Next;
                continue;
            }

            var next = current.Next;
            current.Value.Reset();
            _freeAgents.Push(current.Value);
            _workingAgents.Remove(current);
            ReferencePool.Release(task);
            current = next;
        }
    }

    private void ProcessWaitingTasks(float elapseSeconds, float realElapseSeconds)
    {
        var current = _waitingTasks.First;
        while (current != null && FreeAgentCount > 0)
        {
            var agent = _freeAgents.Pop();
            var agentNode = _workingAgents.AddLast(agent);
            var task = current.Value;
            var next = current.Next;
            var status = agent.Start(task);
            if (status is StartTaskStatus.Done or StartTaskStatus.HasToWait or StartTaskStatus.UnknownError)
            {
                agent.Reset();
                _freeAgents.Push(agent);
                _workingAgents.Remove(agentNode);
            }

            if (status is StartTaskStatus.Done or StartTaskStatus.CanResume or StartTaskStatus.UnknownError)
                _waitingTasks.Remove(current);

            if (status is StartTaskStatus.Done or StartTaskStatus.UnknownError) ReferencePool.Release(task);

            current = next;
        }
    }
}