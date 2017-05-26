using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VRSimTk
{
    public class EntityRepresentation : MonoBehaviour
    {
        public enum AssetType
        {
            None,
            Prefab,
            AssetBundle,
            Primitive,
            Model,
        }
        public enum AssetPrimType
        {
            None,
            Sphere,
            Cube,
            Cylinder,
            Capsule,
            Quad,
            Plane,
        }
        [Tooltip("Representation type")]
        public AssetType assetType = AssetType.None;
        [Tooltip("Representation primitive type (if Representation type is Primitive)")]
        public AssetPrimType assetPrimType = AssetPrimType.None;
        [Tooltip("Representation asset bundle")]
        public string assetBundleName = string.Empty;
        [Tooltip("Representation asset")]
        public string assetName = string.Empty;
        [Tooltip("Model import options")]
        public AsImpL.ImportOptions importOptions = null;
    }
}
