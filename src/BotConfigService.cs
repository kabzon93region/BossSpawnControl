using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using EFT;

namespace BossSpawnControl
{
    internal sealed class BotConfigService
    {
        private readonly Dictionary<string, ConfigEntry<int>> _botCounts = new Dictionary<string, ConfigEntry<int>>(StringComparer.OrdinalIgnoreCase);

    internal ConfigEntry<bool> BotSpawnButtonTrigger { get; private set; }
    internal ConfigEntry<BotDifficulty> BotDifficulty { get; private set; }

    internal void Bind(ConfigFile config)
    {
        BotSpawnButtonTrigger = config.Bind(
            "Боты",
            "SpawnBotsNow",
            false,
            new ConfigDescription(
                "Служебный флаг кнопки F12. Не менять вручную.",
                null,
                new ConfigurationManagerAttributes
                {
                    Browsable = false,
                    HideSettingName = true,
                    HideDefaultButton = true
                }));

        BotDifficulty = config.Bind(
            "Боты",
            "Difficulty",
            global::BotDifficulty.normal,
            new ConfigDescription(
                "Сложность заспавненных ботов.",
                null,
                new ConfigurationManagerAttributes { Order = 2100 }));

        config.Bind(
            "Боты",
            "SpawnBotsButton",
            false,
            new ConfigDescription(
                "Заспавнить выбранных обычных ботов (дикие / PMC) в указанном количестве.",
                null,
                new ConfigurationManagerAttributes
                {
                    CustomDrawer = BossSpawnUi.DrawBotSpawnButton,
                    HideSettingName = true,
                    HideDefaultButton = true,
                    Order = 2000
                }));

            var order = 900;
            foreach (var bot in BotCatalog.AllBots)
            {
                order--;
                var entry = config.Bind(
                    "Боты",
                    bot.Id,
                    0,
                    new ConfigDescription(
                        $"Сколько {bot.DisplayName} заспавнить.",
                        new AcceptableValueRange<int>(0, 20),
                        new ConfigurationManagerAttributes
                        {
                            DispName = bot.DisplayName,
                            Order = order
                        }));

                _botCounts[bot.Id] = entry;
            }
        }

        internal int GetBotCount(string botId)
        {
            if (string.IsNullOrWhiteSpace(botId))
            {
                return 0;
            }

            return _botCounts.TryGetValue(botId, out var entry) ? entry.Value : 0;
        }

        internal IEnumerable<(BotDefinition Definition, int Count)> GetEnabledBotsWithCount()
        {
            foreach (var bot in BotCatalog.AllBots)
            {
                var count = GetBotCount(bot.Id);
                if (count > 0)
                {
                    yield return (bot, count);
                }
            }
        }
    }
}
