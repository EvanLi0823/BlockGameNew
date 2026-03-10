using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using Beebyte.Obfuscator;

public class ObfuscatorPreBuildPlayer : IPreprocessBuildWithReport
{
 

    int IOrderedCallback.callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        Debug.Log("Pre build ");
        Debug.Log( report.ToString());
    }

}

class ObfuscatorPostBuildPlayer : IPostprocessBuildWithReport
{
    public int callbackOrder => 1000;

    public void OnPostprocessBuild(BuildReport report)
    {
        Debug.Log("post build");
        Debug.Log(report.ToString());
    }
}
