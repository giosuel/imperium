#region

using System;
using BepInEx.Configuration;
using Imperium.Core;

#endregion

namespace Imperium.Util.Binding;

/// <summary>
/// Imperium configuration that is linked to a BepInEx config file.
/// It is recommended to set the ignoreBroadcasts flag when multiple configs have the same expensive update function
/// e.g. PlayerManager.UpdateCameras()
/// </summary>
public sealed class ImpConfig<T> : ImpBinding<T>
{
    private readonly ConfigEntry<T> config;

    public ImpConfig(
        ConfigFile configFile,
        string section,
        string key,
        T defaultValue,
        Action<T> onUpdate = null,
        Action<T> fromLocalUpdate = null,
        bool ignoreRefresh = false
    ) : base(defaultValue, default, onUpdate, fromLocalUpdate, ignoreRefresh)
    {
        config = configFile.Bind(section, key, defaultValue);
        Value = config.Value;
    }

    public override void Set(T updatedValue, bool invokeUpdate = true)
    {
        config.Value = updatedValue;
        base.Set(updatedValue, invokeUpdate);
    }
}