#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace VRSimTk
{
    public class MenuDataUtil : MonoBehaviour
    {
        public static EntityData MakeGameObjectEntity(GameObject go)
        {
            EntityData entity = null;
            if (go)
            {
                entity = go.GetComponent<EntityData>();
                if (entity == null)
                {
                    GameObject entObj = new GameObject(go.name);
                    entity = entObj.AddComponent<EntityData>();
                    go.transform.SetParent(entObj.transform);
                    var repr = MakeGameObjectRepresentation(go);
                    entity.activeRepresentation = repr;
                    //entity = go.AddComponent<EntityData>();
                    entity.id = string.Empty;
                    //var prefab = PrefabUtility.GetPrefabObject(go);
                    GameObject prefabParent = PrefabUtility.GetPrefabParent(go) as GameObject;
                    if (prefabParent)
                    {
                        entity.id = prefabParent.name;
                    }
                    else
                    {
                        entity.id = go.name;
                    }
                    go.name = entity.id + "_repr";
                    entity.id = DataUtil.CreateNewEntityId(entity);
                }
            }
            return entity;
        }

        public static EntityRepresentation MakeGameObjectRepresentation(GameObject go)
        {
            EntityRepresentation entityRepr = null;
            if (go)
            {
                entityRepr = go.GetComponent<EntityRepresentation>();
                if (entityRepr == null)
                {
                    // Register the creation in the undo system
                    entityRepr = Undo.AddComponent<EntityRepresentation>(go);
                    entityRepr.assetType = EntityRepresentation.AssetType.None;
                    var prefab = PrefabUtility.GetPrefabObject(go);
                    if (prefab)
                    {
                        Debug.LogFormat("Prefab of {0} is a {1}: {2}", go.name, prefab.GetType().Name, prefab.name);
                    }
                    GameObject prefabParent = PrefabUtility.GetPrefabParent(go) as GameObject;
                    if (prefabParent)
                    {
                        entityRepr.assetType = EntityRepresentation.AssetType.Prefab;
                        Debug.LogFormat("Prefab parent of {0} is a {1}: {2}", go.name, PrefabUtility.GetPrefabType(prefabParent), prefabParent.name);
                        string prefabParentPath = AssetDatabase.GetAssetPath(prefabParent);

                        entityRepr.assetBundleName = null;
                        /*
                        Object prefabAncestor = prefabParent;
                        while (prefabAncestor)
                        {
                        Debug.LogFormat("Path: {0}", AssetDatabase.GetAssetPath(prefabAncestor));
                            string path = AssetDatabase.GetAssetPath(prefabAncestor);
                            AssetImporter assetImporter = AssetImporter.GetAtPath(path);
                            if (!string.IsNullOrEmpty(entityRepr.assetBundleName))
                            {
                                entityRepr.assetType = EntityRepresentation.AssetType.AssetBundle;
                                entityRepr.assetBundleName = assetImporter.assetBundleName;
                                break;
                            }
                        Debug.LogFormat("Prefab parent of {0} is a {1}: {2}", go.name, PrefabUtility.GetPrefabType(prefabParent), prefabParent.name);
                            prefabAncestor = PrefabUtility.GetPrefabParent(prefabAncestor);
                        }*/
                        AssetDatabase.GetMainAssetTypeAtPath(prefabParentPath);
                        string[] assetBundleNames = AssetDatabase.GetAllAssetBundleNames();
                        foreach (string abName in assetBundleNames)
                        {
                            string[] assetPaths = AssetDatabase.GetAssetPathsFromAssetBundle(abName);
                            if (assetPaths.Contains(prefabParentPath))
                            {
                                entityRepr.assetType = EntityRepresentation.AssetType.AssetBundle;
                                entityRepr.assetBundleName = abName;
                                int dotPos = entityRepr.assetBundleName.IndexOf('.');
                                if (dotPos > 0)
                                {
                                    entityRepr.assetBundleName = entityRepr.assetBundleName.Substring(0, dotPos);
                                }
                                break;
                            }
                        }
                        entityRepr.assetName = prefabParent.name;
                    }
                    else
                    {
                        if (entityRepr.transform.childCount == 0)
                        {
                            MeshFilter meshFilter = go.GetComponent<MeshFilter>();
                            if (meshFilter)
                            {
                                entityRepr.assetType = EntityRepresentation.AssetType.Primitive;
                                Mesh mesh = meshFilter.sharedMesh;
                                if (mesh)
                                {
                                    switch (mesh.name)
                                    {
                                        case "Cube":
                                            entityRepr.assetPrimType = EntityRepresentation.AssetPrimType.Cube;
                                            break;
                                        case "Sphere":
                                            entityRepr.assetPrimType = EntityRepresentation.AssetPrimType.Sphere;
                                            break;
                                        case "Cylinder":
                                            entityRepr.assetPrimType = EntityRepresentation.AssetPrimType.Cylinder;
                                            break;
                                        case "Capsule":
                                            entityRepr.assetPrimType = EntityRepresentation.AssetPrimType.Capsule;
                                            break;
                                        case "Quad":
                                            entityRepr.assetPrimType = EntityRepresentation.AssetPrimType.Quad;
                                            break;
                                        case "Plane":
                                            entityRepr.assetPrimType = EntityRepresentation.AssetPrimType.Plane;
                                            break;
                                    }
                                }
                            }

                        }
                        else
                        {
                            Debug.LogError("Cannot create a representation from a hierarchy of primitives, create a prefab or an asset bundle and create the representation from it.");
                        }
                    }
                }
            }
            Transform parent = entityRepr.gameObject.transform.parent;
            if (parent)
            {
                var entity = parent.gameObject.GetComponent<EntityData>();
                if (entity)
                {
                    if (entity.activeRepresentation == null)
                    {
                        entity.activeRepresentation = entityRepr;
                    }
                }
                else
                {
                    Debug.LogWarning("The parent of this representation is not an entity.");
                }
            }
            else
            {
                Debug.LogWarning("This representation must be attached to an entity object.");
            }
            return entityRepr;
        }

        public static void MakeChildrenEntities(EntityData entity)
        {
            GameObject selObj = entity.gameObject;
            for (int i = 0; i < selObj.transform.childCount; i++)
            {
                EntityData childEntity = MakeGameObjectEntity(selObj.transform.GetChild(i).gameObject);
                childEntity.OnValidate();
            }
        }

        static public GameObject CreateEntityFromAsset(GameObject prefab)
        {
                GameObject entObj = null;
                // Create a custom game object
                GameObject go = Instantiate(prefab);
                var entity = go.GetComponent<EntityData>();
                if (entity)
                {
                    entObj = go;
                }
                else
                {
                    entObj = new GameObject(go.name);
                    go.transform.SetParent(entObj.transform);
                    var repr = go.AddComponent<EntityRepresentation>();
                    repr.assetType = EntityRepresentation.AssetType.Prefab;
                    string path = AssetDatabase.GetAssetPath(prefab);
                    AssetImporter assetImporter = AssetImporter.GetAtPath(path);
                    repr.assetBundleName = assetImporter.assetBundleName;
                    repr.assetName = prefab.name;
                    entity = entObj.AddComponent<EntityData>();
                    if (PrefabUtility.GetPrefabType(prefab) == PrefabType.Prefab)
                    {
                        entity.id = prefab.name;
                    }
                    entity.name = prefab.name;
                }
                entity.id = DataUtil.CreateNewEntityId(entity);
                entObj.name = prefab.name;
                // Register the creation in the undo system
                Undo.RegisterCreatedObjectUndo(entObj, "Create " + entObj.name + " " + entity.id);
                return entObj;
        }
        public static void RemoveRelationship(GameObject selObj)
        {
            var relationship = selObj.GetComponent<Relationship>();
            relationship.UnlinkEntities();
            Undo.DestroyObjectImmediate(relationship);
        }

        public static void RemoveEntity(GameObject selObj)
        {
                var entity = selObj.GetComponent<EntityData>();
                entity.UnlinkRelationships();
                var ownedRelationship = selObj.GetComponents<Relationship>();
                foreach (var rel in ownedRelationship)
                {
                    Undo.DestroyObjectImmediate(rel);
                }
                Undo.DestroyObjectImmediate(entity);
        }

        public static void IncludeChildrenInRelationship(OneToManyRelationship rel)
        {
            GameObject selObj = rel.gameObject;
            if (rel.objectEntities == null)
            {
                rel.objectEntities = new List<EntityData>();
            }
            for (int i = 0; i < selObj.transform.childCount; i++)
            {
                var childEntity = selObj.transform.GetChild(i).gameObject.GetComponent<EntityData>();
                if (childEntity)
                {
                    rel.objectEntities.Add(childEntity);
                }
            }
            rel.OnValidate();
        }

        public static void SelectObjectEntitiesOneToMany(OneToManyRelationship rel)
        {
            List<GameObject> go = new List<GameObject>();
            go.AddRange(rel.objectEntities.Select(ent => ent.gameObject));
            Selection.objects = go.ToArray();
        }

        public static void SelectSubjectEntitiesManyToMany(ManyToManyRelationship rel)
        {
            List<GameObject> go = new List<GameObject>();
            go.AddRange(rel.subjectEntities.Select(ent => ent.gameObject));
            Selection.objects = go.ToArray();
        }

        public static void SelectObjectEntitiesManyToMany(ManyToManyRelationship rel)
        {
            List<GameObject> go = new List<GameObject>();
            go.AddRange(rel.objectEntities.Select(ent => ent.gameObject));
            Selection.objects = go.ToArray();
        }

        public static void SetupRelationship(Relationship rel, string name)
        {
            GameObject selObj = rel.gameObject;
            rel.id = DataUtil.CreateNewId(rel, selObj.name + "-" + name);
            rel.typeName = name;
        }

        public static void SetupOneToOneRelationship(OneToOneRelationship rel, string name)
        {
            SetupRelationship(rel, name);
            GameObject selObj = rel.gameObject;
            rel.subjectEntity = selObj.GetComponent<EntityData>();
        }

        public static void SetupOneToManyRelationship(OneToManyRelationship rel, string name)
        {
            SetupRelationship(rel, name);
            GameObject selObj = rel.gameObject;
            rel.subjectEntity = selObj.GetComponent<EntityData>();
        }
    }
}

#endif
