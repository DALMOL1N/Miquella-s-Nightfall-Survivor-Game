using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
namespace NoiteSemFim {
public sealed partial class SurvivorGame {
 bool capturingModelStudio;
 IEnumerator CaptureModelStudio(string folder){verifying=true;capturingModelStudio=true;Application.runInBackground=true;Directory.CreateDirectory(folder);ReturnMenu();state=ScreenState.Menu;finalOrigin=Vector2.zero;finalWorld=new GameObject("Model QA studio").transform;cam.rect=new Rect(0,0,1,1);cam.orthographic=false;cam.fieldOfView=34;cam.nearClipPlane=.05f;cam.farClipPlane=70;cam.backgroundColor=new Color(.018f,.026f,.048f);cam.transform.position=new Vector3(3.5f,2.8f,-7);cam.transform.LookAt(new Vector3(.15f,1.3f,0));RenderSettings.ambientMode=AmbientMode.Flat;RenderSettings.ambientLight=new Color(.24f,.27f,.33f);RenderSettings.fog=false;QualitySettings.shadows=ShadowQuality.All;QualitySettings.shadowDistance=45;QualitySettings.antiAliasing=4;
  StudioLight("QA key",new Vector3(45,-35,0),new Color(.78f,.85f,1),1.0f);StudioLight("QA rim",new Vector3(30,145,0),new Color(1,.62f,.32f),.6f);
  var floor=GameObject.CreatePrimitive(PrimitiveType.Cylinder);floor.name="Studio floor";floor.transform.SetParent(finalWorld,false);floor.transform.localScale=new Vector3(9,.05f,7);floor.transform.position=new Vector3(0,-.1f,0);var mat=new Material(Shader.Find("Standard"));mat.color=new Color(.055f,.07f,.11f);mat.SetFloat("_Glossiness",.15f);floor.GetComponent<MeshRenderer>().material=mat;
  var heroKey=new GameObject("QA hero key");var dragonKey=new GameObject("QA dragon key");var h=Model(heroKey,"hero-0-0");var d=Model(dragonKey,"dragon-0");PlaceModel(h,new Vector2(-.8f,0),1,0);PlaceModel(d,new Vector2(1,0),1,0);PoseModel(h,0,Vector2.down,false,false);PoseModel(d,0,Vector2.down,false,false);yield return Capture(folder,"modelos-na-unity-v15");
  for(int i=0;i<24;i++){float t=i/12f;PoseModel(h,t,Vector2.down,i>=16,i<16);PoseModel(d,t,Vector2.down,i>=16,true);yield return Frames(2);ScreenCapture.CaptureScreenshot(Path.Combine(folder,"movimento-"+i.ToString("000")+".png"));yield return Frames(2);}
  foreach(var pair in finalModels)if(pair.Value.root!=null)pair.Value.root.gameObject.SetActive(false);
  var lineKeys=new[]{new GameObject(),new GameObject(),new GameObject(),new GameObject()};for(int i=0;i<4;i++){var m=Model(lineKeys[i],i==3?"enemy-4":"hero-"+i+"-0");PlaceModel(m,new Vector2(-2.2f+i*1.5f,0),i==3?1.13f:1,0);PoseModel(m,.2f,Vector2.down,false,false);}cam.transform.position=new Vector3(4,3,-10);cam.transform.LookAt(new Vector3(0,1.4f,0));yield return Capture(folder,"herois-e-soberano-na-unity-v15");Debug.Log("REFINED MODEL STUDIO CAPTURE PASS");Application.Quit();
 }
 void StudioLight(string name,Vector3 angles,Color color,float intensity){var o=new GameObject(name);o.transform.SetParent(finalWorld,false);o.transform.rotation=Quaternion.Euler(angles);var l=o.AddComponent<Light>();l.type=LightType.Directional;l.color=color;l.intensity=intensity;l.shadows=LightShadows.Soft;l.shadowStrength=.6f;l.shadowBias=.05f;l.shadowNormalBias=.3f;}
}
}
