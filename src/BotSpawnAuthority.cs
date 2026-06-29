using System;
using System.Reflection;
using BepInEx.Bootstrap;
using Comfort.Common;
using EFT;

namespace BossSpawnControl
{
    /// <summary>
    /// Bot spawn/removal authority: Fika raid host (listen-host on player PC or dedicated headless),
    /// singleplayer/local otherwise. Joining clients (EClientType.Client) have no authority.
    /// </summary>
    internal static class BotSpawnAuthority
    {
        private const string FikaHeadlessGuid = "com.fika.headless";

        private static bool _fikaProbeDone;
        private static bool _fikaAvailable;
        private static PropertyInfo _isServerProp;
        private static PropertyInfo _isClientProp;
        private static PropertyInfo _isHeadlessProp;
        private static PropertyInfo _isHeadlessGameProp;

        internal static bool HasBotRemovalAuthority(out string reason)
        {
            reason = null;

            if (!Singleton<GameWorld>.Instantiated || !Singleton<IBotGame>.Instantiated)
            {
                reason = "не в рейде (GameWorld/IBotGame недоступен)";
                return false;
            }

            if (!TryProbeFika())
            {
                return true;
            }

            if (Chainloader.PluginInfos.ContainsKey(FikaHeadlessGuid))
            {
                return true;
            }

            if (GetFikaBool(_isHeadlessProp) || GetFikaBool(_isHeadlessGameProp))
            {
                return true;
            }

            // Fika 2.3: IsServer == (ClientType == Host) — listen-host on player PC and Fika host in general.
            if (GetFikaBool(_isServerProp))
            {
                return true;
            }

            if (!GetFikaBool(_isClientProp))
            {
                return true;
            }

            reason = "Fika client (подключившийся игрок) — спавн/сброс только на ПК хоста рейда (IsServer/Host)";
            return false;
        }

        internal static string DescribeAuthority()
        {
            if (!TryProbeFika())
            {
                return "singleplayer/local (без Fika)";
            }

            if (Chainloader.PluginInfos.ContainsKey(FikaHeadlessGuid))
            {
                return "fika-headless plugin (authority хоста)";
            }

            var isServer = GetFikaBool(_isServerProp);
            var isClient = GetFikaBool(_isClientProp);
            var isHeadless = GetFikaBool(_isHeadlessProp);
            var isHeadlessGame = GetFikaBool(_isHeadlessGameProp);

            if (isServer)
            {
                return isHeadless || isHeadlessGame
                    ? "fika host (headless, IsServer)"
                    : "fika host (listen-host на ПК игрока, IsServer)";
            }

            if (isHeadless || isHeadlessGame)
            {
                return "fika headless (IsHeadless/IsHeadlessGame)";
            }

            if (!isClient)
            {
                return "fika host (не Client — authority есть)";
            }

            return "fika client (подключившийся игрок — без authority)";
        }

        private static bool TryProbeFika()
        {
            if (_fikaProbeDone)
            {
                return _fikaAvailable;
            }

            _fikaProbeDone = true;

            try
            {
                var fikaType = Type.GetType("Fika.Core.Main.Utils.FikaBackendUtils, Fika.Core");
                if (fikaType == null)
                {
                    _fikaAvailable = false;
                    return false;
                }

                const BindingFlags flags = BindingFlags.Static | BindingFlags.Public;
                _isServerProp = fikaType.GetProperty("IsServer", flags);
                _isClientProp = fikaType.GetProperty("IsClient", flags);
                _isHeadlessProp = fikaType.GetProperty("IsHeadless", flags);
                _isHeadlessGameProp = fikaType.GetProperty("IsHeadlessGame", flags);
                _fikaAvailable = _isServerProp != null && _isClientProp != null;
                return _fikaAvailable;
            }
            catch
            {
                _fikaAvailable = false;
                return false;
            }
        }

        private static bool GetFikaBool(PropertyInfo prop)
        {
            if (prop == null)
            {
                return false;
            }

            try
            {
                return prop.GetValue(null) is bool value && value;
            }
            catch
            {
                return false;
            }
        }
    }
}
