using BepInEx;

using HarmonyLib;

using UnityEngine;



namespace BossSpawnControl

{

    [BepInPlugin(PluginInfo.GUID, PluginInfo.NAME, PluginInfo.VERSION)]

    public sealed class PluginCore : BaseUnityPlugin

    {

        internal static PluginCore Instance { get; private set; }



        internal readonly BossConfigService ConfigService = new BossConfigService();

        internal readonly BotConfigService BotConfigService = new BotConfigService();

        internal readonly PopulationMaintenanceConfigService PopulationConfig = new PopulationMaintenanceConfigService();



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

            BotConfigService.Bind(Config);

            PopulationConfig.Bind(Config);



            var harmony = new Harmony(PluginInfo.GUID);

            harmony.PatchAll(typeof(PluginCore).Assembly);



            var go = new GameObject("BossSpawnControl_Population");

            DontDestroyOnLoad(go);

            go.hideFlags = HideFlags.HideAndDontSave;

            go.AddComponent<PopulationMaintenanceBehaviour>();
            go.AddComponent<BotRemovalPollRunner>();



            Logger.LogInfo($"{PluginInfo.NAME} v{PluginInfo.VERSION} loaded");

            Logger.LogInfo($"Bosses in F12 config: {BossCatalog.AllBosses.Count}");

            Logger.LogInfo($"Bots in F12 config: {BotCatalog.AllBots.Count}");

            Logger.LogInfo("F12 -> Boss Spawn Control: Боссы / Боты / Население");

        }

    }

}

