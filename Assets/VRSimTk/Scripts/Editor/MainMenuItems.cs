#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using AssetBundleWorkshop;
using EditorTkEx;
using AssetBundles;

namespace VRSimTk
{
    public class MainMenuItems : MonoBehaviour
    {
        // Add a menu item called "Load Scenario" to a DataSync's context menu.
        [MenuItem("VRSimTk/Load Scenario", false, 10)]
        static void LoadScenario(MenuCommand command)
        {
            DataSync sync = FindObjectOfType<DataSync>();
            sync.LoadDataToScene();
        }

        // Validate the menu item defined by the function above.
        [MenuItem("VRSimTk/Load Scenario", true)]
        static bool ValidateLoadScenario()
        {
            // The AssetBundleManager cannot work in editor mode
            return DataUtil.DataSyncAvailable() && Application.isPlaying;
        }

        [MenuItem("VRSimTk/Save Scenario", false, 20)]
        static void SaveScenario(MenuCommand command)
        {
            DataSync sync = FindObjectOfType<DataSync>();
            sync.SaveDataFromScene();
        }

        // Validate the menu item defined by the function above.
        [MenuItem("VRSimTk/Save Scenario", true)]
        static bool ValidateSaveScenario()
        {
            return DataUtil.DataSyncAvailable();// && Application.isPlaying;
        }

        [MenuItem("VRSimTk/New Entity", false, 30)]
        static void CreateEntityGameObject(MenuCommand menuCommand)
        {
            // Create a custom game object
            GameObject go = new GameObject("NewEntity");
            var entity = Undo.AddComponent<EntityData>(go);
            entity.id = DataUtil.CreateNewId(entity);
            // Ensure it gets reparented if this was a context click (otherwise does nothing)
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            // Register the creation in the undo system
            Undo.RegisterCreatedObjectUndo(go, "New Entity " + go.name + " " + entity.id);
            Selection.activeObject = go;
        }

        // Validate the menu item defined by the function above.
        [MenuItem("VRSimTk/New Entity", true)]
        static bool ValidateCreateEntityGameObject()
        {
            return Selection.activeGameObject != null;
        }

        [MenuItem("VRSimTk/Make Entity", false, 30)]
        static void MakeSelectionEntity()
        {
            foreach (GameObject go in Selection.gameObjects)
            {
                MenuDataUtil.MakeGameObjectEntity(go);
            }
        }

        // Validate the menu item defined by the function above.
        [MenuItem("VRSimTk/Make Entity", true)]
        static bool ValidateMakeSelectionEntity()
        {
            return Selection.activeGameObject != null
                && Selection.activeGameObject.GetComponent<EntityData>() == null
                && Selection.activeGameObject.GetComponent<DataSync>() == null;
        }

        [MenuItem("VRSimTk/Make Representation", false, 30)]
        static void MakeSelectionRepresentation()
        {
            foreach (GameObject go in Selection.gameObjects)
            {
                MenuDataUtil.MakeGameObjectRepresentation(go);
            }
        }

        [MenuItem("VRSimTk/Relationship/Add One-to-one", false, 50)]
        static void AddRelationshipOneToOne(MenuCommand menuCommand)
        {
            GameObject selObj = Selection.activeGameObject;
            var rel = Undo.AddComponent<OneToOneRelationship>(selObj);
            MenuDataUtil.SetupOneToOneRelationship(rel, "OneToOne");
        }

        [MenuItem("VRSimTk/Relationship/Add One-to-many", false, 51)]
        static void AddRelationshipOneToMany(MenuCommand menuCommand)
        {
            GameObject selObj = Selection.activeGameObject;
            var rel = Undo.AddComponent<OneToManyRelationship>(selObj);
            MenuDataUtil.SetupOneToManyRelationship(rel, "OneToMany");
        }

        [MenuItem("VRSimTk/Relationship/Add Many-to-many", false, 52)]
        static void AddRelationshipManyToMany(MenuCommand menuCommand)
        {
            GameObject selObj = Selection.activeGameObject;
            var rel = Undo.AddComponent<ManyToManyRelationship>(selObj);
            MenuDataUtil.SetupRelationship(rel, "ManyToMany");
        }

        [MenuItem("VRSimTk/Relationship/Add Composition", false, 65)]
        static void AddRelationshipComposition(MenuCommand menuCommand)
        {
            GameObject selObj = Selection.activeGameObject;
            var composition = Undo.AddComponent<CompositionRelationship>(selObj);
            MenuDataUtil.SetupOneToManyRelationship(composition, "Composition");
        }

        [MenuItem("VRSimTk/Relationship/Add Inclusion", false, 66)]
        static void AddRelationshipInclusion(MenuCommand menuCommand)
        {
            GameObject selObj = Selection.activeGameObject;
            var composition = Undo.AddComponent<InclusionRelationship>(selObj);
            MenuDataUtil.SetupOneToManyRelationship(composition, "Inclusion");
        }

        static bool ValidateAddRelationship()
        {
            return Selection.activeGameObject != null && Selection.activeGameObject.GetComponent<EntityData>() != null;
        }

        [MenuItem("VRSimTk/Relationship/Add One-to-one", true)]
        static bool ValidateAddRelationshipOneToOne()
        {
            return ValidateAddRelationship();
        }
        [MenuItem("VRSimTk/Relationship/Add One-to-many", true)]
        static bool ValidateAddRelationshipOneToMany()
        {
            return ValidateAddRelationship();
        }
        [MenuItem("VRSimTk/Relationship/Add Many-to-many", true)]
        static bool ValidateAddRelationshipManyToMany()
        {
            return ValidateAddRelationship();
        }
        [MenuItem("VRSimTk/Relationship/Add Inclusion", true)]
        static bool ValidateAddRelationshipInclusion()
        {
            return ValidateAddRelationship();
        }
        [MenuItem("VRSimTk/Relationship/Add Composition", true)]
        static bool ValidateAddRelationshipComposition()
        {
            return ValidateAddRelationship();
        }

        [MenuItem("VRSimTk/Build app...")]
        public static void BuildPlayerTo()
        {
            BuildPlayer(false);
        }
        [MenuItem("VRSimTk/Build and run...")]
        public static void BuildAndRunPlayerTo()
        {
            BuildPlayer(true);
        }

        // TODO: to be tested
        public static void BuildPlayer(bool runAfterBuild)
        {
            string outputPath = BuildTk.SelectBuildPath(ref AssetBundleUtil.lastTargetBuildFolder, ref AssetBundleUtil.lastTargetBuildName);
            if (outputPath == null)
            {
                return;
            }
            string path = BuildTk.GetAppDataPath(AssetBundleUtil.lastTargetBuildFolder, AssetBundleUtil.lastTargetBuildName);
            if(path == null)
            {
                path = AssetBundleUtil.lastTargetBuildFolder;
            }
            // Build and copy AssetBundles.
            AssetBundleUtil.BuildAssetBundles(EditorUserBuildSettings.activeBuildTarget, path+ AssetBundleSettings.AssetBundlesPath);
            AssetBundleUtil.WriteServerURL();
            // copy scenario
            FileUtil.CopyFileOrDirectory("scenario", path+"/scenario");

            PlayerSettings.Android.forceSDCardPermission = true;
            BuildTk.BuildPlayer(outputPath, runAfterBuild);
        }
    }
}

#endif
