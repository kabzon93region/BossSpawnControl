using System.Text;
using System.Threading.Tasks;
using Comfort.Common;
using EFT;

namespace BossSpawnControl
{
    internal static class PopulationMaintenanceService
    {
        internal static async Task RunMaintenanceTickAsync(PluginCore plugin)
        {
            var cfg = plugin.PopulationConfig;
            var log = new StringBuilder();
            log.AppendLine("[POPULATION] ===== MAINTENANCE TICK =====");
            log.AppendLine($"  MaintenanceRunning={cfg.MaintenanceRunning.Value} AutoSpawn={cfg.AutoSpawnOnMaintenance.Value}");

            if (!cfg.MaintenanceRunning.Value)
            {
                log.AppendLine("  ABORT: maintenance not running.");
                plugin.Log(log.ToString(), true);
                return;
            }

            if (!IsInRaid())
            {
                log.AppendLine("  ABORT: not in raid.");
                plugin.Log(log.ToString(), true);
                return;
            }

            if (BotClearSpawnBlock.IsActive())
            {
                log.AppendLine(
                    $"  ABORT: spawn blocked after clear ({BotClearSpawnBlock.RemainingSeconds():0.0}s remaining).");
                log.AppendLine("[POPULATION] ===== END MAINTENANCE TICK =====");
                plugin.Log(log.ToString(), true);
                return;
            }

            if (!BotSpawnAuthority.HasBotRemovalAuthority(out var authorityReason))
            {
                log.AppendLine($"  ABORT: no bot authority on this machine ({authorityReason}). Scan/spawn skipped.");
                log.AppendLine("[POPULATION] ===== END MAINTENANCE TICK =====");
                plugin.Log(log.ToString(), true);
                return;
            }

            PopulationSpawnerLimitSync.Apply(plugin);

            var snapshot = BotPopulationCounter.Collect(plugin, "maintenance");
            cfg.SetLastSnapshot(snapshot);

            var trimmed = PopulationExcessTrimmer.TrimIfOverLimits(plugin, snapshot, log);
            if (trimmed > 0)
            {
                log.AppendLine($"  Trimmed {trimmed} bot(s) over configured limits.");
                snapshot = BotPopulationCounter.Collect(plugin, "maintenance-after-trim");
                cfg.SetLastSnapshot(snapshot);
            }

            if (!cfg.AutoSpawnOnMaintenance.Value)
            {
                log.AppendLine("  AutoSpawn disabled — scan/trim only.");
                log.AppendLine("[POPULATION] ===== END MAINTENANCE TICK =====");
                plugin.Log(log.ToString(), true);
                return;
            }

            if (!TryGetBotSpawner(out var spawner, out var spawnerError, log))
            {
                log.AppendLine($"  ABORT spawn phase: {spawnerError}");
                log.AppendLine("[POPULATION] ===== END MAINTENANCE TICK =====");
                plugin.Log(log.ToString(), true);
                return;
            }

            var deficits = PopulationMaintenanceDeficits.Calculate(plugin, snapshot, log);
            if (deficits.Count == 0)
            {
                log.AppendLine("  All faction/total limits satisfied — nothing to spawn.");
                log.AppendLine("[POPULATION] ===== END MAINTENANCE TICK =====");
                plugin.Log(log.ToString(), true);
                return;
            }

            var spawnedTotal = await PopulationMaintenanceSpawner.SpawnDeficitsAsync(plugin, spawner, deficits, log);

            log.AppendLine($"  MAINTENANCE RESULT spawnedThisTick={spawnedTotal}");
            log.AppendLine("[POPULATION] ===== END MAINTENANCE TICK =====");
            plugin.Log(log.ToString(), true);
        }

        private static bool IsInRaid()
        {
            return Singleton<GameWorld>.Instantiated && Singleton<IBotGame>.Instantiated;
        }

        private static bool TryGetBotSpawner(out BotSpawner spawner, out string error, StringBuilder log)
        {
            spawner = null;
            error = null;

            if (!Singleton<IBotGame>.Instantiated)
            {
                error = "IBotGame not instantiated.";
                return false;
            }

            spawner = Singleton<IBotGame>.Instance?.BotsController?.BotSpawner;
            if (spawner == null)
            {
                error = "BotSpawner is null.";
                return false;
            }

            return true;
        }
    }
}
