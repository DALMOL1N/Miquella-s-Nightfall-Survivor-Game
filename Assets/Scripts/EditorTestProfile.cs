using UnityEngine;
namespace NoiteSemFim {
public static class EditorTestProfile {
 public const string SessionFlag="NoiteSemFim.FullTest";
 public static bool Enabled {get {
#if UNITY_EDITOR
 return UnityEditor.SessionState.GetBool(SessionFlag,false);
#else
 return false;
#endif
 }}
 public static string Key(string normalKey)=>Enabled?normalKey+".UnityTeste.v15":normalKey;
 public static SaveData CreateSave()=>new SaveData{completedStages=15,rescuedHeroes=63,rescueVersion=1,coins=10000,totalKills=1000,wins=1,bestLevel=12,curseUnlocks=7,dragonKills=1000,bossKills=3,dragonUnlocks=7,dragonMissionVersion=1,relicOwned=255};
}
}
