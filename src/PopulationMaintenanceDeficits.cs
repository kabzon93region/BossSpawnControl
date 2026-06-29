using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Comfort.Common;
using EFT;

namespace BossSpawnControl
{
    internal static class PopulationMaintenanceDeficits
    {
        internal static List<(BotFactionCategory Faction, int Deficit)> Calculate(
            PluginCore plugin,
            BotPopulationSnapshot snapshot,
            StringBuilder log)
        {
            var cfg = plugin.PopulationConfig;
            var result = new List<(BotFactionCategory, int)>();

            var totalLimit = cfg.LimitTotal.Value;
            var currentTotal = snapshot.TotalActiveBots;
            var globalRoom = totalLimit > 0 ? Math.Max(0, totalLimit - currentTotal) : int.MaxValue;

            log.AppendLine(
                $"  Limits: configuredTotal={totalLimit} currentActive={currentTotal} globalRoom={FormatRoom(globalRoom)}");
            log.AppendLine($"  Sum of faction limits={cfg.GetConfiguredFactionLimitSum()} (may exceed total — priority applies)");

            if (totalLimit > 0 && globalRoom == 0)
            {
                log.AppendLine("  Global total limit reached — no spawns until bots die.");
                return result;
            }

            foreach (var faction in cfg.GetFactionsByPriority())
            {
                var factionLimit = cfg.GetFactionLimit(faction);
                if (factionLimit <= 0)
                {
                    log.AppendLine($"  {faction.GetDisplayName()}: limit=0 (unlimited), skip deficit.");
                    continue;
                }

                var current = snapshot.GetFactionCount(faction);
                var factionDeficit = Math.Max(0, factionLimit - current);
                if (factionDeficit <= 0)
                {
                    log.AppendLine($"  {faction.GetDisplayName()}: {current}/{factionLimit} OK");
                    continue;
                }

                var allowed = Math.Min(factionDeficit, globalRoom);
                log.AppendLine(
                    $"  {faction.GetDisplayName()}: {current}/{factionLimit} deficit={factionDeficit} " +
                    $"allowedByGlobalRoom={allowed} priority={GetPriority(plugin, faction)}");

                if (allowed > 0)
                {
                    result.Add((faction, allowed));
                    if (globalRoom != int.MaxValue)
                    {
                        globalRoom -= allowed;
                    }
                }
            }

            return result;
        }

        private static int GetPriority(PluginCore plugin, BotFactionCategory faction)
        {
            return plugin.PopulationConfig.GetFactionsByPriority()
                .Select((f, i) => new { f, i })
                .First(x => x.f == faction).i + 1;
        }

        private static string FormatRoom(int room)
        {
            return room == int.MaxValue ? "unlimited" : room.ToString();
        }
    }
}
