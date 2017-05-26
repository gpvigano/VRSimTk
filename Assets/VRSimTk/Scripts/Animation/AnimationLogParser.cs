using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Xml.Serialization;

namespace VRSimTk
{
    [System.Serializable]
    public class AnimationRecord
    {
        public DateTime startTime;
        public DateTime endTime;
        public string origin;
        public Vector3 position;
        public Matrix4x4 rotMatrix;
        public string parentId;
    }

    public class AnimationLogParser : MonoBehaviour
    {
        public string animationFileName;
        public float timeScaling = 1f;
        public bool origUpAxisIsZ = false;

        public DateTime historyStartTime;
        public List<AnimationRecord> history = new List<AnimationRecord>();
        Animation anim;

        bool ParseFile(StreamReader stream)
        {
            int lineCount = 0;
            DateTimeFormatInfo myDTFI = new CultureInfo("en-US", false).DateTimeFormat;
            while (!stream.EndOfStream && lineCount < 10)
            {
                string line = stream.ReadLine();
                lineCount++;
                char[] delimiter = { ' ', '\t' };
                string[] tokens = line.Split(delimiter, StringSplitOptions.RemoveEmptyEntries);
                if (tokens.Length != 17)
                {
                    Debug.LogWarningFormat("{0} line values in line {1}", tokens.Length, lineCount);
                    int c = 0;
                    foreach (var t in tokens)
                    {
                        c++;
                        Debug.LogFormat("{0} {1}", c, t);
                    }
                    return false;
                }
                int i = 0;
                AnimationRecord state = new AnimationRecord();
                state.startTime = Convert.ToDateTime(tokens[i++], myDTFI);
                if (history.Count == 0)
                {
                    historyStartTime = state.startTime;
                }
                state.endTime = Convert.ToDateTime(tokens[i++], myDTFI);
                state.origin = tokens[i++];
                Vector3 pos = Vector3.zero;
                pos.x = float.Parse(tokens[i++]);
                pos.y = float.Parse(tokens[i++]);
                pos.z = float.Parse(tokens[i++]);
                float scaleToMeter = float.Parse(tokens[i++]);
                state.position = CsConv.VecToVecRL(pos * scaleToMeter, origUpAxisIsZ);
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

        void Awake()
        {
#if (UNITY_ANDROID || UNITY_IPHONE)
            string data_path = Application.persistentDataPath;
#else
            string data_path = Application.dataPath;
#endif
            string cfg_file_path = data_path + "/" + animationFileName;

            FileInfo cfgPath = new FileInfo(cfg_file_path);
            if (cfgPath.Exists)
            {
                StreamReader str = cfgPath.OpenText();
                ParseFile(str);
                str.Close();
            }
        }

        void Start()
        {
            anim = gameObject.GetComponent<Animation>();

            if (anim == null)
            {
                anim = gameObject.AddComponent<Animation>();
            }
            AnimationCurve curve_pos_x = new AnimationCurve();
            AnimationCurve curve_pos_y = new AnimationCurve();
            AnimationCurve curve_pos_z = new AnimationCurve();
            foreach (var record in history)
            {
                float time = (record.startTime - historyStartTime).Ticks / TimeSpan.TicksPerSecond;
                float t = time * timeScaling;
                curve_pos_x.AddKey(new Keyframe(t, record.position.x, 0, 0));
                curve_pos_y.AddKey(new Keyframe(t, record.position.y, 0, 0));
                curve_pos_z.AddKey(new Keyframe(t, record.position.z, 0, 0));
            }
            AnimationClip clip = new AnimationClip();
            clip.name = "position";
            clip.legacy = true;
            clip.SetCurve("", typeof(Transform), "localPosition.x", curve_pos_x);
            clip.SetCurve("", typeof(Transform), "localPosition.y", curve_pos_y);
            clip.SetCurve("", typeof(Transform), "localPosition.z", curve_pos_z);
            anim.wrapMode = WrapMode.Loop;
            //anim.clip = clip;
            //anim.Play(PlayMode.StopAll);
            anim.AddClip(clip, clip.name);
            anim.Play(clip.name, PlayMode.StopAll);
        }

        void Update()
        {
        }
    }
}
