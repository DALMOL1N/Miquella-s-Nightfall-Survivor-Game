using System;
using System.Collections.Generic;
using UnityEngine;
namespace NoiteSemFim {
public sealed partial class SurvivorGame {
 enum Element {None,Fire,Ice,Lightning}
 enum GroundKind {Fire,Frost,Water,Steam}
 sealed class GroundEffect {public Vector2 p;public float radius,life,age,dps,charged;public GroundKind kind;public bool hostile;public SpriteRenderer sprite;}
 sealed class Fuel {public Vector2 p;public SpriteRenderer sprite,flame;public bool grass,charred;public float burn;}
 readonly List<GroundEffect> ground=new List<GroundEffect>();
 readonly List<Fuel> fuel=new List<Fuel>();
 float elementClock,playerBurn,playerBurnDps,playerWet,playerChill,playerShock;
 SpriteRenderer playerStatus;
 bool elementTraining,elementPreviewFrozen;
 const int GroundLimit=56;
 Color ElementColor(Element e)=>e==Element.Ice?new Color(.3f,.85f,1):e==Element.Lightning?new Color(.8f,.5f,1):new Color(1,.43f,.15f);
 void ResetElements(){ground.Clear();fuel.Clear();elementClock=playerBurn=playerBurnDps=playerWet=playerChill=playerShock=0;playerStatus=null;}
 void AddFuel(SpriteRenderer sprite,Vector2 p,bool grass){fuel.Add(new Fuel{sprite=sprite,p=p,grass=grass});}
 void BuildElementTerrain(){var r=new System.Random(461+stageIndex);for(int i=0;i<40;i++){Vector2 p=i<8?new Vector2(4+i%4*1.7f,-2+i/4*2):new Vector2((float)r.NextDouble()*80-40,(float)r.NextDouble()*80-40);var sprite=Sprite("Dry Grass",ArtBank.ElementFx(8),p,1,Color.white,Depth(p)-1);AddFuel(sprite,p,true);}AddGround(new Vector2(-4,-1),1.3f,GroundKind.Water,-1,0);AddGround(new Vector2(3,4),1.15f,GroundKind.Water,-1,0);AddGround(new Vector2(-12,8),1.5f,GroundKind.Water,-1,0);AddGround(new Vector2(13,-7),1.4f,GroundKind.Water,-1,0);}
 Sprite GroundSprite(GroundEffect g)=>g.kind==GroundKind.Fire?ArtBank.Status(0,ArtBank.Frame(g.age,9)):g.kind==GroundKind.Steam?ArtBank.Status(4,Mathf.Min(3,(int)(g.age*4))):g.kind==GroundKind.Water&&g.charged>0?ArtBank.Status(5,ArtBank.Frame(g.age,10)):ArtBank.ElementFx(GroundIcon(g));
 int GroundIcon(GroundEffect g)=>g.kind==GroundKind.Fire?(int)(time*6)%2:g.kind==GroundKind.Frost?2:g.kind==GroundKind.Steam?10:g.charged>0?11:3;
 GroundEffect AddGround(Vector2 p,float radius,GroundKind kind,float life,float dps,bool hostile=false){radius=Mathf.Clamp(radius,.4f,1.8f);foreach(var g in ground)if(g.kind==kind&&(kind!=GroundKind.Frost||g.hostile==hostile)&&(g.p-p).sqrMagnitude<.65f*.65f){if(g.life>=0)g.life=Mathf.Max(g.life,life);g.dps=Mathf.Max(g.dps,dps);return g;}if(ground.Count>=GroundLimit)return null;var effect=new GroundEffect{p=p,radius=radius,kind=kind,life=life,dps=dps,hostile=hostile};effect.sprite=Sprite("Element Ground "+kind,GroundSprite(effect),p,radius*1.9f,Color.white,kind==GroundKind.Fire?Depth(p)+1:-200);ground.Add(effect);return effect;}
 void RemoveGround(int index){Dispose(ground[index].sprite.gameObject);ground.RemoveAt(index);}
 void Steam(Vector2 p){AddGround(p,.7f,GroundKind.Steam,.9f,0);}
 void ReactElement(Vector2 p,float radius,Element element,float damage,bool hostile=false,bool leaveGround=true){if(element==Element.None)return;
  if(element==Element.Fire){foreach(var g in ground)if((g.kind==GroundKind.Water||g.kind==GroundKind.Frost)&&(g.p-p).magnitude<g.radius+.2f){Steam(p);return;}AddGround(p,Mathf.Min(radius,.9f),GroundKind.Fire,3.5f,Mathf.Clamp(damage*.15f,2,6));return;}
  if(element==Element.Ice){bool quenched=false;for(int i=ground.Count-1;i>=0;i--){var g=ground[i];if(g.kind==GroundKind.Fire&&(g.p-p).magnitude<radius+g.radius){RemoveGround(i);quenched=true;}}foreach(var f in fuel)if(f.burn>0&&(f.p-p).magnitude<radius+.3f)ExtinguishFuel(f,false);if((pos-p).magnitude<radius){playerBurn=0;if(hostile)playerChill=Mathf.Max(playerChill,1.5f);}if(!hostile)foreach(var e in enemies)if((e.p-p).magnitude<radius){FreezeEnemy(e);}if(leaveGround)AddGround(p,Mathf.Min(radius,1.2f),GroundKind.Frost,4,0,hostile);if(quenched)Steam(p);return;}
  foreach(var g in ground)if(g.kind==GroundKind.Water&&(g.p-p).magnitude<radius+g.radius){g.charged=3;g.dps=Mathf.Clamp(damage*.15f,3,7);}
  foreach(var e in enemies)if(e.wet>0&&(e.p-p).magnitude<radius)e.shock=1;
 }
 void FreezeEnemy(Enemy enemy){enemy.burn=0;enemy.frozen=Mathf.Max(enemy.frozen,IsBoss(enemy.type)?.35f:.8f);enemy.slow=Mathf.Max(enemy.slow,2);}
 void BurnPlayer(float duration,float dps){if(playerWet>0||dashTimer>0)return;playerBurn=Mathf.Max(playerBurn,duration);playerBurnDps=Mathf.Max(playerBurnDps,Mathf.Clamp(dps,1,7));}
 void BurnEnemy(Enemy e,float duration,float dps){if(e.wet>0)return;e.burn=Mathf.Max(e.burn,duration);e.burnDps=Mathf.Max(e.burnDps,Mathf.Clamp(dps,1,9));}
 void HurtPlayer(float amount,bool continuous=false){if(hp<=0||dashTimer>0||(!continuous&&invincible>0))return;hp=Mathf.Max(0,hp-amount*(activeCurse==2?1.3f:1)*IncomingMultiplier);if(!continuous){invincible=.7f;shake=.13f;Play(hitSound);}if(hp<=0)EndRun(false);}
 void IgniteFuel(Fuel f){if(f.charred||f.burn>0)return;foreach(var g in ground)if((g.kind==GroundKind.Water||g.kind==GroundKind.Frost)&&(g.p-f.p).sqrMagnitude<g.radius*g.radius)return;f.burn=4;f.flame=Sprite("Burning Terrain",ArtBank.Status(1),f.p,f.grass?1:1.7f,Color.white,Depth(f.p)+4);AddGround(f.p,.65f,GroundKind.Fire,3.5f,3);}
 void ExtinguishFuel(Fuel f,bool consumed){f.burn=0;if(f.flame!=null)Dispose(f.flame.gameObject);f.flame=null;if(!consumed)return;f.charred=true;if(f.grass)f.sprite.sprite=ArtBank.ElementFx(9);else f.sprite.color=new Color(.25f,.24f,.27f);}
 void SpreadFire(Vector2 p,float radius){foreach(var f in fuel)if(!f.charred&&f.burn<=0&&(f.p-p).sqrMagnitude<radius*radius){IgniteFuel(f);break;}}
 void UpdateElements(float dt){playerWet=Mathf.Max(0,playerWet-dt);playerChill=Mathf.Max(0,playerChill-dt);playerShock=Mathf.Max(0,playerShock-dt);
  for(int i=ground.Count-1;i>=0;i--){var g=ground[i];g.age+=dt;if(g.life>=0){g.life-=dt;if(g.life<=0){RemoveGround(i);continue;}}g.charged=Mathf.Max(0,g.charged-dt);g.sprite.sprite=GroundSprite(g);g.sprite.color=new Color(1,1,1,g.kind==GroundKind.Steam?Mathf.Clamp01(g.life):g.kind==GroundKind.Fire&&g.age<.45f?.45f:1);}
  foreach(var f in fuel)if(f.burn>0){f.burn-=dt;if(f.burn<=0)ExtinguishFuel(f,true);else if(f.flame!=null)f.flame.sprite=ArtBank.Status(1,ArtBank.Frame(4-f.burn,9));}
  elementClock+=dt;if(elementClock>=.25f){float step=elementClock;elementClock=0;
   // Water is resolved first so overlapping water protects against ignition.
   foreach(var g in ground)if(g.kind==GroundKind.Water){if((g.p-pos).sqrMagnitude<g.radius*g.radius){playerWet=2;playerBurn=0;}foreach(var e in enemies)if((g.p-e.p).sqrMagnitude<g.radius*g.radius){e.wet=2;e.burn=0;}}
   for(int i=ground.Count-1;i>=0;i--){var g=ground[i];if(g.kind==GroundKind.Steam||g.age<.45f)continue;bool atPlayer=(g.p-pos).sqrMagnitude<g.radius*g.radius;
    if(g.kind==GroundKind.Fire){SpreadFire(g.p,g.radius+1.05f);if(atPlayer)BurnPlayer(2.5f,g.dps);for(int j=enemies.Count-1;j>=0;j--)if((enemies[j].p-g.p).sqrMagnitude<g.radius*g.radius)BurnEnemy(enemies[j],2.5f,g.dps);}
    else if(g.kind==GroundKind.Frost){if(atPlayer){playerBurn=0;if(g.hostile)playerChill=Mathf.Max(playerChill,1);}if(!g.hostile)foreach(var e in enemies)if((e.p-g.p).sqrMagnitude<g.radius*g.radius){e.burn=0;e.slow=Mathf.Max(e.slow,1);}}
    else if(g.charged>0){if(atPlayer){playerShock=.6f;HurtPlayer(g.dps*step,true);if(state!=ScreenState.Playing)return;}for(int j=enemies.Count-1;j>=0;j--){var e=enemies[j];if((e.p-g.p).sqrMagnitude<g.radius*g.radius){e.shock=.6f;Damage(e,g.dps*step*2);if(state!=ScreenState.Playing)return;}}}
   }
   if(playerBurn>0){playerBurn=Mathf.Max(0,playerBurn-step);HurtPlayer(playerBurnDps*step,true);if(state!=ScreenState.Playing)return;SpreadFire(pos,.85f);}else playerBurnDps=0;
   for(int i=enemies.Count-1;i>=0;i--){var e=enemies[i];e.wet=Mathf.Max(0,e.wet-step);e.shock=Mathf.Max(0,e.shock-step);if(e.burn>0){e.burn=Mathf.Max(0,e.burn-step);e.spreadTick-=step;if(e.spreadTick<=0){e.spreadTick=1;SpreadFire(e.p,.8f);foreach(var other in enemies)if(other!=e&&(other.p-e.p).sqrMagnitude<.45f*.45f)BurnEnemy(other,1,2);}Damage(e,e.burnDps*step);if(state!=ScreenState.Playing)return;}else e.burnDps=0;}
  }
  UpdateStatusSprite(ref playerStatus,pos,playerBurn>0,playerChill>0,playerShock>0,hero==null?100:hero.sortingOrder,1.2f,heroAnimClock);
  foreach(var e in enemies)UpdateStatusSprite(ref e.status,e.p,e.burn>0,e.frozen>0||e.slow>0,e.shock>0,e.sprite.sortingOrder,IsBoss(e.type)?2.1f:1,e.animClock);
 }
 void UpdateStatusSprite(ref SpriteRenderer sprite,Vector2 p,bool burning,bool frozen,bool shocked,int order,float scale,float clock){bool active=burning||frozen||shocked;if(!active){if(sprite!=null)sprite.enabled=false;return;}int row=burning?1:shocked?3:2;int frame=ArtBank.Frame(clock,9);if(sprite==null)sprite=Sprite("Element Status",ArtBank.Status(row,frame),p,scale,Color.white,order+3);sprite.enabled=true;sprite.sprite=ArtBank.Status(row,frame);sprite.transform.position=p+Vector2.up*(scale*.48f);sprite.sortingOrder=order+3;}
 void StartElementTraining(){StartRun();elementTraining=true;activeCurse=-1;runRelics=RelicStats.Empty;runArmor=runTalisman=-1;runRelicRewards.Clear();Array.Clear(weapons,0,weapons.Length);hp=maxHp=250;Toast("TREINO: aponte o mouse e use 1 fogo, 2 gelo, 3 raio. Esc para pausar.");for(int i=0;i<6;i++){var e=SpawnEnemy(i==5?5:0,false);e.p=new Vector2(3+i%3*1.5f,-1+i/3*2);e.hp=e.maxHp=100;e.sprite.transform.position=e.p;}}
 void TrainingInput(){if(!elementTraining)return;Vector2 target=cam.ScreenToWorldPoint(Input.mousePosition);target=pos+Vector2.ClampMagnitude(target-pos,8);if(Input.GetKeyDown(KeyCode.Alpha1))ReactElement(target,.9f,Element.Fire,25);if(Input.GetKeyDown(KeyCode.Alpha2)){ReactElement(target,2,Element.Ice,12);DamageArea(target,2,12,true);}if(Input.GetKeyDown(KeyCode.Alpha3)){ReactElement(target,1.5f,Element.Lightning,25);DamageArea(target,1.5f,25,false);Burst(target,new Color(.8f,.5f,1),1,.3f);}}
 void DrawElementHud(){if(elementTraining&&!buildLabActive){Frame(1,330,211,620,37);Text("TREINO • Mire com o mouse: 1 fogo / 2 gelo / 3 raio • Esc: pausar",345,215,590,28,Style(13,gold,TextAnchor.MiddleCenter,true));}string status=playerBurn>0?"EM CHAMAS "+playerBurn.ToString("0.0")+"s":playerShock>0?"ELETRIZADO":playerChill>0?"GELADO":playerWet>0?"MOLHADO":"";if(status.Length>0){Frame(1,335,507,290,37);DrawIcon(ArtBank.ElementFx(playerBurn>0?4:playerShock>0?7:playerChill>0?6:3),343,510,30);Text(status,380,511,232,26,Style(13,playerBurn>0?new Color(1,.6f,.25f):cyan,TextAnchor.MiddleCenter,true));}}
 string VerifyElements(){var original=save;bool oldVerify=verifying;int oldDragon=selectedDragon,oldCurse=selectedCurse;verifying=true;save=new SaveData();selectedDragon=0;selectedCurse=-1;StartRun();ground.Clear();fuel.Clear();hp=maxHp=1000;
  var e=SpawnEnemy(0,false);e.p=new Vector2(6,0);e.hp=e.maxHp=1000;ReactElement(e.p,.7f,Element.Fire,20);float before=e.hp;for(int i=0;i<8;i++)UpdateElements(.25f);if(e.hp>=before||e.burn<=0)throw new Exception("Enemy ground burn failed");
  pos=e.p;float playerBefore=hp;for(int i=0;i<5;i++)UpdateElements(.25f);if(hp>=playerBefore||playerBurn<=0)throw new Exception("Own fire must hurt player");
  ReactElement(pos,1.5f,Element.Ice,10);if(playerBurn>0||e.burn>0||ground.Exists(g=>g.kind==GroundKind.Fire))throw new Exception("Ice extinguish failed");
  ground.Clear();playerBurn=0;playerWet=0;var water=AddGround(pos,1,GroundKind.Water,-1,0);UpdateElements(.25f);BurnPlayer(3,5);if(playerBurn>0)throw new Exception("Wet immunity failed");int count=ground.Count;ReactElement(pos,.7f,Element.Fire,10);if(ground.Exists(g=>g.kind==GroundKind.Fire))throw new Exception("Fire on water failed");ReactElement(pos,1,Element.Lightning,20);playerBefore=hp;before=e.hp;UpdateElements(.5f);if(hp>=playerBefore||e.hp>=before||water.charged<=0)throw new Exception("Conductive water failed");
  ground.Clear();fuel.Clear();pos=Vector2.zero;var grass=Sprite("Test Fuel",ArtBank.ElementFx(8),new Vector2(4,0),1,Color.white);AddFuel(grass,new Vector2(4,0),true);ReactElement(new Vector2(4,0),.6f,Element.Fire,10);for(int i=0;i<20;i++)UpdateElements(.25f);if(!fuel[0].charred)throw new Exception("Terrain combustion failed");
  for(int i=0;i<100;i++)AddGround(new Vector2(i*3,20),.6f,GroundKind.Fire,1,3);if(ground.Count>GroundLimit)throw new Exception("Ground limit failed");for(int i=0;i<10;i++)UpdateElements(.25f);if(ground.Exists(g=>g.kind==GroundKind.Fire))throw new Exception("Ground expiry failed");
  StartRun();if(playerBurn>0||playerChill>0||ground.Exists(g=>g.kind==GroundKind.Fire)||fuel.Exists(f=>f.charred))throw new Exception("Element run reset failed");
  var caster=SpawnEnemy(5,false);caster.p=new Vector2(5,0);caster.attack=0;UpdateEnemies(.05f);if(!shots.Exists(s=>s.hostile&&s.element==Element.Fire))throw new Exception("Enemy elemental attack failed");
  ground.Clear();hazards.Clear();AddHazard(new Vector2(8,0),.8f,15,Element.Fire);UpdateHazards(1.3f);if(!ground.Exists(g=>g.kind==GroundKind.Fire))throw new Exception("Boss elemental ground failed");
  int accountCoins=save.coins,accountKills=save.totalKills,accountUnlocks=save.curseUnlocks;StartElementTraining();UnlockCurse(7);EndRun(false);if(save.coins!=accountCoins||save.totalKills!=accountKills||save.curseUnlocks!=accountUnlocks)throw new Exception("Training altered progression");
  save=original;selectedDragon=oldDragon;selectedCurse=oldCurse;verifying=oldVerify;ReturnMenu();return "Elemental fire/player-enemy DOT, ice extinguish, water protection/conduction, burning terrain, caster/boss attacks, lifetime/cap/reset and isolated training PASS.";
 }
}
}
