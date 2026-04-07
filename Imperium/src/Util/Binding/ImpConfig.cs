#region

using System;
using BepInEx.Configuration;

#endregion

namespace Imperium.Util.Binding;

/// <summary>
///     An ImpBingind that is linked to a BepInEx config.
///     It is recommended to set the ignoreBroadcasts flag when multiple configs have the same expensive update function
///     e.g. PlayerManager.UpdateCameras()
/// </summary>
public sealed class ImpConfig<T> : ImpBinding<T>
{
    private readonly ConfigEntry<T> config;

    /// <summary>
    ///     If set to true, this config will be active even if Imperium is not currently enabled.
    ///     This allows to selectively activate / deactivate certain configs when Imperium is currently not
    ///     enabled (e.g. Night Vision, God Mode).
    /// </summary>
    private readonly bool allowWhenDisabled;

    public new T Value => Imperium.IsImperiumEnabled || allowWhenDisabled ? base.Value : DefaultValue;

    public ImpConfig(
        ConfigFile configFile,
        string section,
        string key,
        T defaultValue,
        Action<T> primaryUpdate = null,
        Action<T> secondaryUpdate = null,
        bool ignoreRefresh = false,
        bool allowWhenDisabled = false,
        string description = null
    ) : base(defaultValue, default, primaryUpdate, secondaryUpdate, ignoreRefresh)
    {
        this.allowWhenDisabled = allowWhenDisabled;

        config = configFile.Bind(
            section, key,
            defaultValue,
            configDescription: !string.IsNullOrEmpty(description) ? new ConfigDescription(description) : null
        );
        base.Value = config.Value;
    }

    public override void Set(T updatedValue, bool invokePrimary = true, bool invokeSecondary = true)
    {
        config.Value = updatedValue;
        base.Set(updatedValue, invokePrimary, invokeSecondary);
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