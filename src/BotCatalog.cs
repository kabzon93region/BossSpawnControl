using System;
using System.Collections.Generic;
using System.Linq;
using EFT;

namespace BossSpawnControl
{
    internal static class BotCatalog
    {
        internal static readonly IReadOnlyList<BotDefinition> AllBots = BuildCatalog();

    private static List<BotDefinition> BuildCatalog()
    {
        var list = new List<BotDefinition>
        {
            new BotDefinition("assault", "Дикий (assault)", WildSpawnType.assault, EPlayerSide.Savage),
            new BotDefinition("cursedAssault", "Проклятый дикий (cursedAssault)", WildSpawnType.cursedAssault, EPlayerSide.Savage),
            new BotDefinition("infectedAssault", "Зомби дикий (infectedAssault)", WildSpawnType.infectedAssault, EPlayerSide.Savage),
            new BotDefinition("infectedCivil", "Зомби гражданский (infectedCivil)", WildSpawnType.infectedCivil, EPlayerSide.Savage),
            new BotDefinition("infectedLaborant", "Зомби лаборант (infectedLaborant)", WildSpawnType.infectedLaborant, EPlayerSide.Savage),
            new BotDefinition("pmcBEAR", "PMC BEAR (pmcBEAR)", WildSpawnType.pmcBEAR, EPlayerSide.Bear),
            new BotDefinition("pmcUSEC", "PMC USEC (pmcUSEC)", WildSpawnType.pmcUSEC, EPlayerSide.Usec),
            new BotDefinition("pmcBot", "PMC случайный (pmcBot)", WildSpawnType.pmcBot, EPlayerSide.Usec),
            new BotDefinition("exUsec", "Бывший USEC (exUsec)", WildSpawnType.exUsec, EPlayerSide.Usec),
        };

        return list
            .OrderBy(x => x.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
    }

    internal sealed class BotDefinition
    {
        internal BotDefinition(string id, string displayName, WildSpawnType spawnType, EPlayerSide side)
        {
            Id = id;
            DisplayName = displayName;
            SpawnType = spawnType;
            Side = side;
        }

        internal string Id { get; }
        internal string DisplayName { get; }
        internal WildSpawnType SpawnType { get; }
        internal EPlayerSide Side { get; }
    }
}
