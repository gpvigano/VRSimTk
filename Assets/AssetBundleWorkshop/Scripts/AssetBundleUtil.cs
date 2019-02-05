using System;
using UnityEngine;
#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using UnityEditor;
using UnityEditor.Callbacks;
using AssetBundles;
using EditorTkEx;
#endif

namespace AssetBundleWorkshop
{
    /// <summary>
    /// Asset bundles utility 
    /// </summary>
    /// <remarks>This implementation comes from BuildScript.cs in https://github.com/agentc0re/AssetBundle_Manager_5.5.0 </remarks>
    public class AssetBundleUtil
    {
#if UNITY_EDITOR
        public static string overloadedDevelopmentServerURL = "";

        public static string lastTargetBuildName = null;
        public static string lastTargetBuildFolder = "";

        private static bool runAfterBuild = false;

        [MenuItem("AssetBundleWorkshop/Create asset bundle from selection", false)]
        private static void CreateAssetBundle()
        {
            throw new NotImplementedException();
        }
        [MenuItem("AssetBundleWorkshop/Create asset bundle from selection", true)]
        private static bool ValidateCreateAssetBundle()
        {
            return Selection.activeObject as GameObject != null;
        }

        /// <summary>
        /// Create the asset bundle directory according to the current settings.
        /// </summary>
        /// <param name="target">target platform</param>
        /// <param name="targetFolder">target folder</param>
        /// <returns></returns>
        static private string CreateAssetBundleDirectory(BuildTarget target, string targetFolder)
        {
            string outputPath = null;
            // Choose the output path according to the build target.
            switch (target)
            {
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    if (AssetBundleSettings.AssetBundlesBasePath == AssetBundleSettings.BasePathType.None)
                    {
                        outputPath = AssetBundleSettings.AssetBundlesPath;
                    }
                    else
                    {
                        //switch(AssetBundleSettings.BasePathType)
                        outputPath = Path.Combine(targetFolder, AssetBundleSettings.AssetBundlesPath);
                    }
                    break;
                case BuildTarget.StandaloneOSXIntel:
                case BuildTarget.StandaloneOSXIntel64:
                case BuildTarget.StandaloneOSXUniversal:
                //return "/" + targetName + ".app";
                case BuildTarget.Android:
                //return "/" + targetName + ".apk";
#if !UNITY_5_4_OR_NEWER
                case BuildTarget.WebPlayer:
                case BuildTarget.WebPlayerStreamed:
#endif
                case BuildTarget.WebGL:
                case BuildTarget.iOS:
                //return "";
                // Add more build targets for your own.
                default:
                    Debug.Log("Target not implemented.");
                    return null;
            }
            if (!Directory.Exists(outputPath))
            {
                Debug.Log("Creating directory " + outputPath);
                Directory.CreateDirectory(outputPath);
            }

            return outputPath;
        }

        /// <summary>
        /// Build the asset bundles for the given target platform in the given (root) folder).
        /// </summary>
        /// <param name="target">target platform</param>
        /// <param name="targetFolder">target (root) folder</param>
        public static void BuildAssetBundles(BuildTarget target, string targetFolder)
        {
            // Choose the output path according to the build target.
            string outputPath = CreateAssetBundleDirectory(target, targetFolder);

            var options = BuildAssetBundleOptions.ChunkBasedCompression;

            bool shouldCheckODR = EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS;
#if UNITY_TVOS
            shouldCheckODR |= EditorUserBuildSettings.activeBuildTarget == BuildTarget.tvOS;
#endif
            if (shouldCheckODR)
            {
#if ENABLE_IOS_ON_DEMAND_RESOURCES
                if (PlayerSettings.iOS.useOnDemandResources)
                    options |= BuildAssetBundleOptions.UncompressedAssetBundle;
#endif
#if ENABLE_IOS_APP_SLICING
                options |= BuildAssetBundleOptions.UncompressedAssetBundle;
#endif
            }

            //@TODO: use append hash... (Make sure pipeline works correctly with it.)
            BuildPipeline.BuildAssetBundles(outputPath, options, EditorUserBuildSettings.activeBuildTarget);
        }

        /// <summary>
        /// Write the Server URL to file
        /// </summary>
        public static void WriteServerURL()
        {
            string downloadURL;
            if (string.IsNullOrEmpty(overloadedDevelopmentServerURL) == false)
            {
                downloadURL = overloadedDevelopmentServerURL;
            }
            else
            {
                IPHostEntry host;
                string localIP = "";
                host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (IPAddress ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        localIP = ip.ToString();
                        break;
                    }
                }
                downloadURL = "http://" + localIP + ":7888/";
            }

            string assetBundleManagerResourcesDirectory = "Assets/AssetBundleManager/Resources";
            string assetBundleUrlPath = Path.Combine(assetBundleManagerResourcesDirectory, "AssetBundleServerURL.bytes");
            Directory.CreateDirectory(assetBundleManagerResourcesDirectory);
            File.WriteAllText(assetBundleUrlPath, downloadURL);
            AssetDatabase.Refresh();
        }

        [MenuItem("AssetBundleWorkshop/Build app...")]
        public static void BuildPlayerTo()
        {
            runAfterBuild = false;
            BuildPlayer();
        }

        [MenuItem("AssetBundleWorkshop/Build and run...")]
        public static void BuildAndRunPlayerTo()
        {
            runAfterBuild = true;
            BuildPlayer();
        }

        /// <summary>
        /// Build the application using the active build target
        /// </summary>
        public static void BuildPlayer()
        {
            string outputPath = BuildTk.SelectBuildPath(ref lastTargetBuildFolder, ref lastTargetBuildName);
            if (outputPath == null)
            {
                return;
            }

            // Build and copy AssetBundles.
            BuildAssetBundles(EditorUserBuildSettings.activeBuildTarget, lastTargetBuildFolder);
            WriteServerURL();
            BuildTk.BuildPlayer(outputPath, runAfterBuild);
        }

        /// <summary>
        /// Get the base path for asset bundles. Fall back to PersistentData in case the selected mode is not available on the current device.
        /// </summary>
        /// <returns></returns>
        public static string GetAssetBundlesBuildPath(BuildTarget? target = null)
        {
            if (!target.HasValue)
            {
                target = EditorUserBuildSettings.activeBuildTarget;
            }
            bool isStandalone = false;
            bool isLinux = false;
            bool isWindows = false;
            bool isOSX = false;
            switch (target.Value)
            {
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    isWindows = true;
                    isStandalone = true;
                    break;
                case BuildTarget.StandaloneLinux:
                case BuildTarget.StandaloneLinux64:
                case BuildTarget.StandaloneLinuxUniversal:
                    isLinux = true;
                    isStandalone = true;
                    break;
                case BuildTarget.StandaloneOSXIntel:
                case BuildTarget.StandaloneOSXIntel64:
                case BuildTarget.StandaloneOSXUniversal:
                    isOSX = true;
                    isStandalone = true;
                    break;
            }
            switch (AssetBundleSettings.AssetBundlesBasePath)
            {
                case AssetBundleSettings.BasePathType.None:
                    return string.Empty;
                case AssetBundleSettings.BasePathType.StreamingAssets:
                    return Application.streamingAssetsPath;
                case AssetBundleSettings.BasePathType.Data:
                    if (isStandalone)
                    {
                        return Application.dataPath;
                    }
                    else
                    {
                        return Application.persistentDataPath;
                    }
                case AssetBundleSettings.BasePathType.PersistentData:
                    return Application.persistentDataPath;
                case AssetBundleSettings.BasePathType.Executable:
                    if (Application.isEditor)
                    {
                        // return the project path
                        return Application.dataPath + "/../";
                    }
                    else if (!isStandalone)
                    {
                        return Application.persistentDataPath;
                    }
                    else if (isOSX)
                    {
                        return Application.dataPath + "/../../";
                    }
                    else if (isWindows && isLinux)
                    {
                        return Application.dataPath + "/../";
                    }
                    break;
            }
            return Application.persistentDataPath;
        }
#endif
    }
}
