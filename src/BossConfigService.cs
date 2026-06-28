using System;
using System.Collections.Generic;
using BepInEx.Configuration;

namespace BossSpawnControl
{
    internal sealed class BossConfigService
    {
        private readonly Dictionary<string, ConfigEntry<bool>> _bossEnabled = new Dictionary<string, ConfigEntry<bool>>(StringComparer.OrdinalIgnoreCase);

        internal ConfigEntry<bool> ModEnabled { get; private set; }
        internal ConfigEntry<bool> DebugLogging { get; private set; }
        internal ConfigEntry<bool> SpawnButtonTrigger { get; private set; }

        internal void Bind(ConfigFile config)
        {
            ModEnabled = config.Bind(
                "Общие",
                "ModEnabled",
                true,
                new ConfigDescription(
                    "Авто-спавн включённых боссов при старте рейда (если волна есть на карте).",
                    null,
                    new ConfigurationManagerAttributes { Order = 100 }));

            DebugLogging = config.Bind(
                "Общие",
                "DebugLogging",
                true,
                new ConfigDescription(
                    "Подробные логи [BOSS_SPAWN] в BepInEx/LogOutput.log.",
                    null,
                    new ConfigurationManagerAttributes { Order = 90 }));

            SpawnButtonTrigger = config.Bind(
                "Отладка",
                "SpawnEnabledBossesNow",
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

            config.Bind(
                "Отладка",
                "SpawnEnabledBossesButton",
                false,
                new ConfigDescription(
                    "Принудительно заспавнить всех ВКЛЮЧЁННЫХ боссов на текущей карте (даже если уже были убиты). Работает и при выключенном ModEnabled.",
                    null,
                    new ConfigurationManagerAttributes
                    {
                        CustomDrawer = BossSpawnUi.DrawSpawnButton,
                        HideSettingName = true,
                        HideDefaultButton = true,
                        Order = 1000
                    }));

            var order = 1000;
            foreach (var boss in BossCatalog.AllBosses)
            {
                order--;
                var entry = config.Bind(
                    "Боссы",
                    boss.Id,
                    false,
                    new ConfigDescription(
                        $"Включить босса {boss.DisplayName}.",
                        null,
                        new ConfigurationManagerAttributes
                        {
                            DispName = boss.DisplayName,
                            Order = order
                        }));

                _bossEnabled[boss.Id] = entry;
            }
        }

        internal bool IsBossEnabled(string bossId)
        {
            if (string.IsNullOrWhiteSpace(bossId))
            {
                return false;
            }

            return _bossEnabled.TryGetValue(bossId, out var entry) && entry.Value;
        }

        internal IEnumerable<BossDefinition> GetEnabledBosses()
        {
            foreach (var boss in BossCatalog.AllBosses)
            {
                if (IsBossEnabled(boss.Id))
                {
                    yield return boss;
                }
            }
        }
    }
}
