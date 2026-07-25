using UnityEngine;
using System.Diagnostics;



namespace BossSpawnControl

{

    internal sealed class PopulationMaintenanceBehaviour : MonoBehaviour

    {

        internal static PopulationMaintenanceBehaviour Instance { get; private set; }



        private float _timer;

        private bool _tickInProgress;

        private bool _lastPauseTimers;

        private long _lastTickMs;

        private int _tickCount;

        private long _maxTickMs;

        private readonly Stopwatch _tickStopwatch = new Stopwatch();



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



            // PAUSE_TIMERS

            if (plugin.PopulationConfig.PauseTimers.Value)

            {

                if (!_lastPauseTimers)

                {

                    _lastPauseTimers = true;

                    _tickInProgress = false;

                    _timer = 0f;

                    BotRemovalPollRunner.Instance?.StopAll();

                    plugin.Log("[PAUSE_TIMERS] BossSpawnControl - all background processes stopped.", true);

                }

                return;

            }

            else

            {

                if (_lastPauseTimers)

                {

                    _lastPauseTimers = false;

                    plugin.Log("[PAUSE_TIMERS] BossSpawnControl - resuming normal operations.", true);

                }

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

            _tickStopwatch.Restart();

            try

            {

                await PopulationMaintenanceService.RunMaintenanceTickAsync(plugin);

            }

            catch (System.Exception ex)

            {

                plugin.Log($"[POPULATION] TICK EXCEPTION: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}", true);

            }

            finally

            {

                _tickStopwatch.Stop();

                _lastTickMs = _tickStopwatch.ElapsedMilliseconds;

                _tickCount++;

                if (_lastTickMs > _maxTickMs) _maxTickMs = _lastTickMs;

                _tickInProgress = false;

                if (plugin.PopulationConfig.MaintenanceRunning.Value && !plugin.PopulationConfig.PauseTimers.Value)

                {

                    if (_lastTickMs > 500)

                    {

                        plugin.Log(

                            $"[POPULATION] TICK PERF WARNING: tick #{_tickCount} took {_lastTickMs}ms (max={_maxTickMs}ms). " +

                            $"This may contribute to frame drops.", true);

                    }

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

            if (plugin.ConfigService.DebugLogging.Value)

            {

                plugin.Log(

                    $"[POPULATION] Next scan in {_timer:0.0}s (range {minSec}-{maxSec}) " +

                    $"| ticks={_tickCount} lastTick={_lastTickMs}ms maxTick={_maxTickMs}ms");

            }

        }

    }

}
