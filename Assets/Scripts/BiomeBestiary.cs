using System;
using System.Collections.Generic;
using UnityEngine;
namespace NoiteSemFim {
// Readable, transparent animation atlases: one stable foot anchor and scale per creature.
public static class BiomeSprites {
 public static readonly string[] Sheets={"bestiary-forest-v26","bestiary-moon-v26","bestiary-forge-v26","bestiary-eclipse-v26"};
 static readonly Dictionary<int,Sprite[]> cache=new Dictionary<int,Sprite[]>();
 public static Sprite Get(int biome,int kind,int frame){
  if(!cache.TryGetValue(biome,out var sprites)){
   var t=Resources.Load<Texture2D>("Art/"+Sheets[biome]);
   if(t==null||!t.isReadable)throw new InvalidOperationException("Missing readable bestiary atlas: "+Sheets[biome]);
   var px=t.GetPixels32();int w=t.width/4,h=t.height/4;sprites=new Sprite[16];
   for(int row=0;row<4;row++){
    var boxes=new RectInt[4];var heights=new List<int>();
    for(int col=0;col<4;col++){
     int x0=col*w,y0=t.height-(row+1)*h,minX=x0+w,minY=y0+h,maxX=x0-1,maxY=y0-1;
     for(int y=y0;y<y0+h;y++)for(int x=x0;x<x0+w;x++)if(px[y*t.width+x].a>=96){minX=Mathf.Min(minX,x);minY=Mathf.Min(minY,y);maxX=Mathf.Max(maxX,x);maxY=Mathf.Max(maxY,y);}
     if(maxX-minX>=w-2&&maxY-minY>=h-2)throw new InvalidOperationException("Opaque bestiary cell or border: "+Sheets[biome]+"/"+row+"/"+col);
     if(maxX<minX)throw new InvalidOperationException("Empty bestiary pose "+biome+"/"+row+"/"+col);
     boxes[col]=new RectInt(minX,minY,maxX-minX+1,maxY-minY+1);heights.Add(maxY-minY+1);
    }
    heights.Sort();float ppu=heights[2]/1.08f;
    for(int col=0;col<4;col++){var b=boxes[col];var s=UnityEngine.Sprite.Create(t,new Rect(b.x,b.y,b.width,b.height),new Vector2(.5f,0),ppu,0,SpriteMeshType.FullRect);s.name=Sheets[biome]+"_"+row+"_"+col;sprites[row*4+col]=s;}
   }
   cache[biome]=sprites;
  }
  int pose=frame==5?3:frame==0?0:1+(frame-1)%2;
  return sprites[Mathf.Clamp(kind,0,3)*4+pose];
 }
 public static string Audit(){for(int b=0;b<4;b++)for(int k=0;k<4;k++)for(int f=0;f<6;f++)if(Get(b,k,f)==null)throw new Exception("Missing biome frame");return "16 unique creatures/bosses, 64 transparent animation poses, calibrated per-row scale and foot anchors";}
}
public sealed partial class SurvivorGame {
 static readonly string[,] BiomeNames={
  {"Lobo Espinhoso","Andarilho das Raízes","Xamã dos Esporos","Cervo das Raízes Mortas"},
  {"Duelista Ossuário","Lamento Lunar","Sacerdote da Lua","Rainha da Lua Pálida"},
  {"Cão de Escória","Sentinela de Ferro","Artilheiro da Forja","Carrasco da Fornalha"},
  {"Louva-a-deus Sombrio","Olho do Vazio","Arauto da Fenda","Soberano do Eclipse"}
 };
 Sprite EnemyAppearance(Enemy e,int frame=0)=>e.biome>=0?BiomeSprites.Get(e.biome,e.kind,frame):ArtBank.Enemy(e.type,frame);
 string EnemyName(Enemy e)=>e!=null&&e.biome>=0?BiomeNames[e.biome,e.kind]:"O Soberano";
 void ConfigureBiomeEnemy(Enemy e){
  if(!campaignActive&&!final3DActive&&!(e.final&&e.type==4))return;
  e.biome=final3DActive||e.type==4?3:stageIndex;e.kind=IsBoss(e.type)?3:e.type==5?2:e.type%3;
  if(e.kind==3){e.speed=e.biome==1?.95f:e.biome==2?.8f:1.05f;e.attack=2.5f;return;}
  // Each wave contains a runner, a positional threat and a ranged specialist.
  float hpBase=e.kind==0?18:e.kind==1?48:26;
  e.hp=e.maxHp=hpBase*(1+Mathf.Clamp01(time/Duration)*1.8f)*(1+e.biome*.15f);
  e.speed=e.kind==0?2.1f:e.kind==1?1.05f:1.2f;e.damage=e.kind==1?13:8;e.attack=1.2f+UnityEngine.Random.value*1.2f;
 }
 void CancelBiomeWindup(Enemy e){if(e.warning!=null)Dispose(e.warning.gameObject);e.warning=null;e.windup=0;}
 float BiomeReach(Enemy e)=>e.kind==3?12:e.kind==0?6:e.biome==1&&e.kind==1?9:e.kind==2?10:3;
 void AdvanceBiomeFSM(Enemy e,float dt){
  if(e.windup>0){e.windup=Mathf.Max(0,e.windup-dt);e.animAttack=.15f;e.aiState=EnemyState.Attacking;return;}
  e.attack-=dt;
  if(e.attack<=0&&(e.p-pos).sqrMagnitude<BiomeReach(e)*BiomeReach(e)){
   e.lockedAim=pos;e.windup=e.kind==3?1.05f:.65f;e.animAttack=e.windup;
   var color=e.biome==0?new Color(.7f,.9f,.3f):e.biome==1?new Color(.4f,.85f,1):e.biome==2?new Color(1,.45f,.1f):new Color(.85f,.4f,1);
   e.warning=Sprite("Preparação • "+EnemyName(e),ArtBank.Ring,e.lockedAim,e.kind==3?2.8f:1.4f,color,700);
   e.aiState=EnemyState.Attacking;
  }else e.aiState=EnemyState.Chasing;
 }
 Vector2 BiomeMotion(Enemy e,Vector2 delta){
  if(e.biome<0)return delta.normalized;
  if(e.windup>0||e.chargeTime>0||e.aiState==EnemyState.Attacking)return Vector2.zero;
  float d=delta.magnitude;var side=new Vector2(-delta.y,delta.x).normalized;
  if(e.kind==2||e.biome==1&&e.kind==3||e.biome==3&&e.kind==1)return d<3?-delta.normalized:d>6?delta.normalized:side*.65f;
  if(e.biome==1&&e.kind==1)return d>4?delta.normalized:side;
  return delta.normalized;
 }
 void BiomeFan(Enemy e,int count,float spread,Element element,float speed=4.5f){
  var aim=SafeDirection(e.p,e.lockedAim,Vector2.down);
  for(int i=0;i<count;i++)Fire(e.p,Rotate(aim,(i-(count-1)*.5f)*spread)*speed,e.damage*.7f,.2f,1,ElementColor(element),3.2f,0,element,true);
 }
 void BiomeRing(Enemy e,int count,Element element){for(int i=0;i<count;i++){var dir=Rotate(Vector2.right,i*Mathf.PI*2/count+e.attackCycle*.25f);Fire(e.p,dir*3.5f,e.damage*.65f,.2f,1,ElementColor(element),3.5f,0,element,true);}}
 void BiomeLane(Enemy e,Element element,int count,bool cross=false){
  var dir=SafeDirection(e.p,e.lockedAim,Vector2.down);if(cross)dir=new Vector2(-dir.y,dir.x);
  for(int i=1;i<=count;i++)AddHazard(e.p+dir*i*1.6f,.8f,e.damage*.75f,element);
 }
 void ExecuteBiomeAttack(Enemy e){
  if(e.windup>0)return;
  CancelBiomeWindup(e);e.attackCycle++;e.attack=e.kind==3?(e.phaseTwo?2.9f:4.2f):e.kind==0?3.5f:e.kind==1?4.5f:4;
  if(e.kind<3){
   if(e.biome==0){if(e.kind==0){e.chargeDirection=SafeDirection(e.p,e.lockedAim,Vector2.down);e.chargeTime=.45f;}else if(e.kind==1)BiomeLane(e,Element.None,2);else BiomeFan(e,3,.22f,Element.None,3.5f);}
   else if(e.biome==1){if(e.kind==0){e.chargeDirection=SafeDirection(e.p,e.lockedAim,Vector2.down);e.chargeTime=.3f;}else if(e.kind==1)BiomeRing(e,5,Element.Ice);else BiomeFan(e,3,.18f,Element.Ice);}
   else if(e.biome==2){if(e.kind==0){AddHazard(e.lockedAim,1.1f,e.damage,Element.Fire);e.chargeDirection=SafeDirection(e.p,e.lockedAim,Vector2.down);e.chargeTime=.28f;}else if(e.kind==1)BiomeLane(e,Element.Fire,2);else BiomeFan(e,1,0,Element.Fire,5.5f);}
   else {if(e.kind==0){e.chargeDirection=SafeDirection(e.p,e.lockedAim,Vector2.down);e.chargeTime=.55f;}else if(e.kind==1)BiomeFan(e,5,.2f,Element.Lightning,3.8f);else {BiomeRing(e,6,Element.None);AddHazard(e.lockedAim,.8f,e.damage,Element.Lightning);}}
   return;
  }
  // Bosses rotate distinct, readable attacks instead of sharing one hazard loop.
  if(e.biome==0){
   if(e.attackCycle%2==0){e.chargeDirection=SafeDirection(e.p,e.lockedAim,Vector2.down);e.chargeTime=e.phaseTwo?.7f:.5f;}
   else {BiomeLane(e,Element.None,e.phaseTwo?6:4);if(e.phaseTwo)BiomeLane(e,Element.None,4,true);}
   if(e.phaseTwo)BiomeFan(e,3,.4f,Element.None,3);
  }else if(e.biome==1){
   if(e.attackCycle%2==0)BiomeRing(e,e.phaseTwo?12:8,Element.Ice);
   else {BiomeFan(e,e.phaseTwo?7:3,.18f,Element.Ice);AddHazard(e.lockedAim,1.3f,e.damage,Element.Ice);}
  }else if(e.biome==2){
   if(e.attackCycle%2==0){BiomeLane(e,Element.Fire,e.phaseTwo?7:5);if(e.phaseTwo)BiomeLane(e,Element.Fire,5,true);}
   else {AddHazard(e.lockedAim,2,e.damage,Element.Fire);if(e.phaseTwo)BiomeFan(e,5,.25f,Element.Fire,3.2f);}
  }else {
   int pattern=e.attackCycle%3;
   if(pattern==0){BiomeRing(e,e.phaseTwo?14:10,Element.Lightning);BiomeFan(e,3,.23f,Element.Fire);}
   else if(pattern==1){for(int i=-2;i<=2;i++)if(i!=0)AddHazard(e.lockedAim+Vector2.right*i*2,1,e.damage,Element.Lightning);if(e.phaseTwo)BiomeFan(e,5,.18f,Element.Ice);}
   else {int count=enemies.FindAll(x=>x.biome==3&&x.kind<3).Count;if(count<8)for(int i=0;i<2;i++){var minion=SpawnEnemy(i==0?0:2,false);minion.p=e.p+new Vector2(i==0?-3:3,1);minion.sprite.transform.position=minion.p;}BiomeFan(e,e.phaseTwo?7:3,.22f,Element.None);}
  }
 }
 void SpawnEclipseWaves(float dt){spawnTimer-=dt;if(spawnTimer>0||enemies.Count>=24)return;spawnTimer=1.6f;SpawnEnemy(UnityEngine.Random.Range(0,3),false);}
 public string VerifyBiomeBestiary(){
  var account=save;bool oldVerify=verifying;int oldHero=heroIndex,oldStage=stageIndex,pet=selectedDragon,curse=selectedCurse;bool oldQuick=quickMode;
  string disk=PlayerPrefs.GetString(EditorTestProfile.Key("NSF.Save"),"");verifying=true;ReturnMenu();save=new SaveData{completedStages=7};heroIndex=0;selectedDragon=selectedCurse=-1;quickMode=true;
  try{
   string art=BiomeSprites.Audit();
   for(int b=0;b<4;b++){
    if(b<3)StartCampaignStage(b);else {ReturnMenu();StartFinal3DDemo();AdvanceFinalTransition(4);}
    hp=maxHp=100000;
    for(int k=0;k<4;k++){
     var e=k==3?(b==3?finalBoss:SpawnEnemy(StageBossTypes[b],true)):SpawnEnemy(k,false);e.p=pos+Vector2.right*2;e.sprite.transform.position=e.p;e.attack=0;
     if(e.biome!=b||e.kind!=k||EnemyAppearance(e)==null)throw new Exception("Wrong biome identity");
     int before=shots.Count+hazards.Count;AdvanceEnemyFSM(e,.01f);ExecuteEnemyAttack(e);
     if(e.windup<=0||e.warning==null||shots.Count+hazards.Count!=before)throw new Exception("Attack lacks preparation");
     e.frozen=.1f;AdvanceEnemyFSM(e,.01f);if(e.warning!=null||e.windup!=0||e.aiState!=EnemyState.Frozen)throw new Exception("Freeze does not interrupt preparation");
     e.frozen=0;e.attack=0;AdvanceEnemyFSM(e,.01f);AdvanceEnemyFSM(e,2);ExecuteEnemyAttack(e);
     if(e.attack<=0||e.warning!=null||e.attackCycle!=1)throw new Exception("Prepared attack did not release once");
     AdvanceEnemyFSM(e,.01f);if(e.aiState==EnemyState.Attacking)throw new Exception("Attack repeated without cooldown");
     if(k==3){Damage(e,e.maxHp*.51f);if(!e.phaseTwo)throw new Exception("Missing boss phase two");for(int i=0;i<3;i++){e.attack=0;AdvanceEnemyFSM(e,.01f);AdvanceEnemyFSM(e,2);ExecuteEnemyAttack(e);}if(b<3){Damage(e,e.maxHp+1);PrepareStageRescue();if(state!=ScreenState.StageRescue||enemies.Count!=0)throw new Exception("New boss broke rescue");}else {var edge=SpawnEnemy(2,false);edge.p=finalOrigin+Vector2.right*24;edge.attack=10;UpdateEnemies(.01f);if((edge.p-finalOrigin).magnitude>19.01f)throw new Exception("Ranged enemy escaped final arena");FinalCamera();SyncFinal3D();if(pixelFinal.Count<2)throw new Exception("Missing 2.5D sprites");Damage(e,e.maxHp+1);if(state!=ScreenState.Result)throw new Exception("Eclipse victory failed");}}
    }
    ReturnMenu();
   }
   if(PlayerPrefs.GetString(EditorTestProfile.Key("NSF.Save"),"")!=disk)throw new Exception("Bestiary test altered saved progress");
   return "PASS: "+art+"; 4 exclusive enemy pools; 12 specialist enemies; 4 bosses and phase-two patterns; telegraph before damage; freeze cancellation; one-shot release and cooldown; boss rescue flow; Eclipse 2.5D sprite rendering/victory and enemy arena bounds; saved progress preserved.";
  }finally{ReturnMenu();save=account;verifying=oldVerify;heroIndex=oldHero;stageIndex=oldStage;selectedDragon=pet;selectedCurse=curse;quickMode=oldQuick;}
 }
}
}
