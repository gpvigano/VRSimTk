#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Globalization;
using EditorTkEx;

namespace VRSimTk
{
    [CustomEditor(typeof(EntityHistory))]
    ////[CanEditMultipleObjects]
    public class EntityHistoryEditor : Editor
    {
        private static GUIContent positionGUIContent = new GUIContent("Position", "The local position of this Game Object relative to the parent.");
        private static GUIContent rotationGUIContent = new GUIContent("Rotation", "The local rotation of this Game Object relative to the parent.");
        private bool foldOutStates = true;

        public override void OnInspectorGUI()
        {
            // Update the serializedProperty - always do this in the beginning of OnInspectorGUI.
            serializedObject.Update();
            DrawDefaultInspector();
            GUILayout.Space(10);
            GUIStyle guiStyle = EditorStyles.foldout;
            FontStyle previousStyle = guiStyle.fontStyle;
            guiStyle.fontStyle = FontStyle.Bold;

            EntityHistory history = target as EntityHistory;
            GUI.enabled = false;
            DateTimeTk.DrawDateTime("Start Time", history.historyStartTime);
            DateTimeTk.DrawDateTime("End Time", history.historyEndTime);
            DateTimeTk.DrawTimeSpan("Offset", history.StartOffset);
            DateTimeTk.DrawTimeSpan("Duration", history.Duration);
            GUI.enabled = true;
            //foldOutStates = foldOutStates && history.entityStates.Length > 0;
            previousStyle = guiStyle.fontStyle;
            guiStyle.fontStyle = FontStyle.Bold;
            foldOutStates = EditorGUILayout.Foldout(foldOutStates, "States", guiStyle);
            guiStyle.fontStyle = previousStyle;
            GUILayout.Space(2);
            if (foldOutStates && history.entityStates!=null)
            {
                EditorGUI.indentLevel++;
                for (int i = 0; i < history.entityStates.Length; i++)
                {
                    EntityState state = history.entityStates[i];
                    EditorGUILayout.LabelField("State " + i + " "+ state.state);
                    EditorGUI.indentLevel++;
                    DrawState(state);
                    EditorGUI.indentLevel--;
                }
                EditorGUI.indentLevel--;
            }

            // Apply changes to the serializedProperty - always do this in the end of OnInspectorGUI.
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawState(EntityState state)
        {
            state.origin = EditorGUILayout.TextField("Origin", state.origin);
            state.startTime = DateTimeTk.DrawDateTime("Start Time", state.startTime);
            state.endTime = DateTimeTk.DrawDateTime("End Time", state.endTime);
            state.position = EditorGUILayout.Vector3Field(positionGUIContent, state.position);
            state.rotation.eulerAngles = EditorGUILayout.Vector3Field(rotationGUIContent, state.rotation.eulerAngles);
        }

    }
    /*
        [CustomEditor(typeof(SimEvent))]
        public class SimEventEditor : Editor
        {


            public override void OnInspectorGUI()
            {
                // Update the serializedProperty - always do this in the beginning of OnInspectorGUI.
                serializedObject.Update();

                SimEvent ev = target as SimEvent;

                ev.URI = EditorGUILayout.TextField("URI", ev.URI);

                // Apply changes to the serializedProperty - always do this in the end of OnInspectorGUI.
                serializedObject.ApplyModifiedProperties();
            }




        }
    */
}
#endif
