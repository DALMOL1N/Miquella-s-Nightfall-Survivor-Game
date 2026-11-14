using System;
using System.Collections;
using UnityEngine;
namespace NoiteSemFim {
public sealed partial class SurvivorGame {
 static readonly string[] StageNames={"Bosque das Cinzas","Necrópole Lunar","Forja Esquecida","Arena do Eclipse"};
 static readonly string[] StageBossNames={"Cervo das Raízes Mortas","Rainha da Lua Pálida","Carrasco da Fornalha","Soberano do Eclipse"};
 static readonly int[] StageBossTypes={3,6,7,4},StageHeroes={3,4,5,-1};
 bool campaignActive,campaignAwaitingRescue,campaignRescueReady,campaignStageComplete;
 string campaignRescueMessage="";
 bool StageUnlocked(int index)=>index>=0&&index<4&&(index==0||(save.completedStages&(1<<(index-1)))!=0);
 void ResetCampaign(){campaignActive=campaignAwaitingRescue=campaignRescueReady=campaignStageComplete=false;campaignRescueMessage="";}
 void ResumeGame(){state=campaignActive&&campaignAwaitingRescue?ScreenState.StageRescue:ScreenState.Playing;}
 bool StartCampaignStage(int index,bool carryBuild=false){
  if(!StageUnlocked(index)||buildLabActive||elementTraining||final3DDemo)return false;
  bool carry=carryBuild&&campaignActive&&campaignStageComplete&&!finalBonusPending&&state==ScreenState.Result&&index==stageIndex+1;
  if(carryBuild&&!carry)return false;
  var oldWeapons=(int[])weapons.Clone();var oldPassives=(int[])passives.Clone();var oldEvolved=(bool[])evolved.Clone();
  int oldLevel=level,oldCurse=activeCurse,oldBlessings=dragonBlessings;float oldXp=xp,oldHp=hp,oldMaxHp=maxHp;
  stageIndex=index;StartRun();campaignActive=true;if(index<3)BuildCampaignMap();
  if(carry){Array.Copy(oldWeapons,weapons,6);Array.Copy(oldPassives,passives,6);Array.Copy(oldEvolved,evolved,6);level=oldLevel;xp=oldXp;activeCurse=oldCurse;dragonBlessings=oldBlessings;maxHp=oldMaxHp;hp=Mathf.Min(maxHp,oldHp+maxHp*.3f);}
  if(index==3){finalSpawned=true;finalBoss=SpawnEnemy(4,true);finalBoss.hp=finalBoss.maxHp=2600+level*90;BeginFinal3D();Toast("FASE FINAL • ARENA DO ECLIPSE • Derrote o Soberano!");}
  else Toast("FASE "+(index+1)+" • "+StageNames[index]+" • Resgate: "+Catalog.Heroes[StageHeroes[index]].Split(',')[0]);
  return true;
 }
 void BuildCampaignMap(){
  foreach(var r in scenery)if(r!=null)Dispose(r.gameObject);scenery.Clear();
  foreach(var f in fuel){if(f.sprite!=null)Dispose(f.sprite.gameObject);if(f.flame!=null)Dispose(f.flame.gameObject);}
  foreach(var g in ground)if(g.sprite!=null)Dispose(g.sprite.gameObject);ResetElements();
  var road=Sprite("Caminho da fase",ArtBank.Floor(stageIndex==0?1:stageIndex),Vector2.zero,1,new Color(1.15f,1.15f,1.15f),-9999);
  road.drawMode=SpriteDrawMode.Tiled;road.size=stageIndex==1?new Vector2(4,44):new Vector2(3,40);
  if(stageIndex==1){var cross=Sprite("Alameda da necrópole",ArtBank.Floor(1),Vector2.zero,1,new Color(1.15f,1.15f,1.15f),-9998);cross.drawMode=SpriteDrawMode.Tiled;cross.size=new Vector2(42,3);}
  for(int i=0;i<72;i++){float a=i*Mathf.PI*2/72;Vector2 p=new Vector2(Mathf.Cos(a)*27,Mathf.Sin(a)*27);CampaignProp(p,i);}
  var random=new System.Random(8200+stageIndex);
  for(int i=0;i<58;i++){Vector2 p=new Vector2((float)random.NextDouble()*46-23,(float)random.NextDouble()*46-23);if(Mathf.Abs(p.x)<3||p.sqrMagnitude<25||(p-new Vector2(0,8)).sqrMagnitude<20||stageIndex==1&&Mathf.Abs(p.y)<3)continue;CampaignProp(p,i);}
  for(int side=-1;side<=1;side+=2)for(int i=0;i<8;i++)CampaignProp(new Vector2(side*(stageIndex==0?5.5f:7),-18+i*5),i);
  if(stageIndex==0){for(int i=0;i<24;i++){Vector2 p=new Vector2((i%2==0?-1:1)*(4+i%5*2),-16+i*1.4f);var grass=Sprite("Mato seco",ArtBank.ElementFx(8),p,1,Color.white,Depth(p)-1);AddFuel(grass,p,true);}AddGround(new Vector2(-10,3),1.5f,GroundKind.Water,-1,0);AddGround(new Vector2(11,-8),1.5f,GroundKind.Water,-1,0);}
  else if(stageIndex==1){foreach(var p in new[]{new Vector2(-9,-6),new Vector2(9,6),new Vector2(-12,13)})AddGround(p,1.5f,GroundKind.Water,-1,0);}
  else {AddGround(new Vector2(-12,-9),1.3f,GroundKind.Fire,-1,3);AddGround(new Vector2(12,10),1.3f,GroundKind.Fire,-1,3);AddGround(new Vector2(-8,14),1.3f,GroundKind.Fire,-1,3);}
 }
 void CampaignProp(Vector2 p,int index){int id=stageIndex*4+index%4;var r=Sprite("Cenário • "+StageNames[stageIndex],ArtBank.Prop(id),p,stageIndex==0?2:1.5f,Color.white,Depth(p));scenery.Add(r);if(stageIndex==0&&index%4<2)AddFuel(r,p,false);}
 int CampaignEnemyType(){if(time<(quickMode?8:15))return 0;float r=UnityEngine.Random.value;return r<.5f?0:r<.8f?1:2;}
 void SpawnCampaignWaves(float dt){
  if(campaignAwaitingRescue||campaignStageComplete)return;
  if(stageIndex==3){SpawnEclipseWaves(dt);return;}
  if(time>=Duration&&!finalSpawned){
   finalSpawned=true;finalBoss=SpawnEnemy(StageBossTypes[stageIndex],true);finalBoss.p=new Vector2(Mathf.Clamp(pos.x+5,-23,23),Mathf.Clamp(pos.y+3,-23,23));finalBoss.sprite.transform.position=finalBoss.p;
   finalBoss.hp=finalBoss.maxHp=700+stageIndex*350+level*35;finalBoss.attack=2;
   Toast(StageBossNames[stageIndex]+" • derrote o chefe para libertar o herói!");
  }
  float progress=Mathf.Clamp01(time/Duration);spawnTimer-=dt;
  int cap=finalSpawned?28:100+stageIndex*20;
  if(spawnTimer<=0&&enemies.Count<cap){spawnTimer=finalSpawned?1.2f:Mathf.Lerp(.7f,.25f,progress);for(int i=0;i<(finalSpawned?1:1+(int)(progress*2));i++){var e=SpawnEnemy(CampaignEnemyType(),false);e.hp*=1+stageIndex*.2f;e.maxHp=e.hp;}}
 }
 void QueueStageRescue(){if(stageIndex==3){CompleteFinalCampaign();return;}if(!campaignActive||campaignAwaitingRescue||campaignStageComplete)return;campaignAwaitingRescue=true;CancelPowerPreparation();state=ScreenState.StageRescue;}
 void PrepareStageRescue(){
  if(!campaignAwaitingRescue||campaignRescueReady)return;
  // Run after the damage iteration has returned; its projectile/enemy lists may still be in use.
  foreach(var e in enemies){CancelBiomeWindup(e);if(e.status!=null)Dispose(e.status.gameObject);if(e.sprite!=null)Dispose(e.sprite.gameObject);}enemies.Clear();
  foreach(var s in shots)if(s.sprite!=null)Dispose(s.sprite.gameObject);shots.Clear();
  foreach(var h in hazards)if(h.sprite!=null)Dispose(h.sprite.gameObject);hazards.Clear();
  foreach(var g in ground)if(g.sprite!=null)Dispose(g.sprite.gameObject);
  foreach(var f in fuel)if(f.flame!=null)Dispose(f.flame.gameObject);if(playerStatus!=null)Dispose(playerStatus.gameObject);ResetElements();
  // Collect earned floor rewards without opening combat choices in the rescue scene.
  foreach(var l in loot){if(l.kind==4)CollectRelic((int)l.value);else if(l.kind==1)runCoins+=25;else if(l.kind==0)xp+=l.value;if(l.sprite!=null)Dispose(l.sprite.gameObject);}loot.Clear();
  foreach(var f in fallen)if(f.sprite!=null)Dispose(f.sprite.gameObject);fallen.Clear();
  foreach(var v in signatureVisuals)if(v.sprite!=null)Dispose(v.sprite.gameObject);ResetArchetype();
  foreach(var fx in effects)if(fx.sprite!=null)Dispose(fx.sprite.gameObject);effects.Clear();
  ResetRelicAttacks();ResetExclusive();CancelPowerPreparation();foreach(var orb in orbitVisuals)if(orb!=null)orb.enabled=false;
  ResetRescue();rescueHero=StageHeroes[stageIndex];rescuePosition=new Vector2(Mathf.Clamp(pos.x+2,-24,24),Mathf.Clamp(pos.y-1,-24,24));
  prisoner=Sprite("Herói da fase",ArtBank.Hero(rescueHero),rescuePosition,1.15f,Color.white,Depth(rescuePosition));
  cage=Sprite("Jaula da fase",CageArt(),rescuePosition,1.7f,Color.white,Depth(rescuePosition)+2);
  if(playerStatus!=null)playerStatus.enabled=false;invincible=dashTimer=0;hero.color=Color.white;campaignRescueReady=true;
 }
 void TickStageRescue(float dt){
  PrepareStageRescue();var move=new Vector2(Input.GetAxisRaw("Horizontal"),Input.GetAxisRaw("Vertical"));
  if(!manualAim&&Input.GetMouseButton(0)&&!OverHud()){move=ArenaPointFromScreen(Input.mousePosition)-pos;if(move.magnitude<.15f)move=Vector2.zero;}
  move=Vector2.ClampMagnitude(move,1);if(move.sqrMagnitude>.01f)facing=move.normalized;pos+=move*Speed*dt;pos.x=Mathf.Clamp(pos.x,-25,25);pos.y=Mathf.Clamp(pos.y,-25,25);AnimateHero(dt,move);hero.sortingOrder=Depth(pos);hero.flipX=facing.x<0;
  var cp=Vector2.Lerp(cam.transform.position,pos,dt*7);cam.transform.position=new Vector3(cp.x,cp.y,-10);
  if(prisoner!=null)prisoner.sprite=ArtBank.Hero(rescueHero,BodyFrame(Time.unscaledTime,false,false));UpdateEffects(dt);
  if(Input.GetKeyDown(KeyCode.E))RescueStageHero();
 }
 bool RescueStageHero(){
  if(!campaignActive||!campaignAwaitingRescue||!campaignRescueReady||state!=ScreenState.StageRescue||rescueHero<0||(rescuePosition-pos).sqrMagnitude>4||hp<=0)return false;
  int rescued=rescueHero;bool already=IsHeroUnlocked(rescued);save.rescuedHeroes|=1<<rescued;save.rescueVersion=1;save.completedStages|=1<<stageIndex;
  if(already)runCoins+=50;
  campaignRescueMessage=Catalog.Heroes[rescued]+(already?" • já disponível • +50 moedas":" • desbloqueado para jogar!");
  campaignAwaitingRescue=false;campaignStageComplete=true;ResetRescue();EndRun(true);finalBonusPending=true;
  chestMessage="FASE "+(stageIndex+1)+" CONCLUÍDA";
  return true;
 }
 bool AdvanceCampaignStage(){if(!campaignActive||!campaignStageComplete||finalBonusPending||state!=ScreenState.Result||stageIndex>=3)return false;return StartCampaignStage(stageIndex+1,true);}
 void CompleteFinalCampaign(){
  if(!campaignActive||stageIndex!=3||campaignStageComplete||credited)return;
  save.completedStages|=8;campaignStageComplete=true;campaignAwaitingRescue=false;
  campaignRescueMessage="O Soberano caiu. Você atravessou a noite até o amanhecer.";
  EndRun(true);finalBonusPending=true;chestMessage="CAMPANHA CONCLUÍDA";
 }
 void DrawCampaignRescue(){
  Frame(12,290,170,700,104);Text("CHEFE DERROTADO • RESGATE O HERÓI",310,192,660,25,Style(19,gold,TextAnchor.MiddleCenter,true));
  Text("Aproxime-se da jaula e pressione E para libertar "+Catalog.Heroes[StageHeroes[stageIndex]].Split(',')[0]+".",310,226,660,25,Style(14,Color.white,TextAnchor.MiddleCenter));
  bool near=campaignRescueReady&&(rescuePosition-pos).sqrMagnitude<=4;
  if(Button(near?"E • LIBERTAR HERÓI":"APROXIME-SE DA JAULA",390,510,500,49,near,true,17))RescueStageHero();
 }
 void DrawCampaignHud(){if(!campaignActive||final3DActive||state!=ScreenState.Playing||toastUntil>Time.unscaledTime)return;Frame(1,370,165,540,40);Text("FASE "+(stageIndex+1)+" / 4 • "+StageNames[stageIndex].ToUpper(),382,172,516,25,Style(16,gold,TextAnchor.MiddleCenter,true));}
 void DrawCampaignResult(){
  if(finalBonusPending&&Button("ESCOLHER RECOMPENSA DO CHEFE",350,610,580,45,true,true,16))OpenBossReward(StageBossTypes[stageIndex],true);
  Panel(265,112,750,486);DrawIcon(campaignStageComplete?(stageIndex==3?BiomeSprites.Get(3,3,0):ArtBank.Hero(StageHeroes[stageIndex])):ArtBank.Icon(15),591,129,98);
  Text(campaignStageComplete?chestMessage:"A NOITE TE ENCONTROU",300,232,680,49,Style(29,gold,TextAnchor.MiddleCenter,true));
  Text(campaignStageComplete?campaignRescueMessage:StageNames[stageIndex]+" • tente novamente para concluir o resgate",300,283,680,32,Style(16,cyan,TextAnchor.MiddleCenter));
  ResultStat("TEMPO",Clock(time),330);ResultStat("ABATES",kills.ToString(),540);ResultStat("NÍVEL",level.ToString(),750);
  Text("+"+runCoins+" moedas • Saldo "+save.coins,320,421,640,38,Style(22,gold,TextAnchor.MiddleCenter));DrawRelicResult();
  bool next=campaignStageComplete&&stageIndex<3;
  if(Button(next?(stageIndex==2?"ENTRAR NO ECLIPSE 2.5D":"PRÓXIMA FASE"):"RECOMEÇAR CAMPANHA",310,507,320,55,!finalBonusPending,true,15)){if(next)AdvanceCampaignStage();else StartCampaignStage(0);}
  if(Button("VOLTAR AO MENU",650,507,320,55))ReturnMenu();
  if(finalBonusPending)Text("Escolha uma recompensa ou continue sem bônus na tela de recompensa.",300,570,680,24,Style(12,Color.white,TextAnchor.MiddleCenter));
 }
 void DrawCampaignMaps(){
  Text("CAMPANHA EM SEQUÊNCIA • 3 RESGATES → CONFRONTO FINAL EM 2.5D",50,431,1180,29,Style(15,gold,TextAnchor.MiddleLeft,true));
  for(int i=0;i<4;i++){
   float x=48+i*296;Frame(12,x,471,282,98);
   Text((i+1)+" • "+StageNames[i].ToUpper(),x+13,492,256,25,Style(13,gold,TextAnchor.MiddleCenter,true));
   Text(i==3?"Final 2.5D • Soberano do Eclipse":"Chefe: "+StageBossNames[i]+"\nResgate: "+Catalog.Heroes[StageHeroes[i]].Split(',')[0],x+14,520,254,39,Style(11,Color.white,TextAnchor.MiddleCenter));
   if(i<3)Text("→",x+272,502,25,28,Style(19,cyan,TextAnchor.MiddleCenter,true));
  }
  if(Button(quickMode?"RÁPIDA • 1:40 POR MAPA + FINAL":"CLÁSSICA • 5 MIN POR MAPA + FINAL",790,154,436,45,true,true,12))quickMode=!quickMode;
 }
 public string VerifyCampaignStages(){
  var account=save;bool previous=verifying,oldQuick=quickMode;int oldHero=heroIndex,oldStage=stageIndex,oldPet=selectedDragon,oldCurse=selectedCurse;
  string disk=PlayerPrefs.GetString(EditorTestProfile.Key("NSF.Save"),"");verifying=true;ReturnMenu();save=new SaveData();heroIndex=0;selectedDragon=selectedCurse=-1;quickMode=true;
  try{
   if(StageUnlocked(1)||StageUnlocked(2)||StartCampaignStage(2))throw new Exception("Campaign skips locked map");
   for(int stage=0;stage<3;stage++){
    if(!StartCampaignStage(stage,stage>0))throw new Exception("Campaign stage entry failed");
    if(stage>0&&(weapons[0]!=4||passives[0]!=2||level!=8||maxHp!=140||Mathf.Abs(hp-112)>.01f))throw new Exception("Build/health not carried between maps");
    if(!cam.orthographic||final3DActive||scenery.Count<70)throw new Exception("Campaign map not built in 2D");
    weapons[0]=4;passives[0]=2;level=8;hp=70;maxHp=140;time=Duration*.7f;UpdateRescue(1);
    if(rescueHero>=0||RescueStageHero()||campaignStageComplete)throw new Exception("Rescue available before phase boss");
    time=Duration;SpawnWaves(.1f);if(finalBoss==null||finalBoss.type!=StageBossTypes[stage]||final3DActive)throw new Exception("Wrong stage boss");
    Damage(finalBoss,finalBoss.maxHp*.51f);if(!finalBoss.phaseTwo)throw new Exception("Stage boss second phase missing");
    Damage(finalBoss,finalBoss.maxHp+1);if(state!=ScreenState.StageRescue||save.completedStages!=(1<<stage)-1)throw new Exception("Boss skips rescue");
    PrepareStageRescue();if(enemies.Count!=0||shots.Count!=0||hazards.Count!=0||ground.Count!=0||fallen.Count!=0||signatureVisuals.Count!=0||rescueHero!=StageHeroes[stage])throw new Exception("Unsafe or incorrect final rescue");
    pos=rescuePosition+Vector2.right*5;if(RescueStageHero())throw new Exception("Rescue ignores distance");
    state=ScreenState.Paused;if(RescueStageHero())throw new Exception("Rescue progresses while paused");ResumeGame();if(state!=ScreenState.StageRescue)throw new Exception("Pause resumes combat after boss");
    pos=rescuePosition;if(!RescueStageHero()||!IsHeroUnlocked(StageHeroes[stage])||state!=ScreenState.Result)throw new Exception("Stage rescue failed");
    int coins=save.coins,wins=save.wins;if(RescueStageHero()||save.coins!=coins||save.wins!=wins)throw new Exception("Duplicate phase rewards");
    OpenBossReward(StageBossTypes[stage],true);if(!ChooseBossReward(0))throw new Exception("Stage bonus unavailable");
    if(!StageUnlocked(stage+1))throw new Exception("Next stage not unlocked");
   }
   if(state!=ScreenState.Result||!campaignStageComplete||chestMessage=="CAMPANHA CONCLUÍDA")throw new Exception("Forge ends campaign before Eclipse");
   if(!AdvanceCampaignStage()||stageIndex!=3||!campaignActive||final3DDemo||!final3DTransition||finalBoss==null||finalBoss.biome!=3)throw new Exception("Forge does not lead to real Eclipse stage");
   if(weapons[0]!=4||passives[0]!=2||level!=8||maxHp!=140||Mathf.Abs(hp-112)>.01f)throw new Exception("Build lost on 2.5D entry");
   float clock=time;Tick(.05f);if(time!=clock)throw new Exception("Combat runs during final transition");AdvanceFinalTransition(4);FinalCamera();SyncFinal3D();
   if(cam.orthographic||!final3DVisible||pixelFinal.Count<2)throw new Exception("Final presentation did not change to 2.5D");
   Damage(finalBoss,finalBoss.maxHp+1);if(state!=ScreenState.Result||!campaignStageComplete||chestMessage!="CAMPANHA CONCLUÍDA"||!finalBonusPending)throw new Exception("Final campaign victory missing");
   int finalCoins=save.coins,finalWins=save.wins;CompleteFinalCampaign();if(save.coins!=finalCoins||save.wins!=finalWins||AdvanceCampaignStage())throw new Exception("Duplicate final reward or fifth stage");
   var loaded=JsonUtility.FromJson<SaveData>(JsonUtility.ToJson(save));if(loaded.completedStages!=15||(loaded.rescuedHeroes&56)!=56)throw new Exception("Four-stage progress persistence failed");
   ReturnMenu();save=new SaveData();StartCampaignStage(0);EndRun(false);if(save.completedStages!=0||IsHeroUnlocked(3))throw new Exception("Defeat unlocked stage or hero");
   ReturnMenu();EnterBuildLab();if(StartCampaignStage(0))throw new Exception("Training starts progression");ReturnMenu();
   if(PlayerPrefs.GetString(EditorTestProfile.Key("NSF.Save"),"")!=disk)throw new Exception("Verification wrote real progress");
   return "PASS: linear campaign: Forest > Necropolis > Forge > real Eclipse 2.5D; three rescues, then Sovereign victory; forge cannot finish campaign; build/health carry into final; transition freezes combat; final camera/sprites; once-only campaign completion/rewards; 4-stage save roundtrip; stage locks; timed stage-specific bosses with phase two; rescue only after boss; safe rescue scene; distance and pause; permanent hero/map unlocks; no duplicate rewards; build carry and healing; boss reward choices; save roundtrip; death/training isolation.";
  }finally{ReturnMenu();save=account;heroIndex=oldHero;stageIndex=oldStage;selectedDragon=oldPet;selectedCurse=oldCurse;quickMode=oldQuick;verifying=previous;}
 }
 public void PreviewCampaignStages(){StartCoroutine(CaptureCampaignStages());}
 IEnumerator CaptureCampaignStages(){
  if(state!=ScreenState.Menu&&state!=ScreenState.Selection){Debug.LogWarning("Volte ao menu antes de capturar as fases.");yield break;}
  var account=save;
  int oldHero=heroIndex,oldStage=stageIndex,oldPet=selectedDragon,oldCurse=selectedCurse;
  bool oldVerify=verifying,oldQuick=quickMode,oldBackground=Application.runInBackground;
  ReturnMenu();verifying=true;Application.runInBackground=true;
  try{
   string folder=System.IO.Path.GetFullPath("../Previa-Campanha-v027");System.IO.Directory.CreateDirectory(folder);
   save=new SaveData();heroIndex=0;selectedDragon=selectedCurse=-1;quickMode=true;
   state=ScreenState.Selection;yield return Capture(folder,"campanha-quatro-fases");
   for(int i=0;i<4;i++){
    if(i==0)StartCampaignStage(0);else if(!AdvanceCampaignStage())throw new Exception("Preview cannot advance campaign");
    elementPreviewFrozen=true;toastUntil=0;
    if(i<3){
     yield return Capture(folder,"fase-"+(i+1));time=Duration;SpawnWaves(.1f);
     finalBoss.p=pos+new Vector2(2,1);finalBoss.sprite.transform.position=finalBoss.p;toastUntil=0;
     Damage(finalBoss,finalBoss.maxHp+1);PrepareStageRescue();yield return Capture(folder,"resgate-"+(i+1));
     pos=rescuePosition;RescueStageHero();OpenBossReward(StageBossTypes[i],true);ChooseBossReward(0);
     yield return Capture(folder,"resultado-"+(i+1));
    }else{
     AdvanceFinalTransition(.7f);yield return Capture(folder,"transicao-eclipse");
     AdvanceFinalTransition(4);FinalCamera();SyncFinal3D();toastUntil=0;yield return Capture(folder,"fase-4-eclipse-25d");
     Damage(finalBoss,finalBoss.maxHp+1);yield return Capture(folder,"campanha-concluida");
    }
    elementPreviewFrozen=false;
   }
   Debug.Log("CAMPAIGN PREVIEWS READY: "+folder);
  }finally{
   elementPreviewFrozen=false;ReturnMenu();save=account;heroIndex=oldHero;stageIndex=oldStage;selectedDragon=oldPet;selectedCurse=oldCurse;
   quickMode=oldQuick;verifying=oldVerify;Application.runInBackground=oldBackground;
  }
 }
}
}
