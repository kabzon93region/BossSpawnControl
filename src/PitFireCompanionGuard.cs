using System.Reflection;
using EFT;
using HarmonyLib;

namespace BossSpawnControl
{
    /// <summary>
    /// Optional soft dependency on pitFireTeam (SPT-PitFireTeam) and PitFireTeamFikaFix CompanionGuard API.
    /// </summary>
    internal static class PitFireCompanionGuard
    {
        private static bool _initialized;
        private static bool _fikaFixAvailable;
        private static MethodInfo _fikaFixIsProtectedBot;
        private static MethodInfo _fikaFixIsProtectedProfileId;
        private static MethodInfo _pitFireIsFollowerBot;
        private static MethodInfo _pitFireIsFollowerProfileId;

        internal static bool IsAvailable
        {
            get
            {
                EnsureInitialized();
                return _fikaFixAvailable || _pitFireIsFollowerBot != null;
            }
        }

        internal static bool IsProtectedBot(BotOwner bot)
        {
            if (bot == null)
            {
                return false;
            }

            EnsureInitialized();

            if (_fikaFixAvailable && TryInvokeBool(_fikaFixIsProtectedBot, bot, out var fikaFixResult))
            {
                return fikaFixResult;
            }

            if (TryInvokeBool(_pitFireIsFollowerBot, bot, null, out var pitFireResult))
            {
                return pitFireResult;
            }

            return IsProtectedProfileId(bot.ProfileId);
        }

        internal static bool IsProtectedProfileId(string profileId)
        {
            if (string.IsNullOrEmpty(profileId))
            {
                return false;
            }

            EnsureInitialized();

            if (_fikaFixAvailable && TryInvokeBool(_fikaFixIsProtectedProfileId, profileId, out var fikaFixResult))
            {
                return fikaFixResult;
            }

            return TryInvokeBool(_pitFireIsFollowerProfileId, profileId, out var pitFireResult) && pitFireResult;
        }

        private static void EnsureInitialized()
        {
            if (_initialized)
            {
                return;
            }

            _initialized = true;

            var fikaFixType = AccessTools.TypeByName("PitFireTeamFikaFix.CompanionGuard");
            if (fikaFixType != null)
            {
                _fikaFixIsProtectedBot = AccessTools.Method(fikaFixType, "IsProtectedBot", new[] { typeof(BotOwner) });
                _fikaFixIsProtectedProfileId = AccessTools.Method(fikaFixType, "IsProtectedProfileId", new[] { typeof(string) });
                _fikaFixAvailable = _fikaFixIsProtectedBot != null && _fikaFixIsProtectedProfileId != null;
            }

            var bossPlayersType = AccessTools.TypeByName("pitTeam.Modules.BossPlayers");
            if (bossPlayersType != null)
            {
                _pitFireIsFollowerProfileId = AccessTools.Method(bossPlayersType, "IsFollowerProfileId", new[] { typeof(string) });
                _pitFireIsFollowerBot = AccessTools.Method(
                    bossPlayersType,
                    "IsFollower",
                    new[] { typeof(BotOwner), AccessTools.TypeByName("pitTeam.Components.AIBossPlayer") });
            }
        }

        private static bool TryInvokeBool(MethodInfo method, object arg1, out bool result)
        {
            result = false;
            if (method == null || arg1 == null)
            {
                return false;
            }

            try
            {
                result = (bool)method.Invoke(null, new[] { arg1 });
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool TryInvokeBool(MethodInfo method, object arg1, object arg2, out bool result)
        {
            result = false;
            if (method == null || arg1 == null)
            {
                return false;
            }

            try
            {
                result = (bool)method.Invoke(null, new object[] { arg1, arg2 });
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
