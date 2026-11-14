using System;
using UnityEngine;
namespace NoiteSemFim {
public sealed partial class SurvivorGame {
 int powerSection,powerSlot,powerInspected,powerLastHero=-1;
 int PowerEquippedId(int slot)=>slot==0?(SelectedWeaponChoice<0?NaturalWeaponChoice(heroIndex):SelectedWeaponChoice):slot==1?EquippedRelic(heroIndex,0):slot==2?EquippedRelic(heroIndex,1):selectedDragon;
 int PowerItemCount(int slot)=>slot==0?9:slot==3?4:5;
 int PowerItemId(int slot,int index)=>slot==0?index:index==0?-1:slot==3?index-1:(index-1)*2+(slot==2?1:0);
 void ResetPowerInspection(){powerInspected=PowerEquippedId(powerSlot);}
 string PowerItemName(int slot,int id)=>slot==0?EquipmentWeapons[id]:slot==1?(id<0?"Sem armadura":RelicNames[id]):slot==2?(id<0?"Sem talismã":RelicNames[id]):id<0?"Sem dragão":DragonNames[id];
 Sprite PowerItemIcon(int slot,int id)=>slot==0?EquipmentArt.Weapon(id):slot==3?(id<0?null:ArtBank.Dragon(id)):id<0?null:ArtBank.Relic(id);
 bool PowerItemAllowed(int slot,int id){if(!IsHeroUnlocked(heroIndex))return false;if(slot==0)return id>=-1&&id<9&&WeaponUnlockedIn(save,id);if(slot==1||slot==2)return id==-1||id>=0&&id<8&&id%2==slot-1&&OwnsRelic(id);return slot==3&&(id==-1||IsDragonUnlocked(id));}
 bool TryEquipPowerItem(int slot,int id){
  if(!PowerItemAllowed(slot,id))return false;
  if(slot==0)return TryEquipStartingWeapon(id);
  if(slot==1||slot==2){if(id<0){UnequipRelic(heroIndex,slot-1);return true;}return TryEquipRelic(heroIndex,id);}
  if(!SelectDragon(id))return false;SaveMenuSettings();return true;
 }
 bool TrySelectPowerCurse(int id){if(id< -1||id>2||id>=0&&!CurseUnlocked(id))return false;selectedCurse=id;SaveMenuSettings();return true;}
 void DrawPowerLoadout(){
  Text("EQUIPADO AGORA",76,262,244,27,Style(17,gold,TextAnchor.MiddleCenter,true));DrawMenuHero(heroIndex,104,294,188);
  Text(Catalog.Heroes[heroIndex].Split(',')[0],70,484,256,31,Style(23,gold,TextAnchor.MiddleCenter,true));
  Text("Maldição: "+(selectedCurse<0?"nenhuma":CurseNames[selectedCurse]),70,516,256,19,Style(11,selectedCurse<0?new Color(.73f,.8f,.82f):new Color(1,.66f,.65f),TextAnchor.MiddleCenter));
  for(int slot=0;slot<4;slot++){
   int id=PowerEquippedId(slot);float y=540+slot*22;var icon=PowerItemIcon(slot,id);if(icon!=null)DrawIcon(icon,70,y,19);
   string[] labels={"Arma","Armadura","Talismã","Dragão"};Text(labels[slot],96,y,67,21,Style(11,cyan));Text(PowerItemName(slot,id),165,y,158,21,Style(11,Color.white));
  }
 }
 void DrawPowerPanel(float x,float y,float w,float h){
  Rect(x,y,w,h,new Color(.018f,.031f,.043f,.96f));var edge=new Color(.55f,.45f,.27f,.55f);Rect(x,y,w,1,edge);Rect(x,y+h-1,w,1,edge);Rect(x,y,1,h,edge);Rect(x+w-1,y,1,h,edge);
 }
 bool PowerItemCard(int slot,int id,float x,float y,float width){
  bool allowed=PowerItemAllowed(slot,id),equipped=PowerEquippedId(slot)==id,inspected=powerInspected==id;
  bool hover=new Rect(x,y,width,76).Contains(Event.current.mousePosition);DrawPowerPanel(x,y,width,76);if(equipped||hover)Rect(x+1,y+1,width-2,74,equipped?new Color(.025f,.13f,.14f,.94f):new Color(.05f,.08f,.10f,.94f));if(equipped)Rect(x,y,4,76,cyan);if(inspected&&!equipped)Rect(x+16,y+8,width-32,2,cyan);
  GUI.color=allowed?Color.white:new Color(.5f,.56f,.6f,.78f);var icon=PowerItemIcon(slot,id);if(icon!=null)DrawIcon(icon,x+13,y+15,44);else Text("—",x+16,y+22,38,28,Style(22,gold,TextAnchor.MiddleCenter));GUI.color=Color.white;
  Text(PowerItemName(slot,id),x+68,y+14,width-82,30,Style(14,allowed?gold:new Color(.69f,.72f,.75f)));
  Text(equipped?"EQUIPADO":allowed?"Clique para equipar":"BLOQUEADO • VER MISSÃO",x+68,y+47,width-82,20,Style(11,equipped?cyan:allowed?new Color(.76f,.81f,.81f):new Color(.68f,.7f,.73f)));
  return GUI.Button(new Rect(x+4,y+4,width-8,68),GUIContent.none,GUIStyle.none);
 }
 void DrawPowerEquipment(){
  string[] slots={"ARMA","ARMADURA","TALISMÃ","DRAGÃO"};for(int i=0;i<4;i++)if(Button(slots[i],392+i*204,260,192,38,true,powerSlot==i,14)){powerSlot=i;ResetPowerInspection();}
  for(int i=0;i<PowerItemCount(powerSlot);i++){int id=PowerItemId(powerSlot,i);if(PowerItemCard(powerSlot,id,392+i%3*278,313+i/3*81,260)){powerInspected=id;TryEquipPowerItem(powerSlot,id);}}
  Rect(392,562,816,1,new Color(.52f,.45f,.29f,.6f));Text(PowerItemName(powerSlot,powerInspected),400,574,800,25,Style(17,gold,TextAnchor.MiddleLeft,true));
  string detail;
  if(powerSlot==0)detail=PowerItemAllowed(0,powerInspected)?EquipmentWeaponText[powerInspected]+" Começa no nível 1. A habilidade Q continua sendo do herói.":WeaponUnlockHint(powerInspected)+" para liberar esta arma.";
  else if(powerSlot<3)detail=powerInspected<0?"Deixa este espaço vazio. Nenhum bônus ou maldição de equipamento.":PowerItemAllowed(powerSlot,powerInspected)?RelicBenefits[powerInspected]+" "+RelicCosts[powerInspected]:"Obtida ao derrotar "+RelicBosses[powerInspected/2]+". "+RelicBenefits[powerInspected];
  else detail=powerInspected<0?"Jogue sem companheiro. Você pode escolher um dragão desbloqueado quando quiser.":IsDragonUnlocked(powerInspected)?DragonDescriptions[powerInspected].Replace("\n"," "):DragonMissionNames[powerInspected]+" • "+DragonMissionProgress(powerInspected)+".";
  Text(detail,400,602,800,29,Style(12,PowerItemAllowed(powerSlot,powerInspected)?Color.white:cyan));
 }
 void DrawPowerPermanent(){
  Text("PARA TODOS OS HERÓIS",400,260,800,31,Style(21,gold,TextAnchor.MiddleLeft,true));Text("Gaste moedas uma vez. O bônus continua nas próximas partidas.",400,293,800,24,Style(13,cyan));
  int[] icons={6,9,10,14};for(int i=0;i<4;i++){
   float y=330+i*71;int rank=save.perks[i],cost=50+rank*75;DrawIcon(ArtBank.Icon(icons[i]),400,y+7,43);Text(Catalog.Perks[i],459,y+2,230,26,Style(18,gold,TextAnchor.MiddleLeft,true));Text(Catalog.PerkText[i]+" por nível",459,y+30,370,24,Style(13,Color.white));Text(rank+" / 5",840,y+4,110,23,Style(16,cyan,TextAnchor.MiddleCenter));
   for(int n=0;n<5;n++)Rect(844+n*20,y+36,15,4,n<rank?gold:new Color(.18f,.25f,.28f));
   if(Button(rank>=5?"MÁXIMO":cost+" MOEDAS",974,y+7,218,44,rank<5&&save.coins>=cost,false,15))TryBuyPerk(i);
   if(rank<5&&save.coins<cost)Text("Moedas insuficientes",974,y+51,218,18,Style(10,new Color(.81f,.65f,.58f),TextAnchor.MiddleCenter));
  }
 }
 void DrawPowerRecipes(){
  Text("ARTEFATOS DA PARTIDA",400,260,800,30,Style(21,gold,TextAnchor.MiddleLeft,true));Text("Colete estes artefatos no nível II durante a partida para ativar as combinações.",400,294,800,24,Style(13,cyan));
  for(int i=0;i<3;i++){float y=331+i*61;Text(ComboNames[i],400,y,255,25,Style(16,gold,TextAnchor.MiddleLeft,true));Text(ComboRecipes[i],665,y,527,25,Style(13,Color.white));Text(ComboDescriptions[i],400,y+26,792,24,Style(12,new Color(.78f,.85f,.86f)));}
  Rect(392,518,816,1,new Color(.52f,.45f,.29f,.6f));Text("MALDIÇÃO OPCIONAL",400,528,430,27,Style(17,gold,TextAnchor.MiddleLeft,true));Text("Mais poder em troca de risco.",836,531,356,23,Style(12,Color.white,TextAnchor.MiddleRight));
  for(int i=-1;i<3;i++){bool allowed=i<0||CurseUnlocked(i);if(Button(i<0?"SEM MALDIÇÃO":allowed?CurseNames[i]:"BLOQUEADA",392+(i+1)*204,563,192,34,allowed,selectedCurse==i,12))TrySelectPowerCurse(i);}
  Text(selectedCurse<0?"Nenhuma penalidade extra. Derrote chefes para desbloquear maldições.":CurseDescriptions[selectedCurse].Replace("\n"," "),400,607,792,24,Style(12,selectedCurse<0?new Color(.78f,.85f,.86f):new Color(1,.69f,.67f)));
 }
 public string VerifyOrganizedPowerUp(){
  var account=save;bool previous=verifying;int h=heroIndex,pet=selectedDragon,curse=selectedCurse;var origin=powerUpReturn;
  string disk=PlayerPrefs.GetString(EditorTestProfile.Key("NSF.Save"),"");verifying=true;
  try{
   save=new SaveData{coins=500};heroIndex=0;selectedDragon=selectedCurse=-1;OpenPowerUp(ScreenState.Menu);
   if(powerSection!=0||state!=ScreenState.PowerUp)throw new Exception("Build is not the default section");
   if(TryEquipPowerItem(0,6)||TryEquipPowerItem(1,0)||TryEquipPowerItem(2,1)||TryEquipPowerItem(3,0)||TrySelectPowerCurse(0)||TryEquipPowerItem(1,1)||TryEquipPowerItem(9,0))throw new Exception("Locked/invalid power-up equipped");
   if(!TryEquipPowerItem(0,1)||state!=ScreenState.PowerUp||SelectedWeaponChoice!=1)throw new Exception("Inline weapon equip failed");
   save.relicOwned=255;save.rescuedHeroes=63;save.dragonUnlocks=7;save.curseUnlocks=7;
   foreach(int heroId in new[]{0,3,5}){heroIndex=heroId;foreach(int slot in new[]{0,1,2,3})for(int i=0;i<PowerItemCount(slot);i++){int id=PowerItemId(slot,i);if(!TryEquipPowerItem(slot,id)||PowerEquippedId(slot)!=id||state!=ScreenState.PowerUp)throw new Exception("Inline equipment failed: "+slot+" "+id);}}
   heroIndex=0;TryEquipPowerItem(0,6);TryEquipPowerItem(1,2);TryEquipPowerItem(2,3);heroIndex=1;if(SelectedWeaponChoice!=-1||EquippedRelic(1,0)!=-1)throw new Exception("Gear leaked between heroes");heroIndex=0;
   if(!TrySelectPowerCurse(1)||!TrySelectPowerCurse(-1))throw new Exception("Optional curse selection failed");int coins=save.coins;if(!TryBuyPerk(0)||save.coins!=coins-50||SelectedWeaponChoice!=6||EquippedRelic(0,0)!=2)throw new Exception("Permanent upgrade changed equipment");
   ClosePowerUp();if(state!=ScreenState.Menu)throw new Exception("Hub menu origin lost");OpenPowerUp(ScreenState.Selection);ClosePowerUp();if(state!=ScreenState.Selection)throw new Exception("Hub preparation origin lost");
   if(PlayerPrefs.GetString(EditorTestProfile.Key("NSF.Save"),"")!=disk)throw new Exception("Verification changed real save");
   return "PASS: default equipment section; inline weapons/armor/talismans/dragons without submenu jumps; empty equipment slots; locked items and wrong slots rejected; per-hero gear isolation; optional curses; permanent coin purchase preserves loadout; menu/preparation return; real save unchanged.";
  }finally{save=account;heroIndex=h;selectedDragon=pet;selectedCurse=curse;powerUpReturn=origin;verifying=previous;state=ScreenState.Menu;powerLastHero=-1;}
 }
}
}
