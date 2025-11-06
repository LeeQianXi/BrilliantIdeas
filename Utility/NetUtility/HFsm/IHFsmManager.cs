namespace NetUtility.HFsm;

public interface IHFsmManager
{
    public static IHFsmManager Instance => Internal._instance;

    /// <summary>
    ///     获取有限状态机数量。
    /// </summary>
    int Count { get; }

    /// <summary>
    ///     检查是否存在有限状态机。
    /// </summary>
    /// <typeparam name="T">有限状态机持有者类型。</typeparam>
    /// <returns>是否存在有限状态机。</returns>
    bool HasHFsm<T>() where T : class;

    /// <summary>
    ///     检查是否存在有限状态机。
    /// </summary>
    /// <param name="ownerType">有限状态机持有者类型。</param>
    /// <returns>是否存在有限状态机。</returns>
    bool HasHFsm(Type ownerType);

    /// <summary>
    ///     检查是否存在有限状态机。
    /// </summary>
    /// <typeparam name="T">有限状态机持有者类型。</typeparam>
    /// <param name="name">有限状态机名称。</param>
    /// <returns>是否存在有限状态机。</returns>
    bool HasHFsm<T>(string name) where T : class;

    /// <summary>
    ///     检查是否存在有限状态机。
    /// </summary>
    /// <param name="ownerType">有限状态机持有者类型。</param>
    /// <param name="name">有限状态机名称。</param>
    /// <returns>是否存在有限状态机。</returns>
    bool HasHFsm(Type ownerType, string name);

    /// <summary>
    ///     获取有限状态机。
    /// </summary>
    /// <typeparam name="T">有限状态机持有者类型。</typeparam>
    /// <returns>要获取的有限状态机。</returns>
    IHFsm<T>? GetHFsm<T>() where T : class;

    /// <summary>
    ///     获取有限状态机。
    /// </summary>
    /// <param name="ownerType">有限状态机持有者类型。</param>
    /// <returns>要获取的有限状态机。</returns>
    HFsmBase? GetHFsm(Type ownerType);

    /// <summary>
    ///     获取有限状态机。
    /// </summary>
    /// <typeparam name="T">有限状态机持有者类型。</typeparam>
    /// <param name="name">有限状态机名称。</param>
    /// <returns>要获取的有限状态机。</returns>
    IHFsm<T>? GetHFsm<T>(string name) where T : class;

    /// <summary>
    ///     获取有限状态机。
    /// </summary>
    /// <param name="ownerType">有限状态机持有者类型。</param>
    /// <param name="name">有限状态机名称。</param>
    /// <returns>要获取的有限状态机。</returns>
    HFsmBase? GetHFsm(Type ownerType, string name);

    /// <summary>
    ///     获取所有有限状态机。
    /// </summary>
    /// <returns>所有有限状态机。</returns>
    HFsmBase[] GetAllHFsms();

    /// <summary>
    ///     获取所有有限状态机。
    /// </summary>
    /// <param name="results">所有有限状态机。</param>
    void GetAllHFsms(out List<HFsmBase> results);

    /// <summary>
    ///     创建有限状态机。
    /// </summary>
    /// <typeparam name="T">有限状态机持有者类型。</typeparam>
    /// <param name="owner">有限状态机持有者。</param>
    /// <param name="rootState">有限状态机状态集合。</param>
    /// <returns>要创建的有限状态机。</returns>
    IHFsm<T> CreateHFsm<T>(T owner, HFsmState<T> rootState) where T : class;

    /// <summary>
    ///     创建有限状态机。
    /// </summary>
    /// <typeparam name="T">有限状态机持有者类型。</typeparam>
    /// <param name="name">有限状态机名称。</param>
    /// <param name="owner">有限状态机持有者。</param>
    /// <param name="rootState">有限状态机状态集合。</param>
    /// <returns>要创建的有限状态机。</returns>
    IHFsm<T> CreateHFsm<T>(string name, T owner, HFsmState<T> rootState) where T : class;

    /// <summary>
    ///     销毁有限状态机。
    /// </summary>
    /// <typeparam name="T">有限状态机持有者类型。</typeparam>
    /// <returns>是否销毁有限状态机成功。</returns>
    bool DestroyHFsm<T>() where T : class;

    /// <summary>
    ///     销毁有限状态机。
    /// </summary>
    /// <param name="ownerType">有限状态机持有者类型。</param>
    /// <returns>是否销毁有限状态机成功。</returns>
    bool DestroyHFsm(Type ownerType);

    /// <summary>
    ///     销毁有限状态机。
    /// </summary>
    /// <typeparam name="T">有限状态机持有者类型。</typeparam>
    /// <param name="name">要销毁的有限状态机名称。</param>
    /// <returns>是否销毁有限状态机成功。</returns>
    bool DestroyHFsm<T>(string name) where T : class;

    /// <summary>
    ///     销毁有限状态机。
    /// </summary>
    /// <param name="ownerType">有限状态机持有者类型。</param>
    /// <param name="name">要销毁的有限状态机名称。</param>
    /// <returns>是否销毁有限状态机成功。</returns>
    bool DestroyHFsm(Type ownerType, string name);

    /// <summary>
    ///     销毁有限状态机。
    /// </summary>
    /// <typeparam name="T">有限状态机持有者类型。</typeparam>
    /// <param name="hFsm">要销毁的有限状态机。</param>
    /// <returns>是否销毁有限状态机成功。</returns>
    bool DestroyHFsm<T>(IHFsm<T> hFsm) where T : class;

    /// <summary>
    ///     销毁有限状态机。
    /// </summary>
    /// <param name="hFsm">要销毁的有限状态机。</param>
    /// <returns>是否销毁有限状态机成功。</returns>
    bool DestroyHFsm(HFsmBase hFsm);

    private static class Internal
    {
        // ReSharper disable once InconsistentNaming
        public static readonly IHFsmManager _instance = new HFsmManager();
    }
}