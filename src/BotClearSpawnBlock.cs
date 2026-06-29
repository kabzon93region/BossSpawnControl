using UnityEngine;

namespace BossSpawnControl
{
    /// <summary>
    /// Temporary block for maintenance auto-spawn after a clear operation.
    /// </summary>
    internal static class BotClearSpawnBlock
    {
        private static float _blockedUntilUnscaledTime;

        internal static void Activate(float durationSeconds)
        {
            var duration = Mathf.Max(0f, durationSeconds);
            _blockedUntilUnscaledTime = Time.unscaledTime + duration;
        }

        internal static bool IsActive()
        {
            return Time.unscaledTime < _blockedUntilUnscaledTime;
        }

        internal static float RemainingSeconds()
        {
            return Mathf.Max(0f, _blockedUntilUnscaledTime - Time.unscaledTime);
        }
    }
}
