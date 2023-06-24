using System.IO;
using UnityEditor;
using UnityEngine;

public class BuildAssetBundleEditor
{
    [MenuItem("Tools/Build AssetBundles")]
    static void BuildAllAssetBundles()
    {
        string folderName = "AssetBundles";
        string filePath = Path.Combine(Application.streamingAssetsPath, folderName);
        if (!Directory.Exists(filePath))
        {
            Directory.CreateDirectory(filePath);
        }
        BuildPipeline.BuildAssetBundles(filePath,
                                        BuildAssetBundleOptions.None,
                                        EditorUserBuildSettings.activeBuildTarget);
    }

}
