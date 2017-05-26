#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace EditorTkEx
{
    public class BuildTk
    {
        public static string GetBuildTargetName(BuildTarget target, string targetName)
        {
            switch (target)
            {
                case BuildTarget.Android:
                    return "/" + targetName + ".apk";
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return "/" + targetName + ".exe";
                case BuildTarget.StandaloneOSXIntel:
                case BuildTarget.StandaloneOSXIntel64:
                case BuildTarget.StandaloneOSXUniversal:
                    return "/" + targetName + ".app";
#if !UNITY_5_4_OR_NEWER
                case BuildTarget.WebPlayer:
                case BuildTarget.WebPlayerStreamed:
#endif
                case BuildTarget.WebGL:
                case BuildTarget.iOS:
                    return "";
                // Add more build targets for your own.
                default:
                    Debug.Log("Target not implemented.");
                    return null;
            }
        }

        public static string GetBuildTargetExt(BuildTarget target, string targetName)
        {
            switch (target)
            {
                case BuildTarget.Android:
                    return "apk";
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return "exe";
                case BuildTarget.StandaloneOSXIntel:
                case BuildTarget.StandaloneOSXIntel64:
                case BuildTarget.StandaloneOSXUniversal:
                    return "app";
#if !UNITY_5_4_OR_NEWER
                case BuildTarget.WebPlayer:
                case BuildTarget.WebPlayerStreamed:
#endif
                case BuildTarget.WebGL:
                case BuildTarget.iOS:
                    return "";
                // Add more build targets for your own.
                default:
                    Debug.Log("Target not implemented.");
                    return null;
            }
        }

        public static string SelectBuildPath(ref string lastTargetBuildFolder, ref string lastTargetBuildName)
        {
            if (string.IsNullOrEmpty(lastTargetBuildName))
            {
                lastTargetBuildName = PlayerSettings.productName;
            }
            string targetName = GetBuildTargetName(EditorUserBuildSettings.activeBuildTarget, lastTargetBuildName);
            if (targetName == null)
                return null;
            string outputPath;
            if (targetName == "")
            {
                outputPath = EditorUtility.SaveFolderPanel("Choose location of the built app", lastTargetBuildFolder, lastTargetBuildName);
            }
            else
            {
                outputPath = EditorUtility.SaveFilePanel("Choose location of the built app", lastTargetBuildFolder, targetName.TrimStart('/'), Path.GetExtension(targetName).TrimStart('.'));
            }
            if (outputPath.Length == 0)
                return null;
            lastTargetBuildName = Path.GetFileNameWithoutExtension(outputPath);
            lastTargetBuildFolder = "";
            if (outputPath.Contains("/"))
            {
                int pos = outputPath.LastIndexOf('/');
                lastTargetBuildName = outputPath.Substring(pos + 1);
                lastTargetBuildFolder = outputPath.Substring(0, pos);
            }
            return outputPath;
        }

        public static string[] GetLevelsFromBuildSettings()
        {
            List<string> levels = new List<string>();
            for (int i = 0; i < EditorBuildSettings.scenes.Length; ++i)
            {
                if (EditorBuildSettings.scenes[i].enabled)
                    levels.Add(EditorBuildSettings.scenes[i].path);
            }

            return levels.ToArray();
        }

        public static void BuildPlayer(string outputPath, bool runAfterBuild)
        {
            string[] levels = GetLevelsFromBuildSettings();

            BuildOptions option = EditorUserBuildSettings.development ? BuildOptions.Development : BuildOptions.None;
            if (runAfterBuild)
            {
                option |= BuildOptions.AutoRunPlayer;
            }
            BuildPipeline.BuildPlayer(levels, outputPath, EditorUserBuildSettings.activeBuildTarget, option);
        }

        public static string GetAppDataPath(string targetFolder, string targetName, BuildTarget? target=null)
        {
            if(!target.HasValue)
            {
                target = EditorUserBuildSettings.activeBuildTarget;
            }
            string outputPath = null;

            // Choose the output path according to the build target.
            switch (target.Value)
            {
                // Win/Linux player: <path to executablename_Data folder>
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                case BuildTarget.StandaloneLinux:
                case BuildTarget.StandaloneLinux64:
                case BuildTarget.StandaloneLinuxUniversal:
                    outputPath = targetFolder+"/"+ targetName+"_Data";
                    break;
                // Mac player: <path to player app bundle>/Contents
                case BuildTarget.StandaloneOSXIntel:
                case BuildTarget.StandaloneOSXIntel64:
                case BuildTarget.StandaloneOSXUniversal:
                    outputPath = targetFolder+"/Contents";
                    break;
                //return "/" + targetName + ".app";
//Android: Normally dataPath would point directly to the APK, use persistentDataPath instead.
                case BuildTarget.Android:
                    {
                        string appId = PlayerSettings.bundleIdentifier;//PlayerSettings.productName
                        Debug.LogWarning("Path detection on Android is not yet implemented.\nIt should be (your device)/Phone/Android/data/"+ appId + "/files");
                        //outputPath = devId+"/Phone/Android/data/"+ appId + "/files";
                        return null;
                    }
                //return "/" + targetName + ".apk";
#if !UNITY_5_4_OR_NEWER
                case BuildTarget.WebPlayer:
                case BuildTarget.WebPlayerStreamed:
#endif
//WebGL: The absolute url to the player data file folder (without the actual data file name)
                case BuildTarget.WebGL:
//iOS player: use Application.persistentDataPath to save data).
                case BuildTarget.iOS:
                //return "";
                // Add more build targets
                default:
                    Debug.Log("Target not implemented.");
                    return null;
            }
            return outputPath;
        }
    }
}
#endif
