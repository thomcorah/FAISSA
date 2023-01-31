# SAARPS: System for Audio Augmented Reality in Public Spaces

This repository contains the practical aspects of my PhD work - the creation of a system for the production and delivery of an artistic audio augmented reality system for use in public spaces such as galleries.

This example is intended to be used in the Victorian Gallery at the Leicester Museum and Art Gallery, and provides spatialised audio experiences for six selected paintings in the gallery.

This repository contains two folders:

**FMOD Sounds**  
Contains the full FMOD audio session from which to build the sound bank for Unity.

**Unity**  
The Unity game environment used to create the virtual sound world for the augmented reality experience.

## Installation

Open the FMOD session in FMOD Studio v2.01.08 or later and build for desktop. FMOD Studio is free and can be downloaded from <https://fmod.com>

Open the Unity session. I has been authored with Unity v2020.03.3f1. Unity can be downloaded from <https://unity.com>

Install the FMOD Unity integration to connect FMOD and Unity. Full details are available here: <https://www.fmod.com/unity>

## The Code

### Overview

The Victorian Gallery is represented in Unity as a rectangular space. Six paintings spaced around the gallery have been chosen as those to be augmented with spatialised sound and audio commentary. These paintings are each represented by an object in Unity that contains a range of further assets as appropriate. Additionally, each painting object has an attached script that handles a number of painting-specific actions such as the triggering of the soundscape and audio commentary based on the proximity of the visitor.

These scripts are in the _Assets_ folder and are named for the painting to which they belong: _YellowstoneFalls.cs_, _Bosworth.cs_, _MaryGrant.cs_, _RailwayStation.cs_, _TheGirl.cs_, and _Wollaston.cs_.

The visitor is represented by a cuboid in the game environment. In order for this object to receive location data from the Indoor Atlas indoor positioning system, it is contained within an _IndoorAtlas Session_ object.

The script attached to the visitor (_Assets/Visitor.cs_) responds to the IndoorAtlas API and moves the visitor object accordingly.

Although triggered by the paintings themselves, the commentary is otherwise controlled independently by the _CommentaryController_ object (_Assets/CommentaryController.cs_).

### The Paintings

#### Common Properties

There are several properties that are common to all top level painting scripts.

The first stores a reference to the _CommentaryController_ object.

```java
private CommentaryController commentaryController;
```

The _visitor_ property stores a reference to the visitor avatar object. The script also keeps track of the distance from the visitor to the painting so as to trigger entrance and exit from the painting's sound world.

```java
private Visitor visitor;
private float distance;
```

The _Transitioning_ and _Active_ properties are used to track the state of the painting's sound world. When the visitor enters or exits the sound world, there is a period of transition. The _Transitioning_ boolean property tracks if the painting is currently in the transitioning state. Similarly, whilst the visitor is in the sound world of a painting, that painting is active.

```java
private bool Transitioning = false;
private bool Active = false;
```

Each painting object has within it its own game object representing the location of the audio guide. The _AudioGuide_ property stores a reference to this object, in order to pass it to _CommenteryController_ as the object to which to attach any painting-specific commentary.

The commentary begins after a short pause when the visitor 'enters' the painting. The _timeStarted_ property is used to log the time when the visitor enters the painting in order to start the commentary after a short delay.

```java
private GameObject AudioGuide;
private DateTime timeStarted;
```

#### On Update

The _Update()_ method in each painting script is run on every frame and is used to do two things.

Firstly, it monitors the distance from the painting to the visitor and begins the process of either entering or exiting the painting sound world based on a combination of this distance and the current state of the painting.

```java
distance = Vector3.Distance(visitor.transform.position, transform.position);
if(distance < 3 && !Active){
  Enter();
}

if(distance > 3.5 && Active) {
  Exit();
}
```

Secondly, any activities that occur as part of a transition either into or out of a sound world are triggered.

```java
if(Active && Transitioning) {
  TransitionIn();
} else if(!Active && Transitioning) {
  TransitionOut();
}
```

#### On Enter

In each painting, the _Enter()_ method performs some common tasks when the visitor crosses the threshold and 'enters' a painting.

The properties tracking the current state of the painting are updated.

```java
Active = true;
Transitioning = true;
```

The current time is logged in order to trigger the commentary after a short delay.

```java
timeStarted = DateTime.Now;
```

The _CommentaryController_ has this painting's audio guide object set as its sound location. The _CommentaryController_ contains a property that stores the current location in order to play the correct audio files. This property is updated to the current painting - in this case, the Battle of Bosworth Field.

```java
commentaryController.SetAudioGuide(AudioGuide);
commentaryController.SetCurrentLocation(CommentaryController.Location.Bosworth);
```

#### On Exit

The _Exit()_ method is run when the visitor leaves the painting. All that is required here is to update the current state.

```java
Transitioning = true;
Active = false;
```

#### Starting the Commentary

The final piece of code common to all paintings is that which starts the commentary playing. This is done in the _TransitionIn()_ method, run every frame whilst the painting is transitioning in.

The code compares the time logged at the beginning of the transition with the current time and begins the commentary if the duration threshold has been crossed. In this case, after five seconds.

```java
if(DateTime.Now > timeStarted.Add(new TimeSpan(0, 0, 5))){
  commentaryController.StartCommentary();
}
```

### The Visitor

The visitor to the space is represented by the _Visitor_ object, contained within the _IndoorAtlas Session_ object which utilises the IndoorAtlas indoor positioning API.

The script attached to the Visitor game object deals with just two things: initiating gallery-level commentary and moving the visitor avatar within the game environment. The latter is achieved through either the use of keyboard and mouse input when in the editor environment, or by translating positioning data from the _IndoorAtlas Session_ object that contains it when being run as a deployed application.

There is a single item of gallery-level commentary - the gallery introduction. This is initiated after a short delay in the same way as for the painting-specific commentary.

```java
if(DateTime.Now > timeStarted.Add(new TimeSpan(0, 0, 5)) && !commentaryController.GalleryIntroPlayed){
  commentaryController.StartCommentary();
}
```

As with the paintings, the visitor contains an AudioGuide game object which is passed to the _CommentaryController_ to be used as the origin for gallery-level commentary.

### Commentary Controller

The _CommentaryController_ object controls the playback of all commentary in the system. It contains two enums that contain a list of commentary clips and locations. Each painting has the same set of clips, with the FMOD event names identical but prepended by the name of the location as set in the locations enum property.

For example, the FMOD event with more information on the music created for the Battle of Bosworth painting is called _BosworthMoreMusic_.

```java
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
```

Both the current location and commentary clip are stored.

```java
private Location currentLocation;
private CommentaryClip currentCommentaryClip;
```

The _CommentaryController_ also has a single FMOD event object into which the appropriate FMOD event is loaded.

```java
private CommentaryClip currentCommentaryClip;
```

Finally, the _CommentaryController_ also keeps track of various aspects of state, including whether the commentary is currently playing.

```java
private bool CommentaryTalking = false;
private bool WaitingForCommentaryOffTest = false;
private bool WaitingForNodOrShake = false;
```

#### On Update

The _Update()_ method in _CommentaryController_ is used to check if a playing clip has stopped. This then triggers the logic that either progresses to the next clip or waits for user input.

```java
if(CommentaryTalking) {
  if(commentaryClipInstance.isValid()){
    FMOD.Studio.PLAYBACK_STATE playbackState;
    commentaryClipInstance.getPlaybackState(out playbackState);
    if (playbackState == FMOD.Studio.PLAYBACK_STATE.STOPPED){
      CommentaryTalking = false;
      OnCommentaryComplete();
    }
  }
}
```

#### BOSE Gesture Input

The _CommentaryController_ registers methods with the BOSE Headphone API to handle affirmative (head nod), negative (head shake), and input (tap and hold on the side of the headphones) gestures.

```java
WearableControl.Instance.AffirmativeGestureDetected += HandleAffirmativeGesture;
WearableControl.Instance.NegativeGestureDetected += HandleNegativeGesture;
WearableControl.Instance.InputGestureDetected += HandleInputGesture;
```

The _HandleAffirmativeGesture()_ gesture handles instances of head nods. It begins by checking if the system is actually waiting for a head nod or shake.

```java
if(WaitingForNodOrShake){
  ...
}
```

Within that conditional block the action taken depends upon what the current clip is. This is handled with a _switch_ block.

```java
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
```

The handling of negative gestures with _HandleNegativeGesture()_ is similar, except that as well as being used as a direct response to a question, a head shake can be used to exit out of the current commentary, in which case the system moves on to the next section.

The input gesture, a tap on hold on the face of the right-hand headphone cup, is used turn the commentary on and off. This is handled by the _HandleInputGesture()_ method. The mechanism for turning the commentary on and off is introduced the first time the visitor enters a painting, at which point they are presented with the instruction to test the gesture. _HandleInputGesture()_ therefore handles the case of an input gesture when the system is waiting for this test case and handles this separately to the use of the gesture at any other time.

In addition, the first time the visitor turns the commentary off, they are given a vocal confirmation as well as an instruction on how to turn the commentary back on.

```java
if(!PaintingIntroPlayed && WaitingForCommentaryOffTest){
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
```

#### Commentary Clip Playback

The playing of the commentary itself is handled by the _PlayClip()_ method. Firstly, if a commentary clip is currently playing it is stopped.

```java
if(CommentaryTalking){
  commentaryClipInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
}
```

The _commentaryClipInstance_ property is then assigned a new FMOD event based on the commentary clip name passed in.

```java
commentaryClipInstance = FMODUnity.RuntimeManager.CreateInstance("event:/Commentary/" + commentaryClip);
```

Before going on to play the commentary clip, we check that there is a currently assigned audio guide object to which to attach it.

```java
  if(audioGuide){
    ...
  }
```

Within this conditional block, we use an FMOD parameter to duck the level of all non-commentary events currently playing.

```java
  FMODUnity.RuntimeManager.StudioSystem.setParameterByName("DuckMusic", 40);
```

Finally, we attach the FMOD event instance to the audio guide object and play it.

```java
  FMODUnity.RuntimeManager.AttachInstanceToGameObject(commentaryClipInstance, audioGuide.GetComponent<Transform>(), audioGuide.GetComponent<Rigidbody>());
  CommentaryTalking = true;
  commentaryClipInstance.start();
```

This all happens after we've determined which clip to play. This decision is handled through several methods. _StartCommentary()_ is called by the visitor object or a painting object in order to initiate the audio commentary. This method checks a number of aspects of state in order to select the correct clip to start with. This will either be the gallery introduction clip, the general introduction to paintings that includes some instructions on first entering a painting, or a painting-specific introduction to be used when the visitor has already been played the general introduction.

The _OnCommentaryComplete()_ method is called whenever a clip has finished and used to progress onto the next audio clip if appropriate. For example, once the painting specific introduction has finished the commentary goes on to ask if the visitor would like to hear more about the artist.
