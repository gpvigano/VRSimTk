using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using AssetBundles;

/// <summary>
/// Virtual Reality Simulation Toolkit main namespace.
/// </summary>
namespace VRSimTk
{
    public abstract class DataSync : MonoBehaviour
    {

        [Header("Paths", order = 10)]

        [Tooltip("Scenario data path (relative to data path, or persistent data path for mobile platform)")]
        public string scenarioDataPath = "../";

        [Tooltip("Scenario file for data storage")]
        public string scenarioDataFile = "../scenario.xml";

        /// <summary>
        /// Return true if the entity data has Z as vertical axis.
        /// </summary>
        /// <remarks>This must be bound to the data source settings.</remarks>
        public abstract bool OriginalUpAxisIsZ
        {
            get;
            set;
        }

        public abstract float Progress { get; }
        public bool Busy { get { return isBusy; } }

        protected Dictionary<string, GameObject> entityObjects = new Dictionary<string, GameObject>();

        protected bool isBusy = false;
        protected int createdRepresentations = 0;
        protected int updatedEntities = 0;
        protected int representationCount = 0;

        private bool assetManagerInitialized = false;

        /// <summary>
        /// Event triggered <b>before</b> loading the scenario
        /// </summary>
        public event Action OnStartLoadingScenario;

        /// <summary>
        /// Event triggered <b>after</b> the scenario has been loaded
        /// </summary>
        public event Action OnScenarioLoaded;
        ////public event Action OnScenarioUnloaded;
        ////public event Action OnSavingScenario;
        ////public event Action OnScenarioSaved;

        /// <summary>
        /// Get the list of variants as read from the data source
        /// </summary>
        public abstract string[] ActiveVariants { get; }

        /// <summary>
        /// Clear scenario data without affecting the scene
        /// </summary>
        public abstract void ClearScenarioData();

        /// <summary>
        /// Check if the entity with the given identifier is defined in the scenario data
        /// </summary>
        /// <param name="entityId"></param>
        /// <returns></returns>
        public abstract bool EntityInScenario(string entityId);

        /// <summary>
        /// Get the current scenario full path
        /// </summary>
        /// <returns>Full path (inside the data or prsistent dat path)</returns>
        public string GetScenarioPath()
        {
#if (UNITY_ANDROID || UNITY_IPHONE) && !UNITY_EDITOR
            string data_path = Application.persistentDataPath;
#else
            string data_path = Application.dataPath;
#endif
            string path = data_path + "/" + scenarioDataPath;
            if (!path.EndsWith("/"))
            {
                path += "/";
            }
            path += scenarioDataFile;
            return path;
        }

        /// <summary>
        /// Load the scenario data from the currently defined location and update the scene
        /// </summary>
        public virtual void LoadDataToScene()
        {
            if (string.IsNullOrEmpty(scenarioDataFile))
            {
                Debug.LogWarning("Project URL not defined");
                return;
            }
            StartCoroutine(LoadScenario());
        }

        /// <summary>
        /// Update the scenario data and save it to the currently defined location
        /// </summary>
        public virtual void SaveDataFromScene()
        {
            if (string.IsNullOrEmpty(scenarioDataFile))
            {
                Debug.LogWarning("Project URL not defined");
                return;
            }
            InitScenarioData();
            StartCoroutine(SaveScenario());
        }

        /// <summary>
        /// Read the scenario data from the currently defined location
        /// </summary>
        public bool ReadScenario()
        {
            if (!ReadScenario(GetScenarioPath()))
            {
                Debug.LogWarning("Failed to load scenario from " + GetScenarioPath());
                return false;
            }
            return true;
        }

        /// <summary>
        /// Read the scenario data from the given URL or file name
        /// </summary>
        /// <param name="url">URL (or file name)</param>
        /// <returns>True on success, false on error.</returns>
        public abstract bool ReadScenario(string url);

        /// <summary>
        /// Update the scenario data from the scene components
        /// </summary>
        public abstract void UpdateScenarioData();

        /// <summary>
        /// Write the scenario data to the currently defined location
        /// </summary>
        public bool WriteScenario()
        {
            if (!WriteScenario(GetScenarioPath()))
            {
                Debug.LogWarning("Failed to save scenario to " + GetScenarioPath());
                return false;
            }
            return true;
        }

        /// <summary>
        /// Write the scenario data to the given URL or file name
        /// </summary>
        /// <param name="url">URL (or file name)</param>
        /// <returns>True on success, false on error.</returns>
        public abstract bool WriteScenario(string url);

        /// <summary>
        /// Initialize scenario data (create it if not defined)
        /// </summary>
        protected abstract void InitScenarioData();

        /// <summary>
        /// Update the scene from the scenario data, if available
        /// </summary>
        /// <returns>It must be called iteratively (e.g. with StartCoroutine())</returns>
        protected abstract IEnumerator UpdateScene();

        /// <summary>
        /// Update the scenario data from the scene components
        /// </summary>
        /// <returns>It must be called iteratively (e.g. with StartCoroutine())</returns>
        protected abstract IEnumerator UpdateScenario();

        /// <summary>
        /// Load the scenario data from the currently defined location and update the scene
        /// </summary>
        /// <returns>It must be called iteratively (e.g. with StartCoroutine())</returns>
        protected virtual IEnumerator LoadScenario()
        {
            isBusy = true;
            if (OnStartLoadingScenario != null)
            {
                OnStartLoadingScenario();
            }
            yield return Initialize();
            UpdateEntityObjectsList();
            if (!ReadScenario())
            {
                yield break;
            }
            // list of variants (aka representation contexts)
            // if the first one is not available try the second one and so on

            // Set active variants.
            AssetBundleManager.ActiveVariants = ActiveVariants;

            CleanUpScene();
            yield return StartCoroutine(UpdateScene());
            //yield return UpdateScene();

            // refresh all the reflection probes in the scene that are set to be updated via scripting
            GameObject[] rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (GameObject obj in rootObjects)
            {
                var probes = obj.GetComponentsInChildren<ReflectionProbe>();
                foreach (var probe in probes)
                {
                    if (probe.refreshMode == UnityEngine.Rendering.ReflectionProbeRefreshMode.ViaScripting)
                    {
                        probe.RenderProbe();
                    }
                    yield return null;
                }
            }
            isBusy = false;
            if (OnScenarioLoaded != null)
            {
                OnScenarioLoaded();
            }
        }


        /// <summary>
        /// Update the scenario data from the scene and save it to the currently defined location
        /// </summary>
        /// <returns>It must be called iteratively (e.g. with StartCoroutine())</returns>
        protected virtual IEnumerator SaveScenario()
        {
            //yield return Initialize();
            UpdateScenarioData();
            //StartCoroutine(UpdateScenario());
            WriteScenario();
            yield return null;
        }

        /// <summary>
        /// Update the internal cache mapping the entity id to the related game object
        /// </summary>
        /// <param name="obj">The game object (including children) to be mapped</param>
        protected void UpdateEntityObjectsListFromObject(GameObject obj)
        {
            if (obj == null)
            {
                return;
            }
            EntityData entity = obj.GetComponent<EntityData>();
            if (entity)
            {
                entityObjects.Add(entity.id, obj);
            }
            for (int i = 0; i < obj.transform.childCount; i++)
            {
                UpdateEntityObjectsListFromObject(obj.transform.GetChild(i).gameObject);
            }
        }

        /// <summary>
        /// Update the whole internal cache mapping the entity id to the related game object
        /// </summary>
        protected void UpdateEntityObjectsList()
        {
            entityObjects.Clear();
            GameObject[] rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (GameObject obj in rootObjects)
            {
                UpdateEntityObjectsListFromObject(obj);
            }
        }

        protected virtual IEnumerator Start()
        {
            //yield return Initialize();
            yield return StartCoroutine(Initialize());

            ////// list of variants (aka representation contexts)
            ////// if the first one is not available try the second one and so on
            ////string[] activeVariants = new string[2];
            ////activeVariants[0] = "context2";
            ////activeVariants[1] = "context1";

            ////// Set active variants.
            ////AssetBundleManager.ActiveVariants = activeVariants;
        }

        /// <summary>
        /// Initialize the asset bundle manager
        /// </summary>
        /// <returns>It must be called iteratively (e.g. with StartCoroutine())</returns>
        protected IEnumerator Initialize()
        {
            if (assetManagerInitialized)
            {
                yield break;
            }

            //// Don't destroy this gameObject as we depend on it to run the loading script.
            //DontDestroyOnLoad(gameObject);

            // With this code, when in-editor or using a development builds: Always use the AssetBundle Server
            // (This is very dependent on the production workflow of the project. 
            // 	Another approach would be to make this configurable in the standalone player.)
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            AssetBundleManager.SetDevelopmentAssetBundleServer();
#else
#endif
            AssetBundleManager.SetSourceAssetBundleURL(Utility.GetAssetBundlesOutputUrl());

            // Initialize AssetBundleManifest which loads the AssetBundleManifest object.
            var request = AssetBundleManager.Initialize();
            if (request != null)
            {
                yield return StartCoroutine(request);
            }
            assetManagerInitialized = true;
        }

        /// <summary>
        /// Instantiate a new game object in an asynchronous way.
        /// </summary>
        /// <param name="assetBundleName">Asset bundle name</param>
        /// <param name="assetName">Asset name inside the asset bundle</param>
        /// <param name="reprId">representation identifier to be used for the EntityRepresentation component</param>
        /// <param name="entityId">Entity identifier to be used for the EntityData component</param>
        /// <returns>It must be called iteratively (e.g. with StartCoroutine())</returns>
        protected IEnumerator InstantiateGameObjectAsync(string assetBundleName, string assetName, string reprId, string entityId)
        {
            // This is simply to get the elapsed time for this phase of AssetLoading.
            float startTime = Time.realtimeSinceStartup;

            // Load asset from assetBundle.
            AssetBundleLoadAssetOperation request = AssetBundleManager.LoadAssetAsync(assetBundleName, assetName, typeof(GameObject));
            if (request == null)
            {
                yield break;
            }
            yield return StartCoroutine(request);

            // Get the asset.
            GameObject prefab = request.GetAsset<GameObject>();
            if (prefab != null)
            {
                if (!entityObjects.ContainsKey(entityId))
                {
                    Debug.LogWarning("Asset " + assetName + " not found.");
                    yield break;
                }
                GameObject go = GameObject.Instantiate(prefab);
                go.transform.SetParent(entityObjects[entityId].transform, false);
                if (string.IsNullOrEmpty(reprId))
                {
                    reprId = DataUtil.CreateNewId(go, null);
                }
                var entityRepr = go.GetComponent<EntityRepresentation>();
                if (entityRepr == null)
                {
                    entityRepr = go.AddComponent<EntityRepresentation>();
                }
                go.name = reprId;
                entityRepr.assetType = EntityRepresentation.AssetType.AssetBundle;
                entityRepr.name = prefab.name;
                entityRepr.assetBundleName = assetBundleName;
                entityRepr.assetName = assetName;
                entityObjects[entityId].GetComponent<EntityData>().activeRepresentation = entityRepr;
                createdRepresentations++;
            }

            // Calculate and display the elapsed time.
            float elapsedTime = Time.realtimeSinceStartup - startTime;
            if (prefab)
            {
                Debug.Log(assetName + " was loaded successfully in " + elapsedTime + " seconds");
            }
            else
            {
                Debug.LogWarning("Failed to load " + assetName + "  (" + elapsedTime + " seconds)");
            }
        }

        /// <summary>
        /// Destroy all the entity objects in the scene (and their children)
        /// </summary>
        protected virtual void CleanUpScene()
        {
            foreach (var entry in entityObjects)
            {
                Destroy(entry.Value);
            }
            entityObjects.Clear();
            createdRepresentations = 0;
            updatedEntities = 0;
            representationCount = 0;
        }

        /*
        protected void CleanUpScene(bool keepExisting)
        {
            if (!keepExisting)
            {
                foreach (var entry in entityObjects)
                {
                    Destroy(entry.Value);
                }
                entityObjects.Clear();
                return;
            }
            // the following code is not used for now
            // It would be useful to update objects without destroying and recreating them

            // create a list of identifiers initially filled with all those already assigned
            // to objects in the scene, then the identifiers present in the scenario are removed,
            // in this way we get only the identifiers that must be removed from the scene
            var deadObjectIdList = new List<string>();
            foreach (var entry in entityObjects)
            {
                if (!EntityInScenario(entry.Key))
                {
                    deadObjectIdList.Add(entry.Key);
                }
            }

            // delete objects not present in the scenario
            // objects attached to another object that will be destroyed will be destroyed along with it
            foreach (var id in deadObjectIdList)
            {
                bool destroy = true;
                if (entityObjects[id].transform.parent)
                {
                    var parentEntities = entityObjects[id].transform.parent.GetComponentsInParent<EntityData>();
                    foreach (var entity in parentEntities)
                    {
                        if (deadObjectIdList.Contains(entity.id))
                        {
                            destroy = false;
                            break;
                        }
                    }
                }
                // do not destroy children (?)
                //entityObjects[id].transform.DetachChildren();
                //contentObjects[id].SetActive(false);
                // remove the object from the scene and schedule it for destruction
                if (destroy)
                {
                    Destroy(entityObjects[id]);
                }
                // update the related id-object mapping
                entityObjects.Remove(id);
            }
        }
        */

    }
}