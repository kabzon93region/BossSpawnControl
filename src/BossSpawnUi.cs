using BepInEx.Configuration;

using UnityEngine;



namespace BossSpawnControl

{

    internal static class BossSpawnUi

    {

        internal static void DrawSpawnButton(ConfigEntryBase entry)

        {

            var plugin = PluginCore.Instance;

            if (plugin == null)

            {

                GUILayout.Label("Boss Spawn Control: plugin not ready");

                return;

            }



            GUILayout.Label("Принудительный спавн для отладки");

            GUILayout.Label("Спавнит всех ВКЛЮЧЁННЫХ боссов на текущей карте.");

            GUILayout.Label("Работает даже если ModEnabled = false или босс уже убит.");



            if (GUILayout.Button("Заспавнить включённых боссов", GUILayout.Height(28f)))

            {

                BossSpawnExecutor.ForceSpawnFromButton(plugin);

            }

        }



        internal static void DrawBotSpawnButton(ConfigEntryBase entry)

        {

            var plugin = PluginCore.Instance;

            if (plugin == null)

            {

                GUILayout.Label("Boss Spawn Control: plugin not ready");

                return;

            }



            GUILayout.Label("Спавн обычных ботов (дикие / PMC)");

            GUILayout.Label("Укажите количество для каждой роли выше. Нажмите кнопку, чтобы заспавнить.");



            if (GUILayout.Button("Заспавнить выбранных ботов", GUILayout.Height(28f)))

            {

                BossSpawnExecutor.ForceSpawnBotsFromButton(plugin);

            }

        }



        internal static void DrawPopulationMaintenanceButton(ConfigEntryBase entry)

        {

            var plugin = PluginCore.Instance;

            if (plugin == null)

            {

                GUILayout.Label("Boss Spawn Control: plugin not ready");

                return;

            }



            var cfg = plugin.PopulationConfig;

            var running = cfg.MaintenanceRunning.Value;



            GUILayout.Label("Поддержание населения карты");

            GUILayout.Label("Лимиты — в штуках (не %). Пример: 3 диких + 5 BEAR + 7 отступников при общем лимите 15.");

            DrawPopulationStatus(plugin);



            var label = running

                ? "■ Остановить поддержание"

                : "▶ Запустить поддержание";



            if (GUILayout.Button(label, GUILayout.Height(32f)))

            {

                cfg.MaintenanceRunning.Value = !running;

                PopulationMaintenanceBehaviour.Instance?.SyncFromConfig();

            }

        }



        internal static void DrawPopulationScanButton(ConfigEntryBase entry)

        {

            var plugin = PluginCore.Instance;

            if (plugin == null)

            {

                GUILayout.Label("Boss Spawn Control: plugin not ready");

                return;

            }



            GUILayout.Label("Разовое сканирование (только лог, без автоспавна).");



            if (GUILayout.Button("Сканировать сейчас", GUILayout.Height(26f)))

            {

                BotPopulationCounter.Collect(plugin, "manual button");

            }

        }



        internal static void DrawClearBotsButton(ConfigEntryBase entry)

        {

            var plugin = PluginCore.Instance;

            if (plugin == null)

            {

                GUILayout.Label("Boss Spawn Control: plugin not ready");

                return;

            }



            var cfg = plugin.PopulationConfig;

            var confirmed = cfg.ConfirmClearBots.Value;



            GUILayout.Label("Сброс: удалить всех AI-ботов с карты");

            GUILayout.Label("Поддержание остановится автоматически. Игроки не затрагиваются.");

            if (cfg.ProtectPitFireCompanions.Value)
            {
                GUILayout.Label("Компаньоны Pit Fire Team (отряд) не удаляются.");
            }

            if (cfg.ProtectBtrDuringClear.Value)
            {
                GUILayout.Label("БТР (shooterBTR) не удаляется при сбросе.");
            }

            if (!BotSpawnAuthority.HasBotRemovalAuthority(out var authorityReason))
            {
                GUILayout.Label($"⚠ Сброс недоступен: {authorityReason}");
            }
            else
            {
                GUILayout.Label($"Authority: {BotSpawnAuthority.DescribeAuthority()}");
            }

            GUI.enabled = confirmed && BotSpawnAuthority.HasBotRemovalAuthority(out _);

            if (GUILayout.Button("Удалить всех ботов с карты", GUILayout.Height(30f)))

            {

                BotRemovalExecutor.ClearAllBotsFromButton(plugin);

            }



            GUI.enabled = true;



            if (!confirmed)

            {

                GUILayout.Label("Включите «Подтверждаю удаление всех ботов» выше.");

            }

        }



        private static void DrawPopulationStatus(PluginCore plugin)

        {

            var snapshot = PopulationMaintenanceBehaviour.LastSnapshot ?? plugin.PopulationConfig.LastSnapshot;

            if (snapshot == null || !snapshot.InRaid)

            {

                GUILayout.Label("Статус: вне рейда или ещё не сканировали.");

                return;

            }



            var cfg = plugin.PopulationConfig;

            var totalLimit = cfg.LimitTotal.Value;

            var totalText = totalLimit > 0

                ? $"{snapshot.TotalActiveBots}/{totalLimit} шт."

                : $"{snapshot.TotalActiveBots} шт.";



            GUILayout.Label($"Всего ботов: {totalText}");



            foreach (BotFactionCategory faction in System.Enum.GetValues(typeof(BotFactionCategory)))

            {

                var limit = cfg.GetFactionLimit(faction);

                var count = snapshot.GetFactionCount(faction);

                var line = limit > 0

                    ? $"{faction.GetDisplayName()}: {count}/{limit} шт."

                    : $"{faction.GetDisplayName()}: {count} шт.";

                GUILayout.Label(line);

            }



            if (snapshot.SpawnerMaxBots > 0)

            {

                GUILayout.Label(

                    $"Игра MaxBots: {snapshot.SpawnerAllBotsWithLoaded}/{snapshot.SpawnerMaxBots} " +

                    $"(очередь {snapshot.SpawnerQueueWaitCount})");

            }

        }

    }

}

