using System.Collections;
using UnityEngine;
namespace NoiteSemFim {
public sealed partial class SurvivorGame {
 public void PreviewBestiary(){if(state!=ScreenState.Menu&&state!=ScreenState.Selection){Debug.LogWarning("Volte ao menu para capturar o bestiário.");return;}StartCoroutine(CaptureBestiary());}
 IEnumerator CaptureBestiary(){
  var account=save;int oldHero=heroIndex,oldStage=stageIndex,oldPet=selectedDragon,oldCurse=selectedCurse;bool oldVerify=verifying,oldBackground=Application.runInBackground;
  ReturnMenu();verifying=true;Application.runInBackground=true;save=new SaveData{completedStages=7};heroIndex=0;selectedDragon=selectedCurse=-1;
  string folder=System.IO.Path.GetFullPath("../Previa-Bestiario-v026");System.IO.Directory.CreateDirectory(folder);
  try{
   for(int b=0;b<4;b++){
    if(b<3)StartCampaignStage(b);else {StartFinal3DDemo();AdvanceFinalTransition(4);FinalCamera();}
    elementPreviewFrozen=true;toastUntil=0;
    for(int k=0;k<3;k++){var e=SpawnEnemy(k,false);e.p=pos+new Vector2(-3+k*3,b==3?1:-2.3f);e.sprite.transform.position=e.p;}
    if(b<3){finalBoss=SpawnEnemy(StageBossTypes[b],true);finalBoss.p=pos+new Vector2(0,1);finalBoss.sprite.transform.position=finalBoss.p;}
    else {finalBoss.p=pos+new Vector2(0,4);finalBoss.sprite.transform.position=finalBoss.p;SyncFinal3D();}
    yield return Capture(folder,"mapa-"+(b+1)+"-inimigos-chefe");
    foreach(var e in enemies){e.attack=0;AdvanceEnemyFSM(e,.01f);AnimateEnemy(e,.01f,Vector2.zero);}if(b==3)SyncFinal3D();
    yield return Capture(folder,"mapa-"+(b+1)+"-preparacao");
    Damage(finalBoss,finalBoss.maxHp*.51f);if(b==3)SyncFinal3D();
    yield return Capture(folder,"mapa-"+(b+1)+"-chefe-fase2");
    elementPreviewFrozen=false;ReturnMenu();
   }
   Debug.Log("BESTIARY PREVIEWS READY: "+folder);
  }finally{elementPreviewFrozen=false;ReturnMenu();save=account;heroIndex=oldHero;stageIndex=oldStage;selectedDragon=oldPet;selectedCurse=oldCurse;verifying=oldVerify;Application.runInBackground=oldBackground;}
 }
}
}
