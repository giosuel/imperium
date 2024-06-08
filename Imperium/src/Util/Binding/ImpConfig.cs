#region

using System;
using BepInEx.Configuration;

#endregion

namespace Imperium.Util.Binding;

/// <summary>
///     ImpBinding that also maintains and updates a BepInEx config in the background.
/// </summary>
/// <typeparam name="T"></typeparam>
public class ImpConfig<T> : ImpBinding<T>
{
    private readonly ConfigEntry<T> config;

    /// <summary>
    ///     Imperium configuration that is linked to a BepInEx config file.
    ///     It is recommended to set the ignoreBroadcasts flag when multiple configs have the same expensive update function
    ///     e.g. PlayerManager.UpdateCameras()
    /// </summary>
    /// <param name="section">BepInEx config file section</param>
    /// <param name="key">BepInEx config file key</param>
    /// <param name="defaultValue"></param>
    /// <param name="onUpdate">
    ///     <see cref="ImpBinding{T}.onUpdate" />
    /// </param>
    /// <param name="syncUpdate">
    ///     <see cref="ImpBinding{T}.onUpdateSync" />
    /// </param>
    /// <param name="ignoreRefresh">
    ///     <see cref="ImpBinding{T}.ignoreRefresh" />
    /// </param>
    public ImpConfig(
        string section,
        string key,
        T defaultValue,
        Action<T> onUpdate = null,
        Action<T> syncUpdate = null,
        bool ignoreRefresh = false
    ) : base(defaultValue, onUpdate, syncUpdate, ignoreRefresh)
    {
        config = Imperium.ConfigFile.Bind(section, key, defaultValue);
        Value = config.Value;
    }

    public override void Set(T value, bool skipSync)
    {
        config.Value = value;
        base.Set(config.Value, skipSync);
    }
}