using UnityEngine;
using System;
using System.Collections.Generic;

namespace VRSimTk
{
    [Serializable]
    public class SimHistory
    {
        public string historyUri;
        public string historyName;
        public string description;
        [Multiline]
        public string details;
        public DateTime startTime;
        public DateTime endTime;
        public List<EntityHistory> entityHistoryList;
        public List<SimEvent> eventList;
    }
}
