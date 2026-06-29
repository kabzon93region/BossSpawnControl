namespace BossSpawnControl

{

    internal enum BotFactionCategory

    {

        Rogues,

        Scavs,

        Zombies,

        Usec,

        Bear,

        BossesAndFollowers

    }



    internal static class BotFactionCategoryExtensions

    {

        internal static string GetDisplayName(this BotFactionCategory category)

        {

            return category switch

            {

                BotFactionCategory.Rogues => "Отступники",

                BotFactionCategory.Scavs => "Дикие",

                BotFactionCategory.Zombies => "Зомби",

                BotFactionCategory.Usec => "USEC",

                BotFactionCategory.Bear => "BEAR",

                BotFactionCategory.BossesAndFollowers => "Боссы+свита",

                _ => category.ToString()

            };

        }



        internal static string GetConfigKey(this BotFactionCategory category)

        {

            return category switch

            {

                BotFactionCategory.Rogues => "Rogues",

                BotFactionCategory.Scavs => "Scavs",

                BotFactionCategory.Zombies => "Zombies",

                BotFactionCategory.Usec => "Usec",

                BotFactionCategory.Bear => "Bear",

                BotFactionCategory.BossesAndFollowers => "Bosses",

                _ => category.ToString()

            };

        }

    }

}

