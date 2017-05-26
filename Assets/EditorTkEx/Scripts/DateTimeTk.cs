#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Globalization;

namespace EditorTkEx
{
    public class DateTimeTk
    {
        static private DateTimeFormatInfo enDTFI = new CultureInfo("en-US", false).DateTimeFormat;

        static public float DrawTimeSpan(string label, float time)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(time);
            TimeSpan newTimeSpan;

            string format = TimeSpan.Zero.ToString();
            bool valid = TimeSpan.TryParse(EditorGUILayout.TextField(label, timeSpan.ToString()), out newTimeSpan);
            if (!valid)
            {
                DrawError("Wrong time format! Use this format:\n" + format);
                return time;
            }
            return (float)newTimeSpan.TotalSeconds;

        }

        static public DateTime DrawDateTime(string label, DateTime dateTime)
        {
            string format = "dd/MM/yyyy hh:mm:ss";
            DateTime newDateTime;
            bool valid = DateTime.TryParseExact(EditorGUILayout.TextField(label, dateTime.ToString(format)), format, enDTFI, DateTimeStyles.None, out newDateTime);

            if (!valid)
            {
                DrawError("Wrong date-time format! Use this format: " + format);
                //Debug.LogError("Wrong date-time format! Use this format:\n" + format);
                //EditorGUI.indentLevel+=12;
                //EditorGUILayout.HelpBox("Wrong date-time format! Use this format: " + format, MessageType.Error);
                //EditorGUI.indentLevel-=12;
                return dateTime;
            }
            else
            {
                //EditorGUILayout.HelpBox("Format: " + format, MessageType.None);
                return newDateTime;
            }
        }

        static private void DrawError(string message)
        {
            Debug.LogError(message);
            //EditorGUILayout.HelpBox(message, MessageType.Error);
        }
    }
}
 
#endif
