using UnityEngine;



namespace BossSpawnControl

{

    internal sealed class PopulationMaintenanceBehaviour : MonoBehaviour

    {

        internal static PopulationMaintenanceBehaviour Instance { get; private set; }



        private float _timer;

        private bool _tickInProgress;



        internal static BotPopulationSnapshot LastSnapshot { get; private set; }



        internal static void UpdateLastSnapshot(BotPopulationSnapshot snapshot)

        {

            LastSnapshot = snapshot;

            var plugin = PluginCore.Instance;

            plugin?.PopulationConfig.SetLastSnapshot(snapshot);

        }



        private void Awake()

        {

            Instance = this;

        }



        private void OnDestroy()

        {

            if (Instance == this)

            {

                Instance = null;

            }

        }



        internal void SyncFromConfig()

        {

            var plugin = PluginCore.Instance;

            if (plugin == null)

            {

                return;

            }



            if (plugin.PopulationConfig.MaintenanceRunning.Value)

            {

                ScheduleNextScan(plugin);

                plugin.Log("[POPULATION] Maintenance mode STARTED from config.", true);

            }

            else

            {

                _timer = 0f;

                PopulationSpawnerLimitSync.RestoreIfSaved();

                plugin.Log("[POPULATION] Maintenance mode STOPPED from config.", true);

            }

        }



        private void Update()

        {

            var plugin = PluginCore.Instance;

            if (plugin == null || !plugin.PopulationConfig.MaintenanceRunning.Value)

            {

                return;

            }



            _timer -= Time.deltaTime;

            if (_timer > 0f || _tickInProgress)

            {

                return;

            }



            _tickInProgress = true;

            RunTickAsync(plugin);

        }



        private async void RunTickAsync(PluginCore plugin)

        {

            try

            {

                await PopulationMaintenanceService.RunMaintenanceTickAsync(plugin);

            }

            finally

            {

                _tickInProgress = false;

                if (plugin.PopulationConfig.MaintenanceRunning.Value)

                {

                    ScheduleNextScan(plugin);

                }

            }

        }



        private void ScheduleNextScan(PluginCore plugin)

        {

            var cfg = plugin.PopulationConfig;

            var minSec = Mathf.Max(1, cfg.ScanIntervalMinSec.Value);

            var maxSec = Mathf.Max(minSec, cfg.ScanIntervalMaxSec.Value);

            _timer = Random.Range(minSec, maxSec);

            plugin.Log($"[POPULATION] Next scan in {_timer:0.0}s (range {minSec}-{maxSec}).", plugin.ConfigService.DebugLogging.Value);

        }

    }

}

