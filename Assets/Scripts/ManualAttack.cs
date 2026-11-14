using System;
using UnityEngine;
namespace NoiteSemFim {
public sealed partial class SurvivorGame {
 // Input injection is used only by deterministic editor verification.
 bool manualAttackHeldForCheck=true,manualAttackPressedForCheck;
 readonly bool[] manualAttackRequested=new bool[6];
 float manualCrownRemaining;
 bool ManualAttackHeld=>verifying?manualAttackHeldForCheck:Input.GetMouseButton(0)&&PointerInArena()&&!OverHud();
 bool ManualAttackPressed=>verifying?manualAttackPressedForCheck:Input.GetMouseButtonDown(0)&&PointerInArena()&&!OverHud();
 void ResetManualAttack(){Array.Clear(manualAttackRequested,0,6);manualCrownRemaining=0;}
 void ReadManualAttack(float dt){manualCrownRemaining=Mathf.Max(0,manualCrownRemaining-dt);if(!manualAim){ResetManualAttack();return;}if(ManualAttackPressed){for(int w=0;w<6;w++)if(weapons[w]>0&&PowerTarget(w)!=null)manualAttackRequested[w]=true;if(orbitPowerReady)manualCrownRemaining=.38f;}if(verifying)manualAttackPressedForCheck=false;}
 string VerifyManualAttack(){var account=save;bool oldVerify=verifying,oldAim=manualAim,oldHeld=manualAttackHeldForCheck;int oldHero=heroIndex,pet=selectedDragon,curse=selectedCurse;verifying=true;save=new SaveData();selectedDragon=selectedCurse=-1;manualAim=true;manualAttackHeldForCheck=false;
  foreach(int h in new[]{0,1,2,3,4,5}){heroIndex=h;StartRun();for(int w=0;w<6;w++)weapons[w]=1;var e=SpawnEnemy(0,false);e.p=Vector2.right;e.hp=e.maxHp=100000;manualAimPoint=Vector2.right*4;lastAimDirection=Vector2.right;AdvanceWeaponsForCheck(2);if(PreparingPowerId>=0||shots.Count!=0||signatureVisuals.Count!=0||orbitPowerReady)throw new Exception("Manual hero attacked without left button");manualAttackPressedForCheck=true;UpdateWeapons(.01f);if(PreparingPowerId<0)throw new Exception("Left click did not start preparation");AdvanceWeaponsForCheck(.55f);if(shots.Count==0&&signatureVisuals.Count==0)throw new Exception("Quick tap was cancelled before release");int count=shots.Count,visuals=signatureVisuals.Count;AdvanceWeaponsForCheck(3);if(shots.Count!=count||signatureVisuals.Count!=visuals||PreparingPowerId>=0||orbitPowerReady)throw new Exception("Tap repeated without another click");manualAttackHeldForCheck=true;AdvanceWeaponsForCheck(2.5f);if(shots.Count<=count&&signatureVisuals.Count<=visuals)throw new Exception("Holding left mouse failed to repeat attacks");manualAttackHeldForCheck=false;AdvanceWeaponsForCheck(.6f);count=shots.Count;visuals=signatureVisuals.Count;AdvanceWeaponsForCheck(2);if(shots.Count!=count||signatureVisuals.Count!=visuals||PreparingPowerId>=0)throw new Exception("Released mouse kept attacking");}
  heroIndex=3;StartRun();var target=SpawnEnemy(0,false);target.p=Vector2.right;target.hp=100000;manualAim=false;AdvanceWeaponsForCheck(.8f);if(signatureVisuals.Count==0)throw new Exception("Automatic mode now requires left click");manualAim=true;StartRun();manualAttackPressedForCheck=true;AdvanceWeaponsForCheck(1);if(shots.Count!=0||PreparingPowerId>=0)throw new Exception("Left click bypasses enemy range gate");manualAttackPressedForCheck=false;manualAttackHeldForCheck=oldHeld;save=account;heroIndex=oldHero;selectedDragon=pet;selectedCurse=curse;manualAim=oldAim;verifying=oldVerify;ReturnMenu();return "Manual attack PASS: six heroes, no attacks without left mouse, quick tap completes windup once, holding repeats with cooldown, release stops repeats, automatic mode unchanged and enemy-range gate retained.";
 }
}
}
