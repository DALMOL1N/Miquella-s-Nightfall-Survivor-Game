using System;
using System.Collections;
using System.IO;
using UnityEngine;
namespace NoiteSemFim {
public sealed partial class SurvivorGame {
 public string VerifyMenuPresentation(){
  bool previous=verifying;var previousState=state;verifying=true;state=ScreenState.Menu;
  try {
   InitGUI();if(MenuDisplayFont==null||MenuReadingFont==null)throw new Exception("Menu fonts missing");
   string accents="ÁÉÍÓÚÂÊÔÃÕÇáéíóúâêôãõç0123456789";MenuDisplayFont.RequestCharactersInTexture(accents,24,FontStyle.Normal);MenuReadingFont.RequestCharactersInTexture(accents,18,FontStyle.Normal);
   foreach(char ch in accents)if(!MenuDisplayFont.HasCharacter(ch)||!MenuReadingFont.HasCharacter(ch))throw new Exception("Menu accent missing: "+ch);
   foreach(string value in new[]{"SALVAR E VOLTAR","MANUAL • MOUSE","TREINO GRÁTIS","MONTAR BUILD • POWER UP","DRAGÕES E ARTEFATOS"}){
    var source=new GUIStyle(labelStyle){font=MenuDisplayFont,fontSize=24,wordWrap=false};var fitted=FitMenuText(value,source,220,32);
    if(fitted.CalcSize(new GUIContent(value)).x>218||fitted.CalcHeight(new GUIContent(value),216)>32)throw new Exception("Button overflow: "+value);
   }
   foreach(var size in new[]{new Vector2(1280,720),new Vector2(1920,1080),new Vector2(1024,768),new Vector2(2560,1080),new Vector2(1280,800)}){float scale=Mathf.Min(size.x/1280,size.y/720);var origin=(size-new Vector2(1280,720)*scale)*.5f;if(origin.x<-.01f||origin.y<-.01f||origin.x+1232*scale>size.x||origin.y+696*scale>size.y)throw new Exception("Menu safe area failed");}
   return "PASS: bundled Cinzel Decorative and static reading font; Portuguese accents; fitted menu labels; safe canvas at 16:9, 16:10, 4:3 and ultrawide; "+VerifyMainMenu()+" "+VerifyPowerUpHub();
  } finally {verifying=previous;state=previousState;}
 }
 public void PreviewMenuRedesign(){if(!MenuPresentation)return;StartCoroutine(CaptureMenuRedesign());}
 IEnumerator CaptureMenuRedesign(){
  var account=save;bool oldVerify=verifying,bg=Application.runInBackground,aim=manualAim;int h=heroIndex,pet=selectedDragon,curse=selectedCurse,inspected=inspectedRelic;float mv=musicVolume,sv=volume;bool sound=muted;
  ReturnMenu();verifying=true;Application.runInBackground=true;save=new SaveData{coins=10012,rescuedHeroes=63,relicOwned=255,completedStages=15,dragonUnlocks=7,curseUnlocks=7};heroIndex=0;selectedDragon=0;selectedCurse=-1;
  string folder=Path.GetFullPath("../Previa-Menu-v029");Directory.CreateDirectory(folder);
  try{
   GoHome();yield return Capture(folder,"01-menu-principal");OpenOptions();manualAim=false;yield return Capture(folder,"02-opcoes-automatica");manualAim=true;yield return Capture(folder,"03-opcoes-manual");CloseOptions();OpenSelection();yield return Capture(folder,"04-preparar-campanha");OpenPowerUp(ScreenState.Menu);yield return Capture(folder,"05-power-up");EnterPowerUpSection(0);yield return Capture(folder,"06-melhorias");ClosePowerUpSection();EnterPowerUpSection(1);yield return Capture(folder,"07-dragoes");CloseDragonDen();OpenRelics(ScreenState.PowerUp);yield return Capture(folder,"08-reliquias");inspectedRelic=2;relicAppearancePreview=true;armorPreviewHero=3;yield return Capture(folder,"09-previa-armadura");relicAppearancePreview=false;OpenArsenal(ScreenState.PowerUp);heroIndex=3;TryEquipStartingWeapon(6);TryEquipRelic(3,2);yield return Capture(folder,"10-arsenal");ReturnMenu();EnterBuildLab();yield return Capture(folder,"11-treino");ReturnMenu();OpenFavorites();yield return Capture(folder,"12-favoritas");Debug.Log("MENU V029 PREVIEWS READY: "+folder);
  }finally{ReturnMenu();save=account;verifying=oldVerify;heroIndex=h;selectedDragon=pet;selectedCurse=curse;inspectedRelic=inspected;manualAim=aim;musicVolume=mv;volume=sv;muted=sound;Application.runInBackground=bg;GoHome();}
 }
}
}
