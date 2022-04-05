using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bose.Wearable;


public class CommentaryController : MonoBehaviour
{

  [SerializeField]
  private WearableControl wearableControl;


  public bool GalleryIntroPlayed = true;
  public bool PaintingIntroPlayed = false;
  public bool NoddedYet = false;
  public bool QuestionAsked = false;
  public bool CommentaryOn = true;
  private bool CommentaryTurnedOffYet = false;


  private GameObject audioGuide;
  private enum CommentaryClip {
    GalleryIntroduction,
    PaintingIntroduction,
    CommentaryOffTest,
    CommentaryOffFirstTime,
    CommentaryOff,
    CommentaryOn,
    FirstNod,
    NodOrShake,
    Intro,
    MoreArtist,
    Artist,
    MorePainting,
    Painting,
    MoreMusic,
    Music
  }
  public enum Location {
    Gallery,
    Yellowstone,
    Bosworth,
    Mary,
    TheGirl,
    Wollaston,
    RailwayStation
  }
  private Location currentLocation;
  private bool CommentaryTalking = false;
  private bool WaitingForCommentaryOffTest = false;
  private bool WaitingForNodOrShake = false;
  private CommentaryClip currentCommentaryClip;
  FMOD.Studio.EventInstance commentaryClipInstance;




  // Start is called before the first frame update
  void Start()
  {

  }

  // Update is called once per frame
  void Update()
  {
    if(CommentaryTalking) {
      if(commentaryClipInstance.isValid()){
        FMOD.Studio.PLAYBACK_STATE playbackState;
        commentaryClipInstance.getPlaybackState(out playbackState);
        if (playbackState == FMOD.Studio.PLAYBACK_STATE.STOPPED){
          Debug.Log("Commentary Stopped");
          CommentaryTalking = false;

          OnCommentaryComplete();
        }
      }
    }

  }

  void OnEnable(){
    WearableControl.Instance.AffirmativeGestureDetected += HandleAffirmativeGesture;
    WearableControl.Instance.NegativeGestureDetected += HandleNegativeGesture;
    WearableControl.Instance.InputGestureDetected += HandleInputGesture;
    /*OnNod += NodDetected;
    OnShake += ShakeDetected;
    OnCommentaryOff += StopCommentary;
    OnTestCommentaryOff += TestStopCommentary;*/
  }

  void OnDisable(){
    WearableControl.Instance.AffirmativeGestureDetected -= HandleAffirmativeGesture;
    WearableControl.Instance.NegativeGestureDetected -= HandleNegativeGesture;
    WearableControl.Instance.InputGestureDetected -= HandleInputGesture;
    /*OnNod -= NodDetected;
    OnShake -= ShakeDetected;
    OnCommentaryOff -= StopCommentary;
    OnTestCommentaryOff -= TestStopCommentary;*/
  }

  public void SetAudioGuide(GameObject newAudioGuide) {
    audioGuide = newAudioGuide;
  }

  public void SetCurrentLocation(Location newCurrentLocation) {
    currentLocation = newCurrentLocation;
  }

  public void StartCommentary(){
    Debug.Log("StartCommentary");
    if(CommentaryOn){
      Debug.Log(currentLocation);
      if(currentLocation == Location.Gallery){
        if(!GalleryIntroPlayed){
          PlayClip(CommentaryClip.GalleryIntroduction.ToString());
          GalleryIntroPlayed = true;
          Debug.Log("Playing Gallery Introduction");
        }
      } else {
        if(!PaintingIntroPlayed){
          PaintingIntroPlayed = true;
          currentCommentaryClip = CommentaryClip.PaintingIntroduction;
        } else {
          currentCommentaryClip = CommentaryClip.Intro;
        }
        PlayPaintingCommentary();
      }
    }
  }


  private void PlayPaintingCommentary(){
    switch(currentCommentaryClip){
      case CommentaryClip.PaintingIntroduction:
        PlayClip(CommentaryClip.PaintingIntroduction.ToString());
        WaitingForCommentaryOffTest = true;
      break;
      case CommentaryClip.Intro:
        PlayClip(currentLocation.ToString() + CommentaryClip.Intro.ToString());

      break;
    }
  }

  private void OnCommentaryComplete(){
    CommentaryTalking = false;
    if(CommentaryOn){
      switch(currentCommentaryClip){
        case CommentaryClip.PaintingIntroduction:
          currentCommentaryClip = CommentaryClip.CommentaryOffTest;
          break;
        case CommentaryClip.CommentaryOffTest:
          currentCommentaryClip = CommentaryClip.Intro;
          PlayClip(currentLocation.ToString() + CommentaryClip.Intro.ToString());
          break;
        case CommentaryClip.Intro:
          currentCommentaryClip = CommentaryClip.MoreArtist;
          WaitingForNodOrShake = true;
          PlayClip(currentLocation.ToString() + CommentaryClip.MoreArtist.ToString());
          break;
        case CommentaryClip.Artist:
          currentCommentaryClip = CommentaryClip.MorePainting;
          WaitingForNodOrShake = true;
          PlayClip(currentLocation.ToString() + CommentaryClip.MorePainting.ToString());
          break;
        case CommentaryClip.Painting:
          currentCommentaryClip = CommentaryClip.MoreMusic;
          WaitingForNodOrShake = true;
          PlayClip(currentLocation.ToString() + CommentaryClip.MoreMusic.ToString());
          break;
        case CommentaryClip.Music:
          currentCommentaryClip = CommentaryClip.MoreArtist;
          WaitingForNodOrShake = true;
          PlayClip(currentLocation.ToString() + CommentaryClip.MoreArtist.ToString());
          break;
        default:
          FMODUnity.RuntimeManager.StudioSystem.setParameterByName("DuckMusic", 0);
          break;
      }

    } else {
      FMODUnity.RuntimeManager.StudioSystem.setParameterByName("DuckMusic", 0);
    }
  }

  private void PlayClip(string commentaryClip){
    if(CommentaryTalking){
      commentaryClipInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }
    commentaryClipInstance = FMODUnity.RuntimeManager.CreateInstance("event:/Commentary/" + commentaryClip);
    if(audioGuide){
      FMODUnity.RuntimeManager.StudioSystem.setParameterByName("DuckMusic", 40);
      FMODUnity.RuntimeManager.AttachInstanceToGameObject(commentaryClipInstance, audioGuide.GetComponent<Transform>(), audioGuide.GetComponent<Rigidbody>());
      CommentaryTalking = true;
      commentaryClipInstance.start();
    }
  }


  public void HandleAffirmativeGesture() {
    Debug.Log("Nod");
    if(WaitingForNodOrShake){
      WaitingForNodOrShake = false;
      switch(currentCommentaryClip){
        case CommentaryClip.MoreArtist:
          currentCommentaryClip = CommentaryClip.Artist;
          PlayClip(currentLocation.ToString() + CommentaryClip.Artist.ToString());
          break;
        case CommentaryClip.MorePainting:
          currentCommentaryClip = CommentaryClip.Painting;
          PlayClip(currentLocation.ToString() + CommentaryClip.Painting.ToString());
          break;
        case CommentaryClip.MoreMusic:
          currentCommentaryClip = CommentaryClip.Music;
          PlayClip(currentLocation.ToString() + CommentaryClip.Music.ToString());
          break;
      }
    }
  }

  public void HandleNegativeGesture() {
    Debug.Log("Shake");
    if(WaitingForNodOrShake){
      Debug.Log("WaitingForNodOrShake");
      switch(currentCommentaryClip){
        case CommentaryClip.MoreArtist:
          Debug.Log("Shake for More Artist");
          currentCommentaryClip = CommentaryClip.MorePainting;
          PlayClip(currentLocation.ToString() + CommentaryClip.MorePainting.ToString());
          break;
        case CommentaryClip.MorePainting:
          currentCommentaryClip = CommentaryClip.MoreMusic;
          PlayClip(currentLocation.ToString() + CommentaryClip.MoreMusic.ToString());
          break;
        case CommentaryClip.MoreMusic:
          currentCommentaryClip = CommentaryClip.MoreArtist;
          PlayClip(currentLocation.ToString() + CommentaryClip.MoreArtist.ToString());
          break;
        }
      } else {
        switch(currentCommentaryClip){
          case CommentaryClip.Artist:
            currentCommentaryClip = CommentaryClip.MorePainting;
            WaitingForNodOrShake = true;
            PlayClip(currentLocation.ToString() + CommentaryClip.MorePainting.ToString());
            break;
          case CommentaryClip.Painting:
            currentCommentaryClip = CommentaryClip.MoreMusic;
            WaitingForNodOrShake = true;
            PlayClip(currentLocation.ToString() + CommentaryClip.MoreMusic.ToString());
            break;
          case CommentaryClip.Music:
            currentCommentaryClip = CommentaryClip.MoreArtist;
            WaitingForNodOrShake = true;
            PlayClip(currentLocation.ToString() + CommentaryClip.MoreArtist.ToString());
            break;
          }
      }
    }


  public void HandleInputGesture() {
    Debug.Log("Input");
    if(WaitingForCommentaryOffTest){
      PaintingIntroPlayed = true;
      WaitingForCommentaryOffTest = false;
      currentCommentaryClip = CommentaryClip.CommentaryOffTest;
      PlayClip(CommentaryClip.CommentaryOffTest.ToString());
    } else if(CommentaryOn){
      CommentaryOn = false;
      if(!CommentaryTurnedOffYet){
        CommentaryTurnedOffYet = true;
        currentCommentaryClip = CommentaryClip.CommentaryOffFirstTime;
        PlayClip(CommentaryClip.CommentaryOffFirstTime.ToString());
      } else {
        currentCommentaryClip = CommentaryClip.CommentaryOff;
        PlayClip(CommentaryClip.CommentaryOff.ToString());
      }
    } else if(!CommentaryOn) {
      CommentaryOn = true;
      currentCommentaryClip = CommentaryClip.CommentaryOn;
      PlayClip(CommentaryClip.CommentaryOn.ToString());
    }
  }

}
