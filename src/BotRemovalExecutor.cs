namespace BossSpawnControl
{
    internal static class BotRemovalExecutor
    {
        internal static void ClearAllBotsFromButton(PluginCore plugin)
        {
            var cfg = plugin.PopulationConfig;

            if (!cfg.ConfirmClearBots.Value)
            {
                plugin.Log(
                    "[POPULATION] Clear button pressed — ABORT: включите «Подтверждаю удаление всех ботов».",
                    true);
                return;
            }

            if (!BotSpawnAuthority.HasBotRemovalAuthority(out var authorityReason))
            {
                plugin.Log(
                    $"[POPULATION] Clear button pressed — ABORT: {authorityReason}. " +
                    $"({BotSpawnAuthority.DescribeAuthority()})",
                    true);
                return;
            }

            if (BotRemovalPollRunner.Instance == null)
            {
                plugin.Log("[POPULATION] Clear runner not ready — retry after raid load.", true);
                return;
            }

            plugin.Log("[POPULATION] Clear button pressed — starting instant clear.", true);
            BotRemovalPollRunner.Instance.StartClear(plugin);
        }
    }
}
