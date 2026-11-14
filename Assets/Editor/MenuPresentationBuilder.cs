using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using NoiteSemFim;
public static class MenuPresentationBuilder {
 [MenuItem(GameIdentity.Title+"/Verificar menu e enquadramento")]
 public static void Verify(){if(EditorApplication.isPlaying)return;var g=UnityEngine.Object.FindFirstObjectByType<SurvivorGame>();if(g==null)g=new GameObject("Menu verification").AddComponent<SurvivorGame>();typeof(SurvivorGame).GetMethod("Awake",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(g,null);var result=g.VerifyMenuPresentation();File.WriteAllText("VERIFICACAO-MENU.txt",DateTime.UtcNow.ToString("O")+"\n"+result);Debug.Log(result);}
 [MenuItem(GameIdentity.Title+"/Capturar menus revisados (em Play)")]
 public static void Preview(){if(EditorApplication.isPlaying)UnityEngine.Object.FindFirstObjectByType<SurvivorGame>()?.PreviewMenuRedesign();}
}
