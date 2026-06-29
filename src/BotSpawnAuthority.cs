using System;
using System.Reflection;
using Comfort.Common;
using EFT;

namespace BossSpawnControl
{
    /// <summary>
    /// Bot spawn/removal authority: headless host in Fika coop, local game otherwise.
    /// Uses reflection so BossSpawnControl does not require a Fika.Core reference.
    /// </summary>
    internal static class BotSpawnAuthority
    {
        private static bool _fikaProbeDone;
        private static bool _fikaAvailable;
        private static PropertyInfo _isServerProp;
        private static PropertyInfo _isClientProp;

        internal static bool HasBotRemovalAuthority(out string reason)
        {
            reason = null;

            if (!Singleton<GameWorld>.Instantiated || !Singleton<IBotGame>.Instantiated)
            {
                reason = "not in raid (GameWorld/IBotGame unavailable)";
                return false;
            }

            if (!TryProbeFika())
            {
                return true;
            }

            if (!GetFikaBool(_isClientProp))
            {
                return true;
            }

            if (GetFikaBool(_isServerProp))
            {
                return true;
            }

            reason = "Fika client — bot removal only on headless host / listen host (IsServer)";
            return false;
        }

        internal static string DescribeAuthority()
        {
            if (!TryProbeFika())
            {
                return "singleplayer/local (no Fika)";
            }

            return $"fika isServer={GetFikaBool(_isServerProp)} isClient={GetFikaBool(_isClientProp)}";
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

                _isServerProp = fikaType.GetProperty("IsServer", BindingFlags.Static | BindingFlags.Public);
                _isClientProp = fikaType.GetProperty("IsClient", BindingFlags.Static | BindingFlags.Public);
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
