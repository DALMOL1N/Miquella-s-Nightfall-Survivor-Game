using System;
using UnityEngine;
namespace NoiteSemFim {
public sealed partial class SurvivorGame {
 float musicVolume=.4f;
 int mainFocus;

 GUIStyle sliderTrack,sliderThumb;
 bool IsHeroUnlocked(int id)=>HeroUnlockedIn(save,id);
 void LoadMenuSettings(){manualAim=PlayerPrefs.GetInt(EditorTestProfile.Key("NSF.ManualAim"),0)==1;selectedDragon=Mathf.Clamp(PlayerPrefs.GetInt(EditorTestProfile.Key("NSF.Dragon"),EditorTestProfile.Enabled?0:-1),-1,2);ValidateDragonSelection();selectedCurse=PlayerPrefs.GetInt(EditorTestProfile.Key("NSF.Curse"),-1);if(!CurseUnlocked(selectedCurse))selectedCurse=-1;volume=Mathf.Clamp01(PlayerPrefs.GetFloat(EditorTestProfile.Key("NSF.SfxVolume"),.22f));musicVolume=Mathf.Clamp01(PlayerPrefs.GetFloat(EditorTestProfile.Key("NSF.MusicVolume"),.4f));if(music!=null)music.volume=musicVolume;}
 void SaveMenuSettings(){if(verifying||buildLabActive)return;PlayerPrefs.SetInt(EditorTestProfile.Key("NSF.ManualAim"),manualAim?1:0);PlayerPrefs.SetInt(EditorTestProfile.Key("NSF.Dragon"),selectedDragon);PlayerPrefs.SetInt(EditorTestProfile.Key("NSF.Curse"),selectedCurse);PlayerPrefs.SetFloat(EditorTestProfile.Key("NSF.SfxVolume"),volume);PlayerPrefs.SetFloat(EditorTestProfile.Key("NSF.MusicVolume"),musicVolume);PlayerPrefs.SetInt(EditorTestProfile.Key("NSF.Muted"),muted?1:0);PlayerPrefs.Save();}
 void GoHome(){SaveMenuSettings();state=ScreenState.Menu;mainFocus=0;}
 void OpenSelection(){if(!IsHeroUnlocked(heroIndex))heroIndex=0;stageIndex=0;state=ScreenState.Selection;}
 void OpenOptions(){state=ScreenState.Options;}
 void CloseOptions(){SaveMenuSettings();state=ScreenState.Menu;}
 void MainAction(int action){if(action==0)OpenSelection();else if(action==1)OpenPowerUp(ScreenState.Menu);else if(action==2)OpenOptions();else if(action==3)ExitGame();}
 void HandleMainMenuKeys(){mainFocus=Mathf.Clamp(mainFocus,0,3);if(Input.GetKeyDown(KeyCode.DownArrow)||Input.GetKeyDown(KeyCode.Tab))mainFocus=(mainFocus+1)%4;if(Input.GetKeyDown(KeyCode.UpArrow))mainFocus=(mainFocus+3)%4;if(Input.GetKeyDown(KeyCode.Return)||Input.GetKeyDown(KeyCode.KeypadEnter))MainAction(mainFocus);}
 void ExitGame(){if(verifying)return;SaveMenuSettings();save.Store();
 #if UNITY_EDITOR
 UnityEditor.EditorApplication.isPlaying=false;
 #else
 Application.Quit();
 #endif
 }
 bool TryBuyPerk(int id){if(id<0||id>=save.perks.Length||save.perks[id]>=5)return false;int cost=50+save.perks[id]*75;if(save.coins<cost)return false;save.coins-=cost;save.perks[id]++;if(!verifying)save.Store();return true;}
 void MainBackdrop(){Rect(0,0,1280,720,ink);if(UISkin.Background!=null)GUI.DrawTexture(new Rect(0,0,1280,720),UISkin.Background,ScaleMode.ScaleAndCrop);Rect(0,0,1280,720,new Color(.008f,.015f,.03f,.14f));}
 void DrawMainMenu(){
  MainBackdrop();
  for(int i=0;i<32;i++)Rect(i*28,80,29,584,new Color(.006f,.014f,.028f,.58f*(1-i/32f)));
  Rect(0,0,1280,88,new Color(.008f,.015f,.026f,.68f));
  if(Button("SAIR",48,22,136,44,true,mainFocus==3,17))MainAction(3);
  Frame(1,984,18,248,54);DrawIcon(ArtBank.Icon(14),1000,29,32);Text(save.coins.ToString("N0"),1047,28,160,31,Style(22,gold,TextAnchor.MiddleRight));
  var title=Style(68,new Color(.97f,.78f,.43f),TextAnchor.MiddleLeft,true);title.wordWrap=false;
  var subtitle=Style(31,new Color(.97f,.78f,.43f),TextAnchor.MiddleLeft,true);subtitle.wordWrap=false;
  var shadow=new GUIStyle(title);shadow.normal.textColor=new Color(0,0,0,.85f);Text(GameIdentity.MenuTitle,83,147,650,96,shadow);
  Text(GameIdentity.MenuTitle,80,144,650,96,title);Text(GameIdentity.MenuSubtitle,83,241,650,56,new GUIStyle(subtitle){normal={textColor=new Color(0,0,0,.85f)}});Text(GameIdentity.MenuSubtitle,80,238,650,56,subtitle);
  Text("SOBREVIVA ATÉ O AMANHECER",84,303,600,28,Style(14,new Color(.78f,.9f,.88f)));
  if(Button("INICIAR",80,352,480,70,true,mainFocus==0,30))MainAction(0);
  if(Button("POWER UP",80,438,480,58,true,mainFocus==1,24))MainAction(1);
  Text("Monte sua build e prepare-se para a próxima noite.",86,501,474,25,Style(13,new Color(.84f,.88f,.88f),TextAnchor.MiddleCenter));
  if(Button("TREINO GRÁTIS",80,536,232,46,true,false,15))EnterBuildLab();
  if(Button("FAVORITAS",328,536,232,46,true,false,15))OpenFavorites();
  if(Button("OPÇÕES",80,598,480,46,true,mainFocus==2,19))MainAction(2);
  Text("↑ ↓  Selecionar     Enter  Confirmar",48,670,620,24,Style(12,new Color(.8f,.84f,.86f)));
  Text("v0.30 • NIGHTFALL",1024,670,208,24,Style(12,new Color(.8f,.84f,.86f),TextAnchor.MiddleRight));
 }
 float VolumeSlider(string label,float value,float x,float y,float width){
  Text(label,x,y,width-90,30,Style(18,Color.white));Text(Mathf.RoundToInt(value*100)+"%",x+width-84,y,84,30,Style(21,gold,TextAnchor.MiddleRight));
  if(sliderTrack==null){sliderTrack=new GUIStyle{fixedHeight=24};sliderThumb=new GUIStyle{fixedWidth=18,fixedHeight=24};}
  Rect(x,y+48,width,4,new Color(.19f,.26f,.29f));Rect(x,y+48,width*value,4,cyan);float knob=x+Mathf.Clamp01(value)*(width-16);Rect(knob,y+39,16,22,gold);Rect(knob+4,y+43,8,14,new Color(.94f,.91f,.76f));
  return GUI.HorizontalSlider(new Rect(x-2,y+36,width+4,30),value,0,1,sliderTrack,sliderThumb);
 }
 void DrawOptions(){
  Backdrop();Text("OPÇÕES",48,28,1184,64,Style(42,gold,TextAnchor.MiddleLeft,true));Text("Ajuste o áudio e escolha como quer mirar durante a caçada.",52,104,1180,30,Style(16,cyan));
  Frame(12,48,158,560,424);Frame(12,632,158,600,424);
  Text("ÁUDIO",104,211,448,38,Style(24,gold,TextAnchor.MiddleLeft,true));Rect(80,254,496,1,new Color(.55f,.46f,.29f,.6f));
  musicVolume=VolumeSlider("Música",musicVolume,88,274,440);volume=VolumeSlider("Efeitos sonoros",volume,88,364,440);if(music!=null){music.volume=musicVolume;music.mute=muted;}
  if(Button(muted?"SOM DESLIGADO":"SOM LIGADO",88,460,210,48,true,!muted,15)){muted=!muted;if(music!=null)music.mute=muted;}
  if(Button("TESTAR EFEITO",314,460,214,48,!muted,false,15))Play(gemSound);
  Text("Os volumes são salvos para a próxima vez que jogar.",88,530,440,38,Style(13,new Color(.76f,.81f,.83f)));
  Text("CONTROLES E MIRA",688,211,488,38,Style(24,gold,TextAnchor.MiddleLeft,true));Rect(664,254,536,1,new Color(.55f,.46f,.29f,.6f));
  if(Button("AUTOMÁTICA",672,280,244,51,true,!manualAim,17))manualAim=false;
  if(Button("MANUAL • MOUSE",932,280,244,51,true,manualAim,16))manualAim=true;
  Text(AimDescription,676,352,492,65,Style(15,Color.white));
  Text("COMANDOS",676,432,492,28,Style(17,gold,TextAnchor.MiddleLeft,true));
  string[] keys={"WASD / setas","Espaço","Esc","M"},actions={"Mover","Esquiva","Pausar ou voltar","Ligar / desligar som"};for(int i=0;i<4;i++){Text(keys[i],676,470+i*24,164,23,Style(14,cyan));Text(actions[i],854,470+i*24,310,23,Style(14,new Color(.78f,.84f,.86f)));}
  Text("As alterações são aplicadas durante o ajuste.",52,628,670,40,Style(14,new Color(.78f,.84f,.86f)));
  if(Button("SALVAR E VOLTAR",850,620,382,56,true,true,20))CloseOptions();
 }
 string VerifyMainMenu(){var original=save;bool previous=verifying;verifying=true;save=new SaveData();state=ScreenState.Menu;OpenSelection();if(state!=ScreenState.Selection)throw new Exception("Start navigation failed");GoHome();MainAction(1);if(state!=ScreenState.PowerUp)throw new Exception("Power up navigation failed");GoHome();MainAction(2);if(state!=ScreenState.Options)throw new Exception("Options navigation failed");CloseOptions();if(state!=ScreenState.Menu)throw new Exception("Options return failed");save.coins=500;if(!TryBuyPerk(0)||save.coins!=450||save.perks[0]!=1)throw new Exception("Power up purchase failed");save.coins=0;if(TryBuyPerk(0))throw new Exception("Insufficient funds regression");save.coins=9999;save.perks[0]=5;if(TryBuyPerk(0))throw new Exception("Maximum perk regression");save=original;verifying=previous;state=ScreenState.Menu;return "Main menu, selection, options, return and power up purchase PASS.";}
}
}
