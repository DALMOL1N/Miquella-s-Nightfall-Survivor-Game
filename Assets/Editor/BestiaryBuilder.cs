using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using NoiteSemFim;
public static class BestiaryBuilder {
 [MenuItem(GameIdentity.Title+"/Verificar bestiário dos quatro mapas")]
 public static void Verify(){
  if(EditorApplication.isPlaying){Debug.LogWarning("Pare o Play para verificar o bestiário.");return;}
  var game=UnityEngine.Object.FindFirstObjectByType<SurvivorGame>();if(game==null)game=new GameObject("Bestiary Verification").AddComponent<SurvivorGame>();
  typeof(SurvivorGame).GetMethod("Awake",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(game,null);
  string result=game.VerifyBiomeBestiary();File.WriteAllText("VERIFICACAO-BESTIARIO.txt",DateTime.UtcNow.ToString("O")+"\n"+result);Debug.Log(result);
 }
 [MenuItem(GameIdentity.Title+"/Capturar novos inimigos e chefes (em Play)")]
 public static void Preview(){if(!EditorApplication.isPlaying)return;UnityEngine.Object.FindFirstObjectByType<SurvivorGame>()?.PreviewBestiary();}
}
