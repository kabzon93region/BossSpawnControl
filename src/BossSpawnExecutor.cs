using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Comfort.Common;
using EFT;
using UnityEngine;

namespace BossSpawnControl
{
    internal static class BossSpawnExecutor
    {
        internal static BossLocationSpawn[] CachedWaves { get; private set; } = Array.Empty<BossLocationSpawn>();

        internal static void CacheWaves(BossLocationSpawn[] waves)
        {
            CachedWaves = waves?
                .Where(w => w != null && !string.IsNullOrWhiteSpace(w.BossName))
                .ToArray() ?? Array.Empty<BossLocationSpawn>();
        }

        internal static void ApplyAutoRules(BossLocationSpawn[] waves, PluginCore plugin)
        {
            CacheWaves(waves);

            if (!plugin.ConfigService.ModEnabled.Value || waves == null || waves.Length == 0)
            {
                plugin.Log("[BOSS_SPAWN] Auto rules skipped (ModEnabled=false or no boss waves).", plugin.ConfigService.DebugLogging.Value);
                return;
            }

            var log = new StringBuilder();
            log.AppendLine("[BOSS_SPAWN] Auto rules at raid start:");
            log.AppendLine($"  Map waves: {waves.Length}");

            var changed = 0;
            foreach (var wave in waves)
            {
                if (wave == null || !plugin.ConfigService.IsBossEnabled(wave.BossName))
                {
                    continue;
                }

                var before = wave.BossChance;
                wave.BossChance = 100f;
                changed++;
                log.AppendLine($"  ENABLE {wave.BossName}: chance {before:0.##} -> 100 zone={wave.BossZone} time={wave.Time:0.##}");
            }

            log.AppendLine($"  Changed waves: {changed}");
            plugin.Log(log.ToString(), true);
        }

        internal static void ForceSpawnFromButton(PluginCore plugin)
        {
            var log = new StringBuilder();
            log.AppendLine("[BOSS_SPAWN] ===== DEBUG SPAWN BUTTON =====");
            log.AppendLine($"  ModEnabled={plugin.ConfigService.ModEnabled.Value}");
            log.AppendLine($"  InRaid={IsInRaid()}");
            log.AppendLine($"  CachedWaves={CachedWaves.Length}");

            if (!IsInRaid())
            {
                log.AppendLine("  ABORT: not in raid (GameWorld/BotSpawner unavailable).");
                plugin.Log(log.ToString(), true);
                return;
            }

            if (!TryGetBotSpawner(out var spawner, out var spawnerError))
            {
                log.AppendLine($"  ABORT: {spawnerError}");
                plugin.Log(log.ToString(), true);
                return;
            }

            var enabledBosses = plugin.ConfigService.GetEnabledBosses().ToList();
            log.AppendLine($"  Enabled bosses in config: {enabledBosses.Count}");
            foreach (var boss in enabledBosses)
            {
                log.AppendLine($"    - {boss.DisplayName}");
            }

            if (enabledBosses.Count == 0)
            {
                log.AppendLine("  ABORT: no bosses enabled in F12 -> Боссы.");
                plugin.Log(log.ToString(), true);
                return;
            }

            var zone = PickSpawnZone(spawner, log);
            if (zone == null)
            {
                log.AppendLine("  ABORT: no BotZone found.");
                plugin.Log(log.ToString(), true);
                return;
            }

            log.AppendLine($"  Spawn zone: {zone.name} @ {zone.transform.position}");

            var wavesByBoss = CachedWaves
                .GroupBy(w => w.BossName, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

            var spawned = 0;
            var skipped = 0;

            foreach (var boss in enabledBosses)
            {
                if (wavesByBoss.TryGetValue(boss.Id, out var templates) && templates.Count > 0)
                {
                    foreach (var template in templates)
                    {
                        if (TryForceSpawnFromWave(spawner, template, log))
                        {
                            spawned++;
                        }
                        else
                        {
                            skipped++;
                        }
                    }
                }
                else
                {
                    log.AppendLine($"  FALLBACK {boss.Id}: no map wave, using BotSpawner.method_2 forced");
                    if (TryForceSpawnSynthetic(spawner, zone, boss, log))
                    {
                        spawned++;
                    }
                    else
                    {
                        skipped++;
                    }
                }
            }

            log.AppendLine($"  RESULT spawned={spawned} skipped={skipped}");
            log.AppendLine("[BOSS_SPAWN] ===== END DEBUG SPAWN =====");
            plugin.Log(log.ToString(), true);
        }

        private static bool TryForceSpawnFromWave(BotSpawner spawner, BossLocationSpawn template, StringBuilder log)
        {
            try
            {
                var wave = template.Copy();
                wave.BossChance = 100f;
                wave.ForceSpawn = true;
                wave.IgnoreMaxBots = true;
                wave.Activated = false;
                wave.Delay = 0f;
                wave.ShallSpawn = true;
                wave.ParseMainTypesTypes();
                wave.ShallSpawn = true;

                log.AppendLine(
                    $"  SPAWN WAVE {wave.BossName}: zone={wave.BossZone} escort={wave.BossEscortAmount} diff={wave.BossDifficult} trigger={wave.TriggerName}/{wave.TriggerId}");

                spawner.ActivateBotsByWave(wave);
                return true;
            }
            catch (Exception ex)
            {
                log.AppendLine($"  ERROR wave {template.BossName}: {ex.Message}");
                return false;
            }
        }

        private static bool TryForceSpawnSynthetic(BotSpawner spawner, BotZone zone, BossDefinition boss, StringBuilder log)
        {
            try
            {
                if (!boss.SpawnType.IsBossOrFollower())
                {
                    log.AppendLine($"  SKIP synthetic {boss.Id}: not boss role.");
                    return false;
                }

                RunSyntheticSpawn(spawner, zone, boss, log);
                log.AppendLine($"  SPAWN SYNTHETIC {boss.Id} via method_2 in zone {zone.name}");
                return true;
            }
            catch (Exception ex)
            {
                log.AppendLine($"  ERROR synthetic {boss.Id}: {ex.Message}");
                return false;
            }
        }

        private static async void RunSyntheticSpawn(BotSpawner spawner, BotZone zone, BossDefinition boss, StringBuilder log)
        {
            try
            {
                await spawner.method_2(EPlayerSide.Savage, zone, boss.SpawnType, BotDifficulty.normal, forcedSpawn: true);
            }
            catch (Exception ex)
            {
                log.AppendLine($"  ERROR async synthetic {boss.Id}: {ex.Message}");
            }
        }

        private static BotZone PickSpawnZone(BotSpawner spawner, StringBuilder log)
        {
            var zones = spawner.AllBotZones;
            if (zones != null && zones.Length > 0)
            {
                log.AppendLine($"  Available zones: {zones.Length}");
                return zones[0];
            }

            log.AppendLine("  AllBotZones empty.");
            return null;
        }

        private static bool IsInRaid()
        {
            return Singleton<GameWorld>.Instantiated && Singleton<IBotGame>.Instantiated;
        }

        private static bool TryGetBotSpawner(out BotSpawner spawner, out string error)
        {
            spawner = null;
            error = null;

            if (!Singleton<IBotGame>.Instantiated)
            {
                error = "IBotGame not instantiated (not in raid or not host?).";
                return false;
            }

            spawner = Singleton<IBotGame>.Instance?.BotsController?.BotSpawner;
            if (spawner == null)
            {
                error = "BotsController.BotSpawner is null (client without spawn authority?).";
                return false;
            }

            return true;
        }
    }
}
