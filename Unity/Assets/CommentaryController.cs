/**
 *  \class CommentaryController
 *  The Commentary Controller represents a single place from which the audio commentary
 *  can be controlled. 
 *
 *  The Commentary Controller is informed of the current Visitor location, and uses this to 
 *  determine which commentary clips should be played. 
 *
 *  When the Visitor GameObject 'activates' a painting, the class for that painting passes
 *  a reference to their audioGuide GameObject to Commentary Controller, which is then used
 *  as the GameObject to which to attach the commentary audio. 
 *
 *  The Commentary Controller keeps track of the state of the commentary and plays the 
 *  appropriate audio clip. There are some commentary clips that should only be played once.
 *  The clip introducing the concept of 'entering' a painting for example, should only be 
 *  played the first time a user activates a painting. To this end, there are a number of 
 *  booleans that track whether this has occured or not.
 *
 *  The Commentary Controller is also responsible for responding to gestures detected
 *  by the BOSE Headphone SDK (affirmative, negative, input). These are used to 
 *  provide (almost) hands-free control of the commentary system and navigation of 
 *  the information structure.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bose.Wearable;

public class CommentaryController : MonoBehaviour
{

  /**
   *  A serialized reference to the WearableControl object, which provides an interface
   *  to the BOSE sensor-augmented headphones. This is used to recognise the head gestures
   *  used to control the commentary playback.
   */
  [SerializeField]
  private WearableControl wearableControl;

  /**
   *  GalleryIntroPlayed is set to true once the main introduction to the gallery
   *  has been played. This ensures it isn't played again.
   */
  public bool GalleryIntroPlayed = true;

  /**
   *  PaintingIntroPlayed is set to true once the generic information introduction
   *  to a painting has been played. This ensures it isn't played again.
   */
  public bool PaintingIntroPlayed = false;

  /**
   *  CommentaryOn tracks whether the commentary is currently turned on or off.
   */
  public bool CommentaryOn = true;

  /**
   *  CommentaryTurnedOffYet gets set to true once the user has turned the audio
   *  commentary off for the first time.
   *
   *  The first time the user turns the commentary off, the confirmation message tells
   *  them the commentary is off, and reminds them how to turn it back on again.
   *
   *  Further instances of turning the commentary off result in a confirmation message that
   *  only tells them it is off.
   */
  private bool CommentaryTurnedOffYet = false;

   /**
   *  commentaryTalking is used to monitor whether a piece of commentary audio is 
   *  currently playing.
   */
  private bool commentaryTalking = false;

  /**
   *  waitingForCommentaryOffTest is set to false during the paintingIntroduction piece 
   *  of commentary, at the end of which the user is invited to test the gesture for 
   *  turning off the commentary. 
   */
  private bool waitingForCommentaryOffTest = false;

  /**
   *  waitingForNodOrShake is used to log whether the user has been asked a question for which
   *  the system is waiting for a gesture in answer.
   */
  private bool waitingForNodOrShake = false;

  /**
   *  This object will be set by a painting when it is activated, and used as 
   *  the GameObject to which to attach audio clips for playback.
   */
  private GameObject audioGuide;

  /**
   *  This private enum defines a set of audio clip identifiers. 
   *
   *  The audio commentary for each painting has the same pattern of clips. The FMOD event name
   *  for a specific clip can therefore be derived through a combination of CommentaryClip and
   *  Location, as defined below.
   */
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

    /**
   *  The currentCommentaryClip property is used to store the commentary clip
   *  currently being played.
   */
  private CommentaryClip currentCommentaryClip;

  /**
   *  The CommentaryController needs to know the current location, so as to be able to play
   *  the appropriate audio clips. This enum provides a defined list of public identifiers that are 
   *  exposed to the classes responsible for the logic of each of the paintings.
   */
  public enum Location {
    Gallery,
    Yellowstone,
    Bosworth,
    Mary,
    TheGirl,
    Wollaston,
    RailwayStation
  }

  /**
   *  This private property stores the current location.
   */
  private Location currentLocation;

  /**
   *  An FMOD Event Instance is created to interface with the FMOD sound bank and 
   *  facilitate audio playback.
   */
  FMOD.Studio.EventInstance commentaryClipInstance;

  /**
   *  Update() is called once per frame. 
   *
   *  It is used here to detect if the current commentary clip has finished playing.
   *  It does this by creating a playback state object, and using this to find out
   *  if playback has stopped. 
   *  If it has, then the commentaryTalking flag is set to false, and the 
   *  OnCommentaryComplete() method is called.
   */
  void Update()
  {
    if(commentaryTalking) {
      if(commentaryClipInstance.isValid()){
        FMOD.Studio.PLAYBACK_STATE playbackState;
        commentaryClipInstance.getPlaybackState(out playbackState);
        if (playbackState == FMOD.Studio.PLAYBACK_STATE.STOPPED){
          Debug.Log("Commentary Stopped");
          commentaryTalking = false;
          OnCommentaryComplete();
        }
      }
    }

  }

  /**
   *  When the CommentaryController is enabled, we register event handling methods
   *  with the BOSE headphone SDK in order to respond to the three head gestures
   *  we are interested in in order to provide control over the commentary playback.
   */
  void OnEnable(){
    WearableControl.Instance.AffirmativeGestureDetected += HandleAffirmativeGesture;
    WearableControl.Instance.NegativeGestureDetected += HandleNegativeGesture;
    WearableControl.Instance.InputGestureDetected += HandleInputGesture;
  }

  /**
   *  When the CommentaryController is disabled, we remove the event handler registration
   *  for BOSE headphone gesture detection.
   */
  void OnDisable(){
    WearableControl.Instance.AffirmativeGestureDetected -= HandleAffirmativeGesture;
    WearableControl.Instance.NegativeGestureDetected -= HandleNegativeGesture;
    WearableControl.Instance.InputGestureDetected -= HandleInputGesture;
  }

  /**
   *  The public SetAudioGuide() method is used by the painting classes and Visitor to 
   *  set their audioGuide GameObject as the one to which CommentaryController
   *  should attach the FMOD playback object.
   */
  public void SetAudioGuide(GameObject newAudioGuide) {
    audioGuide = newAudioGuide;
  }

  /**
   *  The public SetCurrentLocation() method is used by the painting and Visitor classes
   *  to register that they are the current location with the CommentaryController, so 
   *  that CommentaryController can play back the correct audio clips.
   */
  public void SetCurrentLocation(Location newCurrentLocation) {
    currentLocation = newCurrentLocation;
  }

  /**
   *  The public StartCommentary() method is called by the painting and Visitor classes to start
   *  the audio commentary at an appropriate time.
   *
   *  The value of currentLocation will either be 'Gallery', or an identifier for one of the paintings.
   *  If it is 'Gallery', then we check to see if the clip introducing the user to the gallery has been
   *  played. If it hasn't, we play it. The gallery introduction is a single audio clip - there is no
   *  structure of information for the user to navigate. For this reason, we trigger playback at this point.
   *
   *  If the value of currentLocation isn't 'Gallery', we're in one of the paintings. We check if the
   *  painting introduction has been played, and set the value of currentCommentaryClip to 
   *  'PaintingIntroduction' if it hasn't, and 'Intro' if it has. 
   *  
   *  The audio commentary for the paintings is comprised of a number of audio clips in a shallow
   *  navigable structure. For this reason, once we've set the value of currentCommentaryClip
   *  appropriately, we call PlayPaintingCommentary() in order to hand control over to the logic
   *  that will allow for the navigation and correct order of commentary clips to be played. 
   */
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

  /**
   *  PlayPaintingCommentary() is used to start the playback of commentary 
   *  specific to one of the paintings. It checks the value of currentCommentaryClip
   *  and plays either the one-off PaintingIntroduction clip (an instructional clip)
   *  that is played the first time a user 'enters' a painting), or the introduction
   *  for this particular painting.
   *
   *  The one-off PaintingIntroduction clip ends by telling the user how to turn the 
   *  commentary on and off, and instructing them to try the gesture. Because of that,
   *  it sets the waitingForCommentaryOffTest boolean flag to true.
   */
  private void PlayPaintingCommentary(){
    switch(currentCommentaryClip){
      case CommentaryClip.PaintingIntroduction:
        PlayClip(CommentaryClip.PaintingIntroduction.ToString());
        waitingForCommentaryOffTest = true;
      break;
      case CommentaryClip.Intro:
        PlayClip(currentLocation.ToString() + CommentaryClip.Intro.ToString());
      break;
    }
  }

  /**
   *  The Update() method above continuously checks to see if the current 
   *  spoken commentary clip has finished. If it has, this OnCommentaryComplete()
   *  method is called. 
   *
   *  It first looks at the value of currentCommentaryClip in order to determine
   *  which clip should come next, and then uses the PlayClip() method to play it,
   *  if appropriate. In some cases, the waitingForNodOrShake boolean flag is set
   *  to true. If the user has just heard brief commentary about the artist, for example,
   *  they are then asked if they would like to hear more about the artist. The system
   *  must then wait for an affirmative or negative gesture to continue.
   *  
   *  If there is no clip to follow, the previously ducked level of the soundscape is
   *  restored to full volume.
   */
  private void OnCommentaryComplete(){
    commentaryTalking = false;
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
          waitingForNodOrShake = true;
          PlayClip(currentLocation.ToString() + CommentaryClip.MoreArtist.ToString());
          break;
        case CommentaryClip.Artist:
          currentCommentaryClip = CommentaryClip.MorePainting;
          waitingForNodOrShake = true;
          PlayClip(currentLocation.ToString() + CommentaryClip.MorePainting.ToString());
          break;
        case CommentaryClip.Painting:
          currentCommentaryClip = CommentaryClip.MoreMusic;
          waitingForNodOrShake = true;
          PlayClip(currentLocation.ToString() + CommentaryClip.MoreMusic.ToString());
          break;
        case CommentaryClip.Music:
          currentCommentaryClip = CommentaryClip.MoreArtist;
          waitingForNodOrShake = true;
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

  /**
   *  The PlayClip() method is responsible for playing the clip as specified
   *  by the commentaryClip parameter.
   *
   *  If some commentary is currently running (commentaryTalking is true), then
   *  it first stops that commentary clip.
   *
   *  It then creates an FMOD event instance using the value of commentaryClip.
   *  
   *  It uses an FMOD global parameter (DuckMusic) to reduce the level of the 
   *  soundscape while the commentary is playing. 
   *
   *  It then attaches the FMOD event instance to the audioGuide GameObject passed
   *  to CommentaryController by the painting or Visitor objects, and starts the 
   *  clip playing.
   */
  private void PlayClip(string commentaryClip){
    if(commentaryTalking){
      commentaryClipInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }
    commentaryClipInstance = FMODUnity.RuntimeManager.CreateInstance("event:/Commentary/" + commentaryClip);
    if(audioGuide){
      FMODUnity.RuntimeManager.StudioSystem.setParameterByName("DuckMusic", 40);
      FMODUnity.RuntimeManager.AttachInstanceToGameObject(commentaryClipInstance, audioGuide.GetComponent<Transform>(), audioGuide.GetComponent<Rigidbody>());
      commentaryTalking = true;
      commentaryClipInstance.start();
    }
  }

  /**
   *  HandleAffirmativeGesture() is the method that is registered with the BOSE AR Headphone SDK
   *  to be called when it detects an affirmative head gesture (nod).
   *
   *  It looks at the value of currentCommentaryClip in order to know which commentary prompt 
   *  the user is responding to, and then sets the next commentary clip accordingly, before 
   *  playing it using PlayClip().
   */
  public void HandleAffirmativeGesture() {
    Debug.Log("Nod");
    if(waitingForNodOrShake){
      waitingForNodOrShake = false;
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

  /**
   *  HandleNegativeGesture() is the method that is registered with the BOSE AR Headphone SDK
   *  to be called when it detects a negative head gesture (shake).
   *
   *  It looks at the value of currentCommentaryClip in order to know which commentary prompt 
   *  the user is responding to, and then sets the next commentary clip accordingly, before 
   *  playing it using PlayClip().
   *
   *  As well as using an affirmative or negative gesture to respond to vocal prompts in the 
   *  commentary, the user can also use a negative gesture (shake) to 'exit' out of the 
   *  current commentary clip and go on to the next section. 
   *
   *  Therefore, if the system receives a negative gesture but isn't currently waiting for
   *  one, it responds by setting currentCommentaryClip to the appropriate clip and 
   *  playing that. For example, if the system is currently playing the more detailed 
   *  information about an artist and receives a negative gesture, it will cue up
   *  the brief introduction to information about the painting, and use PlayClip() to
   *  play it. PlayClip() will automatically stop (with a fade out) the current clip
   *  if one is playing before playing the one specified. 
   */
  public void HandleNegativeGesture() {
    Debug.Log("Shake");
    if(waitingForNodOrShake){
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
            waitingForNodOrShake = true;
            PlayClip(currentLocation.ToString() + CommentaryClip.MorePainting.ToString());
            break;
          case CommentaryClip.Painting:
            currentCommentaryClip = CommentaryClip.MoreMusic;
            waitingForNodOrShake = true;
            PlayClip(currentLocation.ToString() + CommentaryClip.MoreMusic.ToString());
            break;
          case CommentaryClip.Music:
            currentCommentaryClip = CommentaryClip.MoreArtist;
            waitingForNodOrShake = true;
            PlayClip(currentLocation.ToString() + CommentaryClip.MoreArtist.ToString());
            break;
          }
      }
    }

  /**
   *  The HandleInputGesture() method is registered with the BOSE AR SDK to be called when
   *  the BOSE system receives the Input Gesture. On the 700 Headphones, this is a 
   *  tap-and-hold on the face of the right-hand headphone cup. 
   *
   *  This is used to turn the commentary on or off. 
   *
   *  The one-off instructional PaintingIntroduction clip tells the user about this 
   *  functionality, and asks then to try out the gesture. This method therefore
   *  responds differently if that is the case.
   *
   *  Additionally, the first time the user turns the commentary off, they are played
   *  a clip that goes on to remind them how to turn it on again. Subsequent to this,
   *  they are only played a clip confirming that they've turned the commentary off or on.
   */  
  public void HandleInputGesture() {
    Debug.Log("Input");
    if(waitingForCommentaryOffTest){
      PaintingIntroPlayed = true;
      waitingForCommentaryOffTest = false;
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
