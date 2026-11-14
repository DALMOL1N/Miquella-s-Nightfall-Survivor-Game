using System;
using System.Collections;
using UnityEngine;
namespace NoiteSemFim {
public sealed partial class SurvivorGame {
 public string VerifyEquipmentBuilds(){
  var account=save;bool prev=verifying,oldAim=manualAim;int oldHero=heroIndex,oldStage=stageIndex,pet=selectedDragon,curse=selectedCurse;
  string disk=PlayerPrefs.GetString(EditorTestProfile.Key("NSF.Save"),"");verifying=true;ReturnMenu();save=new SaveData();selectedDragon=selectedCurse=-1;heroIndex=0;
  try{
   string art=EquipmentArt.Audit()+"; "+EquipmentArt.AuditMenuGrips();save.NormalizeWeapons();for(int h=0;h<6;h++)if(save.weaponLoadouts[h]!=-1)throw new Exception("Old save changed starting weapons");
   if(TryEquipStartingWeapon(6)||TryEquipStartingWeapon(3)||TryEquipStartingWeapon(8)||TryEquipStartingWeapon(99)||EquipBuildPreset(0))throw new Exception("Locked weapon/build equipped");
   if(!TryEquipStartingWeapon(1)||SelectedWeaponChoice!=1||StartingWeapon!=1||WeaponArchetype!=-1)throw new Exception("Basic weapon selection failed");
   save=new SaveData{rescuedHeroes=63,relicOwned=255,completedStages=15};
   for(int h=0;h<6;h++)for(int set=0;set<4;set++){
    heroIndex=h;TryEquipStartingWeapon(-1);TryEquipRelic(h,set*2);StartRun();AnimateHero(.15f,Vector2.right);UpdateHeldWeapon();
    if(hero.sprite!=EquipmentArt.Body(h,set*2,BodyFrame(heroAnimClock,true,false))||heldWeapon==null||!heldWeapon.enabled||heldWeapon.sprite!=EquipmentArt.Held(NaturalWeaponChoice(h)))throw new Exception("Armor did not change live hero or weapon");
    VerifyWeaponGrips();var old=hero.sprite;AnimateHero(.15f,Vector2.right);if(hero.sprite==old)throw new Exception("Armored walking did not advance");CastVisual();AnimateHero(.01f,Vector2.zero);if(hero.sprite!=EquipmentArt.Body(h,set*2,5))throw new Exception("Armored attack pose missing");VerifyWeaponGrips();hero.flipX=true;UpdateHeldWeapon();VerifyWeaponGrips();hero.flipX=false;
    UnequipRelic(h,0);StartRun();AnimateHero(.01f,Vector2.zero);if(hero.sprite!=ArtBank.Hero(h)||heldWeapon!=null&&heldWeapon.enabled)throw new Exception("Original outfit/weapon not restored");
   }
   for(int h=0;h<6;h++)for(int weapon=0;weapon<9;weapon++){
    heroIndex=h;if(!TryEquipStartingWeapon(weapon))throw new Exception("Unlocked cross-hero weapon refused");StartRun();manualAim=false;
    if(runWeaponChoice!=weapon||weapons[WeaponSlot(weapon,h)]!=1||level!=1)throw new Exception("Starting weapon rank/slot wrong");
    var e=SpawnEnemy(0,false);e.p=pos+Vector2.right*1.2f;e.hp=e.maxHp=100000;float before=e.hp;ReleasePower(StartingWeapon,2);if(StartingWeapon==2)UpdateCrown(2);UpdateHeldWeapon();
    if(heldWeapon==null||heldWeapon.sprite!=EquipmentArt.Held(weapon)||hero.sprite!=EquipmentArt.Body(h,-1,0))throw new Exception("Equipped weapon/outfit not visible");
    if(weapon==6||weapon==7){if(e.hp>=before)throw new Exception("Cross-hero melee did not damage");}
    else if(weapon==8){if(!shots.Exists(x=>x.lunarArrow))throw new Exception("Cross-hero bow lacks arrows");}
    else if(weapon!=2&&weapon!=4&&shots.Count==0)throw new Exception("Selected magical weapon did not fire");
   }
   heroIndex=0;TryEquipStartingWeapon(6);TryEquipRelic(0,1);StartRun();if(!HasAttackRelic)throw new Exception("Cross-hero katana relic missing");var target=SpawnEnemy(0,false);target.p=Vector2.right*5;target.hp=10000;ReleasePower(1,3);if(!shots.Exists(x=>x.relicFxRow==0))throw new Exception("Katana relic did not emit slash");
   TryEquipStartingWeapon(7);TryEquipRelic(0,5);StartRun();ReleasePower(1,3);if(relicFissures.Count!=1)throw new Exception("Cross-hero greatsword relic missing");
   TryEquipStartingWeapon(8);TryEquipRelic(0,7);StartRun();ReleasePower(0,3);if(!shots.Exists(x=>x.splitOnHit))throw new Exception("Cross-hero split bow relic missing");
   TryEquipStartingWeapon(3);TryEquipRelic(0,2);StartFinal3DDemo();AdvanceFinalTransition(4);AnimateHero(.15f,Vector2.right);FinalCamera();UpdateHeldWeapon();SyncPixelFinal();
   if(!pixelFinal.TryGetValue(hero,out var body)||body.view.sprite!=hero.sprite||!pixelFinal.TryGetValue(heldWeapon,out var hand)||hand.view.sprite!=EquipmentArt.Held(3))throw new Exception("Equipment absent from 2.5D");
   VerifyWeaponGrips();var local=EquipmentArt.PalmLocal(heroIndex,hero.sprite);if(hero.flipX)local.x=-local.x;if(Vector3.Distance(hand.view.transform.position,body.view.transform.TransformPoint(local))>.001f)throw new Exception("2.5D grip drift");
   ReturnMenu();heroIndex=0;TryEquipStartingWeapon(7);TryEquipRelic(0,4);TryEquipRelic(0,5);var normal=save;string json=JsonUtility.ToJson(normal);EnterBuildLab();OpenArsenal(ScreenState.BuildLab);TryEquipStartingWeapon(8);CloseArsenal();labArmor=2;labTalisman=7;ApplyLabBuild();favoriteName="Arqueira glacial";SaveFavorite(0);ReturnMenu();
   if(SelectedWeaponChoice!=7||save.coins!=normal.coins||save.weaponLoadouts[0]!=7)throw new Exception("Training changed normal weapon/coins");
   if(save.favorites[0].startingWeapon!=8||!EquipFavoriteNormal(0)||SelectedWeaponChoice!=8)throw new Exception("Favorite did not save/equip starting weapon");
   var loaded=JsonUtility.FromJson<SaveData>(JsonUtility.ToJson(save));loaded.NormalizeWeapons();if(loaded.weaponLoadouts[0]!=8)throw new Exception("Weapon save roundtrip failed");
   ReturnMenu();TryEquipStartingWeapon(6);TryEquipRelic(0,0);StartCampaignStage(0);for(int i=0;i<3;i++){time=Duration;SpawnWaves(.01f);Damage(finalBoss,finalBoss.maxHp+1);PrepareStageRescue();pos=rescuePosition;RescueStageHero();OpenBossReward(StageBossTypes[i],true);ChooseBossReward(0);if(!AdvanceCampaignStage()||runWeaponChoice!=6||runArmor!=0||WeaponArchetype!=3)throw new Exception("Equipment lost between campaign stages");}
   if(PlayerPrefs.GetString(EditorTestProfile.Key("NSF.Save"),"")!=disk)throw new Exception("Equipment verification altered real save");
   return "PASS: "+art+"; all 24 hero/armor combinations animate idle/walk/attack; armor removal restores original; all 54 hero/weapon pairings equip and attack; unlock validation; katana/fissure/split-arrow relics follow weapon; palm-anchored grips during idle/walk/attack and mirroring, separate dual blades, finger occlusion and exact 2.5D grip projection; per-hero saves; favorites include starting weapon; free training preserves normal gear; gear carried through all four campaign stages; real save unchanged.";
  }finally{ReturnMenu();save=account;heroIndex=oldHero;stageIndex=oldStage;selectedDragon=pet;selectedCurse=curse;manualAim=oldAim;verifying=prev;}
 }
 void VerifyWeaponGrips(){
  if(Vector2.Distance(heldWeapon.transform.position,WeaponPalm())>.001f)throw new Exception("Weapon left primary palm");
  int choice=runWeaponChoice<0?NaturalWeaponChoice(heroIndex):runWeaponChoice;
  if(choice==1&&(offhandWeapon==null||!offhandWeapon.enabled||heldWeapon.sprite==offhandWeapon.sprite||Vector2.Distance(offhandWeapon.transform.position,WeaponPalm(true))>.001f))throw new Exception("Dual blades are not independently held");
  if(gripFingers==null||!gripFingers.enabled||gripFingers.sortingOrder<=heldWeapon.sortingOrder)throw new Exception("Fingers missing above handle");
 }
 public void PreviewEquipmentBuilds(){if(state!=ScreenState.Menu&&state!=ScreenState.Selection&&state!=ScreenState.PowerUp&&state!=ScreenState.Arsenal)return;StartCoroutine(CaptureEquipmentBuilds());}
 IEnumerator CaptureEquipmentBuilds(){
  var account=save;bool prev=verifying,background=Application.runInBackground;int h=heroIndex,st=stageIndex,pet=selectedDragon,curse=selectedCurse;
  ReturnMenu();verifying=true;Application.runInBackground=true;save=new SaveData{rescuedHeroes=63,relicOwned=255,completedStages=15};heroIndex=3;selectedDragon=selectedCurse=-1;
  string folder=System.IO.Path.GetFullPath("../Previa-Armas-Menu-v0282");System.IO.Directory.CreateDirectory(folder);
  try{
   OpenArsenal(ScreenState.PowerUp);TryEquipRelic(3,2);TryEquipStartingWeapon(1);yield return Capture(folder,"ren-geada-duas-laminas");
   for(int id=0;id<6;id++){heroIndex=id;TryEquipRelic(id,2);for(int weapon=0;weapon<9;weapon++){TryEquipStartingWeapon(weapon);yield return Capture(folder,"arsenal-"+id+"-arma-"+weapon);}}
   for(int id=0;id<6;id++){heroIndex=id;TryEquipStartingWeapon(id==3?1:id==4?7:id==5?8:id);for(int set=-1;set<4;set++){if(set<0)UnequipRelic(id,0);else TryEquipRelic(id,set*2);yield return Capture(folder,"menu-heroi-"+id+"-traje-"+(set+1));}}
   heroIndex=3;TryEquipRelic(3,2);TryEquipStartingWeapon(1);OpenPowerUp(ScreenState.Menu);yield return Capture(folder,"power-up-com-arma");state=ScreenState.Selection;yield return Capture(folder,"selecao-com-armas");OpenRelics(ScreenState.PowerUp);yield return Capture(folder,"reliquias-com-arma");OpenArsenal(ScreenState.PowerUp);
   for(int heroId=0;heroId<6;heroId++){heroIndex=heroId;TryEquipStartingWeapon(heroId%3==0?6:heroId%3==1?7:8);for(int set=0;set<4;set++){TryEquipRelic(heroId,set*2);StartRun();elementPreviewFrozen=true;AnimateHero(.15f,Vector2.right);UpdateHeldWeapon();toastUntil=0;yield return Capture(folder,"heroi-"+heroId+"-armadura-"+set);ReturnMenu();}}
   heroIndex=0;TryEquipStartingWeapon(8);TryEquipRelic(0,2);TryEquipRelic(0,7);StartFinal3DDemo();elementPreviewFrozen=true;AdvanceFinalTransition(4);FinalCamera();AnimateHero(.15f,Vector2.right);UpdateHeldWeapon();SyncPixelFinal();toastUntil=0;yield return Capture(folder,"miquella-arco-geada-25d");
   Debug.Log("EQUIPMENT PREVIEWS READY: "+folder);
  }finally{elementPreviewFrozen=false;ReturnMenu();save=account;verifying=prev;heroIndex=h;stageIndex=st;selectedDragon=pet;selectedCurse=curse;Application.runInBackground=background;}
 }
}
}
