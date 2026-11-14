using System;
using System.Collections.Generic;
using UnityEngine;
namespace NoiteSemFim {
public static class Final3DAssets {
 static readonly Dictionary<string,GameObject> prefabs=new Dictionary<string,GameObject>();
 public static GameObject Prefab(string name){if(prefabs.TryGetValue(name,out var p)&&p!=null)return p;p=Resources.Load<GameObject>("Final3D/"+name);if(p==null)throw new InvalidOperationException("Missing 3D prefab: "+name);prefabs[name]=p;return p;}
 public static string HeroName(int hero,int armor)=>"hero-"+hero+"-"+(armor>=0?armor/2+1:0);
 public static int Audit(){int total=0;for(int h=0;h<3;h++)for(int a=0;a<5;a++)Check("hero-"+h+"-"+a,ref total);foreach(var pair in new[]{new KeyValuePair<string,int>("dragon",3),new KeyValuePair<string,int>("enemy",8),new KeyValuePair<string,int>("ability",8),new KeyValuePair<string,int>("relic",8),new KeyValuePair<string,int>("loot",4),new KeyValuePair<string,int>("status",6),new KeyValuePair<string,int>("ground",4),new KeyValuePair<string,int>("prop",12)})for(int i=0;i<pair.Value;i++)Check(pair.Key+"-"+i,ref total);Check("arena",ref total);if(total!=69)throw new Exception("Incomplete final 3D collection");return total;}
 static void Check(string name,ref int count){var p=Prefab(name);if(p.GetComponentsInChildren<SpriteRenderer>(true).Length>0)throw new Exception("3D asset contains sprite billboard: "+name);var meshes=p.GetComponentsInChildren<MeshFilter>(true);if(meshes.Length==0)throw new Exception("Empty 3D asset: "+name);foreach(var m in meshes)if(m.sharedMesh==null||m.sharedMesh.vertexCount<3||m.GetComponent<MeshRenderer>().sharedMaterial==null)throw new Exception("Invalid 3D asset: "+name);count++;}
}
}
