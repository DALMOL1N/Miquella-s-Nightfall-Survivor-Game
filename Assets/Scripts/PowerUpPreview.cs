using System.Collections;
using System.IO;
using UnityEngine;
namespace NoiteSemFim {
public sealed partial class SurvivorGame {
 public void PreviewOrganizedPowerUp(){if(!MenuPresentation)return;StartCoroutine(CaptureOrganizedPowerUp());}
 IEnumerator CaptureOrganizedPowerUp(){
  var account=save;bool oldVerify=verifying,bg=Application.runInBackground;int h=heroIndex,pet=selectedDragon,curse=selectedCurse;var origin=powerUpReturn;
  ReturnMenu();verifying=true;Application.runInBackground=true;save=new SaveData{coins=10012,rescuedHeroes=63,relicOwned=255,completedStages=15,dragonUnlocks=7,curseUnlocks=7};heroIndex=3;selectedDragon=0;selectedCurse=-1;
  string folder=Path.GetFullPath("../Previa-Power-Up-v030");Directory.CreateDirectory(folder);
  try{
   TryEquipStartingWeapon(6);TryEquipRelic(3,2);TryEquipRelic(3,3);OpenPowerUp(ScreenState.Menu);
   for(int i=0;i<4;i++){powerSlot=i;powerLastHero=heroIndex;ResetPowerInspection();yield return Capture(folder,"0"+(i+1)+"-equipamento-"+i);}
   powerSection=1;yield return Capture(folder,"05-melhorias-permanentes");powerSection=2;yield return Capture(folder,"06-combos-sem-maldicao");TrySelectPowerCurse(0);yield return Capture(folder,"07-combos-com-risco");
   save=new SaveData{coins=10};heroIndex=0;selectedDragon=selectedCurse=-1;OpenPowerUp(ScreenState.Menu);powerSlot=3;powerLastHero=heroIndex;ResetPowerInspection();powerInspected=0;yield return Capture(folder,"08-dragao-bloqueado");powerSection=1;yield return Capture(folder,"09-moedas-insuficientes");Debug.Log("ORGANIZED POWER UP PREVIEWS READY: "+folder);
  }finally{save=account;heroIndex=h;selectedDragon=pet;selectedCurse=curse;verifying=oldVerify;powerUpReturn=origin;powerSection=powerSlot=0;powerLastHero=-1;Application.runInBackground=bg;state=ScreenState.PowerUp;}
 }
}
}
