using System.Reflection;
using EFT;
using HarmonyLib;

namespace BossSpawnControl
{
    /// <summary>
    /// Optional soft dependency on pitFireTeam (SPT-PitFireTeam) and PitFireTeamFikaFix CompanionGuard API.
    /// pitFireTeam 0.8.x: IsFollower(BotOwner, AIBossPlayer) — AIBossPlayer is EFT global type, not pitTeam.Components.
    /// </summary>
    internal static class PitFireCompanionGuard
    {
        private static bool _initialized;
        private static bool _fikaFixAvailable;
        private static MethodInfo _fikaFixIsProtectedBot;
        private static MethodInfo _fikaFixIsProtectedProfileId;
        private static MethodInfo _pitFireIsFollowerBot;
        private static MethodInfo _pitFireIsFollowerProfileId;
        private static MethodInfo _pitFireGetFollowerByProfileId;

        internal static bool IsAvailable
        {
            get
            {
                EnsureInitialized();
                return _fikaFixAvailable || _pitFireIsFollowerBot != null;
            }
        }

        internal static string DescribeProbe()
        {
            EnsureInitialized();
            return
                $"fikaFix={_fikaFixAvailable} BossPlayers={_pitFireIsFollowerBot != null} " +
                $"IsFollowerProfileId={_pitFireIsFollowerProfileId != null} " +
                $"AIBossPlayer={typeof(AIBossPlayer).FullName}";
        }

        internal static bool IsProtectedBot(BotOwner bot)
        {
            if (bot == null)
            {
                return false;
            }

            EnsureInitialized();

            if (_fikaFixAvailable && TryInvokeBool(_fikaFixIsProtectedBot, bot, out var fikaFixResult) && fikaFixResult)
            {
                return true;
            }

            if (TryInvokeIsFollower(bot))
            {
                return true;
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

            if (_fikaFixAvailable && TryInvokeBool(_fikaFixIsProtectedProfileId, profileId, out var fikaFixResult) && fikaFixResult)
            {
                return true;
            }

            if (TryInvokeBool(_pitFireIsFollowerProfileId, profileId, out var pitFireResult) && pitFireResult)
            {
                return true;
            }

            return TryGetFollowerRecord(profileId);
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
                _pitFireGetFollowerByProfileId = AccessTools.Method(bossPlayersType, "GetFollowerByProfileId", new[] { typeof(string) });
                _pitFireIsFollowerBot = AccessTools.Method(
                    bossPlayersType,
                    "IsFollower",
                    new[] { typeof(BotOwner), typeof(AIBossPlayer) });
            }
        }

        private static bool TryInvokeIsFollower(BotOwner bot)
        {
            if (_pitFireIsFollowerBot == null)
            {
                return IsFollowerByBotFollowerState(bot);
            }

            try
            {
                var result = (bool)_pitFireIsFollowerBot.Invoke(null, new object[] { bot, null });
                if (result)
                {
                    return true;
                }
            }
            catch
            {
                // fall through
            }

            return IsFollowerByBotFollowerState(bot);
        }

        private static bool IsFollowerByBotFollowerState(BotOwner bot)
        {
            if (bot?.BotFollower?.HaveBoss != true)
            {
                return false;
            }

            var bossToFollow = bot.BotFollower.BossToFollow;
            if (bossToFollow == null)
            {
                return false;
            }

            try
            {
                var playerMethod = bossToFollow.GetType().GetMethod("Player", BindingFlags.Instance | BindingFlags.Public);
                var player = playerMethod?.Invoke(bossToFollow, null) as Player;
                if (player == null)
                {
                    return false;
                }

                return TryInvokePlayerBoss(player.ProfileId);
            }
            catch
            {
                return false;
            }
        }

        private static bool TryInvokePlayerBoss(string profileId)
        {
            var bossPlayersType = AccessTools.TypeByName("pitTeam.Modules.BossPlayers");
            var isPlayerBoss = bossPlayersType != null
                ? AccessTools.Method(bossPlayersType, "IsPlayerBoss", new[] { typeof(string) })
                : null;
            return isPlayerBoss != null && TryInvokeBool(isPlayerBoss, profileId, out var isBoss) && isBoss;
        }

        private static bool TryGetFollowerRecord(string profileId)
        {
            if (_pitFireGetFollowerByProfileId == null)
            {
                return false;
            }

            try
            {
                return _pitFireGetFollowerByProfileId.Invoke(null, new object[] { profileId }) != null;
            }
            catch
            {
                return false;
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
    }
}
