using UnityEngine;
namespace NoiteSemFim {
public sealed partial class SurvivorGame {
 int galleryCategory,galleryMode;
 float galleryClock;
 bool galleryPaused;
 static readonly string[] GalleryTabs={"HERÓIS","INIMIGOS","CHEFES","DRAGÕES","HABILIDADES","ITENS","ELEMENTOS"};
 static readonly string[] EnemyNames={"Esqueleto","Morcego","Errante","Conjurador"};
 static readonly string[] BossNames={"Cavaleiro Sombrio","Soberano","Arquimago Glacial","Colosso Ígneo"};
 static readonly string[] ItemNames={"Baú","Cristal de experiência","Cura","Atração"};
 static readonly string[] StatusNames={"Chão em chamas","Queimadura","Geada","Eletricidade","Vapor","Água eletrizada"};
 void OpenGallery(){galleryClock=0;galleryPaused=false;state=ScreenState.AnimationGallery;}
 int GalleryCount=>galleryCategory==0||galleryCategory==3?3:galleryCategory==4?8:galleryCategory==6?6:4;
 int GalleryFrame(int index){if(galleryCategory>=4||galleryCategory==5)return ArtBank.Frame(galleryClock+index*.09f,8);if(galleryMode==1)return galleryCategory==3?4:0;if(galleryMode==2)return ArtBank.Frame(galleryClock,2,2)==0?5:0;return galleryCategory==3?ArtBank.Frame(galleryClock,8):1+ArtBank.Frame(galleryClock,8);}
 Sprite GallerySprite(int i,int f)=>galleryCategory==0?ArtBank.Hero(i,f):galleryCategory==1?ArtBank.Enemy(i==3?5:i,f):galleryCategory==2?ArtBank.Enemy(new[]{3,4,6,7}[i],f):galleryCategory==3?ArtBank.Dragon(i,f):galleryCategory==4?ArtBank.Ability(i,f):galleryCategory==5?ArtBank.Loot(i==0?1:i==1?0:i,f):ArtBank.Status(i,f);
 string GalleryName(int i)=>galleryCategory==0?Catalog.Heroes[i]:galleryCategory==1?EnemyNames[i]:galleryCategory==2?BossNames[i]:galleryCategory==3?DragonNames[i]:galleryCategory==4?(i<6?Catalog.Weapons[i]:i==6?"Impacto":"Esquiva"):galleryCategory==5?ItemNames[i]:StatusNames[i];
 void DrawAnchoredPreview(Sprite sprite,float x,float y,float w,float h,bool feet,float fixedScale=0){if(sprite==null)return;if(!feet){DrawIcon(sprite,x+(w-Mathf.Min(w,h))/2,y,Mathf.Min(w,h));return;}float scale=fixedScale>0?fixedScale:h/1.45f;float sw=sprite.rect.width/sprite.pixelsPerUnit*scale,sh=sprite.rect.height/sprite.pixelsPerUnit*scale;float footX=x+w*.5f,footY=y+h-7;Rect(x+15,footY,w-30,1,new Color(.3f,.7f,.7f,.4f));GUI.DrawTextureWithTexCoords(new Rect(footX-sprite.pivot.x/sprite.pixelsPerUnit*scale,footY-(sprite.rect.height-sprite.pivot.y)/sprite.pixelsPerUnit*scale,sw,sh),sprite.texture,new Rect(sprite.rect.x/sprite.texture.width,sprite.rect.y/sprite.texture.height,sprite.rect.width/sprite.texture.width,sprite.rect.height/sprite.texture.height),true);}
 void DrawGallery(){Backdrop();Text("GALERIA DE ANIMAÇÕES",48,22,1184,55,Style(36,gold,TextAnchor.MiddleLeft,true));for(int c=0;c<7;c++)if(Button(GalleryTabs[c],48+c*171,90,158,45,true,galleryCategory==c,13)){galleryCategory=c;galleryClock=0;}int count=GalleryCount;int cols=count>4?4:count;int rows=count>4?2:1;float cw=(1184-(cols-1)*14)/cols,ch=rows==2?196:406;for(int i=0;i<count;i++){float x=48+i%cols*(cw+14),y=154+i/cols*(ch+12);Frame(12,x,y,cw,ch);Rect(x+14,y+27,cw-28,32,new Color(.02f,.025f,.035f,.94f));Text(GalleryName(i),x+15,y+28,cw-30,30,Style(17,gold,TextAnchor.MiddleCenter,true));float previewScale=0;if(galleryCategory<4){float left=.1f,right=.1f,top=.1f;for(int a=0;a<6;a++){var pose=GallerySprite(i,a);left=Mathf.Max(left,pose.pivot.x/pose.pixelsPerUnit);right=Mathf.Max(right,(pose.rect.width-pose.pivot.x)/pose.pixelsPerUnit);top=Mathf.Max(top,(pose.rect.height-pose.pivot.y)/pose.pixelsPerUnit);}previewScale=Mathf.Min((cw/2-25)/left,(cw/2-25)/right,(ch-130)/top);}int frame=GalleryFrame(i);DrawAnchoredPreview(GallerySprite(i,frame),x+14,y+65,cw-28,ch-112,galleryCategory<4,previewScale);Text("Pose "+(frame+1),x+14,y+ch-34,cw-28,20,Style(12,cyan,TextAnchor.MiddleCenter));}
  if(Button("MOVIMENTO",48,584,210,48,true,galleryMode==0,15))galleryMode=0;if(Button("REPOUSO",273,584,210,48,true,galleryMode==1,15))galleryMode=1;if(Button("ATAQUE",498,584,210,48,true,galleryMode==2,15))galleryMode=2;if(Button(galleryPaused?"RETOMAR":"CONGELAR POSE",723,584,240,48,true,galleryPaused,15))galleryPaused=!galleryPaused;
  Text("A linha marca o apoio dos pés. Observe os ciclos e alterne entre os estados.",48,650,900,30,Style(14,Color.white));if(Button("VOLTAR AO MENU",993,640,239,49,true,true,14))GoHome();
 }
}
}
