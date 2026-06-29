using System.Collections;
using System.Linq;
using System.Text;
using Comfort.Common;
using EFT;
using UnityEngine;

namespace BossSpawnControl
{
    internal sealed class BotRemovalPollRunner : MonoBehaviour
    {
        internal static BotRemovalPollRunner Instance { get; private set; }

        private Coroutine _clearCoroutine;

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        internal void StartClear(PluginCore plugin)
        {
            if (_clearCoroutine != null)
            {
                plugin.Log("[POPULATION] Clear already in progress — ignored.", true);
                return;
            }

            _clearCoroutine = StartCoroutine(ClearCoroutine(plugin));
        }

        private IEnumerator ClearCoroutine(PluginCore plugin)
        {
            var cfg = plugin.PopulationConfig;
            var log = new StringBuilder();
            log.AppendLine("[POPULATION] ===== CLEAR ALL BOTS (instant) =====");
            log.AppendLine($"  Authority: {BotSpawnAuthority.DescribeAuthority()}");

            if (!cfg.ConfirmClearBots.Value)
            {
                log.AppendLine("  ABORT: включите «Подтверждаю удаление всех ботов» выше.");
                plugin.Log(log.ToString(), true);
                _clearCoroutine = null;
                yield break;
            }

            if (!BotSpawnAuthority.HasBotRemovalAuthority(out var authorityError))
            {
                log.AppendLine($"  ABORT: {authorityError}");
                plugin.Log(log.ToString(), true);
                _clearCoroutine = null;
                yield break;
            }

            if (cfg.MaintenanceRunning.Value)
            {
                cfg.MaintenanceRunning.Value = false;
                PopulationMaintenanceBehaviour.Instance?.SyncFromConfig();
                log.AppendLine("  Maintenance auto-stopped before clear.");
            }

            var blockSec = Mathf.Max(0, cfg.ClearSpawnBlockSec.Value);
            BotClearSpawnBlock.Activate(blockSec);
            log.AppendLine($"  Spawn block active for {blockSec}s (maintenance cannot respawn).");

            var protectCompanions = cfg.ProtectPitFireCompanions.Value;
            var skipBtr = cfg.ProtectBtrDuringClear.Value;
            log.AppendLine(
                $"  ProtectPitFireCompanions={protectCompanions} ProtectBtr={skipBtr} pitFireDetected={PitFireCompanionGuard.IsAvailable}");

            var snapshotBefore = BotPopulationCounter.Collect(plugin, "before clear");
            log.AppendLine($"  Before clear: activeBots={snapshotBefore.TotalActiveBots}");

            var botsController = Singleton<IBotGame>.Instance?.BotsController;
            if (botsController?.Bots?.BotOwners == null)
            {
                log.AppendLine("  ABORT: BotsController/BotOwners unavailable.");
                plugin.Log(log.ToString(), true);
                _clearCoroutine = null;
                yield break;
            }

            var initialBots = botsController.Bots.BotOwners.ToList();
            log.AppendLine($"  Instant remove pass for {initialBots.Count} BotOwners.");

            var stats = new BotRemovalStats();
            foreach (var bot in initialBots)
            {
                if (protectCompanions && PitFireCompanionGuard.IsProtectedBot(bot))
                {
                    stats.ProtectedCompanions++;
                    log.AppendLine(
                        $"  KEEP companion profileId={bot.ProfileId} role={bot.Profile?.Info?.Settings?.Role}");
                    continue;
                }

                var attempt = BotInstantRemover.TryRemove(bot, skipBtr);
                stats.Record(attempt);
                log.AppendLine(
                    $"  {attempt.Method} profileId={bot?.ProfileId} role={bot?.Profile?.Info?.Settings?.Role} detail={attempt.Detail}");
            }

            log.AppendLine($"  First pass: {stats.SummaryLine()}");

            var timeoutSec = Mathf.Clamp(cfg.ClearPollTimeoutSec.Value, 2, 120);
            var pollInterval = Mathf.Clamp(cfg.ClearPollIntervalSec.Value, 0.25f, 5f);
            var deadline = Time.unscaledTime + timeoutSec;
            var pollPass = 0;

            while (Time.unscaledTime < deadline)
            {
                yield return new WaitForSecondsRealtime(pollInterval);
                pollPass++;

                var remaining = botsController.Bots.BotOwners
                    .Where(b => b != null && BotInstantRemover.IsStillListed(b))
                    .Where(b => !protectCompanions || !PitFireCompanionGuard.IsProtectedBot(b))
                    .Where(b => !skipBtr || (b.Profile?.Info?.Settings?.Role ?? WildSpawnType.assault) != WildSpawnType.shooterBTR)
                    .ToList();

                if (remaining.Count == 0)
                {
                    log.AppendLine($"  Poll pass {pollPass}: all targets gone.");
                    break;
                }

                log.AppendLine($"  Poll pass {pollPass}: stillAlive={remaining.Count} — retry force kill.");
                foreach (var bot in remaining)
                {
                    var attempt = BotInstantRemover.TryForceKill(bot);
                    stats.Record(attempt);
                    log.AppendLine(
                        $"    RETRY {attempt.Method} profileId={bot.ProfileId} role={bot.Profile?.Info?.Settings?.Role} detail={attempt.Detail}");
                }
            }

            var spawner = botsController.BotSpawner;
            if (spawner != null)
            {
                BotSpawnerDiagnostics.AppendSpawnerState(log, spawner);
            }

            var snapshotAfter = BotPopulationCounter.Collect(plugin, "after clear");
            var removedCount = snapshotBefore.TotalActiveBots - snapshotAfter.TotalActiveBots;
            log.AppendLine($"  After clear: activeBots={snapshotAfter.TotalActiveBots} (delta={removedCount})");
            log.AppendLine($"  Stats: {stats.SummaryLine()}");
            log.AppendLine("[POPULATION] ===== END CLEAR ALL BOTS =====");
            plugin.Log(log.ToString(), true);

            cfg.ConfirmClearBots.Value = false;
            _clearCoroutine = null;
        }

        private sealed class BotRemovalStats
        {
            internal int RemoveFromMap;
            internal int Kill;
            internal int LeaveQueued;
            internal int ProtectedCompanions;
            internal int SkippedBtr;
            internal int SkippedInvalid;
            internal int Failed;

            internal void Record(BotRemovalAttempt attempt)
            {
                switch (attempt.Method)
                {
                    case BotRemovalMethod.RemoveFromMap:
                        RemoveFromMap++;
                        break;
                    case BotRemovalMethod.Kill:
                        Kill++;
                        break;
                    case BotRemovalMethod.BotDespawn:
                        RemoveFromMap++;
                        break;
                    case BotRemovalMethod.LeaveQueued:
                        LeaveQueued++;
                        break;
                    case BotRemovalMethod.SkippedBtr:
                        SkippedBtr++;
                        break;
                    case BotRemovalMethod.SkippedInvalid:
                        SkippedInvalid++;
                        break;
                    case BotRemovalMethod.Failed:
                        Failed++;
                        break;
                }
            }

            internal string SummaryLine()
            {
                return
                    $"removeFromMap={RemoveFromMap} kill={Kill} leaveQueued={LeaveQueued} " +
                    $"protectedCompanions={ProtectedCompanions} skippedBtr={SkippedBtr} " +
                    $"skippedInvalid={SkippedInvalid} failed={Failed}";
            }
        }
    }
}
