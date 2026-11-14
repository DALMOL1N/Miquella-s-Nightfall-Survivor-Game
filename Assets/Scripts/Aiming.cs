using System;
using UnityEngine;
namespace NoiteSemFim {
public sealed partial class SurvivorGame {
 bool manualAim,aimPointerValid;
 Vector2 manualAimPoint,lastAimDirection=Vector2.right;
 const float VisualShotHeight=.35f;
 string AimDescription=>manualAim?"Mire com o mouse. Botão esquerdo: atacar. Segure para repetir, respeitando a recarga.\nWASD / setas movem o personagem; clique não move.":"As armas direcionais escolhem o inimigo mais próximo.\nVocê também pode segurar o clique para mover.";
 void ResetAim(){manualAimPoint=pos+Vector2.right*4;lastAimDirection=Vector2.right;aimPointerValid=false;}
 static Vector2 SafeDirection(Vector2 origin,Vector2 target,Vector2 fallback){Vector2 d=target-origin;return d.sqrMagnitude>.0025f?d.normalized:fallback.sqrMagnitude>.001f?fallback.normalized:Vector2.right;}
 Vector2 WeaponDirection(Vector2 origin,Enemy target=null)=>manualAim?SafeDirection(origin,manualAimPoint,lastAimDirection):target!=null?SafeDirection(origin,target.p,facing):facing;
 Vector2 LightningPoint(Enemy automatic)=>manualAim?pos+Vector2.ClampMagnitude(manualAimPoint-pos,12):automatic.p;
 bool PointerInArena()=>cam!=null&&cam.pixelRect.Contains(Input.mousePosition)&&Application.isFocused;
 void ReadAimPointer(){if(!manualAim){aimPointerValid=false;return;}aimPointerValid=PointerInArena();if(!aimPointerValid)return;manualAimPoint=final3DVisible?ArenaPointFromScreen(Input.mousePosition):ArenaPointFromScreen(Input.mousePosition)-Vector2.up*VisualShotHeight;lastAimDirection=SafeDirection(pos,manualAimPoint,lastAimDirection);}
 void RefreshAimCursor(){Cursor.visible=!(manualAim&&state==ScreenState.Playing&&aimPointerValid&&Application.isFocused);}
 void OnDestroy(){Cursor.visible=true;}
 void DrawAimReticle(){if(!manualAim||state!=ScreenState.Playing||!aimPointerValid||cam==null)return;Vector3 point=ArenaPointToScreen(manualAimPoint);float scale=Mathf.Min(UnityEngine.Screen.width/1280f,UnityEngine.Screen.height/720f);float ox=(UnityEngine.Screen.width-1280*scale)/2,oy=(UnityEngine.Screen.height-720*scale)/2;float x=(point.x-ox)/scale,y=(UnityEngine.Screen.height-point.y-oy)/scale;
  Rect(x-13,y-2,9,4,Color.black);Rect(x+4,y-2,9,4,Color.black);Rect(x-2,y-13,4,9,Color.black);Rect(x-2,y+4,4,9,Color.black);Rect(x-12,y-1,7,2,cyan);Rect(x+5,y-1,7,2,cyan);Rect(x-1,y-12,2,7,cyan);Rect(x-1,y+5,2,7,cyan);Rect(x-1,y-1,2,2,gold);
 }
 string VerifyAiming(){var original=save;bool oldVerify=verifying,oldMode=manualAim;int pet=selectedDragon,curse=selectedCurse;verifying=true;save=new SaveData();selectedCurse=-1;selectedDragon=2;StartRun();Array.Clear(weapons,0,6);weapons[0]=1;var near=SpawnEnemy(0,false);near.p=Vector2.left*2;near.hp=1000;var far=SpawnEnemy(0,false);far.p=Vector2.right*5;far.hp=1000;manualAim=false;cooldown[0]=0;AdvanceWeaponsForCheck(PowerPreparationTime(0)+.02f);if(shots.Count!=1||shots[0].v.x>=0)throw new Exception("Automatic aim did not choose nearest enemy");
  shots.Clear();manualAim=true;manualAimPoint=Vector2.right*5;lastAimDirection=Vector2.right;cooldown[0]=0;AdvanceWeaponsForCheck(PowerPreparationTime(0)+.02f);if(shots.Count!=1||shots[0].v.x<=0)throw new Exception("Manual aim followed nearest enemy instead of pointer");
  enemies.Clear();shots.Clear();cooldown[0]=0;AdvanceWeaponsForCheck(PowerPreparationTime(0)+.02f);if(shots.Count!=0||PreparingPowerId>=0)throw new Exception("Manual aim fired without nearby enemies");var nearby=SpawnEnemy(0,false);nearby.p=Vector2.right*3;nearby.hp=1000;
  foreach(int id in new[]{1,5}){shots.Clear();Array.Clear(weapons,0,6);weapons[id]=1;cooldown[id]=0;facing=Vector2.left;AdvanceWeaponsForCheck(PowerPreparationTime(id)+.02f);if(shots.Count==0||shots[0].v.x<=0)throw new Exception("Directional weapon ignored manual aim");}
  var dir=SafeDirection(Vector2.zero,Vector2.zero,Vector2.up);if(dir!=Vector2.up)throw new Exception("Zero-distance aim fallback failed");manualAimPoint=new Vector2(30,0);if(Mathf.Abs(LightningPoint(null).magnitude-12)>.01f)throw new Exception("Lightning range clamp failed");
  Array.Clear(weapons,0,6);weapons[4]=1;cooldown[4]=0;manualAimPoint=new Vector2(3,0);int before=effects.Count;AdvanceWeaponsForCheck(PowerPreparationTime(4)+.02f);if(effects.Count<=before||Vector2.Distance(effects[effects.Count-1].sprite.transform.position,manualAimPoint)>.01f)throw new Exception("Manual lightning missed cursor");
  manualAim=false;enemies.Clear();shots.Clear();Array.Clear(weapons,0,6);weapons[0]=1;cooldown[0]=0;UpdateWeapons(.01f);if(shots.Count!=0)throw new Exception("Auto orb fired without target");
  save=original;selectedDragon=pet;selectedCurse=curse;manualAim=oldMode;verifying=oldVerify;ReturnMenu();return "Automatic nearest target, manual independent target, no empty-space firing, blades/flame, lightning cursor/range and zero-distance fallback PASS.";
 }
}
}
