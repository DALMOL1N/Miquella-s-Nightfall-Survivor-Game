using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class PlaytestBuilder
{
    [MenuItem(NoiteSemFim.GameIdentity.Title + "/Gerar playtest Windows")]
    public static void BuildWindows()
    {
        if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64))
            throw new InvalidOperationException("Instale Windows Build Support (Mono) pelo Unity Hub.");

        ProjectBuilder.Prepare();
        ProjectBuilder.Verify();
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.StandaloneWindows64, false);
        PlayerSettings.SetGraphicsAPIs(BuildTarget.StandaloneWindows64, new[] { UnityEngine.Rendering.GraphicsDeviceType.Direct3D11 });
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone, ScriptingImplementation.Mono2x);
        string output = Path.GetFullPath("../../Playtests-Sem-Codigo/Miquellas-Nightfall-Survivor-Windows/" + NoiteSemFim.GameIdentity.FileName + ".exe");
        Directory.CreateDirectory(Path.GetDirectoryName(output));
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Scenes/NoiteSemFim.unity" },
            locationPathName = output,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        });
        if (report.summary.result != BuildResult.Succeeded)
            throw new Exception("Windows playtest build failed: " + report.summary.result);
        Debug.Log("WINDOWS PLAYTEST BUILD SUCCESS: " + output);
    }
}
