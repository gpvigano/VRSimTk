using UnityEngine;
using System;
using System.Collections.Generic;

namespace VRSimTk
{
    public class SimController : MonoBehaviour
    {
        /// <summary>
        /// Simulation history data
        /// </summary>
        public SimHistory simulationHistory = new SimHistory();
        [Tooltip("Current simulation time (automatically computed if playing)")]
        /// <summary>
        /// Current simulation time (automatically computed if playing)
        /// </summary>
        public float simulationTime = 0;
        [Tooltip("Simulation time speed scale (1=normal, 0=frozen, negative=reverse)")]
        /// <summary>
        /// Simulation time speed scale (1=normal, 0=frozen, negative=reverse)
        /// </summary>
        public float simulationTimeSpeed = 1f;

        private float simulationStartTime = 0;
        private float simulationPauseTime = 0;
        private float simulationUpdateTime = -1f;
        private float simulationDuration = 0;
        private bool simulationStarted = false;
        private bool simulationPaused = false;
        private DateTime simulationDateTime;
        private List<SimExecutor> executorsList = new List<SimExecutor>();

        /// <summary>
        /// Event triggered after the simulation has been loaded
        /// </summary>
        public event Action OnSimulationLoaded;
        /// <summary>
        /// Event triggered each time the simulation has been updated
        /// </summary>
        public event Action OnSimulationUpdated;
        /// <summary>
        /// Event triggered before playing the simulation
        /// </summary>
        public event Action OnSimulationPlay;
        /// <summary>
        /// Event triggered before pausing the simulation
        /// </summary>
        public event Action OnSimulationPause;
        /// <summary>
        /// Event triggered after the simulation has been stopped
        /// </summary>
        public event Action OnSimulationStop;
        /// <summary>
        /// Event triggered after the simulation time is changed
        /// </summary>
        public event Action<float> OnSimulationTimeChanged;

        /// <summary>
        /// Check if a valid not empty simulation is defined
        /// </summary>
        public bool ValidSimulation
        {
            get
            {
                return simulationHistory.entityHistoryList != null && simulationHistory.entityHistoryList.Count > 0;
            }
        }

        /// <summary>
        /// Get the total simulation duration in seconds
        /// </summary>
        public float SimulationDuration { get { return simulationDuration; } }
        /// <summary>
        /// Get the current simulation date and time
        /// </summary>
        public DateTime SimulationDateTime { get { return simulationDateTime; } }
        /// <summary>
        /// This is true if the simulation was started
        /// </summary>
        public bool SimulationStarted { get { return simulationStarted; } }
        /// <summary>
        /// This is true if the simulation was paused
        /// </summary>
        public bool SimulationPaused  { get { return simulationPaused;  } }
        /// <summary>
        /// Force the simulation progress to the given percentage (0..1)
        /// </summary>
        /// <param name="progress">Progress between 0 (start) and 1 (end)</param>
        public void SetSimulationProgress(float progress)
        {
            if (simulationStarted && simulationPaused)
            {
                simulationTime = progress * simulationDuration;
                simulationStartTime = simulationPauseTime - simulationTime;
                UpdateSimulation();
            }
        }
        /// <summary>
        /// Change the time scale to control the speed of the simulation.
        /// </summary>
        /// <param name="scale">Time scale, multiplied by the real time</param>
        /// <remarks>Negative values make the time move backwards</remarks>
        public void SetSimulationTimeSpeed(float scale)
        {
            simulationTimeSpeed = scale;
            if (simulationStarted && simulationPaused)
            {
                UpdateSimulation();
            }
        }

        /// <summary>
        /// Force the simulation time to the given value (seconds since the beginning).
        /// </summary>
        /// <param name="time">Simulation time in seconds since the beginning</param>
        public void SetSimulationTime(float time)
        {
            if (simulationStarted)
            {
                simulationTime = time;
                if (simulationTime > simulationDuration)
                {
                    simulationTime = simulationDuration;
                }
                if (simulationTime < 0)
                {
                    simulationTime = 0;
                }
                if (simulationPaused)
                {
                    simulationStartTime = simulationPauseTime - simulationTime;
                    UpdateSimulation();
                }
                else
                {
                    simulationStartTime = Time.time - simulationTime;
                }
                if(OnSimulationTimeChanged != null)
                {
                    OnSimulationTimeChanged(time);
                }
            }
        }

        /// <summary>
        /// Increase the simulation tim by 5%
        /// </summary>
        public void IncSimulationTime()
        {
            if (simulationTime < simulationDuration)
            {
                SetSimulationTime(simulationTime + simulationDuration * 0.05f);
            }
        }

        /// <summary>
        /// Decrease the simulation tim by 5%
        /// </summary>
        public void DecSimulationTime()
        {
            if (simulationTime > 0)
            {
                SetSimulationTime(simulationTime - simulationDuration * 0.05f);
            }
        }

        /// <summary>
        /// Reset the simulation to the start time
        /// </summary>
        public void ResetSimulationTime()
        {
            SetSimulationTime(0);
        }

        /// <summary>
        /// Forward the simulation to its end time
        /// </summary>
        public void CompleteSimulationTime()
        {
            SetSimulationTime(simulationDuration);
        }

        /// <summary>
        /// Prepare the simulation loading each entity history and computing the overall data.
        /// </summary>
        public void LoadSimulation()
        {
            simulationUpdateTime = -1f;
            DateTime globalStartTime = DateTime.MaxValue;
            DateTime globalEndTime = DateTime.MinValue;
            simulationHistory.entityHistoryList = new List<EntityHistory>(FindObjectsOfType<EntityHistory>());
            foreach (EntityHistory history in simulationHistory.entityHistoryList)
            {
                //DataSync dataSync = FindObjectOfType<DataSync>();
                //bool zUp = dataSync ? dataSync.OriginalUpAxisIsZ : history.sourceUpAxisIsZ;
                //history.sourceUpAxisIsZ = zUp;
                history.ImportSimLog();
                if (globalStartTime > history.historyStartTime)
                {
                    globalStartTime = history.historyStartTime;
                }
                if (globalEndTime < history.historyEndTime)
                {
                    globalEndTime = history.historyEndTime;
                }
                var exec = history.GetComponent<SimExecutor>();
                if (exec == null)
                {
                    exec = history.gameObject.AddComponent<SimExecutor>();
                }
                exec.entityHistory = history;
                exec.targetTransform = history.transform;
                executorsList.Add(exec);
            }
            simulationHistory.startTime = globalStartTime;
            simulationHistory.endTime = globalEndTime;
            simulationDuration = (globalEndTime - globalStartTime).Ticks / TimeSpan.TicksPerSecond;
            if(OnSimulationLoaded!=null)
            {
                OnSimulationLoaded();
            }
        }

        /// <summary>
        /// Play the simulation or resume it if paused
        /// </summary>
        public void PlaySimulation()
        {
            if (OnSimulationPlay!=null && (!simulationStarted || simulationPaused))
            {
                OnSimulationPlay();
            }
            if (simulationStarted)
            {
                if (simulationPaused)
                {
                    simulationPaused = false;
                    simulationStartTime = Time.time - (simulationPauseTime - simulationStartTime);
                    simulationUpdateTime = -1f;
                }
            }
            else
            {
                simulationStartTime = Time.time;
                simulationStarted = true;
                foreach (var executor in executorsList)
                {
                    executor.isRunning = true;
                }
            }

        }

        /// <summary>
        /// Reset and stop the simulation
        /// </summary>
        public void StopSimulation()
        {
            ResetSimulationTime();
            UpdateSimulation();
            simulationUpdateTime = -1f;
            simulationStarted = false;
            simulationPaused = false;
            foreach (var executor in executorsList)
            {
                executor.UpdateTarget();
                executor.isRunning = false;
            }
            if (OnSimulationStop!=null)
            {
                OnSimulationStop();
            }
        }

        /// <summary>
        /// Pause the simulation (if started)
        /// </summary>
        public void PauseSimulation()
        {
            if (simulationStarted && !simulationPaused)
            {
                if (OnSimulationPause!=null)
                {
                    OnSimulationPause();
                }
                simulationPauseTime = Time.time;
                simulationPaused = true;
                UpdateSimulation();
            }
        }

        /// <summary>
        /// Update the simulation time based on the elapsed time and update the simulation.
        /// </summary>
        void UpdateSimulation()
        {
            bool pauseSim = false;

            if (simulationPaused)
            {
                simulationUpdateTime = -1f;
            }
            else
            {
                float t = Time.time;
                float deltaUpdateTime = simulationUpdateTime < 0 ? 0 : Time.time - simulationUpdateTime;
                simulationUpdateTime = t;
                simulationTime += deltaUpdateTime * simulationTimeSpeed;
            }
            if(simulationTime>simulationDuration)
            {
                simulationTime = simulationDuration;
                // if passed the end then pause
                pauseSim = simulationTimeSpeed > 0;
            }
            if(simulationTime<0)
            {
                simulationTime = 0;
                // if passed the beginning then pause
                pauseSim = simulationTimeSpeed < 0;
            }
            simulationDateTime = simulationHistory.startTime.AddSeconds(simulationTime);

            foreach (var executor in executorsList)
            {
                executor.simulationTime = simulationTime;
                executor.simulationDateTime = simulationDateTime;
            }
            if (OnSimulationUpdated!=null)
            {
                OnSimulationUpdated();
            }
            if (pauseSim)
            {
                PauseSimulation();
            }
        }

        void Update()
        {
            if (simulationStarted && !simulationPaused)
            {
                UpdateSimulation();
            }
        }
    }
}