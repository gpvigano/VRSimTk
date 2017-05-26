using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VRSimTk
{
    [RequireComponent(typeof(SimController))]
    public class SimControllerUI : MonoBehaviour
    {
        public GameObject simulationControlUI;
        public Slider progressSlider;
        public Text progressText;
        public Button playButton;
        public Button pauseButton;
        protected SimController simController = null;

        public void SetSimulationTimeSpeedMultiplier(int num)
        {
            float[] speed = { -10f, -1f, -0.5f, 0.5f, 1f, 2f, 10f };
            simController.SetSimulationTimeSpeed(speed[num]);
        }

        public void OnPlaySimulation()
        {
            if (progressSlider)
            {
                progressSlider.interactable = false;
            }
            if (playButton)
            {
                playButton.interactable = false;
            }
            if (pauseButton)
            {
                pauseButton.interactable = true;
            }
        }

        public void OnPauseSimulation()
        {
            if (simController.SimulationStarted)
            {
                if (progressSlider)
                {
                    progressSlider.interactable = true;
                }
            }
            if (playButton)
            {
                playButton.interactable = true;
            }
            if (pauseButton)
            {
                pauseButton.interactable = false;
            }
        }


        public void OnSimulationTimeSet(float time)
        {
            if (simController.SimulationPaused)
            {
                UpdateProgressSlider();
            }
        }

        protected virtual void Awake()
        {
            simController = GetComponent<SimController>();
            UpdateUIState();
            simController.OnSimulationLoaded += UpdateUIState;
            simController.OnSimulationUpdated += OnSimulationUpdated;
            simController.OnSimulationPlay += OnPlaySimulation;
            simController.OnSimulationPause += OnPauseSimulation;
            simController.OnSimulationTimeChanged += OnSimulationTimeSet;
            simController.OnSimulationStop += UpdateUIState;
        }

        protected virtual void OnDestroy()
        {
            simController.OnSimulationLoaded -= UpdateUIState;
            simController.OnSimulationUpdated -= OnSimulationUpdated;
            simController.OnSimulationPlay -= OnPlaySimulation;
            simController.OnSimulationPause -= OnPauseSimulation;
            simController.OnSimulationTimeChanged -= OnSimulationTimeSet;
            simController.OnSimulationStop -= UpdateUIState;
        }

        protected virtual void UpdateUIState()
        {
            if (simulationControlUI)
            {
                simulationControlUI.SetActive(simController.ValidSimulation);
            }
            if (progressSlider)
            {
                progressSlider.interactable = simController.SimulationStarted && simController.SimulationPaused;
                if (!simController.SimulationStarted)
                {
                    progressSlider.value = 0;
                }
            }
            if (pauseButton)
            {
                pauseButton.interactable = simController.SimulationStarted && !simController.SimulationPaused;
            }
            if (playButton)
            {
                playButton.interactable = !simController.SimulationStarted || simController.SimulationPaused;
            }
        }

        protected virtual void Update()
        {
            if (simController.SimulationStarted && !simController.SimulationPaused)
            {
                UpdateProgressSlider();
            }
        }

        void OnSimulationUpdated()
        {
            UpdateProgressText();
        }

        void UpdateProgressSlider()
        {
            if (progressSlider)
            {
                //progressSlider.enabled = false;
                progressSlider.value = simController.simulationTime / simController.SimulationDuration;
                //progressSlider.enabled = true;
            }
        }

        void UpdateProgressText()
        {
            if (progressText)
            {
                TimeSpan simTimeSpan = TimeSpan.FromSeconds(simController.simulationTime);
                progressText.text = "Time: " + simController.SimulationDateTime.ToString("dd/MM/yyyy hh:mm:ss") + " (" + simTimeSpan.ToString() + ")";
                //progressText.text = timeInterval.ToString();
            }
        }
    }
}
