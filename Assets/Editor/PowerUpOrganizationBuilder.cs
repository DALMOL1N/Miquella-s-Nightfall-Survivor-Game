using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using NoiteSemFim;
public static class PowerUpOrganizationBuilder {
 [MenuItem(GameIdentity.Title+"/Verificar Power Up organizado")]
 public static void Verify(){if(EditorApplication.isPlaying)return;var g=UnityEngine.Object.FindFirstObjectByType<SurvivorGame>();if(g==null)g=new GameObject("Power Up verification").AddComponent<SurvivorGame>();typeof(SurvivorGame).GetMethod("Awake",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(g,null);var result=g.VerifyOrganizedPowerUp();File.WriteAllText("VERIFICACAO-POWER-UP.txt",DateTime.UtcNow.ToString("O")+"\n"+result);Debug.Log(result);}
 [MenuItem(GameIdentity.Title+"/Capturar Power Up organizado (em Play)")]
 public static void Preview(){if(EditorApplication.isPlaying)UnityEngine.Object.FindFirstObjectByType<SurvivorGame>()?.PreviewOrganizedPowerUp();}
}
