using UnityEditor;
using System.IO;
using System.Collections.Generic;
using Spine.Unity.Editor;

public class BuildSkeletonDataAsset
{
    [MenuItem ("Assets/Create SkeletonDataAsset", false, 100)]
    static private void CreateSkeletonDataAsset ()
	{
        List<string> filesPath = new List<string>();

        UnityEngine.Object[] objects = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.DeepAssets);
        foreach (var item in objects)
        {
            string path = AssetDatabase.GetAssetPath(item);
            if(!Directory.Exists(path))
            {
                filesPath.Add(path);
            }
        }

        SpineEditorUtilities.ImportSpineContent(filesPath.ToArray());
    }
}
