using UnityEngine;
namespace NoiteSemFim {
public sealed partial class SurvivorGame {
 void LateUpdate(){if(capturingModelStudio)return;RefreshAimCursor();if(cam==null)return;if(state==ScreenState.Arsenal||state==ScreenState.Favorites||state==ScreenState.BuildLab||state==ScreenState.Menu||state==ScreenState.Shop||state==ScreenState.Selection||state==ScreenState.Options||state==ScreenState.DragonDen||state==ScreenState.Relics||state==ScreenState.PowerUp||state==ScreenState.AnimationGallery){cam.rect=new Rect(0,0,1,1);cam.orthographicSize=7;return;}float scale=Mathf.Min(UnityEngine.Screen.width/1280f,UnityEngine.Screen.height/720f);float ox=(UnityEngine.Screen.width-1280*scale)/2,oy=(UnityEngine.Screen.height-720*scale)/2;const float top=154,bottom=590;cam.rect=new Rect(ox/UnityEngine.Screen.width,(oy+(720-bottom)*scale)/UnityEngine.Screen.height,1280*scale/UnityEngine.Screen.width,(bottom-top)*scale/UnityEngine.Screen.height);UpdateHeldWeapon();if(final3DActive){FinalCamera();SyncFinal3D();}else cam.orthographicSize=7*(bottom-top)/720;}
}
}
