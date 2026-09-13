#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class BuildTestHelper
{
    private static readonly string[] Scenes = new[]
    {
        "Assets/Scenes/Main menu.unity",
        "Assets/Scenes/level1.unity",
        "Assets/Scenes/Level2.unity",
        "Assets/Scenes/Level3.unity"
    };

    [MenuItem("TimeLoop/Build/Build Mac Standalone")]
    public static void BuildMac()
    {
        string outDir = "Builds/Mac";
        if (!Directory.Exists(outDir)) Directory.CreateDirectory(outDir);
        string appPath = Path.Combine(outDir, "TimeLoop.app");

        BuildPlayerOptions opts = new BuildPlayerOptions
        {
            scenes = Scenes,
            locationPathName = appPath,
            target = BuildTarget.StandaloneOSX,
            options = BuildOptions.None
        };

        Debug.Log("[BuildTestHelper] Starting Mac Standalone build to: " + appPath);
        BuildReport report = BuildPipeline.BuildPlayer(opts);
        Debug.Log($"[BuildTestHelper] Mac Standalone build completed with result: {report.summary.result}, Total Errors: {report.summary.totalErrors}, Size: {report.summary.totalSize} bytes");
    }

    [MenuItem("TimeLoop/Build/Build WebGL")]
    public static void BuildWebGL()
    {
        string outDir = "Builds/WebGL";
        if (!Directory.Exists(outDir)) Directory.CreateDirectory(outDir);

        BuildPlayerOptions opts = new BuildPlayerOptions
        {
            scenes = Scenes,
            locationPathName = outDir,
            target = BuildTarget.WebGL,
            options = BuildOptions.None
        };

        Debug.Log("[BuildTestHelper] Starting WebGL build to: " + outDir);
        BuildReport report = BuildPipeline.BuildPlayer(opts);
        Debug.Log($"[BuildTestHelper] WebGL build completed with result: {report.summary.result}, Total Errors: {report.summary.totalErrors}, Size: {report.summary.totalSize} bytes");
    }
}
#endif
