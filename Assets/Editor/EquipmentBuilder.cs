using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using NoiteSemFim;
public static class EquipmentBuilder {
 [MenuItem(GameIdentity.Title+"/Verificar armaduras e arsenal")]
 public static void Verify(){if(EditorApplication.isPlaying){Debug.LogWarning("Pare o Play para verificar equipamentos.");return;}var g=UnityEngine.Object.FindFirstObjectByType<SurvivorGame>();if(g==null)g=new GameObject("Equipment verification").AddComponent<SurvivorGame>();typeof(SurvivorGame).GetMethod("Awake",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(g,null);var result=g.VerifyEquipmentBuilds();File.WriteAllText("VERIFICACAO-EQUIPAMENTOS.txt",DateTime.UtcNow.ToString("O")+"\n"+result);Debug.Log(result);}
 [MenuItem(GameIdentity.Title+"/Capturar armaduras e arsenal (em Play)")]
 public static void Preview(){if(EditorApplication.isPlaying)UnityEngine.Object.FindFirstObjectByType<SurvivorGame>()?.PreviewEquipmentBuilds();}
}
