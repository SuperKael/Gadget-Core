#if UNITY_EDITOR
using System.IO;
using UnityEditor;

public class AssetBundleCreator {
    [MenuItem("Assets/Build Asset Bundles")]
	public static void BuildBundles()
    {
		string assetBundleDirectory = "Assets/AssetBundles";
        if (!Directory.Exists(assetBundleDirectory))
        {
            Directory.CreateDirectory(assetBundleDirectory);
        }
        BuildPipeline.BuildAssetBundles(assetBundleDirectory, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64);
    }
}
#endif