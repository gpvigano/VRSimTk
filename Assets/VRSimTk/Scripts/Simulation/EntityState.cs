using UnityEngine;
using System;

namespace VRSimTk
{
    [System.Serializable]
    public class EntityState
    {
#if UNITY_EDITOR
        public string state = string.Empty;
#endif
        public string origin;
        public DateTime startTime;
        public DateTime endTime;
        public Vector3 position = Vector3.zero;
#if UNITY_EDITOR
        public Vector3 eulerAngles = Vector3.zero;
#endif
        public Quaternion rotation = Quaternion.identity;
        public Transform parentTransform = null;
    }
}