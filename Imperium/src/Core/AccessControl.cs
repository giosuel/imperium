#region

using Imperium.Util.Binding;

#endregion

namespace Imperium.Core;

public class AccessControl
{
    public ImpBinding<bool> Spawning;
    public ImpBinding<bool> Visualizers;
    public ImpBinding<bool> Oracle;
    public ImpBinding<bool> Teleportation;
    public ImpBinding<bool> ChangeWeather;
    public ImpBinding<bool> ChangeTime;
    public ImpBinding<bool> Navigator;
}

public class AccessControlDefinition
{
}