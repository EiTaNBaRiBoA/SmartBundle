using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class BundleCreator
{
    //TODO Improve editor
    [MenuItem("Assets/BuildBundles")]
    private static void BuildAllAssetBundles()
    {
        //if Want deploy use Application.persistentDataPath 
        string assetBundlePath = Application.dataPath + "/../AssetsBundles";
        string path = Path.GetFullPath(assetBundlePath).Replace('/', '\\');
        Debug.Log(path);
        Debug.Log(Directory.Exists(path));
        if (!Directory.Exists((path)))
        {
            Debug.Log("doesn't exist");
            Directory.CreateDirectory(path);
        }
        try
        {
            BuildPipeline.BuildAssetBundles(assetBundlePath, BuildAssetBundleOptions.None, EditorUserBuildSettings.activeBuildTarget);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }
}
