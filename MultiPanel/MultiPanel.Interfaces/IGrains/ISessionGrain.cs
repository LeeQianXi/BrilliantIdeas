using Orleans;

namespace MultiPanel.Interfaces.IGrains;

[Alias("MultiPanel.Interfaces.IGrains.ISessionGrain")]
public interface ISessionGrain : IGrainWithIntegerKey
{
}