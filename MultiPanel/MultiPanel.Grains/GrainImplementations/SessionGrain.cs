using MultiPanel.Grains.State;
using MultiPanel.Interfaces.IGrains;
using Orleans;

namespace MultiPanel.Grains.GrainImplementations;

public class SessionGrain : Grain<SessionState>, ISessionGrain
{
}