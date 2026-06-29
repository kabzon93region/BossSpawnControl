using Comfort.Common;
using EFT;
using EFT.HealthSystem;

namespace BossSpawnControl
{
    internal enum BotRemovalMethod
    {
        None,
        RemoveFromMap,
        Kill,
        BotDespawn,
        LeaveQueued,
        SkippedProtected,
        SkippedBtr,
        SkippedInvalid,
        Failed
    }

    internal readonly struct BotRemovalAttempt
    {
        internal BotRemovalAttempt(BotRemovalMethod method, string detail)
        {
            Method = method;
            Detail = detail ?? string.Empty;
        }

        internal BotRemovalMethod Method { get; }
        internal string Detail { get; }
        internal bool CountsAsRemovedAttempt => Method == BotRemovalMethod.RemoveFromMap
            || Method == BotRemovalMethod.Kill
            || Method == BotRemovalMethod.BotDespawn
            || Method == BotRemovalMethod.LeaveQueued;
    }

    internal static class BotInstantRemover
    {
        internal static bool IsStillListed(BotOwner bot)
        {
            return bot != null && bot.gameObject != null && bot.gameObject.activeInHierarchy;
        }

        internal static BotRemovalAttempt TryRemove(BotOwner bot, bool skipBtr)
        {
            if (bot == null)
            {
                return new BotRemovalAttempt(BotRemovalMethod.SkippedInvalid, "bot=null");
            }

            if (bot.LeaveData == null)
            {
                return new BotRemovalAttempt(BotRemovalMethod.SkippedInvalid, "LeaveData=null");
            }

            if (bot.LeaveData.LeaveComplete)
            {
                return new BotRemovalAttempt(BotRemovalMethod.SkippedInvalid, "already leaving/left");
            }

            var role = bot.Profile?.Info?.Settings?.Role ?? WildSpawnType.assault;
            if (skipBtr && role == WildSpawnType.shooterBTR)
            {
                return new BotRemovalAttempt(BotRemovalMethod.SkippedBtr, "shooterBTR protected");
            }

            var removeFromMap = TryRemoveFromMap(bot);
            if (removeFromMap.Method != BotRemovalMethod.Failed)
            {
                return removeFromMap;
            }

            var kill = TryKill(bot);
            if (kill.Method != BotRemovalMethod.Failed)
            {
                return kill;
            }

            var despawn = TryBotGameDespawn(bot);
            if (despawn.Method != BotRemovalMethod.Failed)
            {
                return despawn;
            }

            return TryLeaveExternal(bot);
        }

        internal static BotRemovalAttempt TryForceKill(BotOwner bot)
        {
            if (bot == null)
            {
                return new BotRemovalAttempt(BotRemovalMethod.SkippedInvalid, "bot=null");
            }

            var kill = TryKill(bot);
            if (kill.Method != BotRemovalMethod.Failed)
            {
                return kill;
            }

            var removeFromMap = TryRemoveFromMap(bot);
            if (removeFromMap.Method != BotRemovalMethod.Failed)
            {
                return removeFromMap;
            }

            return TryBotGameDespawn(bot);
        }

        private static BotRemovalAttempt TryRemoveFromMap(BotOwner bot)
        {
            try
            {
                bot.LeaveData.RemoveFromMap();
                return new BotRemovalAttempt(BotRemovalMethod.RemoveFromMap, "RemoveFromMap");
            }
            catch (System.Exception ex)
            {
                return new BotRemovalAttempt(BotRemovalMethod.Failed, $"RemoveFromMap:{ex.GetType().Name}");
            }
        }

        private static BotRemovalAttempt TryKill(BotOwner bot)
        {
            try
            {
                var player = bot.GetPlayer;
                if (player == null)
                {
                    return new BotRemovalAttempt(BotRemovalMethod.Failed, "GetPlayer=null");
                }

                var health = player.ActiveHealthController as ActiveHealthController
                    ?? player.HealthController as ActiveHealthController;

                if (health == null)
                {
                    return new BotRemovalAttempt(BotRemovalMethod.Failed, "health=null");
                }

                if (!health.IsAlive)
                {
                    return new BotRemovalAttempt(BotRemovalMethod.Kill, "already dead");
                }

                health.Kill(EDamageType.Undefined);
                return new BotRemovalAttempt(BotRemovalMethod.Kill, "Kill(Undefined)");
            }
            catch (System.Exception ex)
            {
                return new BotRemovalAttempt(BotRemovalMethod.Failed, $"Kill:{ex.GetType().Name}");
            }
        }

        private static BotRemovalAttempt TryBotGameDespawn(BotOwner bot)
        {
            try
            {
                var botGame = bot?.BotsGroup?.BotGame;
                if (botGame == null && Singleton<IBotGame>.Instantiated)
                {
                    botGame = Singleton<IBotGame>.Instance;
                }

                if (botGame == null)
                {
                    return new BotRemovalAttempt(BotRemovalMethod.Failed, "IBotGame=null");
                }

                botGame.BotDespawn(bot);
                return new BotRemovalAttempt(BotRemovalMethod.BotDespawn, "IBotGame.BotDespawn");
            }
            catch (System.Exception ex)
            {
                return new BotRemovalAttempt(BotRemovalMethod.Failed, $"BotDespawn:{ex.GetType().Name}");
            }
        }

        private static BotRemovalAttempt TryLeaveExternal(BotOwner bot)
        {
            try
            {
                bot.LeaveData.DoLeaveExternal();
                return new BotRemovalAttempt(BotRemovalMethod.LeaveQueued, "DoLeaveExternal fallback");
            }
            catch (System.Exception ex)
            {
                return new BotRemovalAttempt(BotRemovalMethod.Failed, $"Leave:{ex.GetType().Name}");
            }
        }
    }
}
