using System.Text;

using EFT;



namespace BossSpawnControl

{

    internal static class BotSpawnerDiagnostics

    {

        internal static void AppendSpawnerState(StringBuilder log, BotSpawner spawner)

        {

            if (log == null)

            {

                return;

            }



            if (spawner == null)

            {

                log.AppendLine("  Spawner: null");

                return;

            }



            var queue = spawner.SpawnDelaysService?.WaitCount ?? 0;

            var botsLoading = spawner.BotCreator?.BotsLoading ?? 0;

            var atMax = spawner.MaxBots > 0 && spawner.AllBotsWithLoaded >= spawner.MaxBots;



            log.AppendLine(

                $"  Spawner: gameMaxBots={spawner.MaxBots} alive={spawner.AllBotsCount} " +

                $"inSpawn={spawner.InSpawnProcess} loaded={spawner.AllBotsWithLoaded} " +

                $"queue={queue} botsLoading={botsLoading} atGameMax={atMax}");



            if (spawner.MaxBots > 0)

            {

                var room = spawner.MaxBots - spawner.AllBotsWithLoaded;

                log.AppendLine($"  Spawner game room: {room} (MaxBots - AllBotsWithLoaded)");

            }

        }



        internal static void AppendZoneState(StringBuilder log, BotZone zone)

        {

            if (log == null)

            {

                return;

            }



            if (zone == null)

            {

                log.AppendLine("  Zone: null");

                return;

            }



            log.AppendLine($"  Zone: name={zone.name} pos={zone.transform.position}");

        }

    }

}

