using System;
using System.Collections.Generic;
using UnityEngine;
namespace NoiteSemFim {
// Every animated row uses one pixel scale and alpha-derived foot anchors.
public static class ArtBank {
 static readonly Dictionary<string,Sprite[]> sheets=new Dictionary<string,Sprite[]>();
 static readonly Dictionary<string,FrameAudit[]> audit=new Dictionary<string,FrameAudit[]>();
 public struct FrameAudit {public string sheet;public int row,column;public Rect crop;public Vector2 pivot;public float ppu;public bool border;}
 static Sprite[] Sheet(string name,int columns,int rows,bool feet=false){if(sheets.TryGetValue(name,out var cached))return cached;var texture=Resources.Load<Texture2D>("Art/"+name);if(texture==null)return null;texture.filterMode=FilterMode.Point;var sprites=new Sprite[columns*rows];float w=texture.width/(float)columns,h=texture.height/(float)rows;for(int row=0;row<rows;row++)for(int col=0;col<columns;col++)sprites[row*columns+col]=Sprite.Create(texture,new Rect(col*w,texture.height-(row+1)*h,w,h),feet?new Vector2(.5f,.42f):Vector2.one*.5f,w*.72f,0,SpriteMeshType.FullRect);sheets[name]=sprites;return sprites;}
 internal static int[] AxisCuts(Color32[] pixels,int width,int height,int count,bool horizontal,int regionMin=0,int regionMax=-1){int length=horizontal?width:height,other=horizontal?height:width;if(regionMax<0)regionMax=other;var projection=new int[length];for(int a=0;a<length;a++)for(int b=regionMin;b<regionMax;b++)if(pixels[horizontal?b*width+a:a*width+b].a>=96)projection[a]++;var result=new int[count+1];result[count]=length;for(int k=1;k<count;k++){int target=Mathf.RoundToInt(k*length/(float)count),span=Mathf.RoundToInt(length/(float)count*.22f);int lo=Mathf.Max(1,target-span),hi=Mathf.Min(length-1,target+span),minimum=int.MaxValue;for(int a=lo;a<hi;a++)minimum=Mathf.Min(minimum,projection[a]);int start=-1,best=target;float bestScore=float.MinValue;for(int a=lo;a<=hi;a++){bool low=a<hi&&projection[a]<=minimum+1;if(low&&start<0)start=a;if(!low&&start>=0){float center=(start+a-1)*.5f;float score=(a-start)-Mathf.Abs(center-target)*.05f;if(score>bestScore){bestScore=score;best=Mathf.RoundToInt(center);}start=-1;}}result[k]=best;}return result;}
 static Sprite[] Packed(string name,int columns,int rows,bool footAnchor){if(sheets.TryGetValue(name,out var cached))return cached;var t=Resources.Load<Texture2D>("Art/"+name);if(t==null)return null;if(!t.isReadable)throw new InvalidOperationException(name+" must be readable for sprite calibration");var pixels=t.GetPixels32();var sprites=new Sprite[columns*rows];var info=new FrameAudit[sprites.Length];var rowCuts=AxisCuts(pixels,t.width,t.height,rows,false);
  for(int row=0;row<rows;row++){int rowY0=rowCuts[rows-row-1],rowY1=rowCuts[rows-row];var columnCuts=AxisCuts(pixels,t.width,t.height,columns,true,rowY0,rowY1);var boxes=new RectInt[columns];var roots=new float[columns];var heights=new List<float>();var extents=new List<float>();
   for(int col=0;col<columns;col++){int x0=columnCuts[col],x1=columnCuts[col+1];var localRows=AxisCuts(pixels,t.width,t.height,rows,false,x0,x1);int y0=localRows[rows-row-1],y1=localRows[rows-row];int minX=x1,minY=y1,maxX=x0,maxY=y0;
    for(int y=y0;y<y1;y++)for(int x=x0;x<x1;x++)if(pixels[y*t.width+x].a>=96){minX=Mathf.Min(minX,x);maxX=Mathf.Max(maxX,x);minY=Mathf.Min(minY,y);maxY=Mathf.Max(maxY,y);}
    if(minX>maxX)throw new InvalidOperationException("Empty animation frame: "+name+" row "+row+" col "+col);
    // Ignore isolated alpha flecks when finding the supporting foot/body axis.
    double sum=0,total=0;int bodyHeight=maxY-minY+1;int bandStart=name=="dragons-v7"?minY:minY+(int)(bodyHeight*.28f);int bandEnd=name=="dragons-v7"?minY+Mathf.Max(3,bodyHeight/10):minY+(int)(bodyHeight*.72f);for(int y=bandStart;y<=Mathf.Min(maxY,bandEnd);y++)for(int x=minX;x<=maxX;x++){byte a=pixels[y*t.width+x].a;if(a>=96){sum+=x*a;total+=a;}}
    roots[col]=total>0?(float)(sum/total):(minX+maxX)*.5f;bool coreBorder=minX<=x0||maxX>=x1-1||minY<=y0||maxY>=y1-1;float coreHeight=maxY-minY+1,coreExtent=Mathf.Max(maxX-minX+1,maxY-minY+1);if(!footAnchor){for(int gy=y0;gy<y1;gy++)for(int gx=x0;gx<x1;gx++)if(pixels[gy*t.width+gx].a>=8){minX=Mathf.Min(minX,gx);maxX=Mathf.Max(maxX,gx);minY=Mathf.Min(minY,gy);maxY=Mathf.Max(maxY,gy);}}boxes[col]=new RectInt(Mathf.Max(x0,minX-2),Mathf.Max(y0,minY-2),Mathf.Min(x1,maxX+3)-Mathf.Max(x0,minX-2),Mathf.Min(y1,maxY+3)-Mathf.Max(y0,minY-2));heights.Add(coreHeight);extents.Add(coreExtent);
    info[row*columns+col]=new FrameAudit{sheet=name,row=row,column=col,border=coreBorder};
   }
   heights.Sort();extents.Sort();float ppu=footAnchor?heights[heights.Count/2]/1.08f:1;
   for(int col=0;col<columns;col++){var b=boxes[col];int idx=row*columns+col;float unitPpu=footAnchor?ppu:name=="icons-v7"?Mathf.Max(b.width,b.height):extents[extents.Count/2]+4;var pivot=footAnchor?new Vector2(Mathf.Clamp01((roots[col]-b.x)/b.width),2f/b.height):Vector2.one*.5f;var rect=new Rect(b.x,b.y,b.width,b.height);sprites[idx]=Sprite.Create(t,rect,pivot,unitPpu,0,SpriteMeshType.FullRect);sprites[idx].name=name+"_"+row+"_"+col;var item=info[idx];item.crop=rect;item.pivot=pivot;item.ppu=unitPpu;info[idx]=item;}
  }
  sheets[name]=sprites;audit[name]=info;return sprites;
 }
 static Sprite Animated(string name,int columns,int rows,int row,int frame,bool feet){var s=Packed(name,columns,rows,feet);return s==null?null:s[Mathf.Clamp(row,0,rows-1)*columns+Mathf.Clamp(frame,0,columns-1)];}
 static readonly Dictionary<int,Sprite> relicIcons=new Dictionary<int,Sprite>();
 public static Sprite Relic(int id){if(id<0||id>=8)return null;if(relicIcons.TryGetValue(id,out var s))return s;var t=Resources.Load<Texture2D>("Art/relic-"+id+"-v10");if(t==null)return null;t.filterMode=FilterMode.Point;s=Sprite.Create(t,new Rect(0,0,t.width,t.height),Vector2.one*.5f,t.width,0,SpriteMeshType.FullRect);relicIcons[id]=s;return s;}
 public static int Frame(float seconds,float fps,int count=4)=>Mathf.FloorToInt(Mathf.Max(0,seconds)*fps)%count;
 static Sprite NewHero(int id,int frame)=>NewHeroSprites.Get(id,frame);
 public static Sprite Hero(int id,int frame=0)=>id>=3?NewHero(id,frame):Animated("heroes-v7",6,3,id,frame,true)??Sheet("heroes",4,3,true)?[id*4+Mathf.Min(frame,3)];
 public static Sprite ArmoredHero(int heroId,int armorId,int frame=0)=>armorId<0||armorId>=8||armorId%2!=0?Hero(heroId,frame):EquipmentArt.Body(heroId,armorId,frame);
 public static Sprite Enemy(int type,int frame=0){bool boss=type==3||type==4||type==6||type==7;int row=boss?(type==3?0:type==4?1:type==6?2:3):type==5?3:type;return Animated(boss?"bosses-v7":"enemies-v7",6,4,row,frame,true)??Sheet(boss?"bosses":"enemies",4,4,true)?[row*4+Mathf.Min(frame,3)];}
 public static Sprite Dragon(int id,int frame=0)=>Animated("dragons-v7",6,3,id,frame,true)??Sheet("dragons-v5",3,2,true)?[Mathf.Min(frame,1)*3+id];
 public static Sprite AttackRelicFx(int row,int frame=0)=>Animated("relic-attacks-v23",4,3,row,frame,false);
 public static Sprite Ability(int kind,int frame=0)=>Animated("abilities-v7",4,8,kind,frame,false)??Icon(Mathf.Clamp(kind,0,5));
 public static Sprite Loot(int kind,int frame=0)=>Animated("loot-v7",4,4,kind==1?0:kind==0?1:kind==2?2:3,frame,false)??(kind==1?Prop(12):Icon(kind==0?12:kind==2?9:14));
 public static Sprite Status(int row,int frame=0)=>Animated("status-v7",4,6,row,frame,false)??ElementFx(row==0?0:row==1?4:row==2?6:row==3?7:row==4?10:11);
 public static Sprite ElementFx(int id){var s=Sheet("elements-v6",4,3);return s==null?PixelArt.Orb:s[Mathf.Clamp(id,0,11)];}
 public static Sprite Icon(int id)=>Animated("icons-v7",4,4,id/4,id%4,false)??Sheet("icons",4,4)?[Mathf.Clamp(id,0,15)];
 public static Sprite Prop(int id){var s=Sheet("terrain",4,4,true);return s==null?null:s[Mathf.Clamp(id,0,15)];}
 static readonly Dictionary<int,Sprite> floors=new Dictionary<int,Sprite>();
 public static Sprite Floor(int stage){if(floors.TryGetValue(stage,out var f))return f;var s=Sheet("floors",2,2);if(s==null)return PixelArt.Floor(stage);f=Sprite.Create(s[stage].texture,s[stage].rect,Vector2.one*.5f,s[stage].rect.width/5,0,SpriteMeshType.FullRect);floors[stage]=f;return f;}
 public static void Preload(){AttackRelicFx(0);Hero(0);Enemy(0);Enemy(3);Dragon(0);Icon(0);Ability(0);Loot(0);Status(0);for(int i=0;i<4;i++)ArmoredHero(0,i*2);}
 public static string Audit(){Preload();int count=0;var report=new System.Text.StringBuilder();foreach(var pair in audit){foreach(var f in pair.Value){if(f.border)throw new Exception("Animation crosses cell boundary: "+f.sheet+" "+f.row+","+f.column);if(f.crop.width<4||f.crop.height<4||f.ppu<=0)throw new Exception("Invalid sprite crop: "+pair.Key);count++;report.AppendLine(f.sheet+","+f.row+","+f.column+","+f.crop.x+","+f.crop.y+","+f.crop.width+","+f.crop.height+","+f.pivot.x.ToString("F3",System.Globalization.CultureInfo.InvariantCulture)+","+f.pivot.y.ToString("F3",System.Globalization.CultureInfo.InvariantCulture)+","+f.ppu.ToString("F2",System.Globalization.CultureInfo.InvariantCulture)+","+f.border);}}if(count!=184||audit.Count!=9)throw new Exception("Incomplete base animation atlas set");LastAuditCsv="sheet,row,column,x,y,width,height,pivotX,pivotY,pixelsPerUnit,touchesCellBorder\n"+report;return count+" calibrated frames across "+audit.Count+" atlases";}
 public static string LastAuditCsv {get;private set;}
 public static Sprite Ring {get{if(ring!=null)return ring;var t=new Texture2D(64,64,TextureFormat.RGBA32,false);t.filterMode=FilterMode.Point;for(int y=0;y<64;y++)for(int x=0;x<64;x++){float d=Vector2.Distance(new Vector2(x,y),new Vector2(31.5f,31.5f));t.SetPixel(x,y,d<31&&d>28?Color.white:new Color(1,1,1,d<28?.06f:0));}t.Apply();ring=Sprite.Create(t,new Rect(0,0,64,64),new Vector2(.5f,.5f),64);return ring;}}
 static Sprite ring;
}}
