using System;
using System.Collections.Generic;
using UnityEngine;
namespace NoiteSemFim {
public static class EquipmentArt {
 static readonly string[] Bodies={"miquella","caio","iris","ren","dante","kaela"};
 static readonly Dictionary<string,Sprite[]> cache=new Dictionary<string,Sprite[]>();
 static Sprite[] Load(string name,int cols,int rows,bool feet){
  if(cache.TryGetValue(name,out var found))return found;
  var t=Resources.Load<Texture2D>("Art/"+name);if(t==null||!t.isReadable)throw new InvalidOperationException("Missing readable equipment atlas: "+name);
  var px=t.GetPixels32();int w=t.width/cols,h=t.height/rows;var sprites=new Sprite[cols*rows];
  for(int row=0;row<rows;row++){
   var boxes=new RectInt[cols];var heights=new List<int>();
   var rowCuts=ArtBank.AxisCuts(px,t.width,t.height,rows,false);int ry0=rowCuts[rows-row-1],ry1=rowCuts[rows-row];var colCuts=ArtBank.AxisCuts(px,t.width,t.height,cols,true,ry0,ry1);
   for(int col=0;col<cols;col++){
    int x0=colCuts[col],x1=colCuts[col+1];var localRows=ArtBank.AxisCuts(px,t.width,t.height,rows,false,x0,x1);int y0=localRows[rows-row-1],y1=localRows[rows-row];int minX=x1,minY=y1,maxX=x0-1,maxY=y0-1;
    for(int y=y0;y<y1;y++)for(int x=x0;x<x1;x++)if(px[y*t.width+x].a>=128){minX=Mathf.Min(minX,x);minY=Mathf.Min(minY,y);maxX=Mathf.Max(maxX,x);maxY=Mathf.Max(maxY,y);}
    if(maxX<minX||maxX-minX>=x1-x0-2&&maxY-minY>=y1-y0-2)throw new InvalidOperationException("Empty/opaque equipment cell: "+name+" "+row+","+col);
    boxes[col]=new RectInt(minX,minY,maxX-minX+1,maxY-minY+1);heights.Add(maxY-minY+1);
   }
   heights.Sort();float ppu=feet?heights[cols/2]/1.08f:1;
   for(int col=0;col<cols;col++){var b=boxes[col];sprites[row*cols+col]=Sprite.Create(t,new Rect(b.x,b.y,b.width,b.height),feet?new Vector2(.5f,0):row*cols+col==7?new Vector2(.5f,.84f):row*cols+col==1||row*cols+col==5||row*cols+col==8?Vector2.one*.5f:new Vector2(.5f,.2f),feet?ppu:Mathf.Max(b.width,b.height),0,SpriteMeshType.FullRect);sprites[row*cols+col].name=name+"_"+row+"_"+col;}
  }
  cache[name]=sprites;return sprites;
 }
 public static Sprite Body(int hero,int armor,int frame=0){int row=armor>=0?armor/2+1:0;int pose=frame==5?3:frame==0?0:1+(frame-1)%2;return Load("equipment-body-"+Bodies[hero]+"-v28",4,5,true)[row*4+pose];}
 public static Sprite Weapon(int choice)=>Load("equipment-weapons-v28",3,3,false)[Mathf.Clamp(choice,0,8)];
 // Palm coordinates are relative to each cropped pose, not the atlas cell or the torso.
 static readonly Vector2[,] Palms={
  {new Vector2(.87f,.42f),new Vector2(.89f,.43f),new Vector2(.90f,.43f),new Vector2(.87f,.49f)},
  {new Vector2(.84f,.37f),new Vector2(.85f,.38f),new Vector2(.87f,.38f),new Vector2(.92f,.56f)},
  {new Vector2(.84f,.43f),new Vector2(.85f,.45f),new Vector2(.86f,.42f),new Vector2(.86f,.50f)},
  {new Vector2(.91f,.40f),new Vector2(.70f,.40f),new Vector2(.76f,.40f),new Vector2(.78f,.47f)},
  {new Vector2(.90f,.40f),new Vector2(.72f,.40f),new Vector2(.80f,.39f),new Vector2(.82f,.45f)},
  {new Vector2(.90f,.39f),new Vector2(.90f,.37f),new Vector2(.90f,.40f),new Vector2(.92f,.56f)}
 };
 static readonly Vector2[,] OtherPalms={
  {new Vector2(.35f,.37f),new Vector2(.37f,.37f),new Vector2(.36f,.37f),new Vector2(.44f,.36f)},
  {new Vector2(.36f,.37f),new Vector2(.42f,.39f),new Vector2(.40f,.40f),new Vector2(.48f,.40f)},
  {new Vector2(.39f,.43f),new Vector2(.42f,.43f),new Vector2(.40f,.43f),new Vector2(.44f,.43f)},
  {new Vector2(.52f,.38f),new Vector2(.92f,.36f),new Vector2(.88f,.38f),new Vector2(.46f,.46f)},
  {new Vector2(.45f,.38f),new Vector2(.91f,.37f),new Vector2(.50f,.40f),new Vector2(.45f,.45f)},
  {new Vector2(.56f,.39f),new Vector2(.65f,.32f),new Vector2(.58f,.40f),new Vector2(.56f,.41f)}
 };
 static readonly Dictionary<string,Sprite> parts=new Dictionary<string,Sprite>();
 public static Vector2 PalmUV(int hero,Sprite body,bool other=false){int pose=body.name[body.name.Length-1]-'0';return (other?OtherPalms:Palms)[hero,Mathf.Clamp(pose,0,3)];}
 public static Vector2 PalmLocal(int hero,Sprite body,bool other=false){var uv=PalmUV(hero,body,other);return new Vector2((uv.x*body.rect.width-body.pivot.x)/body.pixelsPerUnit,(uv.y*body.rect.height-body.pivot.y)/body.pixelsPerUnit);}
 public static Sprite Held(int choice,bool other=false){
  string key="weapon"+choice+other;if(parts.TryGetValue(key,out var result))return result;
  var icon=Weapon(choice);Rect r=icon.rect;Vector2 pivot=new Vector2(.5f,.25f);
  if(choice==1){float half=r.width*.5f;r=new Rect(r.x+(other?0:half),r.y,half,r.height);pivot=new Vector2(other?.60f:.40f,.20f);}
  if(choice==5)pivot=new Vector2(.5f,.18f);if(choice==6)pivot=new Vector2(.45f,.17f);if(choice==7)pivot=new Vector2(.5f,.87f);if(choice==8)pivot=new Vector2(.33f,.50f);
  result=Sprite.Create(icon.texture,r,pivot,Mathf.Max(r.width,r.height),0,SpriteMeshType.FullRect);result.name=key;parts[key]=result;return result;
 }
 public static Sprite Fingers(int hero,Sprite body,bool other){
  string key=body.name+"fingers"+other;if(parts.TryGetValue(key,out var result))return result;
  var uv=PalmUV(hero,body,other);var r=body.rect;float w=Mathf.Round(r.width*.065f),h=Mathf.Round(r.height*.035f);float px=Mathf.Round(r.x+uv.x*r.width),py=Mathf.Round(r.y+uv.y*r.height);
  result=Sprite.Create(body.texture,new Rect(px-w*.5f,py-h*.5f,w,h),Vector2.one*.5f,body.pixelsPerUnit,0,SpriteMeshType.FullRect);result.name=key;parts[key]=result;return result;
 }
 // Menu idle grips are measured in source-atlas pixels (top-left origin).
 // Each outfit has different crop bounds; a torso-relative offset drifts when armor changes.
 static readonly Vector2[,] MenuHands={
  {new Vector2(208,158),new Vector2(208,438),new Vector2(208,725),new Vector2(208,1004),new Vector2(208,1286)},
  {new Vector2(210,169),new Vector2(210,450),new Vector2(210,729),new Vector2(210,1015),new Vector2(210,1293)},
  {new Vector2(207,157),new Vector2(207,438),new Vector2(207,725),new Vector2(207,1003),new Vector2(207,1287)},
  {new Vector2(211,164),new Vector2(211,446),new Vector2(211,729),new Vector2(211,1014),new Vector2(211,1294)},
  {new Vector2(222,167),new Vector2(222,451),new Vector2(220,733),new Vector2(220,1017),new Vector2(222,1296)},
  {new Vector2(203,163),new Vector2(203,448),new Vector2(203,731),new Vector2(203,1016),new Vector2(203,1299)}
 };
 static readonly Vector2[,] MenuOtherHands={
  {new Vector2(111,173),new Vector2(111,451),new Vector2(111,734),new Vector2(111,1016),new Vector2(111,1294)},
  {new Vector2(118,169),new Vector2(118,450),new Vector2(118,731),new Vector2(118,1016),new Vector2(118,1294)},
  {new Vector2(114,161),new Vector2(114,442),new Vector2(114,727),new Vector2(114,1005),new Vector2(114,1291)},
  {new Vector2(128,166),new Vector2(128,454),new Vector2(128,738),new Vector2(128,1017),new Vector2(128,1308)},
  {new Vector2(115,171),new Vector2(115,455),new Vector2(115,736),new Vector2(116,1018),new Vector2(116,1300)},
  {new Vector2(139,164),new Vector2(139,450),new Vector2(139,730),new Vector2(139,1014),new Vector2(139,1297)}
 };
 static Vector2 MenuAtlasPalm(int h,int armor,bool other){var point=(other?MenuOtherHands:MenuHands)[h,armor<0?0:armor/2+1];point.y=Body(h,armor).texture.height-point.y;return point;}
 public static Vector2 MenuPalm(int h,int armor,bool other=false){var b=Body(h,armor);return (MenuAtlasPalm(h,armor,other)-b.rect.position-b.pivot)/b.pixelsPerUnit;}
 static readonly Vector2[] MenuHandles={new Vector2(249,277),new Vector2(711,275),new Vector2(1065,263),new Vector2(249,623),new Vector2(651,620),new Vector2(1065,650),new Vector2(225,1063),new Vector2(651,816),new Vector2(1009,958)};
 public static Sprite MenuHeld(int choice,bool other=false){
  string key="menu-held-"+choice+other;if(parts.TryGetValue(key,out var result))return result;
  var source=Held(choice,other);var r=source.rect;var point=choice==1&&other?new Vector2(595,275):MenuHandles[choice];point.y=source.texture.height-point.y;Vector2 pivot=(point-r.position)/r.size;
  result=Sprite.Create(source.texture,r,pivot,source.pixelsPerUnit,0,SpriteMeshType.FullRect);result.name=key;parts[key]=result;return result;
 }
 public static Sprite MenuFingers(int h,int armor,bool other=false){
  string key="menu-fingers-"+h+"-"+armor+other;if(parts.TryGetValue(key,out var result))return result;
  var body=Body(h,armor);var point=MenuAtlasPalm(h,armor,other);float w=h==4?22:16,ht=h==4?20:16;
  result=Sprite.Create(body.texture,new Rect(point.x-w/2,point.y-ht/2,w,ht),Vector2.one*.5f,body.pixelsPerUnit,0,SpriteMeshType.FullRect);result.name=key;parts[key]=result;return result;
 }
 public static float HeldSize(int choice)=>choice==7?1.15f:choice==1?.62f:choice==5?.58f:choice==2?.67f:choice==6?.94f:1.02f;
 public static float HeldAngle(int choice,bool other=false)=>choice==7?58:choice==6?-35:choice==1?(other?28:-28):choice==5?0:-8;
 public static string AuditMenuGrips(){
  for(int h=0;h<6;h++)for(int a=-1;a<4;a++)for(int hand=0;hand<2;hand++){
   int armor=a<0?-1:a*2;var body=Body(h,armor);var point=MenuAtlasPalm(h,armor,hand==1);
   if(!body.rect.Contains(point)||body.texture.GetPixel((int)point.x,(int)point.y).a<.5f)throw new Exception("Menu palm misses visible hand: hero "+h+" armor "+armor+" hand "+hand);
   if(MenuFingers(h,armor,hand==1)==null)throw new Exception("Menu fingers missing");
  }
  for(int w=0;w<9;w++)for(int hand=0;hand<(w==1?2:1);hand++){var held=MenuHeld(w,hand==1);var point=held.rect.position+held.pivot;if(!held.rect.Contains(point)||held.texture.GetPixel((int)point.x,(int)point.y).a<.5f)throw new Exception("Menu grip misses weapon handle: "+w+" hand "+hand);}
  return "30 menu outfits with 60 measured palms and 10 weapon grips PASS";
 }
 public static string Audit(){for(int h=0;h<6;h++)for(int a=-1;a<4;a++)for(int f=0;f<6;f++)if(Body(h,a<0?-1:a*2,f)==null)throw new Exception("Missing equipment pose");for(int i=0;i<9;i++)if(Weapon(i)==null)throw new Exception("Missing weapon art");return "6 heroes, 4 visible armor sets each, 120 outfit poses and 9 separate weapon sprites";}
}
public sealed partial class SurvivorGame {
 static readonly string[] EquipmentWeapons={"Cajado Arcano","Lâminas do Vento","Foco Astral","Cajado Glacial","Cetro da Tempestade","Grimório Ígneo","Katana do Eclipse","Espadão das Cinzas","Arco Lunar"};
 static readonly string[] EquipmentWeaponText={"Orbes à distância.","Lâminas perfurantes.","Esferas de proteção ao redor.","Projétil que congela o alvo.","Raios contra inimigos próximos.","Leque de chamas.","Cortes rápidos e críticos.","Golpes lentos em área.","Flechas perfurantes; carregar parado."};
 bool WeaponUnlockedIn(SaveData a,int id)=>id==-1||id>=0&&id<=2||id==3&&(a.relicOwned&4)!=0||id==4&&(a.relicOwned&16)!=0||id==5&&(a.relicOwned&16)!=0||id>=6&&id<=8&&HeroUnlockedIn(a,id-3);
 static int WeaponSlot(int choice,int h)=>choice<0?(h==3||h==4?1:h==5?0:h):choice<6?choice:choice==8?0:1;
 static int NaturalWeaponChoice(int h)=>h>=3?h+3:h;
 int SelectedWeaponChoice {get{save.NormalizeWeapons();int id=save.weaponLoadouts[heroIndex];return WeaponUnlockedIn(save,id)?id:-1;}}
 int WeaponArchetype=>SelectedWeaponChoice<0?(heroIndex>=3?heroIndex:-1):SelectedWeaponChoice>=6?SelectedWeaponChoice-3:-1;
 int runWeaponChoice=-1;
 SpriteRenderer heldWeapon,offhandWeapon,gripFingers,offhandFingers;
 ScreenState arsenalReturn=ScreenState.PowerUp;
 bool EquippedVisual=>runArmor>=0||runWeaponChoice>=0;
 Sprite DressedHero(int frame=0)=>EquippedVisual?EquipmentArt.Body(heroIndex,runArmor,frame):ArtBank.Hero(heroIndex,frame);
 string SelectedWeaponName=>SelectedWeaponChoice<0?"Padrão • "+EquipmentWeapons[NaturalWeaponChoice(heroIndex)]:EquipmentWeapons[SelectedWeaponChoice];
 bool TryEquipStartingWeapon(int id){if(!IsHeroUnlocked(heroIndex)||id< -1||id>8||!WeaponUnlockedIn(save,id))return false;save.NormalizeWeapons();save.weaponLoadouts[heroIndex]=id;if(buildLabActive)labWeapons[WeaponSlot(id,heroIndex)]=Mathf.Max(1,labWeapons[WeaponSlot(id,heroIndex)]);if(!verifying&&!buildLabActive)save.Store();return true;}
 void ResetEquippedVisual(){runWeaponChoice=SelectedWeaponChoice;heldWeapon=offhandWeapon=gripFingers=offhandFingers=null;}
 Vector2 WeaponPalm(bool other=false){var local=EquipmentArt.PalmLocal(heroIndex,hero.sprite,other);if(hero.flipX)local.x=-local.x;return hero.transform.TransformPoint(local);}
 void UpdateHeldWeapon(){
  if(!EquippedVisual||hero==null||world==null){foreach(var r in new[]{heldWeapon,offhandWeapon,gripFingers,offhandFingers})if(r!=null)r.enabled=false;return;}
  int choice=runWeaponChoice<0?NaturalWeaponChoice(heroIndex):runWeaponChoice;
  PlaceWeapon(ref heldWeapon,ref gripFingers,choice,false);
  if(choice==1)PlaceWeapon(ref offhandWeapon,ref offhandFingers,choice,true);
  else {if(offhandWeapon!=null)offhandWeapon.enabled=false;if(offhandFingers!=null)offhandFingers.enabled=false;}
 }
 void PlaceWeapon(ref SpriteRenderer weapon,ref SpriteRenderer fingers,int choice,bool other){
  if(weapon==null)weapon=Sprite(other?"Arma na segunda mão":"Arma na mão",EquipmentArt.Held(choice,other),pos,1,Color.white,Depth(pos)+3);
  if(fingers==null)fingers=Sprite("Dedos sobre a empunhadura",EquipmentArt.Fingers(heroIndex,hero.sprite,other),pos,1,Color.white,Depth(pos)+4);
  weapon.enabled=fingers.enabled=true;weapon.sprite=EquipmentArt.Held(choice,other);fingers.sprite=EquipmentArt.Fingers(heroIndex,hero.sprite,other);weapon.flipX=fingers.flipX=hero.flipX;
  var palm=WeaponPalm(other);weapon.transform.position=fingers.transform.position=palm;
  float swing=heroAttackTime>0?Mathf.Sin(Mathf.Clamp01(heroAttackTime/.18f)*Mathf.PI)*55:0;
  weapon.transform.rotation=Quaternion.Euler(0,0,(hero.flipX?-1:1)*(EquipmentArt.HeldAngle(choice,other)-swing));weapon.transform.localScale=Vector3.one*EquipmentArt.HeldSize(choice);
  fingers.transform.rotation=Quaternion.identity;fingers.transform.localScale=hero.transform.localScale;
  weapon.sortingOrder=Depth(pos)+3;fingers.sortingOrder=Depth(pos)+4;
 }
 void SyncHeldWeaponPixel(){
  if(hero==null||!pixelFinal.TryGetValue(hero,out var body))return;
  SyncWeaponPart(heldWeapon,false,false,body);SyncWeaponPart(offhandWeapon,true,false,body);SyncWeaponPart(gripFingers,false,true,body);SyncWeaponPart(offhandFingers,true,true,body);
 }
 void SyncWeaponPart(SpriteRenderer source,bool other,bool fingers,PixelProxy body){
  if(source==null||!source.enabled)return;
  var p=Pixel(source,pos,source.transform.localScale.x*(fingers?body.view.transform.localScale.x/hero.transform.localScale.x:1.72f),0,false,false,fingers?10:9);
  var local=EquipmentArt.PalmLocal(heroIndex,hero.sprite,other);if(hero.flipX)local.x=-local.x;
  p.view.transform.position=body.view.transform.TransformPoint(local);
  p.view.transform.rotation=cam.transform.rotation*(fingers?Quaternion.identity:source.transform.rotation);
 }
 void OpenArsenal(ScreenState from){arsenalReturn=from;if(buildLabActive){save.NormalizeRelics();save.relicLoadouts[heroIndex*2]=labArmor;save.relicLoadouts[heroIndex*2+1]=labTalisman;}state=ScreenState.Arsenal;}
 void CloseArsenal(){if(buildLabActive){labArmor=EquippedRelic(heroIndex,0);labTalisman=EquippedRelic(heroIndex,1);}state=arsenalReturn;}
 string WeaponUnlockHint(int id)=>id<=2?"Disponível":id==3?"Colete o Peitoral da Geada":id==4||id==5?"Colete a Armadura das Cinzas":"Resgate "+Catalog.Heroes[id-3].Split(',')[0];
 bool EquipBuildPreset(int id){int weapon=id==0?6:id==1?3:7,armor=id*2;if(!WeaponUnlockedIn(save,weapon)||!OwnsRelic(armor)||!OwnsRelic(armor+1)||!IsHeroUnlocked(heroIndex))return false;TryEquipStartingWeapon(weapon);TryEquipRelic(heroIndex,armor);TryEquipRelic(heroIndex,armor+1);if(buildLabActive){labArmor=armor;labTalisman=armor+1;}Toast("Combo equipado • "+RelicSets[id]);return true;}
 void DrawGripSprite(Sprite sprite,Vector2 anchor,float scale,float angle=0){
  var before=GUI.matrix;GUI.matrix=before*Matrix4x4.TRS(anchor,Quaternion.Euler(0,0,-angle),Vector3.one);
  var r=sprite.rect;GUI.DrawTextureWithTexCoords(new Rect(-sprite.pivot.x*scale,-(r.height-sprite.pivot.y)*scale,r.width*scale,r.height*scale),sprite.texture,new Rect(r.x/sprite.texture.width,r.y/sprite.texture.height,r.width/sprite.texture.width,r.height/sprite.texture.height),true);GUI.matrix=before;
 }
 int MenuWeaponChoice(int h){save.NormalizeWeapons();int choice=save.weaponLoadouts[h];return WeaponUnlockedIn(save,choice)?choice:-1;}
 void DrawMenuHero(int h,float x,float y,float size)=>DrawEquippedPortrait(h,EquippedRelic(h,0),MenuWeaponChoice(h),x,y,size);
 void DrawEquippedPortrait(int h,int armor,int choice,float x,float y,float size){
  var body=EquipmentArt.Body(h,armor,0);int weapon=choice<0?NaturalWeaponChoice(h):choice,hands=weapon==1?2:1;
  Vector2 min=-body.pivot/body.pixelsPerUnit,max=(body.rect.size-body.pivot)/body.pixelsPerUnit;
  for(int hand=0;hand<hands;hand++){
   bool other=hand==1;var held=EquipmentArt.MenuHeld(weapon,other);var palm=EquipmentArt.MenuPalm(h,armor,other);var turn=Quaternion.Euler(0,0,EquipmentArt.HeldAngle(weapon,other));
   for(int corner=0;corner<4;corner++){Vector2 point=new Vector2((corner%2)*held.rect.width,(corner/2)*held.rect.height)-held.pivot;point=palm+(Vector2)(turn*(point/held.pixelsPerUnit*EquipmentArt.HeldSize(weapon)/1.25f));min=Vector2.Min(min,point);max=Vector2.Max(max,point);}
  }
  float scale=Mathf.Min(size/(max.x-min.x),size/(max.y-min.y));Vector2 feet=new Vector2(x+size*.5f-(min.x+max.x)*.5f*scale,y+size*.5f+(min.y+max.y)*.5f*scale);
  DrawGripSprite(body,feet,scale/body.pixelsPerUnit);
  for(int hand=0;hand<hands;hand++){bool other=hand==1;var local=EquipmentArt.MenuPalm(h,armor,other);var palm=feet+new Vector2(local.x,-local.y)*scale;var held=EquipmentArt.MenuHeld(weapon,other);DrawGripSprite(held,palm,scale/1.25f*EquipmentArt.HeldSize(weapon)/held.pixelsPerUnit,EquipmentArt.HeldAngle(weapon,other));}
  for(int hand=0;hand<hands;hand++){bool other=hand==1;var local=EquipmentArt.MenuPalm(h,armor,other);DrawGripSprite(EquipmentArt.MenuFingers(h,armor,other),feet+new Vector2(local.x,-local.y)*scale,scale/body.pixelsPerUnit);}
 }
 void DrawArsenal(){
  MainBackdrop();Rect(0,0,1280,720,new Color(.005f,.01f,.02f,.5f));Text("ARSENAL • ARMAS E BUILDS",48,25,1184,54,Style(34,gold,TextAnchor.MiddleLeft,true));Text("Troque a arma inicial de qualquer herói. O ataque e a arma na mão mudam; a habilidade Q continua sendo do herói.",50,85,1180,30,Style(13,cyan));DrawHeroTabs(48,130,1184);
  int armor=EquippedRelic(heroIndex,0);Frame(12,48,190,300,439);DrawEquippedPortrait(heroIndex,armor,SelectedWeaponChoice,78,217,238);
  Text(Catalog.Heroes[heroIndex],69,464,258,42,Style(18,gold,TextAnchor.MiddleCenter,true));Text(SelectedWeaponName,69,511,258,40,Style(17,cyan,TextAnchor.MiddleCenter));Text(armor<0?"Traje original":RelicNames[armor],69,555,258,35,Style(14,Color.white,TextAnchor.MiddleCenter));Text("Arma no nível 1 ao iniciar.\\nOs demais poderes evoluem na partida.".Replace("\\n","\n"),69,581,258,32,Style(11,Color.white,TextAnchor.MiddleCenter));
  for(int i=0;i<9;i++){float x=374+i%3*286,y=191+i/3*131;bool unlocked=WeaponUnlockedIn(save,i),chosen=SelectedWeaponChoice==i;Frame(12,x,y,272,120);GUI.color=unlocked?Color.white:new Color(.4f,.4f,.45f,.7f);DrawIcon(EquipmentArt.Weapon(i),x+14,y+19,60);GUI.color=Color.white;Text(EquipmentWeapons[i],x+83,y+25,174,27,Style(14,gold,TextAnchor.MiddleLeft,true));Text(unlocked?EquipmentWeaponText[i]:WeaponUnlockHint(i),x+83,y+55,174,27,Style(11,Color.white));if(Button(!unlocked?"BLOQUEADA":chosen?"EQUIPADA":"EQUIPAR",x+14,y+84,244,27,unlocked,chosen,12))TryEquipStartingWeapon(i);}
  string[] presets={"KATANA + RAÍZES","GELO + GEADA","ESPADÃO + CINZAS"};Text("COMBOS • USA SOMENTE EQUIPAMENTOS JÁ DESBLOQUEADOS",374,591,844,23,Style(11,cyan));
  for(int i=0;i<3;i++){int weapon=i==0?6:i==1?3:7;bool allowed=WeaponUnlockedIn(save,weapon)&&OwnsRelic(i*2)&&OwnsRelic(i*2+1);if(Button(presets[i],374+i*286,620,272,31,allowed,false,12))EquipBuildPreset(i);}
  if(Button("USAR ARMA PADRÃO DO HERÓI",48,656,430,43,true,SelectedWeaponChoice<0,13))TryEquipStartingWeapon(-1);
  if(Button("CONFIRMAR E VOLTAR",790,662,440,37,true,true,15))CloseArsenal();
 }
}
}
