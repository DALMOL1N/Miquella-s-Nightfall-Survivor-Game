using System;
using System.Collections.Generic;
using UnityEngine;
namespace NoiteSemFim {
public sealed partial class SurvivorGame {
 // All combat still runs on the original XY plane. Only presentation uses XZ.
 sealed class PixelProxy {public SpriteRenderer source,view,shadow;public int visit;}
 readonly Dictionary<SpriteRenderer,PixelProxy> pixelFinal=new Dictionary<SpriteRenderer,PixelProxy>();
 readonly List<SpriteRenderer> retiredPixels=new List<SpriteRenderer>();
 Sprite contactShadowSprite;
 int pixelVisit;
 Sprite ContactShadow(){if(contactShadowSprite!=null)return contactShadowSprite;var t=new Texture2D(64,64,TextureFormat.RGBA32,false){name="Pixel 3D contact shadow",filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};for(int y=0;y<64;y++)for(int x=0;x<64;x++){float radius=new Vector2((x-31.5f)/31.5f,(y-31.5f)/31.5f).magnitude;t.SetPixel(x,y,new Color(1,1,1,Mathf.Pow(Mathf.Clamp01(1-radius),1.5f)));}t.Apply();contactShadowSprite=UnityEngine.Sprite.Create(t,new Rect(0,0,64,64),Vector2.one*.5f,64);return contactShadowSprite;}
 PixelProxy Pixel(SpriteRenderer source,Vector2 point,float scale,float height=0,bool flat=false,bool shadow=false,int layer=0){if(source==null)return null;if(!pixelFinal.TryGetValue(source,out var p)||p.view==null){var o=new GameObject("Pixel 2.5D • "+source.name);o.transform.SetParent(finalWorld,false);p=new PixelProxy{source=source,view=o.AddComponent<SpriteRenderer>()};p.view.sharedMaterial=source.sharedMaterial;p.view.spriteSortPoint=SpriteSortPoint.Pivot;pixelFinal[source]=p;}p.visit=pixelVisit;p.view.sprite=source.sprite;p.view.color=source.color;p.view.flipX=source.flipX;p.view.flipY=source.flipY;p.view.enabled=source.enabled&&source.gameObject.activeInHierarchy;p.view.forceRenderingOff=false;p.view.sortingOrder=(flat?-1000:Depth(point)*2)+layer;p.view.transform.position=FinalPoint(point,height);p.view.transform.rotation=flat?Quaternion.Euler(90,0,0):cam.transform.rotation;p.view.transform.localScale=new Vector3(scale,scale,1);
  if(shadow&&p.shadow==null){var o=new GameObject("Contact shadow");o.transform.SetParent(p.view.transform.parent,false);p.shadow=o.AddComponent<SpriteRenderer>();p.shadow.sprite=ContactShadow();p.shadow.color=new Color(0,0,0,.45f);p.shadow.sortingOrder=-2000;}
  if(p.shadow!=null){p.shadow.enabled=shadow&&p.view.enabled;p.shadow.transform.position=FinalPoint(point,.055f);p.shadow.transform.rotation=Quaternion.Euler(90,0,0);float soft=height>.5f?.8f:1;p.shadow.transform.localScale=new Vector3(scale*.62f*soft,scale*.37f*soft,1);}
  return p;
 }
 void ClearPixelFinal(){pixelFinal.Clear();retiredPixels.Clear();pixelVisit=0;}
 void SyncPixelFinal(){if(!final3DActive||!final3DVisible||world==null||finalWorld==null)return;pixelVisit++;
  // Hide the original XY visuals, but keep the new billboard copies enabled.
  foreach(var r in world.GetComponentsInChildren<SpriteRenderer>(true))if(!r.transform.IsChildOf(finalWorld))r.forceRenderingOff=true;
  var hp=Pixel(hero,pos,2.15f,.045f,false,true,1);if(hp!=null)hp.view.transform.localScale=new Vector3(2.15f*hero.transform.localScale.x/1.25f,2.15f*hero.transform.localScale.y/1.25f,1);
  if(dragon!=null)Pixel(dragon,dragonPos,1.35f,1.0f+Mathf.Sin(dragonAnimClock*3)*.08f,false,true,2);
  foreach(var e in enemies){float size=e.type==4?3.45f:IsBoss(e.type)?2.65f:e.type==1?1.15f:1.55f;Pixel(e.sprite,e.p,size,e.type==1?.8f:.045f,false,true);if(e.status!=null)Pixel(e.status,e.p,size*.8f,size*.35f,false,false,3);if(e.warning!=null)Pixel(e.warning,e.lockedAim,e.warning.transform.localScale.x,.09f,true,false,25);}
  foreach(var shot in shots){var p=Pixel(shot.sprite,shot.p,Mathf.Clamp(shot.radius*3.4f,.32f,1.8f),.65f,false,false,4);if(p!=null&&shot.v.sqrMagnitude>.01f){var a=cam.WorldToScreenPoint(FinalPoint(shot.p,.65f));var b=cam.WorldToScreenPoint(FinalPoint(shot.p+shot.v.normalized,.65f));p.view.transform.rotation=cam.transform.rotation*Quaternion.Euler(0,0,Mathf.Atan2(b.y-a.y,b.x-a.x)*Mathf.Rad2Deg);}}
  foreach(var l in loot)Pixel(l.sprite,l.p,l.kind==1?1.10f:l.kind==4?.9f:.65f,.32f+Mathf.Sin(l.animClock*3)*.06f,false,l.kind==1);
  foreach(var f in effects)if(f.sprite!=null){bool flat=f.kind==3||f.kind==7;Pixel(f.sprite,f.sprite.transform.position,f.sprite.transform.localScale.x,flat?.075f:.65f,flat,false,8);}
  foreach(var h in hazards)Pixel(h.sprite,h.p,h.radius*2,.07f,true,false,20);
  foreach(var g in ground){bool flat=g.kind==GroundKind.Frost||g.kind==GroundKind.Water;Pixel(g.sprite,g.p,g.radius*1.9f,flat?.06f:.08f+g.radius*.35f,flat,false,1);}
  foreach(var f in fuel)if(f.flame!=null)Pixel(f.flame,f.p,f.grass?1:1.6f,.6f,false,false,3);
  foreach(var fallenEnemy in fallen)if(fallenEnemy.sprite!=null)Pixel(fallenEnemy.sprite,fallenEnemy.p,fallenEnemy.sprite.transform.localScale.x,.15f,false,false,-2);
  foreach(var orb in orbitVisuals)if(orb!=null&&orb.enabled)Pixel(orb,(Vector2)orb.transform.position-Vector2.up*.35f,orb.transform.localScale.x*1.8f,.8f,false,false,5);
  SyncHeldWeaponPixel();SyncRescuePixels();SyncRelicAttackPixels();SyncSignaturePixels();SyncExclusivePixel();
  if(playerStatus!=null)Pixel(playerStatus,pos,1.75f,.75f,false,false,3);
  if(powerPreparationHalo!=null)Pixel(powerPreparationHalo,pos,powerPreparationHalo.transform.localScale.x,.08f,true,false,10);
  retiredPixels.Clear();foreach(var pair in pixelFinal)if(pair.Key==null||pair.Value.view==null||pair.Value.visit!=pixelVisit)retiredPixels.Add(pair.Key);foreach(var key in retiredPixels){if(pixelFinal.TryGetValue(key,out var p)){if(p.view!=null)Dispose(p.view.gameObject);if(p.shadow!=null)Dispose(p.shadow.gameObject);}pixelFinal.Remove(key);}
 }
 void VerifyPixelPresentation(){if(!pixelFinal.TryGetValue(hero,out var p)||p.view.sprite!=DressedHero(0))throw new Exception("2.5D hero does not use the original armor pixel sprite");if(p.shadow==null||!p.shadow.enabled)throw new Exception("2.5D contact shadow missing");if(Quaternion.Angle(p.view.transform.rotation,cam.transform.rotation)>.1f)throw new Exception("2.5D character does not face the camera");var old=p.view.sprite;AnimateHero(.15f,Vector2.right);hero.flipX=true;SyncPixelFinal();p=pixelFinal[hero];if(p.view.sprite==old||!p.view.flipX)throw new Exception("2.5D animation/frame/flip not synchronized");var expected=FinalPoint(pos,.045f);if(Vector3.Distance(p.view.transform.position,expected)>.01f)throw new Exception("2.5D foot anchor separated from gameplay position");if(finalModels.Count!=0)throw new Exception("3D character meshes still instantiated");var g=AddGround(pos+Vector2.right*2,1,GroundKind.Water,2,0);SyncPixelFinal();if(!pixelFinal.TryGetValue(g.sprite,out var water)||Vector3.Dot(water.view.transform.forward,Vector3.up)>-.99f)throw new Exception("2.5D water is not on the floor");var enemy=SpawnEnemy(0,false);enemy.p=pos+Vector2.left*2;SyncPixelFinal();var key=enemy.sprite;enemies.Remove(enemy);SyncPixelFinal();if(pixelFinal.ContainsKey(key))throw new Exception("2.5D proxy survived removal");Dispose(key.gameObject);}
}
}
