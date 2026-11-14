using System;
using System.Collections;
using UnityEngine;
namespace NoiteSemFim {
public sealed partial class SurvivorGame {
 ScreenState powerUpReturn=ScreenState.Menu;
 void OpenPowerUp(ScreenState from){powerUpReturn=from==ScreenState.Selection?ScreenState.Selection:ScreenState.Menu;if(!IsHeroUnlocked(heroIndex))heroIndex=0;ValidateDragonSelection();save.NormalizeRelics();save.NormalizeWeapons();powerSection=0;powerLastHero=-1;state=ScreenState.PowerUp;}
 void ClosePowerUp(){SaveMenuSettings();if(!verifying)save.Store();state=powerUpReturn;}
 void EnterPowerUpSection(int section){if(section==0)state=ScreenState.Shop;else if(section==1)OpenDragonDen(ScreenState.PowerUp);else if(section==2)OpenRelics(ScreenState.PowerUp);else if(section==3)OpenArsenal(ScreenState.PowerUp);}
 void ClosePowerUpSection(){state=ScreenState.PowerUp;}
 void DrawPowerUp(){
  Backdrop();Text("POWER UP",48,22,930,62,Style(40,gold,TextAnchor.MiddleLeft,true));Text("Equipe seu herói, compre melhorias ou consulte os combos.",52,82,950,28,Style(15,cyan));
  Frame(1,1008,25,224,55);DrawIcon(ArtBank.Icon(14),1026,37,31);Text(save.coins.ToString("N0"),1070,37,140,28,Style(22,gold,TextAnchor.MiddleRight));
  DrawHeroTabs(48,116,1184);if(powerLastHero!=heroIndex){powerLastHero=heroIndex;ResetPowerInspection();}
  string[] sections={"MONTAR BUILD","MELHORIAS PERMANENTES","COMBOS E MALDIÇÕES"};for(int i=0;i<3;i++)if(Button(sections[i],48+i*400,172,384,43,true,powerSection==i,17))powerSection=i;
  DrawPowerPanel(48,231,300,406);DrawPowerLoadout();DrawPowerPanel(368,231,864,406);
  if(powerSection==0)DrawPowerEquipment();else if(powerSection==1)DrawPowerPermanent();else DrawPowerRecipes();
  if(Button(powerUpReturn==ScreenState.Selection?"VOLTAR À PREPARAÇÃO":"VOLTAR AO MENU",48,655,302,45,true,false,15))ClosePowerUp();
  Text("Escolhas salvas automaticamente.",378,663,485,28,Style(13,new Color(.78f,.85f,.85f)));
  if(Button("TESTAR BUILD GRÁTIS",916,655,316,45,true,false,15))EnterBuildLab();
 }
 string VerifyPowerUpHub(){var account=save;bool previous=verifying;int oldHero=heroIndex,oldDragon=selectedDragon,oldCurse=selectedCurse;var previousReturn=powerUpReturn;verifying=true;save=new SaveData{coins=500,relicOwned=3,dragonUnlocks=1};heroIndex=0;selectedDragon=selectedCurse=-1;
  GoHome();MainAction(1);if(state!=ScreenState.PowerUp)throw new Exception("Main Power Up entry failed");EnterPowerUpSection(0);if(state!=ScreenState.Shop||!TryBuyPerk(0))throw new Exception("Power Up upgrades failed");ClosePowerUpSection();EnterPowerUpSection(1);if(state!=ScreenState.DragonDen||!SelectDragon(0))throw new Exception("Power Up dragon entry failed");CloseDragonDen();if(state!=ScreenState.PowerUp)throw new Exception("Dragon returned outside Power Up");EnterPowerUpSection(2);if(state!=ScreenState.Relics||!TryEquipRelic(0,0)||!TryEquipRelic(0,1))throw new Exception("Power Up relic equip failed");relicAppearancePreview=true;CloseRelics();if(state!=ScreenState.Relics||relicAppearancePreview)throw new Exception("Armor preview return failed");CloseRelics();if(state!=ScreenState.PowerUp||selectedDragon!=0||EquippedRelic(0,0)!=0||EquippedRelic(0,1)!=1||save.perks[0]!=1||save.coins!=450)throw new Exception("Build not retained across sections");ClosePowerUp();if(state!=ScreenState.Menu)throw new Exception("Power Up menu return failed");OpenPowerUp(ScreenState.Selection);EnterPowerUpSection(0);ClosePowerUpSection();ClosePowerUp();if(state!=ScreenState.Selection)throw new Exception("Power Up lost preparation origin");save=account;heroIndex=oldHero;selectedDragon=oldDragon;selectedCurse=oldCurse;powerUpReturn=previousReturn;verifying=previous;ReturnMenu();return "Power Up hub: main/preparation entry, upgrades, dragon and relic navigation, preview/back paths, persistent build choices and origin return PASS.";
 }
 IEnumerator CapturePowerUpHub(string folder){OpenPowerUp(ScreenState.Menu);yield return Capture(folder,"power-up-central-v13");EnterPowerUpSection(0);yield return Capture(folder,"power-up-melhorias-v13");ClosePowerUpSection();EnterPowerUpSection(1);yield return Capture(folder,"power-up-dragoes-v13");CloseDragonDen();EnterPowerUpSection(2);yield return Capture(folder,"power-up-reliquias-v13");CloseRelics();ClosePowerUp();}
}
}
