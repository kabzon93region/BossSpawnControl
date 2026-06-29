using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EFT;
using UnityEngine;

namespace BossSpawnControl
{
    internal static class PopulationMaintenanceSpawner
    {
        private static int _scavSpawnIndex;
        private static int _bossSpawnIndex;

        internal static async Task<int> SpawnDeficitsAsync(
            PluginCore plugin,
            BotSpawner spawner,
            BotZone zone,
            System.Collections.Generic.List<(BotFactionCategory Faction, int Deficit)> deficits,
            StringBuilder log)
        {
            var cfg = plugin.PopulationConfig;
            var maxSpawns = cfg.MaxSpawnsPerTick.Value;
            var spawnedTotal = 0;
            var difficulty = plugin.BotConfigService.BotDifficulty.Value;

            foreach (var (faction, deficit) in deficits)
            {
                if (spawnedTotal >= maxSpawns)
                {
                    log.AppendLine($"  STOP: MaxSpawnsPerTick={maxSpawns} reached.");
                    break;
                }

                if (faction == BotFactionCategory.Zombies && !cfg.SpawnZombies.Value)
                {
                    log.AppendLine($"  SKIP {faction.GetDisplayName()}: SpawnZombies=false (deficit={deficit}).");
                    continue;
                }

                var toSpawn = Math.Min(deficit, maxSpawns - spawnedTotal);
                log.AppendLine($"  SPAWN PLAN {faction.GetDisplayName()}: deficit={deficit} thisTick={toSpawn}");

                for (var i = 0; i < toSpawn; i++)
                {
                    var ok = await TrySpawnForFactionAsync(plugin, spawner, zone, faction, difficulty, log, i + 1, toSpawn);
                    if (ok)
                    {
                        spawnedTotal++;
                    }
                }
            }

            return spawnedTotal;
        }

        internal static BotZone PickSpawnZone(BotSpawner spawner, StringBuilder log)
        {
            var zones = spawner.AllBotZones;
            if (zones == null || zones.Length == 0)
            {
                log.AppendLine("  AllBotZones empty.");
                return null;
            }

            var index = UnityEngine.Random.Range(0, zones.Length);
            var zone = zones[index];
            log.AppendLine($"  Picked zone [{index}/{zones.Length}]: {zone.name}");
            return zone;
        }

        private static async Task<bool> TrySpawnForFactionAsync(
            PluginCore plugin,
            BotSpawner spawner,
            BotZone zone,
            BotFactionCategory faction,
            BotDifficulty difficulty,
            StringBuilder log,
            int attemptIndex,
            int attemptTotal)
        {
            if (!TryResolveSpawnRequest(faction, plugin, log, out var side, out var spawnType, out var label))
            {
                return false;
            }

            log.AppendLine(
                $"  SPAWN ATTEMPT {attemptIndex}/{attemptTotal} faction={faction.GetDisplayName()} " +
                $"role={spawnType} side={side} diff={difficulty} forced=true");

            BotSpawnerDiagnostics.AppendZoneState(log, zone);
            BotSpawnerDiagnostics.AppendSpawnerState(log, spawner);

            try
            {
                await spawner.method_2(side, zone, spawnType, difficulty, forcedSpawn: true);
                log.AppendLine($"  SPAWN OK {label} via method_2 completed.");
                return true;
            }
            catch (Exception ex)
            {
                log.AppendLine($"  SPAWN FAIL {label}: {ex.GetType().Name}: {ex.Message}");
                return false;
            }
        }

        private static bool TryResolveSpawnRequest(
            BotFactionCategory faction,
            PluginCore plugin,
            StringBuilder log,
            out EPlayerSide side,
            out WildSpawnType spawnType,
            out string label)
        {
            side = EPlayerSide.Savage;
            spawnType = WildSpawnType.assault;
            label = faction.ToString();

            switch (faction)
            {
                case BotFactionCategory.Rogues:
                    side = EPlayerSide.Usec;
                    spawnType = WildSpawnType.exUsec;
                    label = "exUsec rogue";
                    return true;

                case BotFactionCategory.Scavs:
                    side = EPlayerSide.Savage;
                    spawnType = PickScavRole();
                    label = $"scav {spawnType}";
                    return true;

                case BotFactionCategory.Zombies:
                    side = EPlayerSide.Savage;
                    spawnType = WildSpawnType.infectedAssault;
                    label = "infectedAssault";
                    return true;

                case BotFactionCategory.Usec:
                    side = EPlayerSide.Usec;
                    spawnType = WildSpawnType.pmcUSEC;
                    label = "pmcUSEC";
                    return true;

                case BotFactionCategory.Bear:
                    side = EPlayerSide.Bear;
                    spawnType = WildSpawnType.pmcBEAR;
                    label = "pmcBEAR";
                    return true;

                case BotFactionCategory.BossesAndFollowers:
                    var enabledBosses = plugin.ConfigService.GetEnabledBosses().ToList();
                    if (enabledBosses.Count == 0)
                    {
                        log.AppendLine("  SKIP Bosses+свита: no bosses enabled in F12 -> Боссы.");
                        return false;
                    }

                    var boss = enabledBosses[_bossSpawnIndex % enabledBosses.Count];
                    _bossSpawnIndex++;

                    if (!boss.SpawnType.IsBossOrFollower())
                    {
                        log.AppendLine($"  SKIP boss {boss.Id}: not boss/follower role.");
                        return false;
                    }

                    side = EPlayerSide.Savage;
                    spawnType = boss.SpawnType;
                    label = $"boss {boss.Id}";
                    return true;

                default:
                    log.AppendLine($"  SKIP unknown faction {faction}.");
                    return false;
            }
        }

        private static WildSpawnType PickScavRole()
        {
            var roles = new[]
            {
                WildSpawnType.assault,
                WildSpawnType.cursedAssault,
                WildSpawnType.marksman
            };

            var role = roles[_scavSpawnIndex % roles.Length];
            _scavSpawnIndex++;
            return role;
        }
    }
}
