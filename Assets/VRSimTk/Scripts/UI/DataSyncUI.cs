using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace VRSimTk
{
    [RequireComponent(typeof(DataSync))]
    public class DataSyncUI : MonoBehaviour
    {
        [Tooltip("Image (panel) used to show the processing progress when busy")]
        public Image progressImage = null;

        [Tooltip("Image (panel) used to obscure the scene when busy")]
        public Image fadeOutImage = null;
        [Tooltip("Duration of the fade out effect (obscure the scene when busy)")]
        public float fadeDuration = 0.5f;

        protected DataSync dataSync = null;
        protected Color initFadeColor = Color.clear;
        private IEnumerator fadeOutCoroutine;

        protected IEnumerator FadeOut()
        {
            yield return null;
            if (fadeOutImage)
            {
                float fadeDuration = 0.5f;
                float timeStep = 0.05f;
                float numSteps = fadeDuration > timeStep ? fadeDuration / timeStep : 1f;
                float fadeStep = 1f / numSteps;
                while (fadeOutImage.color != initFadeColor && dataSync.Busy)
                {
                    // smooth color transition
                    fadeOutImage.color = Color.Lerp(fadeOutImage.color, initFadeColor, fadeStep);
                    yield return new WaitForSeconds(timeStep);
                }
            }
        }

        protected virtual void StartLoadingMode()
        {
            if (fadeOutImage)
            {
                fadeOutImage.gameObject.SetActive(true);
                fadeOutImage.color = Color.clear;
            }
            if (progressImage)
            {
                progressImage.gameObject.SetActive(true);
                progressImage.fillAmount = 0.0f;
            }
            fadeOutCoroutine = FadeOut();
            StartCoroutine(fadeOutCoroutine);
        }

        protected virtual void StopLoadingMode()
        {
            if (fadeOutImage)
            {
                fadeOutImage.gameObject.SetActive(false);
            }
            if (progressImage)
            {
                progressImage.gameObject.SetActive(false);
            }
        }

        protected virtual void OnEnable()
        {
            dataSync.OnStartLoadingScenario += StartLoadingMode;
            dataSync.OnScenarioLoaded += StopLoadingMode;
        }

        protected virtual void OnDisable()
        {
            dataSync.OnStartLoadingScenario -= StartLoadingMode;
            dataSync.OnScenarioLoaded -= StopLoadingMode;
        }

        protected virtual void Awake()
        {
            dataSync = GetComponent<DataSync>();
            if (progressImage)
            {
                progressImage.gameObject.SetActive(false);
                progressImage.fillAmount = 0.0f;
            }
            if (fadeOutImage)
            {
                initFadeColor = fadeOutImage.color;
                fadeOutImage.color = Color.clear;
                fadeOutImage.gameObject.SetActive(false);
            }
        }

        protected virtual void Update()
        {
            if (dataSync.Busy && progressImage)
            {
                progressImage.fillAmount = Mathf.Lerp(progressImage.fillAmount, dataSync.Progress, 0.1f);
                if (dataSync.Progress < 0.01f)
                {
                    progressImage.fillAmount = dataSync.Progress;
                }
            }
        }
    }
}
