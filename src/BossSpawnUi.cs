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
    }
}
