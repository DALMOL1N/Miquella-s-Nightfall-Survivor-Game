using UnityEngine;
namespace NoiteSemFim {
public sealed partial class SurvivorGame {
 Font menuDisplayFont,menuReadingFont;
 bool MenuPresentation=>state==ScreenState.Menu||state==ScreenState.Options||state==ScreenState.Selection||state==ScreenState.Shop||state==ScreenState.PowerUp||state==ScreenState.DragonDen||state==ScreenState.Relics||state==ScreenState.Arsenal||state==ScreenState.BuildLab||state==ScreenState.Favorites;
 Font MenuDisplayFont {get {if(menuDisplayFont==null)menuDisplayFont=Resources.Load<Font>("Fonts/CinzelDecorative-Regular");return menuDisplayFont;}}
 Font MenuReadingFont {get {if(menuReadingFont==null)menuReadingFont=Resources.Load<Font>("Fonts/NightfallInterface-Regular");return menuReadingFont;}}
 GUIStyle FitMenuText(string text,GUIStyle source,float width,float height){
  var style=new GUIStyle(source);if(style.font==null||style.font==labelStyle.font)style.font=MenuReadingFont;style.clipping=TextClipping.Clip;
  var content=new GUIContent(text);int minimum=width<70?9:Mathf.Min(12,style.fontSize);float available=Mathf.Max(1,width-4);
  while(style.fontSize>minimum&&(style.CalcHeight(content,available)>height-2||!style.wordWrap&&style.CalcSize(content).x>available))style.fontSize--;
  return style;
 }
}
}
