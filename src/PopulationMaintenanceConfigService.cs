using System;

using System.Collections.Generic;

using System.Linq;

using BepInEx.Configuration;



namespace BossSpawnControl

{

    internal sealed class PopulationMaintenanceConfigService

    {

        private readonly Dictionary<BotFactionCategory, ConfigEntry<int>> _factionLimits =

            new Dictionary<BotFactionCategory, ConfigEntry<int>>();



        private readonly Dictionary<BotFactionCategory, ConfigEntry<int>> _factionPriorities =

            new Dictionary<BotFactionCategory, ConfigEntry<int>>();



        internal ConfigEntry<bool> MaintenanceRunning { get; private set; }

        internal ConfigEntry<int> ScanIntervalMinSec { get; private set; }

        internal ConfigEntry<int> ScanIntervalMaxSec { get; private set; }

        internal ConfigEntry<int> LimitTotal { get; private set; }

        internal ConfigEntry<int> MaxSpawnsPerTick { get; private set; }

        internal ConfigEntry<bool> AutoSpawnOnMaintenance { get; private set; }

        internal ConfigEntry<bool> VerboseRoleLogging { get; private set; }

        internal ConfigEntry<bool> SpawnZombies { get; private set; }
        internal ConfigEntry<bool> ConfirmClearBots { get; private set; }
        internal ConfigEntry<bool> ProtectPitFireCompanions { get; private set; }
        internal ConfigEntry<bool> ProtectBtrDuringClear { get; private set; }
        internal ConfigEntry<int> ClearSpawnBlockSec { get; private set; }
        internal ConfigEntry<int> ClearPollTimeoutSec { get; private set; }
        internal ConfigEntry<float> ClearPollIntervalSec { get; private set; }



        internal BotPopulationSnapshot LastSnapshot { get; private set; }



        internal void Bind(ConfigFile config)

        {

            MaintenanceRunning = config.Bind(

                "Население",

                "MaintenanceRunning",

                false,

                new ConfigDescription(

                    "Служебный флаг режима поддержания. Не менять вручную.",

                    null,

                    new ConfigurationManagerAttributes

                    {

                        Browsable = false,

                        HideSettingName = true,

                        HideDefaultButton = true

                    }));



            config.Bind(

                "Население",

                "ToggleMaintenanceButton",

                false,

                new ConfigDescription(

                    "Запуск/остановка периодического сканирования и поддержания численности ботов.",

                    null,

                    new ConfigurationManagerAttributes

                {

                    CustomDrawer = BossSpawnUi.DrawPopulationMaintenanceButton,

                    HideSettingName = true,

                    HideDefaultButton = true,

                    Order = 3000

                }));



            config.Bind(

                "Население",

                "ScanNowButton",

                false,

                new ConfigDescription(

                    "Один раз просканировать карту и вывести счётчики в лог (без автospawn).",

                    null,

                    new ConfigurationManagerAttributes

                {

                    CustomDrawer = BossSpawnUi.DrawPopulationScanButton,

                    HideSettingName = true,

                    HideDefaultButton = true,

                    Order = 2990

                }));



            config.Bind(
                "Население",
                "ClearBotsButton",
                false,
                new ConfigDescription(
                    "Удалить всех AI-ботов с карты (сброс после переспавна).",
                    null,
                    new ConfigurationManagerAttributes
                    {
                        CustomDrawer = BossSpawnUi.DrawClearBotsButton,
                        HideSettingName = true,
                        HideDefaultButton = true,
                        Order = 2985
                    }));

            ConfirmClearBots = config.Bind(
                "Население",
                "ConfirmClearBots",
                false,
                new ConfigDescription(
                    "Подтверждаю удаление всех ботов с карты. Обязательно для кнопки сброса.",
                    null,
                    new ConfigurationManagerAttributes { Order = 2984 }));

            ProtectPitFireCompanions = config.Bind(
                "Население",
                "ProtectPitFireCompanions",
                true,
                new ConfigDescription(
                    "Не удалять компаньонов Pit Fire Team (отряд/последователи) при сбросе ботов.",
                    null,
                    new ConfigurationManagerAttributes { Order = 2983 }));

            ProtectBtrDuringClear = config.Bind(
                "Население",
                "ProtectBtrDuringClear",
                true,
                new ConfigDescription(
                    "Не удалять БТР (shooterBTR) при сбросе ботов.",
                    null,
                    new ConfigurationManagerAttributes { Order = 2982 }));

            ClearSpawnBlockSec = config.Bind(
                "Население",
                "ClearSpawnBlockSec",
                45,
                new ConfigDescription(
                    "Секунд блокировать автодоспавн после сброса ботов.",
                    new AcceptableValueRange<int>(0, 300),
                    new ConfigurationManagerAttributes { Order = 2981 }));

            ClearPollTimeoutSec = config.Bind(
                "Население",
                "ClearPollTimeoutSec",
                15,
                new ConfigDescription(
                    "Максимум секунд ждать исчезновения ботов после сброса.",
                    new AcceptableValueRange<int>(2, 120),
                    new ConfigurationManagerAttributes { Order = 2980 }));

            ClearPollIntervalSec = config.Bind(
                "Население",
                "ClearPollIntervalSec",
                1f,
                new ConfigDescription(
                    "Интервал повторной проверки (сек) пока боты не исчезнут.",
                    new AcceptableValueRange<float>(0.25f, 5f),
                    new ConfigurationManagerAttributes { Order = 2979 }));

            ScanIntervalMinSec = config.Bind(

                "Население",

                "ScanIntervalMinSec",

                3,

                new ConfigDescription(

                    "Минимальный интервал сканирования (сек).",

                    new AcceptableValueRange<int>(1, 60),

                    new ConfigurationManagerAttributes { Order = 2980 }));



            ScanIntervalMaxSec = config.Bind(

                "Население",

                "ScanIntervalMaxSec",

                8,

                new ConfigDescription(

                    "Максимальный интервал сканирования (сек). Случайное значение между min и max.",

                    new AcceptableValueRange<int>(1, 120),

                    new ConfigurationManagerAttributes { Order = 2970 }));



            LimitTotal = config.Bind(
                "Население",
                "LimitTotal",
                0,
                new ConfigDescription(
                    "Общий лимит одновременных ботов в штуках (0 = без общего лимита).",
                    null,
                    new ConfigurationManagerAttributes { Order = 2960, DispName = "Общий лимит (шт.)" }));



            MaxSpawnsPerTick = config.Bind(

                "Население",

                "MaxSpawnsPerTick",

                2,

                new ConfigDescription(

                    "Сколько ботов максимум доспавнить за один цикл поддержания.",

                    new AcceptableValueRange<int>(1, 10),

                    new ConfigurationManagerAttributes { Order = 2950 }));



            AutoSpawnOnMaintenance = config.Bind(

                "Население",

                "AutoSpawnOnMaintenance",

                true,

                new ConfigDescription(

                    "При включённом поддержании — автоматически доспавнивать до лимитов.",

                    null,

                    new ConfigurationManagerAttributes { Order = 2940 }));



            VerboseRoleLogging = config.Bind(

                "Население",

                "VerboseRoleLogging",

                false,

                new ConfigDescription(

                    "Логировать каждого живого бота при сканировании (очень много строк).",

                    null,

                    new ConfigurationManagerAttributes { Order = 2930 }));



            SpawnZombies = config.Bind(

                "Население",

                "SpawnZombies",

                false,

                new ConfigDescription(

                    "Разрешить автодоспавн зомби (infected*). По умолчанию выкл.",

                    null,

                    new ConfigurationManagerAttributes { Order = 2920 }));



            var limitOrder = 2900;

            foreach (BotFactionCategory faction in Enum.GetValues(typeof(BotFactionCategory)))

            {

                limitOrder -= 2;

                var key = faction.GetConfigKey();

                _factionLimits[faction] = config.Bind(
                    "Население — лимиты",
                    $"Limit_{key}",
                    0,
                    new ConfigDescription(
                        $"Сколько «{faction.GetDisplayName()}» одновременно на карте, в штуках (0 = без лимита).",
                        null,
                        new ConfigurationManagerAttributes
                        {
                            DispName = $"{faction.GetDisplayName()} (шт.)",
                            Order = limitOrder
                        }));



                _factionPriorities[faction] = config.Bind(
                    "Население — приоритет",
                    $"Priority_{key}",
                    GetDefaultPriority(faction),
                    new ConfigDescription(
                        $"Приоритет доспавна «{faction.GetDisplayName()}» (1 = первый). Если общий лимит меньше суммы лимитов фракций.",
                        null,
                        new ConfigurationManagerAttributes
                        {
                            DispName = faction.GetDisplayName(),
                            Order = limitOrder - 1
                        }));

            }

        }



        internal int GetFactionLimit(BotFactionCategory faction)

        {

            return _factionLimits.TryGetValue(faction, out var entry) ? entry.Value : 0;

        }



        internal IEnumerable<BotFactionCategory> GetFactionsByPriority()

        {

            return Enum.GetValues(typeof(BotFactionCategory))

                .Cast<BotFactionCategory>()

                .OrderBy(f => _factionPriorities.TryGetValue(f, out var entry) ? entry.Value : 99)

                .ThenBy(f => f.ToString(), StringComparer.OrdinalIgnoreCase);

        }



        internal void SetLastSnapshot(BotPopulationSnapshot snapshot)

        {

            LastSnapshot = snapshot;

        }



        internal int GetConfiguredFactionLimitSum()

        {

            return Enum.GetValues(typeof(BotFactionCategory))

                .Cast<BotFactionCategory>()

                .Sum(GetFactionLimit);

        }



        private static int GetDefaultPriority(BotFactionCategory faction)

        {

            return faction switch

            {

                BotFactionCategory.Scavs => 1,

                BotFactionCategory.Usec => 2,

                BotFactionCategory.Bear => 3,

                BotFactionCategory.Rogues => 4,

                BotFactionCategory.BossesAndFollowers => 5,

                BotFactionCategory.Zombies => 6,

                _ => 50

            };

        }

    }

}

