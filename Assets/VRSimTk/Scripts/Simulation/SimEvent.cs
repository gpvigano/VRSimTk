using UnityEngine;
using System;

namespace VRSimTk
{
    [Serializable]
    public class SimEvent
    {
        public string URI;
        public string Category;

        public DateTime Time;
    }
}