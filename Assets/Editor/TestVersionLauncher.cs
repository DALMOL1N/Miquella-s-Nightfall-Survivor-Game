using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using NoiteSemFim;
[InitializeOnLoad]
public static class TestVersionLauncher {
 const string Pending="NoiteSemFim.OpenTestVersion";
 static string RequestPath=>Path.GetFullPath(".nsf-open-full-test");
 static double nextCheck;
 static TestVersionLauncher(){EditorApplication.playModeStateChanged+=OnPlayMode;EditorApplication.update+=CheckRequest;}
 static void CheckRequest(){if(EditorApplication.timeSinceStartup<nextCheck||EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlayingOrWillChangePlaymode)return;nextCheck=EditorApplication.timeSinceStartup+1;if(File.Exists(RequestPath)){File.Delete(RequestPath);Open();}}
 [MenuItem(NoiteSemFim.GameIdentity.Title + "/Abrir versão completa para teste")]
 public static void Open(){if(EditorApplication.isPlaying){Debug.Log("O teste já está em Play. Pare o Play para trocar de perfil.");Report();return;}if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;SessionState.SetBool(EditorTestProfile.SessionFlag,true);EditorSceneManager.OpenScene("Assets/Scenes/NoiteSemFim.unity");SessionState.SetBool(Pending,true);EditorApplication.delayCall+=()=>{EditorApplication.isPlaying=true;};Debug.Log(GameIdentity.Title+": versão completa 0.30.0 (Power Up organizado com equipamento direto e melhorias separadas); perfil de teste isolado; iniciando Play.");}
 [MenuItem(NoiteSemFim.GameIdentity.Title + "/Usar progresso normal (fora do Play)")]
 public static void Normal(){if(EditorApplication.isPlaying){Debug.LogWarning("Pare o Play antes de mudar o perfil.");return;}SessionState.SetBool(EditorTestProfile.SessionFlag,false);Debug.Log("Perfil normal selecionado. O progresso de teste fica separado.");}
 static void OnPlayMode(PlayModeStateChange mode){if(mode==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Pending,false))EditorApplication.delayCall+=Report;}
 static void Report(){var game=UnityEngine.Object.FindFirstObjectByType<SurvivorGame>();if(game==null){Debug.LogError("Game instance unavailable after entering Play");return;}SessionState.SetBool(Pending,false);var viewType=Type.GetType("UnityEditor.GameView,UnityEditor");if(viewType!=null)EditorWindow.GetWindow(viewType).Focus();File.WriteAllText(Path.GetFullPath("TESTE-UNITY.txt"),"Unity: Play ativo\nVersão: 0.30.0 (Power Up organizado com equipamento direto e melhorias separadas) completa\nProjeto: "+Path.GetFullPath(Path.Combine(Application.dataPath,".."))+"\nCena: "+UnityEngine.SceneManagement.SceneManager.GetActiveScene().path+"\nEstado: "+game.state+"\nPerfil de teste separado: "+EditorTestProfile.Enabled+"\nPersonagens, dragões, maldições e relíquias liberados; 10.000 moedas iniciais.\nConfirmado em UTC: "+DateTime.UtcNow.ToString("O"));Debug.Log(GameIdentity.Title+" TESTE PRONTO: Play ativo, versão completa 0.30.0 (Power Up organizado com equipamento direto e melhorias separadas), perfil separado, estado "+game.state);}
 public static void VerifyProfile(){bool previous=SessionState.GetBool(EditorTestProfile.SessionFlag,false);string normal=PlayerPrefs.GetString("NSF.Save","");SessionState.SetBool(EditorTestProfile.SessionFlag,true);try{if(EditorTestProfile.Key("NSF.Save")=="NSF.Save"||EditorTestProfile.Key("NSF.Dragon")=="NSF.Dragon")throw new Exception("Test profile shares normal key");var s=EditorTestProfile.CreateSave();s.NormalizeRelics();s.RefreshDragonMissions();if(s.completedStages!=15||s.coins!=10000||s.relicOwned!=255||s.dragonUnlocks!=7||s.curseUnlocks!=7||s.wins<1||s.totalKills<250)throw new Exception("Incomplete test unlocks");if(PlayerPrefs.GetString("NSF.Save","")!=normal)throw new Exception("Test setup altered normal progress");Debug.Log("EDITOR FULL TEST PROFILE PASS: isolated save/preferences, 10000 coins, all heroes/dragons/relics/curses.");}finally{SessionState.SetBool(EditorTestProfile.SessionFlag,previous);}}
}
