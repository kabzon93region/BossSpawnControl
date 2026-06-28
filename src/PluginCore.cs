using BepInEx;
using HarmonyLib;

namespace BossSpawnControl
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.NAME, PluginInfo.VERSION)]
    public sealed class PluginCore : BaseUnityPlugin
    {
        internal static PluginCore Instance { get; private set; }

        internal readonly BossConfigService ConfigService = new BossConfigService();

        internal void Log(string message, bool force = false)
        {
            if (force || ConfigService.DebugLogging?.Value == true)
            {
                Logger.LogInfo(message);
            }
        }

        private void Awake()
        {
            Instance = this;
            ConfigService.Bind(Config);

            var harmony = new Harmony(PluginInfo.GUID);
            harmony.PatchAll(typeof(PluginCore).Assembly);

            Logger.LogInfo($"{PluginInfo.NAME} v{PluginInfo.VERSION} loaded");
            Logger.LogInfo($"Bosses in F12 config: {BossCatalog.AllBosses.Count}");
            Logger.LogInfo("F12 -> Boss Spawn Control -> Боссы: включите нужных боссов");
            Logger.LogInfo("F12 -> Отладка -> кнопка принудительного спавна");
        }
    }
}
