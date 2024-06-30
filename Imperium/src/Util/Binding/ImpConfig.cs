#region

using System;
using BepInEx.Configuration;

#endregion

namespace Imperium.Util.Binding;

/// <summary>
///     Imperium configuration that is linked to a BepInEx config file.
///     It is recommended to set the ignoreBroadcasts flag when multiple configs have the same expensive update function
///     e.g. PlayerManager.UpdateCameras()
/// </summary>
public sealed class ImpConfig<T> : ImpBinding<T>
{
    private readonly ConfigEntry<T> config;

    private readonly bool allowWhenDisabled;

    // Always return the default value in configs if Imperium is not enabled.
    public new T Value => Imperium.IsImperiumEnabled || allowWhenDisabled ? base.Value : DefaultValue;

    public ImpConfig(
        ConfigFile configFile,
        string section,
        string key,
        T defaultValue,
        Action<T> onUpdate = null,
        Action<T> fromLocalUpdate = null,
        bool ignoreRefresh = false,
        bool allowWhenDisabled = false
    ) : base(defaultValue, default, onUpdate, fromLocalUpdate, ignoreRefresh)
    {
        this.allowWhenDisabled = allowWhenDisabled;

        config = configFile.Bind(section, key, defaultValue);
        base.Value = config.Value;
    }

    public override void Set(T updatedValue, bool invokeUpdate = true)
    {
        config.Value = updatedValue;
        base.Set(updatedValue, invokeUpdate);
    }

    /// <summary>
    ///     Whenever refresh is called while Imperium is disabled, we want the callbacks to also use the default value.
    /// </summary>
    public override void Refresh()
    {
        base.Value = Imperium.IsImperiumEnabled || allowWhenDisabled ? base.Value : DefaultValue;
        base.Refresh();
    }
}