#region

using System;
using BepInEx.Configuration;

#endregion

namespace Imperium.Util.Binding;

/// <summary>
/// ImpBinding that also maintains and updates a BepInEx config in the background.
/// </summary>
/// <typeparam name="T"></typeparam>
public class ImpConfig<T> : ImpBinding<T>
{
    private readonly ConfigEntry<T> config;

    /// <summary>
    /// Imperium configuration that is linked to a BepInEx config file.
    /// 
    /// It is recommended to set the ignoreBroadcasts flag when multiple configs have the same expensive update function
    /// e.g. PlayerManager.UpdateCameras()
    /// </summary>
    /// <param name="section">BepInEx config file section</param>
    /// <param name="key">BepInEx config file key</param>
    /// <param name="defaultValue"></param>
    /// <param name="onUpdate"><see cref="ImpBinding{T}.onUpdate"/></param>
    /// <param name="syncOnUpdate"><see cref="ImpBinding{T}.syncOnUpdate"/></param>
    /// <param name="ignoreRefresh"><see cref="ImpBinding{T}.ignoreRefresh"/></param>
    internal ImpConfig(
        string section,
        string key,
        T defaultValue,
        Action<T> onUpdate = null,
        Action<T> syncOnUpdate = null,
        bool ignoreRefresh = false
    ) : base(defaultValue, onUpdate, syncOnUpdate, ignoreRefresh)
    {
        config = Imperium.ConfigFile.Bind(section, key, defaultValue);
        Value = config.Value;
    }

    internal override void Set(T value, bool skipSync)
    {
        config.Value = value;
        base.Set(config.Value, skipSync);
    }
}