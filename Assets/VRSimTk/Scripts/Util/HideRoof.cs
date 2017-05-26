using UnityEngine;
using System.Collections;

namespace VRSimTk
{
    public class HideRoof : MonoBehaviour
    {
        [Tooltip("Object to hide (if null = try to hide the mesh in this object)")]
        public GameObject objectToHide;
        [Tooltip("Observer position (if null = Main Camera)")]
        public Transform observerTransform;
        [Tooltip("Offset bserver position (if null = Main Camera)")]
        public float verticalOffset = 0f;
        private MeshFilter meshFilter = null;
        private MeshRenderer meshRenderer = null;
        private float meshVerticalOffset = 0f;

        void Awake()
        {
            if (objectToHide == null)
            {
                meshRenderer = GetComponent<MeshRenderer>();
                meshFilter = GetComponent<MeshFilter>();
                if (meshRenderer && meshFilter)
                {
                    meshVerticalOffset = transform.TransformVector(meshFilter.mesh.bounds.extents).y;
                }
                else
                {
                    Debug.LogWarning(GetType() + " component needs an object or a mesh to hide");
                }
            }
            if (observerTransform == null)
            {
                observerTransform = Camera.main.transform;
            }
        }

        void Update()
        {
            if (observerTransform)
            {
                float offset = observerTransform.position.y - transform.position.y;
                if (meshRenderer)
                {
                    meshRenderer.enabled = offset < meshVerticalOffset + verticalOffset;
                }
                else if (objectToHide)
                {
                    objectToHide.SetActive(offset < verticalOffset);
                }
            }
        }
    }
}