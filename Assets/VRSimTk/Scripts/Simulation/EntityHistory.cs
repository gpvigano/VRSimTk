using UnityEngine;
using System;
using System.Collections.Generic;

namespace VRSimTk
{
    public class EntityHistory : MonoBehaviour
    {
        public string historyFileName;
        public DateTime historyStartTime;
        public DateTime historyEndTime;
        [HideInInspector]
        public EntityState[] entityStates = null;
        public bool sourceUpAxisIsZ = false;

        private float historyStartOffset = 0;
        private float historyDuration;

        public float Duration { get { return historyDuration; } }
        public float StartOffset { get { return historyStartOffset; } }

        public void ImportSimLog()
        {
            SimLogParser parser = new SimLogParser();
            parser.ParseFile(historyFileName, sourceUpAxisIsZ);
            historyStartTime = parser.historyStartTime;
            historyEndTime = parser.historyEndTime;
            historyDuration = (parser.historyEndTime - historyStartTime).Ticks / TimeSpan.TicksPerSecond;
            List<SimLogRecord> history = parser.history;
            entityStates = new EntityState[history.Count];
            int i = 0;
            foreach (var record in history)
            {
                EntityState simKf = new EntityState();
                simKf.startTime = record.startTime;// (record.startTime - historyStartTime).Ticks / TimeSpan.TicksPerSecond;
                simKf.endTime = record.endTime;//(record.endTime - historyStartTime).Ticks / TimeSpan.TicksPerSecond;
                simKf.origin = record.origin;
                simKf.position = record.position;
                simKf.rotation = CsConv.RotMatToQuat(record.rotMatrix, sourceUpAxisIsZ);
#if UNITY_EDITOR
                simKf.eulerAngles = simKf.rotation.eulerAngles;
#endif
                EntityData parent_entity = DataUtil.FindEntity(record.parentId);
                simKf.parentTransform = parent_entity ? parent_entity.transform : null;
                entityStates[i] = simKf;
                i++;
            }
        }

        public EntityState FindKeyFrame(DateTime time, out EntityState prevState, out EntityState nextState)
        {
            // binary search of the status inside the history
            int first = 0;
            int last = entityStates.Length - 1;
            int middle = (first + last) / 2;
            prevState = null;
            nextState = null;
            EntityState foundState = null;
            if (entityStates.Length == 0)
            {
                return null;
            }
            int infiniteLoopCheckCounter = 0;
            while (first <= last)
            {
                infiniteLoopCheckCounter++;
                if (infiniteLoopCheckCounter > entityStates.Length)
                {
                    throw new Exception("Infinite loop detected (first = " + first + ", last = " + last + ")");
                }
                if (time >= entityStates[middle].startTime && time <= entityStates[middle].endTime)
                {
                    foundState = entityStates[middle];
                    prevState = (middle > 0) ? entityStates[middle - 1] : null;
                    nextState = (middle < entityStates.Length - 1) ? entityStates[middle + 1] : null;
                    break;
                }
                else if (first == last)
                {
                    if (time > entityStates[first].endTime)
                    {
                        prevState = entityStates[first];
                        nextState = (middle < entityStates.Length - 1) ? entityStates[middle + 1] : null;
                    }
                    if (time < entityStates[first].startTime)
                    {
                        prevState = (middle > 0) ? entityStates[middle - 1] : null;
                        nextState = entityStates[middle];
                    }
                    break;
                }
                if (time > entityStates[middle].endTime)
                {
                    first = middle + 1;
                }
                else if (time < entityStates[middle].startTime)
                {
                    last = middle - 1;
                }
                middle = (first + last) / 2;
            }

            if (time > entityStates[entityStates.Length - 1].endTime)
            {
                prevState = entityStates[entityStates.Length - 1];
                nextState = null;
            }
            if (time < entityStates[0].startTime)
            {
                nextState = entityStates[0];
                prevState = null;
            }
            return foundState;
        }
    }
}