using System.Linq;
using System.Text;

using Comfort.Common;

using EFT;



namespace BossSpawnControl

{

    internal static class BotPopulationCounter

    {

        internal static BotPopulationSnapshot Collect(PluginCore plugin, string reason)

        {

            var snapshot = new BotPopulationSnapshot();

            var log = new StringBuilder();

            log.AppendLine($"[POPULATION] ===== SCAN ({reason}) =====");



            if (!Singleton<GameWorld>.Instantiated)

            {

                log.AppendLine("  ABORT: GameWorld not instantiated.");

                plugin.Log(log.ToString(), true);

                return snapshot;

            }



            snapshot.InRaid = Singleton<GameWorld>.Instance?.MainPlayer != null;

            log.AppendLine($"  GameWorld ok, InRaid={snapshot.InRaid}");



            if (!Singleton<IBotGame>.Instantiated)

            {

                log.AppendLine("  ABORT: IBotGame not instantiated (not host / no bot authority?).");

                plugin.Log(log.ToString(), true);

                return snapshot;

            }



            var botsController = Singleton<IBotGame>.Instance?.BotsController;

            if (botsController == null)

            {

                log.AppendLine("  ABORT: BotsController is null.");

                plugin.Log(log.ToString(), true);

                return snapshot;

            }



            var spawner = botsController.BotSpawner;

            if (spawner != null)

            {

                BotSpawnerDiagnostics.AppendSpawnerState(log, spawner);

                snapshot.SpawnerMaxBots = spawner.MaxBots;

                snapshot.SpawnerAllBotsCount = spawner.AllBotsCount;

                snapshot.SpawnerInSpawnProcess = spawner.InSpawnProcess;

                snapshot.SpawnerAllBotsWithLoaded = spawner.AllBotsWithLoaded;

                snapshot.SpawnerQueueWaitCount = spawner.SpawnDelaysService?.WaitCount ?? 0;

                snapshot.SpawnerBotsLoading = spawner.BotCreator?.BotsLoading ?? 0;

            }

            else

            {

                log.AppendLine("  WARN: BotSpawner is null.");

            }



            var botOwners = botsController.Bots?.BotOwners;

            if (botOwners == null)

            {

                log.AppendLine("  WARN: BotOwners collection is null.");

                snapshot.RecalculateTotal();

                log.AppendLine(snapshot.FormatSummary());

                log.AppendLine("[POPULATION] ===== END SCAN =====");

                plugin.Log(log.ToString(), true);

                return snapshot;

            }



            var botOwnerList = botOwners.ToList();
            log.AppendLine($"  BotOwners entries: {botOwnerList.Count}");

            foreach (var bot in botOwnerList)

            {

                if (!IsActiveBot(bot, log))

                {

                    snapshot.IgnoredInactiveBots++;

                    continue;

                }



                var role = bot.Profile.Info.Settings.Role;

                var side = bot.Profile.Info.Side;

                var roleName = role.ToString();

                var faction = BotFactionClassifier.Classify(role, side);



                snapshot.FactionCounts[faction]++;

                if (!snapshot.RoleCounts.ContainsKey(roleName))

                {

                    snapshot.RoleCounts[roleName] = 0;

                }



                snapshot.RoleCounts[roleName]++;



                if (plugin.PopulationConfig.VerboseRoleLogging.Value)

                {

                    log.AppendLine($"    ACTIVE {roleName} side={side} faction={faction.GetDisplayName()} id={bot.ProfileId}");

                }

            }



            snapshot.RecalculateTotal();

            log.AppendLine(snapshot.FormatSummary());

            log.AppendLine("[POPULATION] ===== END SCAN =====");

            plugin.Log(log.ToString(), true);



            PopulationMaintenanceBehaviour.UpdateLastSnapshot(snapshot);

            return snapshot;

        }



        private static bool IsActiveBot(BotOwner bot, StringBuilder log)

        {

            if (bot?.Profile?.Info?.Settings == null)

            {

                log.AppendLine("    SKIP: bot without profile/settings.");

                return false;

            }



            if (bot.IsDead)

            {

                return false;

            }



            if (bot.BotState == EBotState.Disposed)

            {

                return false;

            }



            try

            {

                var player = bot.GetPlayer;

                if (player?.HealthController != null && !player.HealthController.IsAlive)

                {

                    return false;

                }

            }

            catch

            {

                // lifecycle edge case

            }



            return true;

        }

    }

}

