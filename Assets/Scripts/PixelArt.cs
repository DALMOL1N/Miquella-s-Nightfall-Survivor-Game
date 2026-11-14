using UnityEngine;
namespace NoiteSemFim {
public static class PixelArt {
 public static Sprite Make(string[] rows, Color color, Color accent) {
  var tex=new Texture2D(rows[0].Length,rows.Length,TextureFormat.RGBA32,false); tex.filterMode=FilterMode.Point; tex.wrapMode=TextureWrapMode.Clamp;
  for(int y=0;y<rows.Length;y++) for(int x=0;x<rows[0].Length;x++) {char c=rows[y][x]; tex.SetPixel(x,rows.Length-y-1,c=='.'?Color.clear:c=='#'?color:c=='+'?accent:c=='o'?new Color(.06f,.07f,.12f):Color.white);}
  tex.Apply(); return Sprite.Create(tex,new Rect(0,0,tex.width,tex.height),new Vector2(.5f,.5f),16);
 }
 public static Sprite Hero(Color c) {return Make(new[]{"......++++......",".....++++++.....",".....oooooo.....",".....o**ooo.....","......oooo......","....########....","...##########...","...##+####+##...","...oo+####+oo...",".....######.....",".....######.....",".....##..##.....","....ooo..ooo....","................","................","................"},c,new Color(1,.76f,.37f));}
 public static Sprite Enemy(int type) {
  if(type==3||type==4) return Make(new[]{".....##..##.....","....########....","...##+####+##...","..############..","..##o*oooo*o##..","..###oooooo###..","...##########...","..############..",".##############.","##.##########.##","##.##########.##","...##########...","....###..###....","...####..####...","..ooooo..ooooo..","................"},type==4?new Color(.77f,.19f,.37f):new Color(.6f,.36f,.7f),new Color(1,.65f,.28f));
  if(type==1) return Make(new[]{"................","................",".#............#.",".##....##....##.",".###..####..###.",".##############.","..############..","...##*##*##.....","....######......",".....####.......","................","................","................","................","................","................"},new Color(.53f,.35f,.66f),Color.red);
  return Make(new[]{"................",".....######.....","....########....","....#oo##oo#....","....#oo##oo#....",".....######.....","......####......","....########....","...##.####.##...","...##.####.##...","...##.####.##...","......####......",".....##..##.....",".....##..##.....","....ooo..ooo....","................"},type==2?new Color(.35f,.72f,.51f):new Color(.8f,.83f,.74f),new Color(.95f,.3f,.25f));
 }
 public static Sprite Gem=Make(new[]{"...++...","..+##+..",".+####+.","+######+",".+####+.","..+##+..","...++...","........"},new Color(.18f,.78f,.96f),new Color(.67f,1,1));
 public static Sprite Orb=Make(new[]{"..++++..",".+####+.","+######+","+##**##+","+##**##+","+######+",".+####+.","..++++.."},Color.white,Color.white);
 public static Sprite Chest=Make(new[]{"............","..++++++++..",".+########+.","+##########+","++++++++++++","+####++####+","+####++####+","+##########+","++++++++++++","............"},new Color(.52f,.28f,.13f),new Color(1,.78f,.29f));
 public static Sprite Tree=Make(new[]{".......++.......","......####......",".....######.....","....########....","...##########...","..############..",".##############.","....########....","...##########...","..############..",".##############.","################","......oooo......","......oooo......","......oooo......","................"},new Color(.13f,.28f,.23f),new Color(.25f,.4f,.3f));
 public static Sprite Grave=Make(new[]{"................",".....######.....","....########....","....###++###....","....##++++##....","....###++###....","....###++###....","....########....","....########....","....########....","....########....","...##########...","..############..","................","................","................"},new Color(.3f,.34f,.42f),new Color(.48f,.5f,.6f));
 public static Sprite Floor(int stage) {
  var tex=new Texture2D(32,32,TextureFormat.RGBA32,false);tex.filterMode=FilterMode.Point; tex.wrapMode=TextureWrapMode.Repeat;
  var rng=new System.Random(14+stage); for(int y=0;y<32;y++)for(int x=0;x<32;x++){float v=(float)rng.NextDouble()*.035f;var c=stage==0?new Color(.065f+v,.13f+v,.12f+v):new Color(.105f+v,.1f+v,.15f+v);if((x%16==0||y%16==0)&&stage==1)c*=.85f;tex.SetPixel(x,y,c);}tex.Apply();return Sprite.Create(tex,new Rect(0,0,32,32),new Vector2(.5f,.5f),16,0,SpriteMeshType.FullRect);
 }
}}
