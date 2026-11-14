using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using NoiteSemFim;

[InitializeOnLoad]
public static class Final25DTestLauncher {
 const string Pending="NSF.PendingFinal25DTest";
 static Final25DTestLauncher(){EditorApplication.playModeStateChanged+=OnPlayMode;}
 [MenuItem(GameIdentity.Title+"/Testar fase final 2.5D")]
 public static void Open(){
  if(EditorApplication.isPlaying){Launch();return;}
  SessionState.SetBool(Pending,true);
  TestVersionLauncher.Open();
  if(!EditorApplication.isPlayingOrWillChangePlaymode)EditorApplication.delayCall+=CheckPending;
 }
 static void CheckPending(){if(!EditorApplication.isPlayingOrWillChangePlaymode)SessionState.SetBool(Pending,false);}
 static void OnPlayMode(PlayModeStateChange mode){if(mode==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Pending,false))EditorApplication.delayCall+=Launch;}
 static void Launch(){
  SessionState.SetBool(Pending,false);
  var game=UnityEngine.Object.FindFirstObjectByType<SurvivorGame>();
  if(game==null){Debug.LogError("Instância do jogo não encontrada para testar a fase final.");return;}
  game.StartFinal3DDemo();
  var view=Type.GetType("UnityEditor.GameView,UnityEditor");if(view!=null)EditorWindow.GetWindow(view).Focus();
  File.WriteAllText("TESTE-FINAL-25D.txt","Arena do Eclipse • teste direto da fase final 2.5D\nBase visual recuperada: v0.24 (pixel art + cenário 3D).\nPersonagem selecionado, nível 12, vida 500, armas e artefatos de teste.\nSem moedas ou desbloqueios concedidos pelo combate deste teste.\nProjeto: "+Path.GetFullPath(".")+"\nAberto em UTC: "+DateTime.UtcNow.ToString("O"));
  Debug.Log("FINAL 2.5D TESTE ABERTO: Arena do Eclipse, pixel art + cenário 3D, Soberano.");
 }
}
