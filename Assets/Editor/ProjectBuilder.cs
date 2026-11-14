using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using UnityEngine;
using NoiteSemFim;
public static class ProjectBuilder {
 [MenuItem(NoiteSemFim.GameIdentity.Title + "/Preparar cena")]
 public static void Prepare(){
  Directory.CreateDirectory("Assets/Scenes");var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);new GameObject("Game").AddComponent<SurvivorGame>();EditorSceneManager.SaveScene(scene,"Assets/Scenes/NoiteSemFim.unity");EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/Scenes/NoiteSemFim.unity",true)};
  PlayerSettings.bundleVersion="0.30.0";PlayerSettings.companyName="Noite Studio";PlayerSettings.productName=GameIdentity.Title;PlayerSettings.defaultScreenWidth=1280;PlayerSettings.defaultScreenHeight=720;PlayerSettings.fullScreenMode=FullScreenMode.FullScreenWindow;PlayerSettings.runInBackground=false;PlayerSettings.colorSpace=ColorSpace.Gamma;
  AssetDatabase.SaveAssets();Debug.Log("PROJECT PREPARED");
 }
 [MenuItem(NoiteSemFim.GameIdentity.Title + "/Verificar gameplay")]
 public static void Verify(){TestVersionLauncher.VerifyProfile();var g=UnityEngine.Object.FindFirstObjectByType<SurvivorGame>();if(g==null)g=new GameObject("Verification").AddComponent<SurvivorGame>();var awake=typeof(SurvivorGame).GetMethod("Awake",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance);awake.Invoke(g,null);string result=g.VerifyGameplay();File.WriteAllText("VERIFICACAO.txt",result);File.WriteAllText("ANIMACOES-AUDITORIA.csv",ArtBank.LastAuditCsv);Debug.Log(result);}
 [MenuItem(NoiteSemFim.GameIdentity.Title + "/Gerar aplicativo Mac")]
 public static void BuildMac(){Prepare();Verify();string root=Path.GetFullPath("../Build-Mac/"+GameIdentity.FileName+".app");Directory.CreateDirectory(Path.GetDirectoryName(root));var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/Scenes/NoiteSemFim.unity"},locationPathName=root,target=BuildTarget.StandaloneOSX,options=BuildOptions.None});if(result.summary.result!=BuildResult.Succeeded)throw new Exception("Build failed: "+result.summary.result);Debug.Log("BUILD SUCCESS: "+root);}
}
