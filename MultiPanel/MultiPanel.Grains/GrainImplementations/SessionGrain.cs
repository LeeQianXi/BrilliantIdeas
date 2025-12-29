using MultiPanel.Grains.State;
using MultiPanel.Interfaces.IGrains;

namespace MultiPanel.Grains.GrainImplementations;

public class SessionGrain : Grain<SessionState>, ISessionGrain
{
}