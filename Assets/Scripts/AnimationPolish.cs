using System;
using System.Collections.Generic;
using UnityEngine;
namespace NoiteSemFim {
public sealed partial class SurvivorGame {
 bool heroIsMoving;
 float heroAnimClock,heroAttackTime,heroCastGate,dragonAnimClock,dragonAttackTime,chestOpenedAt;
 readonly List<SpriteRenderer> orbitVisuals=new List<SpriteRenderer>();
 readonly List<DeathVisual> fallen=new List<DeathVisual>();
 sealed class DeathVisual {public SpriteRenderer sprite;public float age,scale;public Vector2 p;}
 void ResetAnimations(){heroIsMoving=false;heroAnimClock=heroAttackTime=heroCastGate=dragonAnimClock=dragonAttackTime=0;orbitVisuals.Clear();fallen.Clear();}
 void CastVisual(){if(heroCastGate>0)return;heroAttackTime=.18f;heroCastGate=.38f;}
 static int BodyFrame(float clock,bool moving,bool attacking){return attacking?5:moving?1+ArtBank.Frame(clock,8):0;}
 void AnimateHero(float dt,Vector2 move){heroAnimClock+=dt;heroAttackTime=Mathf.Max(0,heroAttackTime-dt);heroCastGate=Mathf.Max(0,heroCastGate-dt);bool moving=move.sqrMagnitude>.01f;heroIsMoving=moving;hero.sprite=DressedHero(BodyFrame(heroAnimClock,moving,heroAttackTime>0||PreparingPowerId>=0));hero.transform.position=pos;float breath=!moving&&heroAttackTime<=0?Mathf.Sin(heroAnimClock*2.7f)*.012f:0;hero.transform.localScale=new Vector3(1.25f*(1-breath*.35f),1.25f*(1+breath),1);UpdateHeldWeapon();}
 void AnimateEnemy(Enemy e,float dt,Vector2 delta){e.animClock+=dt*(e.type==1?1.5f:Mathf.Clamp(e.speed, .7f,2))*(e.slow>0?.4f:1);e.animAttack=Mathf.Max(0,e.animAttack-dt);int frame=BodyFrame(e.animClock,delta.sqrMagnitude>.02f,e.animAttack>0);e.sprite.sprite=EnemyAppearance(e,frame);e.sprite.flipX=delta.x<0;}
 void OrbitVisual(int index,Vector2 p,float scale){while(orbitVisuals.Count<=index)orbitVisuals.Add(Sprite("Solar Orb",ArtBank.Ability(2),p,scale,Color.white,600));var r=orbitVisuals[index];r.enabled=true;r.sprite=ArtBank.Ability(2,ArtBank.Frame(heroAnimClock+index*.13f,9));r.transform.position=p+Vector2.up*.35f;r.transform.localScale=Vector3.one*scale;}
 void PlayDeath(Enemy e){if(fallen.Count>=32)return;float scale=e.sprite.transform.localScale.x;var r=Sprite("Defeated Enemy",e.sprite.sprite,e.p,scale,new Color(.7f,.65f,.7f,.8f),e.sprite.sortingOrder);r.flipX=e.sprite.flipX;fallen.Add(new DeathVisual{sprite=r,p=e.p,scale=scale});}
 void UpdateAnimationDetails(float dt){for(int i=fallen.Count-1;i>=0;i--){var f=fallen[i];f.age+=dt;float t=f.age/.35f;if(t>=1){Dispose(f.sprite.gameObject);fallen.RemoveAt(i);continue;}f.sprite.color=new Color(.65f,.6f,.7f,(1-t)*.8f);f.sprite.transform.position=f.p+Vector2.down*t*.1f;f.sprite.transform.localScale=new Vector3(f.scale*(1+t*.08f),f.scale*(1-t*.5f),1);}}
 string VerifyAnimationPolish(){string atlas=ArtBank.Audit();for(int heroId=0;heroId<Catalog.Heroes.Length;heroId++)for(int frame=0;frame<6;frame++)if(ArtBank.Hero(heroId,frame)==null)throw new Exception("Missing hero frame");for(int t=0;t<8;t++){if(t==4||t==3||t==6||t==7||t<3||t==5)for(int f=0;f<6;f++)if(ArtBank.Enemy(t,f)==null)throw new Exception("Missing enemy frame");}if(BodyFrame(.15f,false,false)!=0||BodyFrame(.15f,true,true)!=5)throw new Exception("Animation state priority failed");var walked=new HashSet<int>();for(int i=0;i<20;i++)walked.Add(BodyFrame(i*.05f,true,false));if(walked.Count!=4||walked.Contains(0)||walked.Contains(5))throw new Exception("Walk cycle must contain exactly four walk poses");if(ArtBank.Frame(.5f,8)!=0)throw new Exception("Animation loop discontinuity");return "Animation states, four-pose walk loops, attack separation, cached sprites and "+atlas+" PASS.";}
}
}
