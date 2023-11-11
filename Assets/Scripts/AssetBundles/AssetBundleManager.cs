using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public static class AssetBundleManager
{
    static private Dictionary<string, AssetBundle> dictAssetBundle;
    static AssetBundleManager()
    {
        dictAssetBundle = new Dictionary<string, AssetBundle>();
    }

    private class AssetBundleRef
    {
        public AssetBundle assetBundle = null;
        public string path;
        public AssetBundleRef(string inputPath)
        {
            path = inputPath;
        }
    }

    public static AssetBundle GetAssetBundle(string path)
    {
        if (dictAssetBundle.ContainsKey(path))
        {
            return dictAssetBundle[path];
        }
        return null;
    }

    public static AssetBundle LoadAssetBundle(string path)
    {
        if (dictAssetBundle.ContainsKey(path))
        {
            return dictAssetBundle[path];
        }
        var assetBundle = AssetBundle.LoadFromFile(
            Path.Combine(Application.streamingAssetsPath, path));
        dictAssetBundle.Add(path, assetBundle);
        return assetBundle;
    }

    public static void UnloadAssetBundle(string path)
    {
        if (dictAssetBundle.ContainsKey(path))
        {
            dictAssetBundle[path].Unload(false);
            dictAssetBundle.Remove(path);
        }
    }
}
