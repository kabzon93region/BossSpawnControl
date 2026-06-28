using System;
using System.Collections.Generic;
using System.Linq;
using EFT;

namespace BossSpawnControl
{
    internal static class BossCatalog
    {
        internal static readonly IReadOnlyList<BossDefinition> AllBosses = BuildCatalog();

        private static List<BossDefinition> BuildCatalog()
        {
            var list = new List<BossDefinition>();
            foreach (WildSpawnType type in Enum.GetValues(typeof(WildSpawnType)))
            {
                if (!type.IsBoss())
                {
                    continue;
                }

                list.Add(new BossDefinition(type.ToString(), GetDisplayName(type), type));
            }

            return list
                .OrderBy(x => x.DisplayName, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static string GetDisplayName(WildSpawnType type)
        {
            return type switch
            {
                WildSpawnType.bossBully => "Решала (bossBully)",
                WildSpawnType.bossKilla => "Killa (bossKilla)",
                WildSpawnType.bossKojaniy => "Штурман (bossKojaniy)",
                WildSpawnType.bossGluhar => "Гluhar (bossGluhar)",
                WildSpawnType.bossSanitar => "Sanitar (bossSanitar)",
                WildSpawnType.bossTagilla => "Tagilla (bossTagilla)",
                WildSpawnType.bossKnight => "Knight (bossKnight)",
                WildSpawnType.bossZryachiy => "Zryachiy (bossZryachiy)",
                WildSpawnType.bossBoar => "Kaban (bossBoar)",
                WildSpawnType.bossBoarSniper => "Kaban sniper (bossBoarSniper)",
                WildSpawnType.bossKolontay => "Kolontay (bossKolontay)",
                WildSpawnType.bossPartisan => "Partisan (bossPartisan)",
                WildSpawnType.bossTagillaAgro => "Tagilla Agro (bossTagillaAgro)",
                WildSpawnType.bossKillaAgro => "Killa Agro (bossKillaAgro)",
                WildSpawnType.tagillaHelperAgro => "Tagilla helper (tagillaHelperAgro)",
                WildSpawnType.sectantPriest => "Sectant priest (sectantPriest)",
                WildSpawnType.sectactPriestEvent => "Sectant priest event (sectactPriestEvent)",
                WildSpawnType.arenaFighterEvent => "Arena fighter event (arenaFighterEvent)",
                WildSpawnType.exUsec => "Lightkeeper USEC (exUsec)",
                WildSpawnType.pmcBot => "PMC bot boss (pmcBot)",
                WildSpawnType.gifter => "Gifter (gifter)",
                WildSpawnType.infectedTagilla => "Infected Tagilla (infectedTagilla)",
                WildSpawnType.bossTest => "Boss test (bossTest)",
                _ => type.ToString()
            };
        }
    }

    internal sealed class BossDefinition
    {
        internal BossDefinition(string id, string displayName, WildSpawnType spawnType)
        {
            Id = id;
            DisplayName = displayName;
            SpawnType = spawnType;
        }

        internal string Id { get; }
        internal string DisplayName { get; }
        internal WildSpawnType SpawnType { get; }
    }
}
