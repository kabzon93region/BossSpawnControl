using Comfort.Common;
using EFT;

namespace BossSpawnControl
{
    /// <summary>
    /// Keeps vanilla BotSpawner.MaxBots aligned with mod LimitTotal while maintenance runs.
    /// </summary>
    internal static class PopulationSpawnerLimitSync
    {
        private static int _savedMaxBots = -1;
        private static bool _saved;

        internal static void Apply(PluginCore plugin)
        {
            var limit = plugin.PopulationConfig.LimitTotal.Value;
            if (limit <= 0 || !Singleton<IBotGame>.Instantiated)
            {
                return;
            }

            var spawner = Singleton<IBotGame>.Instance?.BotsController?.BotSpawner;
            if (spawner == null)
            {
                return;
            }

            if (!_saved)
            {
                _savedMaxBots = spawner.MaxBots;
                _saved = true;
            }

            if (spawner.MaxBots != limit)
            {
                spawner.SetMaxBots(limit);
            }
        }

        internal static void RestoreIfSaved()
        {
            if (!_saved || !Singleton<IBotGame>.Instantiated)
            {
                ResetState();
                return;
            }

            var spawner = Singleton<IBotGame>.Instance?.BotsController?.BotSpawner;
            if (spawner != null && _savedMaxBots >= 0)
            {
                spawner.SetMaxBots(_savedMaxBots);
            }

            ResetState();
        }

        internal static void ResetState()
        {
            _saved = false;
            _savedMaxBots = -1;
        }
    }
}
