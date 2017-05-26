using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;

namespace VRSimTk
{
    [System.Serializable]
    public class SimLogRecord
    {
        public DateTime startTime;
        public DateTime endTime;
        public string origin;
        public Vector3 position;
        public Matrix4x4 rotMatrix;
        public string parentId;
    }

    public class SimLogParser
    {
        public DateTime historyStartTime;
        public DateTime historyEndTime;
        public List<SimLogRecord> history = new List<SimLogRecord>();

        private bool ParseStream(StreamReader stream, bool zUp)
        {
            int lineCount = 0;
            DateTimeFormatInfo enUsDTFI = new CultureInfo("en-US", false).DateTimeFormat;
            while (!stream.EndOfStream)// && lineCount < 10)
            {
                string line = stream.ReadLine();
                lineCount++;
                char[] delimiter = { ' ', '\t' };
                string[] tokens = line.Split(delimiter, StringSplitOptions.RemoveEmptyEntries);
                if(tokens.Length==0)
                {
                    break;
                }
                if (tokens.Length != 17)
                {
                    Debug.LogWarningFormat("{0} line values in line {1}", tokens.Length, lineCount);
                    for (int c = 0; c<tokens.Length; c++)
                    {
                        var t = tokens[c];
                        Debug.LogFormat("{0} {1}", c, t);
                    }
                    return false;
                }
                int i = 0;
                SimLogRecord state = new SimLogRecord();
                state.startTime = Convert.ToDateTime(tokens[i++], enUsDTFI);
                if (history.Count == 0)
                {
                    historyStartTime = state.startTime;
                }
                state.endTime = Convert.ToDateTime(tokens[i++], enUsDTFI);
                if (historyEndTime<state.endTime)
                {
                    historyEndTime = state.endTime;
                }
                state.origin = tokens[i++];
                Vector3 pos = Vector3.zero;
                pos.x = float.Parse(tokens[i++]);
                pos.y = float.Parse(tokens[i++]);
                pos.z = float.Parse(tokens[i++]);
                float scaleToMeter = float.Parse(tokens[i++]);
                state.position = CsConv.VecToVecRL(pos * scaleToMeter, zUp);
                Matrix4x4 rotMat = Matrix4x4.identity;
                for (int n = 0; n < 9; n++)
                {
                    rotMat[n % 3, n / 3] = float.Parse(tokens[i++]);
                }
                state.rotMatrix = rotMat;
                state.parentId = tokens[i++];
                history.Add(state);
            }
            return true;
        }

        public void ParseFile(string animationFileName, bool zUp)
        {
#if (UNITY_ANDROID || UNITY_IPHONE) && !UNITY_EDITOR
            string data_path = Application.persistentDataPath;
#else
            string data_path = Application.dataPath;
#endif
            string simFilePath = data_path + "/" + animationFileName;

            FileInfo simPath = new FileInfo(simFilePath);
            if (simPath.Exists)
            {
                StreamReader str = simPath.OpenText();
                ParseStream(str,zUp);
                str.Close();
                Debug.LogFormat("File {0} parsed.", simPath.FullName);
            }
            else
            {
                Debug.LogWarning("Unable to open file " + simPath.FullName);
            }
        }
    }
}
