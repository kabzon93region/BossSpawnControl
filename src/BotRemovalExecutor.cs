namespace BossSpawnControl
{
    internal static class BotRemovalExecutor
    {
        internal static void ClearAllBotsFromButton(PluginCore plugin)
        {
            if (BotRemovalPollRunner.Instance == null)
            {
                plugin.Log("[POPULATION] Clear runner not ready — retry after raid load.", true);
                return;
            }

            BotRemovalPollRunner.Instance.StartClear(plugin);
        }
    }
}
