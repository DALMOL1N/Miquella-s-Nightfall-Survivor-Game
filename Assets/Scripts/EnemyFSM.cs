using System;
using UnityEngine;
namespace NoiteSemFim {
 // Priority: death, training, frozen, attack ready, chase.
 // An attacking enemy executes once, resets its timer, and resumes chasing next tick.
 public enum EnemyState { Chasing, Attacking, Frozen, Training, Dead }
 public sealed partial class SurvivorGame {
  void AdvanceEnemyFSM(Enemy e,float dt){
   if(e.hp<=0){e.aiState=EnemyState.Dead;return;}
   if(e.trainingDummy){e.aiState=EnemyState.Training;return;}
   if(e.frozen>0){if(e.biome>=0)CancelBiomeWindup(e);e.aiState=EnemyState.Frozen;return;}
   if(e.biome>=0){AdvanceBiomeFSM(e,dt);return;}
   e.attack-=dt;
   bool canAttack=IsBoss(e.type)||(e.type==5&&(e.p-pos).sqrMagnitude<144);
   e.aiState=canAttack&&e.attack<=0?EnemyState.Attacking:EnemyState.Chasing;
  }
  void ExecuteEnemyAttack(Enemy e){
   if(e.biome>=0){ExecuteBiomeAttack(e);return;}
   if(IsBoss(e.type)){
    e.animAttack=1.25f;e.attack=(e.type==4?3.4f:6)*(e.phaseTwo?.65f:1);
    if(e.phaseTwo)PhaseAttack(e);
    int rings=e.type==4?5:e.type==6?3:1;
    for(int r=0;r<rings;r++)AddHazard(pos+UnityEngine.Random.insideUnitCircle*(rings>1?3:.1f),e.type==7?2.2f:1.4f,e.damage,e.type==6?Element.Ice:e.type==4&&((int)time%2==0)?Element.Lightning:Element.Fire);
   }else if(e.type==5){
    e.animAttack=.3f;e.attack=5;
    Fire(e.p,(pos-e.p).normalized*4,e.damage,.2f,1,new Color(1,.4f,.15f),3,-1,Element.Fire,true);
   }
  }
  string VerifyEnemyFSM(){
   var account=save;bool oldVerify=verifying;int oldHero=heroIndex;
   verifying=true;save=new SaveData();heroIndex=0;StartRun();
   var e=SpawnEnemy(5,false);e.p=pos+Vector2.right*4;e.attack=1;
   UpdateEnemies(.1f);if(e.aiState!=EnemyState.Chasing)throw new Exception("FSM chase failed");
   e.attack=0;int before=shots.Count;UpdateEnemies(.01f);
   if(e.aiState!=EnemyState.Attacking||shots.Count!=before+1||e.attack!=5)throw new Exception("FSM attack failed");
   UpdateEnemies(.01f);if(e.aiState!=EnemyState.Chasing||shots.Count!=before+1)throw new Exception("FSM repeated attack");
   e.frozen=.2f;e.attack=0;Vector2 prior=e.p;UpdateEnemies(.1f);
   if(e.aiState!=EnemyState.Frozen||e.p!=prior||e.attack!=0||shots.Count!=before+1)throw new Exception("FSM freeze failed");
   UpdateEnemies(.11f);UpdateEnemies(.01f);if(e.aiState!=EnemyState.Attacking)throw new Exception("FSM thaw failed");
   e.p=pos+Vector2.right*13;e.attack=0;UpdateEnemies(.01f);if(e.aiState!=EnemyState.Chasing)throw new Exception("FSM range failed");
   e.trainingDummy=true;UpdateEnemies(.01f);if(e.aiState!=EnemyState.Training)throw new Exception("FSM dummy failed");
   e.hp=0;AdvanceEnemyFSM(e,.1f);if(e.aiState!=EnemyState.Dead)throw new Exception("FSM death priority failed");
   e.trainingDummy=false;e.hp=100;e.type=6;e.p=pos+Vector2.right*5;e.attack=0;before=hazards.Count;UpdateEnemies(.01f);
   if(e.aiState!=EnemyState.Attacking||hazards.Count!=before+3)throw new Exception("FSM boss attack failed");
   save=account;heroIndex=oldHero;verifying=oldVerify;ReturnMenu();
   return "Enemy FSM PASS: explicit states drive chase, single cast, cooldown, freeze/resume, range, training, death priority and boss hazards.";
  }
 }
}
