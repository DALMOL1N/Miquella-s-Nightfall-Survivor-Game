using System;
using UnityEngine;
namespace NoiteSemFim {
public sealed partial class SurvivorGame {
 static readonly string[] DragonMissionNames={"Acumule 1.000 abates","Derrote qualquer chefe","Vença uma noite e derrote o Soberano"};
 bool IsDragonUnlocked(int id)=>id>=0&&id<3&&(save.dragonUnlocks&(1<<id))!=0;
 string SelectedDragonName=>selectedDragon>=0&&selectedDragon<3?DragonNames[selectedDragon]:"Nenhum";
 string DragonMissionProgress(int id)=>id==0?Mathf.Min(save.dragonKills,1000).ToString("N0")+" / 1.000 abates":id==1?Mathf.Min(save.bossKills,1)+" / 1 chefe":Mathf.Min(save.wins,1)+" / 1 vitória";
 void ValidateDragonSelection(){save.RefreshDragonMissions();if(!IsDragonUnlocked(selectedDragon))selectedDragon=-1;}
 bool SelectDragon(int id){if(id!=-1&&!IsDragonUnlocked(id))return false;selectedDragon=id;return true;}
 void AnnounceDragonUnlocks(int bits){if(bits==0)return;string names="";for(int i=0;i<3;i++)if((bits&(1<<i))!=0)names+=(names.Length>0?", ":"")+DragonNames[i];Toast("MISSÃO CONCLUÍDA: "+names+"! Equipe no menu de dragões.");Play(levelSound);}
 void RecordDragonDefeat(int type){if(elementTraining||final3DDemo)return;save.dragonKills++;if(IsBoss(type))save.bossKills++;int unlocked=save.RefreshDragonMissions();AnnounceDragonUnlocks(unlocked);if(unlocked!=0&&!verifying)save.Store();}
 string VerifyDragonMissions(){var original=save;bool previous=verifying;int selected=selectedDragon,curse=selectedCurse;verifying=true;save=new SaveData();selectedDragon=0;selectedCurse=-1;StartRun();if(save.dragonUnlocks!=0||dragon!=null||selectedDragon!=-1||SelectDragon(0))throw new Exception("Fresh save allowed a locked dragon");
  save.dragonKills=999;save.RefreshDragonMissions();if(IsDragonUnlocked(0))throw new Exception("Brasa unlocked before 1000 kills");RecordDragonDefeat(0);if(!IsDragonUnlocked(0)||save.dragonKills!=1000||!SelectDragon(0))throw new Exception("Brasa mission failed");StartRun();if(dragon==null)throw new Exception("Unlocked equipped dragon did not spawn");
  if(IsDragonUnlocked(1))throw new Exception("Geada unlocked without boss");var boss=SpawnEnemy(3,false);Damage(boss,boss.maxHp+1);if(!IsDragonUnlocked(1)||save.bossKills!=1)throw new Exception("Boss kill did not unlock Geada");if(IsDragonUnlocked(2))throw new Exception("Aurora unlocked before victory");EndRun(true);if(!IsDragonUnlocked(2)||save.wins!=1)throw new Exception("Night victory did not unlock Aurora");
  string json=JsonUtility.ToJson(save);var loaded=JsonUtility.FromJson<SaveData>(json);loaded.RefreshDragonMissions();if(loaded.dragonUnlocks!=7||loaded.dragonKills!=1001||loaded.bossKills!=1)throw new Exception("Dragon mission persistence failed");
  save=new SaveData{totalKills=1500,wins=1,curseUnlocks=1};save.RefreshDragonMissions();if(save.dragonUnlocks!=7||save.dragonKills!=1500||save.bossKills!=1)throw new Exception("Legacy progress migration failed");save.RefreshDragonMissions();if(save.dragonKills!=1500||save.bossKills!=1)throw new Exception("Legacy progress counted twice");
  save=new SaveData();selectedDragon=-1;StartElementTraining();RecordDragonDefeat(3);EndRun(true);if(save.dragonKills!=0||save.bossKills!=0||save.wins!=0||save.dragonUnlocks!=0)throw new Exception("Training unlocked a dragon");
  save=original;selectedDragon=selected;selectedCurse=curse;verifying=previous;ReturnMenu();return "Dragon missions: locked start/no companion, 999/1000 threshold, real boss defeat, victory, selection/spawn, JSON persistence, legacy migration and training isolation PASS.";
 }
}
}
