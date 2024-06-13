using BepInEx.Configuration;

namespace Imperium.Core;

public class SettingBase(ConfigFile config)
{
    protected ConfigFile config = config;
}