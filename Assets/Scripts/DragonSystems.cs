using System;
using UnityEngine;
namespace NoiteSemFim {
public sealed partial class SurvivorGame {
 static readonly string[] DragonNames={"Brasa","Geada","Aurora"};
 static readonly string[] DragonDescriptions={"Cospe fogo a cada 1,6 s.\nDano do dragão cresce com seu nível (até o nível 30).","Dispara gelo a cada 3 s contra inimigos próximos. O impacto congela o alvo.","Recupera 3 de vida a cada 5 s e dispara luz contra os inimigos."};
 static readonly string[] ComboNames={"Pacto das Brasas","Inverno Eterno","Constelação Viva"};
 static readonly string[] ComboRecipes={"Cristal Rubro II + Ampulheta II","Coração Antigo II + Lente Celeste II","Ímã II + Botas Lunares II"};
 static readonly string[] ComboDescriptions={"Dragão: +40% de dano. Brasa dispara uma chama extra.","Herói: +20% de área. Geada dispara 30% mais rápido.","Coleta: +1,5 de alcance. Aurora cura mais 3 PV por pulso."};
 static readonly string[] CurseNames={"Sangue","Cobiça","Frenesi"};
 static readonly string[] CurseDescriptions={"+35% de dano do herói e do dragão.\nCusto: -25% de vida máxima.","+50% de moedas no resultado.\nCusto: inimigos se movem 20% mais rápido.","Recarga das armas 25% menor.\nCusto: você recebe 30% mais dano."};
 int selectedDragon=-1,selectedCurse=-1,activeCurse=-1;
 float dragonTimer,dragonHealTimer;
 SpriteRenderer dragon;
 Vector2 dragonPos;
 bool offerCurseAfterChest;
 readonly bool[] comboAnnounced=new bool[3];
 ScreenState denReturn=ScreenState.Menu;
 bool HasCombo(int id)=>id==0?passives[0]>=2&&passives[1]>=2:id==1?passives[3]>=2&&passives[5]>=2:passives[2]>=2&&passives[4]>=2;
 bool CurseUnlocked(int id)=>id>=0&&id<3&&(save.curseUnlocks&(1<<id))!=0;
 void OpenDragonDen(ScreenState from){ValidateDragonSelection();denReturn=from;state=ScreenState.DragonDen;}
 void CloseDragonDen(){SaveMenuSettings();state=denReturn;}
 void ResetDragonRun(){activeCurse=-1;dragonTimer=.5f;dragonHealTimer=0;offerCurseAfterChest=false;Array.Clear(comboAnnounced,0,3);dragonPos=pos+new Vector2(-1,.7f);dragon=null;ValidateDragonSelection();if(selectedDragon>=0)dragon=Sprite("Dragon Companion",ArtBank.Dragon(selectedDragon)??PixelArt.Orb,dragonPos,.8f,Color.white,Depth(dragonPos)+2);if(CurseUnlocked(selectedCurse))SetCurse(selectedCurse);}
 void RefreshCombos(){for(int i=0;i<3;i++)if(HasCombo(i)&&!comboAnnounced[i]){comboAnnounced[i]=true;Toast("COMBINAÇÃO: "+ComboNames[i]);Burst(pos,cyan,2,.4f);}}
 void UpdateDragon(float dt){if(dragon==null)return;dragonAnimClock+=dt;dragonAttackTime=Mathf.Max(0,dragonAttackTime-dt);var targetPos=pos+new Vector2(facing.x<0?1:-1,.65f+Mathf.Sin(time*3)*.12f);dragonPos=Vector2.Lerp(dragonPos,targetPos,Mathf.Min(1,dt*6));dragon.transform.position=dragonPos;dragon.sortingOrder=Depth(dragonPos)+2;var sprite=ArtBank.Dragon(selectedDragon,dragonAttackTime>0?5:ArtBank.Frame(dragonAnimClock,8));if(sprite!=null)dragon.sprite=sprite;dragon.flipX=facing.x<0;dragonTimer-=dt;dragonHealTimer+=dt;
  if(selectedDragon==2&&dragonHealTimer>=5){dragonHealTimer=0;hp=Mathf.Min(maxHp,hp+3+(HasCombo(2)?3:0));Burst(pos,new Color(.65f,1,.7f),1,.4f);}
  if(dragonTimer>0)return;float damage=(8+Mathf.Min(level,30)*.8f)*Power*(HasCombo(0)?1.4f:1)*runRelics.dragon*(1+dragonBlessings*.2f)*DragonMasteryBonus;var target=Nearest();
  if(target==null||(target.p-pos).sqrMagnitude>100){dragonTimer=0;return;}
  if(selectedDragon==1){dragonAttackTime=.25f;dragonTimer=HasCombo(1)?2.1f:3;Fire(dragonPos,(target.p-dragonPos).normalized*8,damage,.24f,1,new Color(.3f,.9f,1),1.5f,0,Element.Ice);return;}
  if(target==null){dragonTimer=.2f;return;}dragonAttackTime=.25f;dragonTimer=selectedDragon==0?1.6f:2.4f;Vector2 direction=(target.p-dragonPos).normalized;Fire(dragonPos,direction*7,damage,.22f,2,selectedDragon==0?new Color(1,.4f,.15f):new Color(.8f,.65f,1),2.5f,-1,selectedDragon==0?Element.Fire:Element.None);
  if(selectedDragon==0&&HasCombo(0))Fire(dragonPos,Rotate(direction,.2f)*7,damage,.22f,2,new Color(1,.65f,.2f),2.5f,-1,Element.Fire);
 }
 void UnlockCurse(int bossType){if(elementTraining||final3DDemo)return;int id=bossType==6?1:bossType==7?2:0;bool wasUnlocked=CurseUnlocked(id);save.curseUnlocks|=1<<id;if(!verifying)save.Store();if(!wasUnlocked)Toast("Maldição desbloqueada: "+CurseNames[id]+". Equipe no menu de dragões.");}
 void SetCurse(int id){if(activeCurse==id)return;float ratio=maxHp>0?hp/maxHp:1;if(activeCurse==0)maxHp/= .75f;activeCurse=id;if(activeCurse==0)maxHp*=.75f;hp=Mathf.Clamp(maxHp*ratio,0,maxHp);}
 void ContinueChest(){if(rewardBossType>=0&&!elementTraining&&!final3DDemo){int type=rewardBossType;rewardBossType=-1;OpenBossReward(type,false);offerCurseAfterChest=false;return;}if(offerCurseAfterChest&&activeCurse<0){state=ScreenState.CurseChoice;offerCurseAfterChest=false;}else state=ScreenState.Playing;}
 bool AcceptCurse(int id){if(!CurseUnlocked(id)||activeCurse>=0||state!=ScreenState.CurseChoice)return false;SetCurse(id);state=ScreenState.Playing;Toast("Maldição ativa: "+CurseNames[id]);return true;}
 void DeclineCurse(){offerCurseAfterChest=false;state=ScreenState.Playing;}
 void DrawDragonHud(){Frame(1,26,550,302,37);if(selectedDragon>=0)DrawIcon(ArtBank.Dragon(selectedDragon),33,551,35);int combos=0;for(int i=0;i<3;i++)if(HasCombo(i))combos++;Text(SelectedDragonName+"  •  Combos "+combos+"/3",76,555,242,24,Style(13,cyan,TextAnchor.MiddleLeft,true));if(activeCurse>=0){Frame(1,952,550,302,37);Text("MALDIÇÃO: "+CurseNames[activeCurse],965,555,276,24,Style(13,new Color(1,.55f,.62f),TextAnchor.MiddleCenter,true));}GUI.Label(new Rect(26,550,302,37),new GUIContent("",selectedDragon>=0?DragonDescriptions[selectedDragon]:"Complete missões no menu para desbloquear dragões."));}
 void DrawDragonDen(){ValidateDragonSelection();Backdrop();Text("PACTOS DRACÔNICOS",48,18,1184,59,Style(38,gold,TextAnchor.MiddleLeft,true));Text("Complete missões para desbloquear companheiros. O progresso é acumulado e salvo.",51,83,1170,27,Style(15,cyan));
  for(int i=0;i<3;i++){float x=48+i*400;bool unlocked=IsDragonUnlocked(i);Frame(12,x,122,382,223);GUI.color=unlocked?Color.white:new Color(.4f,.44f,.5f,.72f);DrawIcon(ArtBank.Dragon(i),x+16,149,94);GUI.color=Color.white;Text(DragonNames[i],x+127,159,232,28,Style(21,gold,TextAnchor.MiddleLeft,true));Text(unlocked?DragonDescriptions[i]:DragonMissionNames[i],x+127,194,231,58,Style(13,Color.white));Text(unlocked?"MISSÃO CONCLUÍDA":DragonMissionProgress(i),x+20,255,342,24,Style(13,unlocked?cyan:gold,TextAnchor.MiddleCenter,true));if(Button(!unlocked?"BLOQUEADO":selectedDragon==i?"COMPANHEIRO ATIVO":"EQUIPAR",x+24,292,334,40,unlocked,selectedDragon==i,14))SelectDragon(i);}
  if(Button("SEM COMPANHEIRO",48,353,260,36,true,selectedDragon<0,13))SelectDragon(-1);Text("Equipado: "+SelectedDragonName,330,354,902,32,Style(14,cyan));
  Text("COMBINAÇÕES DE ARTEFATOS • ATIVAM AUTOMATICAMENTE",50,390,1180,26,Style(15,gold,TextAnchor.MiddleLeft,true));
  for(int i=0;i<3;i++){float x=48+i*400;Frame(12,x,424,382,93);Text(ComboNames[i],x+18,449,346,22,Style(17,cyan,TextAnchor.MiddleLeft,true));Text(ComboRecipes[i],x+18,475,346,17,Style(12,gold));Text(ComboDescriptions[i],x+18,494,346,17,Style(11,Color.white));}
  Text("MALDIÇÃO INICIAL • OPCIONAL • UMA POR PARTIDA",50,520,1180,26,Style(14,gold,TextAnchor.MiddleLeft,true));
  if(Button("SEM MALDIÇÃO",48,554,246,46,true,selectedCurse<0,14))selectedCurse=-1;
  for(int i=0;i<3;i++){float x=309+i*307;bool unlocked=CurseUnlocked(i);if(Button(unlocked?CurseNames[i].ToUpper():"DERROTE "+(i==0?"O CERVO":i==1?"A RAINHA":"O CARRASCO"),x,554,292,46,unlocked,selectedCurse==i,13))selectedCurse=i;GUI.Label(new Rect(x,554,292,46),new GUIContent("",CurseDescriptions[i]));}
  Text(selectedCurse<0?"Derrote guardiões para desbloquear. Após abrir o baú, você também poderá aceitar uma maldição.":CurseDescriptions[selectedCurse],50,609,820,58,Style(14,Color.white));if(Button("CONFIRMAR E VOLTAR",900,625,326,54,true,true,16))CloseDragonDen();
 }
 void DrawCurseChoice(){Text("PACTO DO GUARDIÃO",48,164,1184,57,Style(38,gold,TextAnchor.MiddleCenter,true));Text("Escolha um poder com um custo. Aceitar é opcional e vale só para esta partida.",48,225,1184,30,Style(16,Color.white,TextAnchor.MiddleCenter));for(int i=0;i<3;i++){float x=48+i*400;bool unlocked=CurseUnlocked(i);Frame(12,x,279,382,222);DrawIcon(ArtBank.Icon(i==0?6:i==1?14:7),x+20,303,54);Text(CurseNames[i],x+88,311,270,34,Style(25,gold,TextAnchor.MiddleLeft,true));Text(unlocked?CurseDescriptions[i]:"Derrote outro tipo de guardião para desbloquear este pacto.",x+25,368,332,66,Style(16,Color.white,TextAnchor.MiddleCenter));if(Button(unlocked?(i+1)+" • ACEITAR":"BLOQUEADO",x+25,445,332,40,unlocked,false,14))AcceptCurse(i);}if(Button("RECUSAR E CONTINUAR",405,526,470,55,true,true,18))DeclineCurse();}
 string VerifyDragonSystems(){var original=save;bool oldVerifying=verifying;int oldDragon=selectedDragon,oldCurse=selectedCurse;verifying=true;save=new SaveData{dragonUnlocks=7};selectedCurse=-1;
  for(int pet=0;pet<3;pet++){selectedDragon=pet;StartRun();var enemy=SpawnEnemy(0,false);enemy.p=pos+Vector2.right;enemy.hp=enemy.maxHp=10000;dragonTimer=0;int before=shots.Count;UpdateDragon(.05f);if(pet!=1&&shots.Count<=before)throw new Exception("Dragon projectile missing");if(pet==1){for(int step=0;step<40;step++)UpdateShots(.01f);if(enemy.slow<=0||enemy.frozen<=0)throw new Exception("Ice dragon projectile freeze missing");}if(pet==2){hp=50;dragonHealTimer=5;UpdateDragon(.05f);if(hp<=50)throw new Exception("Healing dragon failed");}}
  for(int i=0;i<6;i++)passives[i]=2;for(int i=0;i<3;i++)if(!HasCombo(i))throw new Exception("Artifact combination failed");selectedDragon=0;dragonTimer=0;int comboShots=shots.Count;UpdateDragon(.05f);if(shots.Count!=comboShots+2)throw new Exception("Fire combination bonus missing");if(Area<1.48f)throw new Exception("Area combination bonus missing");passives[0]=1;if(HasCombo(0))throw new Exception("Premature artifact combination");
  StartRun();var guardian=SpawnEnemy(3,false);Damage(guardian,guardian.maxHp+1);if(!CurseUnlocked(0)||!loot.Exists(l=>l.kind==1&&l.value==3))throw new Exception("Guardian reward integration failed");save.curseUnlocks=0;activeCurse=-1;state=ScreenState.CurseChoice;if(AcceptCurse(0))throw new Exception("Locked curse accepted");UnlockCurse(3);hp=maxHp=100;if(!AcceptCurse(0)||maxHp!=75||hp!=75)throw new Exception("Blood curse failed");if(AcceptCurse(1))throw new Exception("Stacked curse regression");
  activeCurse=-1;offerCurseAfterChest=true;state=ScreenState.Chest;ContinueChest();if(state!=ScreenState.CurseChoice)throw new Exception("Boss reward choice missing");DeclineCurse();if(state!=ScreenState.Playing)throw new Exception("Curse refusal failed");UnlockCurse(6);UnlockCurse(7);if(save.curseUnlocks!=7)throw new Exception("Curse progression failed");
  selectedCurse=1;StartRun();if(activeCurse!=1)throw new Exception("Pre-run curse not applied");EndRun(false);if(runCoins!=3)throw new Exception("Greed coin multiplier failed");
  selectedCurse=-1;StartRun();if(activeCurse!=-1||HasCombo(1))throw new Exception("Run reset failed");save=original;selectedDragon=oldDragon;selectedCurse=oldCurse;verifying=oldVerifying;ReturnMenu();return "Dragons (fire, slow, healing), three combinations, boss curse unlocks, opt-in/refusal, run reset and coin reward PASS.";
 }
}
}
