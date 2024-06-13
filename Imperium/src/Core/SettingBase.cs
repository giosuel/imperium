#region

using BepInEx.Configuration;

#endregion

namespace Imperium.Core;

public class SettingBase(ConfigFile config)
{
    protected ConfigFile config = config;
}