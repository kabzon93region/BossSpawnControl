using System.Collections.Generic;
using System.Linq;
using System.Text;
using Comfort.Common;
using EFT;

namespace BossSpawnControl
{
    /// <summary>
    /// Removes bots over configured total/faction limits (vanilla queue may push count above mod cap).
    /// </summary>
    internal static class PopulationExcessTrimmer
    {
        internal static int TrimIfOverLimits(PluginCore plugin, BotPopulationSnapshot snapshot, StringBuilder log)
        {
            var cfg = plugin.PopulationConfig;
            var totalLimit = cfg.LimitTotal.Value;
            if (totalLimit <= 0 && !HasAnyFactionLimit(cfg))
            {
                return 0;
            }

            if (!Singleton<IBotGame>.Instantiated)
            {
                return 0;
            }

            var botsController = Singleton<IBotGame>.Instance?.BotsController;
            var botOwners = botsController?.Bots?.BotOwners;
            if (botOwners == null)
            {
                return 0;
            }

            var candidates = new List<(BotOwner Bot, BotFactionCategory Faction, int Priority)>();
            foreach (var bot in botOwners.ToList())
            {
                if (bot?.Profile?.Info?.Settings == null || bot.IsDead)
                {
                    continue;
                }

                if (cfg.ProtectPitFireCompanions.Value && PitFireCompanionGuard.IsProtectedBot(bot))
                {
                    continue;
                }

                if (cfg.ProtectBtrDuringClear.Value
                    && bot.Profile.Info.Settings.Role == WildSpawnType.shooterBTR)
                {
                    continue;
                }

                var faction = BotFactionClassifier.Classify(
                    bot.Profile.Info.Settings.Role,
                    bot.Profile.Info.Side);
                var priority = GetPriority(cfg, faction);
                candidates.Add((bot, faction, priority));
            }

            var removed = 0;
            var effectiveTotal = snapshot.GetEffectiveTotal();

            if (totalLimit > 0 && effectiveTotal > totalLimit)
            {
                var need = effectiveTotal - totalLimit;
                log.AppendLine(
                    $"  TRIM: effectiveTotal={effectiveTotal} > limit={totalLimit}, removing up to {need}");
                removed += RemoveLowestPriority(candidates, need, log, "total");
            }

            foreach (var faction in cfg.GetFactionsByPriority())
            {
                var factionLimit = cfg.GetFactionLimit(faction);
                if (factionLimit <= 0)
                {
                    continue;
                }

                var count = snapshot.GetFactionCount(faction);
                if (count <= factionLimit)
                {
                    continue;
                }

                var need = count - factionLimit;
                log.AppendLine(
                    $"  TRIM: {faction.GetDisplayName()} {count}/{factionLimit}, removing up to {need}");
                removed += RemoveFromFaction(candidates, faction, need, log);
            }

            return removed;
        }

        private static int RemoveFromFaction(
            List<(BotOwner Bot, BotFactionCategory Faction, int Priority)> candidates,
            BotFactionCategory faction,
            int need,
            StringBuilder log)
        {
            var removed = 0;
            foreach (var entry in candidates
                .Where(c => c.Faction == faction)
                .OrderByDescending(c => c.Priority)
                .ThenByDescending(c => c.Bot?.ActivateTime ?? 0f)
                .ToList())
            {
                if (removed >= need)
                {
                    break;
                }

                if (TryRemoveEntry(entry.Bot, log, faction.GetDisplayName()))
                {
                    removed++;
                    candidates.Remove(entry);
                }
            }

            return removed;
        }

        private static int RemoveLowestPriority(
            List<(BotOwner Bot, BotFactionCategory Faction, int Priority)> candidates,
            int need,
            StringBuilder log,
            string reason)
        {
            var removed = 0;
            foreach (var entry in candidates
                .OrderByDescending(c => c.Priority)
                .ThenByDescending(c => c.Bot?.ActivateTime ?? 0f)
                .ToList())
            {
                if (removed >= need)
                {
                    break;
                }

                if (TryRemoveEntry(entry.Bot, log, reason))
                {
                    removed++;
                    candidates.Remove(entry);
                }
            }

            return removed;
        }

        private static bool TryRemoveEntry(BotOwner bot, StringBuilder log, string reason)
        {
            var attempt = BotInstantRemover.TryRemove(bot, skipBtr: false);
            if (!attempt.CountsAsRemovedAttempt && attempt.Method != BotRemovalMethod.Failed)
            {
                return false;
            }

            log.AppendLine(
                $"    TRIM {attempt.Method} {reason} profileId={bot?.ProfileId} " +
                $"role={bot?.Profile?.Info?.Settings?.Role} detail={attempt.Detail}");
            return attempt.CountsAsRemovedAttempt;
        }

        private static bool HasAnyFactionLimit(PopulationMaintenanceConfigService cfg)
        {
            foreach (BotFactionCategory faction in System.Enum.GetValues(typeof(BotFactionCategory)))
            {
                if (cfg.GetFactionLimit(faction) > 0)
                {
                    return true;
                }
            }

            return false;
        }

        private static int GetPriority(PopulationMaintenanceConfigService cfg, BotFactionCategory faction)
        {
            var list = cfg.GetFactionsByPriority().ToList();
            var index = list.IndexOf(faction);
            return index >= 0 ? index : 99;
        }
    }
}
