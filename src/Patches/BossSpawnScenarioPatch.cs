using HarmonyLib;

namespace BossSpawnControl.Patches
{
    /// <summary>
    /// Патч после ABPS progressive boss patch (priority 0 = последний prefix).
    /// </summary>
    [HarmonyPatch(typeof(BossSpawnScenario), nameof(BossSpawnScenario.smethod_0))]
    internal static class BossSpawnScenarioPatch
    {
        [HarmonyPrefix]
        [HarmonyPriority(0)]
        private static void Prefix(BossLocationSpawn[] bossWaves)
        {
            var plugin = PluginCore.Instance;
            if (plugin == null)
            {
                return;
            }

            BossSpawnExecutor.ApplyAutoRules(bossWaves, plugin);
        }
    }
}
