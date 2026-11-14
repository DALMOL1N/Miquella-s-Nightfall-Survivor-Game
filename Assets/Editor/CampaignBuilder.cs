using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using NoiteSemFim;

public static class CampaignBuilder {
 [MenuItem(GameIdentity.Title + "/Jogar campanha com progresso normal")]
 public static void OpenCampaign() {
  if(EditorApplication.isPlaying) { Debug.LogWarning("Pare o Play antes de trocar o perfil."); return; }
  if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
  SessionState.SetBool(EditorTestProfile.SessionFlag, false);
  EditorSceneManager.OpenScene("Assets/Scenes/NoiteSemFim.unity");
  EditorApplication.delayCall += () => EditorApplication.isPlaying = true;
 }
 [MenuItem(GameIdentity.Title + "/Verificar fases e resgates")]
 public static void Verify() {
  if(EditorApplication.isPlaying) { Debug.LogWarning("Pare o Play para verificar as fases."); return; }
  var game = UnityEngine.Object.FindFirstObjectByType<SurvivorGame>();
  if(game == null) game = new GameObject("Campaign Verification").AddComponent<SurvivorGame>();
  typeof(SurvivorGame).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(game, null);
  string result = game.VerifyCampaignStages();
  File.WriteAllText("VERIFICACAO-FASES.txt", DateTime.UtcNow.ToString("O") + "\n" + result);
  Debug.Log(result);
 }
 [MenuItem(GameIdentity.Title + "/Capturar prévia das fases (em Play)")]
 public static void Preview() {
  if(!EditorApplication.isPlaying) { Debug.LogWarning("Inicie o Play primeiro."); return; }
  var game = UnityEngine.Object.FindFirstObjectByType<SurvivorGame>();
  if(game != null) game.PreviewCampaignStages();
 }
}
