#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace VRSimTk
{
    public class ContextMenuItems : MonoBehaviour
    {
        /*
        [MenuItem("CONTEXT/GameObject/Create grid of objects")]
        static void CreateGridOfObjects(MenuCommand menuCommand)
        {
            GameObject sampleObject = menuCommand.context as GameObject;
            EditorToolkit.CreateGridOfObjects(sampleObject,10,1,10, Vector3.one);
        }
        */

        [MenuItem("CONTEXT/EntityData/Make children entities")]
        static void MakeChildrenEntities(MenuCommand menuCommand)
        {
            EntityData entity = menuCommand.context as EntityData;
            MenuDataUtil.MakeChildrenEntities(entity);
        }

        // Validate the menu item defined by the function above.
        [MenuItem("CONTEXT/EntityData/Make children entities", true)]
        static bool ValidateMakeChildrenEntities()
        {
            return Selection.activeGameObject != null
                && Selection.activeGameObject.transform.childCount > 0;
        }

        [MenuItem("CONTEXT/DataSync/Update scenario data")]
        static void UpdateScenarioData(MenuCommand menuCommand)
        {
            DataSync sync = menuCommand.context as DataSync;
            sync.UpdateScenarioData();
        }

        [MenuItem("CONTEXT/DataSync/Save scenario data")]
        static void SaveScenarioData(MenuCommand menuCommand)
        {
            DataSync sync = menuCommand.context as DataSync;
            sync.WriteScenario();
        }

        [MenuItem("CONTEXT/DataSync/Load scenario data")]
        static void LoadScenarioData(MenuCommand menuCommand)
        {
            DataSync sync = menuCommand.context as DataSync;
            sync.ReadScenario();
        }

        [MenuItem("CONTEXT/DataSync/Clear scenario data")]
        static void ClearScenarioData(MenuCommand menuCommand)
        {
            DataSync sync = menuCommand.context as DataSync;
            sync.ClearScenarioData();
        }

        [MenuItem("CONTEXT/DataSync/Show data path in Explorer")]
        static void ShowScenarioDataPath(MenuCommand menuCommand)
        {
            DataSync sync = menuCommand.context as DataSync;
            //EditorUtility.RevealInFinder(Application.dataPath + "/" + sync.scenarioDataPath);
            EditorUtility.RevealInFinder(sync.GetScenarioPath());
        }

        [MenuItem("Assets/VRSimTk/Create Entity from prefab", false, 10)]
        static public void CreateEntityFromAsset()
        {
            GameObject prefab = Selection.activeObject as GameObject;
            if (prefab)
            {
                GameObject entObj = MenuDataUtil.CreateEntityFromAsset(prefab);
                if (entObj)
                {
                    Selection.activeObject = entObj;
                }
            }
        }

        [MenuItem("Assets/VRSimTk/Create Entity from prefab", true)]
        static public bool ValidateCreateEntityFromAsset()
        {
            return Selection.activeObject as GameObject != null;
        }

        [MenuItem("CONTEXT/Relationship/Remove relationship")]
        static void RemoveRelationship()
        {
            MenuDataUtil.RemoveRelationship( Selection.activeGameObject );
        }

        [MenuItem("CONTEXT/EntityData/Remove entity")]
        static void RemoveEntity()
        {
            foreach (GameObject selObj in Selection.gameObjects)
            {
                MenuDataUtil.RemoveEntity(selObj);
            }
        }

        [MenuItem("CONTEXT/OneToManyRelationship/Include children entities in relationship")]
        static void IncludeChildrenInRelationship(MenuCommand menuCommand)
        {
            MenuDataUtil.IncludeChildrenInRelationship(menuCommand.context as OneToManyRelationship);
        }

        [MenuItem("CONTEXT/OneToOneRelationship/Select relationship subject entity")]
        static void SelectSubjectEntityOneToOne(MenuCommand menuCommand)
        {
            var rel = menuCommand.context as OneToOneRelationship;
            Selection.activeGameObject = rel.subjectEntity.gameObject;
        }

        [MenuItem("CONTEXT/OneToOneRelationship/Select relationship object entities")]
        static void SelectObjectEntitiesOneToOne(MenuCommand menuCommand)
        {
            var rel = menuCommand.context as OneToOneRelationship;
            Selection.activeGameObject = rel.objectEntity.gameObject;
        }

        [MenuItem("CONTEXT/OneToManyRelationship/Select relationship subject entity")]
        static void SelectSubjectEntityOneToMany(MenuCommand menuCommand)
        {
            var rel = menuCommand.context as OneToManyRelationship;
            Selection.activeGameObject = rel.subjectEntity.gameObject;
        }

        [MenuItem("CONTEXT/OneToManyRelationship/Select relationship object entities")]
        static void SelectObjectEntitiesOneToMany(MenuCommand menuCommand)
        {
            MenuDataUtil.SelectObjectEntitiesOneToMany(menuCommand.context as OneToManyRelationship);
        }

        [MenuItem("CONTEXT/ManyToManyRelationship/Select relationship subject entities")]
        static void SelectSubjectEntitiesManyToMany(MenuCommand menuCommand)
        {
            MenuDataUtil.SelectSubjectEntitiesManyToMany(menuCommand.context as ManyToManyRelationship);
        }

        [MenuItem("CONTEXT/ManyToManyRelationship/Select relationship object entities")]
        static void SelectObjectEntitiesManyToMany(MenuCommand menuCommand)
        {
            MenuDataUtil.SelectObjectEntitiesManyToMany(menuCommand.context as ManyToManyRelationship);
        }
    }
}

#endif
