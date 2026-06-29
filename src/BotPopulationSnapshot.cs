using System;

using System.Collections.Generic;

using System.Linq;

using System.Text;



namespace BossSpawnControl

{

    internal sealed class BotPopulationSnapshot

    {

        internal readonly Dictionary<BotFactionCategory, int> FactionCounts = new Dictionary<BotFactionCategory, int>();

        internal readonly Dictionary<string, int> RoleCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);



        internal int TotalActiveBots { get; private set; }

        internal int IgnoredInactiveBots { get; set; }

        internal bool InRaid { get; set; }



        internal int SpawnerMaxBots { get; set; }

        internal int SpawnerAllBotsCount { get; set; }

        internal int SpawnerInSpawnProcess { get; set; }

        internal int SpawnerAllBotsWithLoaded { get; set; }

        internal int SpawnerQueueWaitCount { get; set; }

        internal int SpawnerBotsLoading { get; set; }



        internal BotPopulationSnapshot()

        {

            foreach (BotFactionCategory faction in Enum.GetValues(typeof(BotFactionCategory)))

            {

                FactionCounts[faction] = 0;

            }

        }



        internal int GetFactionCount(BotFactionCategory faction)

        {

            return FactionCounts.TryGetValue(faction, out var count) ? count : 0;

        }



        internal void RecalculateTotal()

        {

            TotalActiveBots = FactionCounts.Values.Sum();

        }



        internal string FormatSummary()

        {

            var sb = new StringBuilder();

            sb.AppendLine($"  InRaid={InRaid} activeBots={TotalActiveBots} ignoredInactive={IgnoredInactiveBots}");

            sb.AppendLine(

                $"  Spawner: gameMaxBots={SpawnerMaxBots} alive={SpawnerAllBotsCount} inSpawn={SpawnerInSpawnProcess} " +

                $"loaded={SpawnerAllBotsWithLoaded} queue={SpawnerQueueWaitCount} botsLoading={SpawnerBotsLoading}");



            foreach (BotFactionCategory faction in Enum.GetValues(typeof(BotFactionCategory)))

            {

                sb.AppendLine($"  {faction.GetDisplayName()}: {GetFactionCount(faction)}");

            }



            if (RoleCounts.Count > 0)

            {

                sb.AppendLine("  Roles:");

                foreach (var pair in RoleCounts.OrderByDescending(x => x.Value).ThenBy(x => x.Key, StringComparer.OrdinalIgnoreCase))

                {

                    sb.AppendLine($"    {pair.Key}={pair.Value}");

                }

            }



            return sb.ToString();

        }

    }

}

