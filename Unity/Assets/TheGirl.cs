/**
 *  \class TheGirl
 *  TheGirl is attached to the GameObject representing the The Girl I Left Behind Me
 *  painting in the gallery. 
 *
 *  As with all the painting-specific classes, it is responsible for triggering the
 *  painting's soundscape when the user is close enough, and collapsing it again
 *  when the user walks away. 
 *
 *  In order to do this, the Update() method is continuously monitoring the distance between
 *  the painting's GameObject and that of the Visitor. If the painting is not 'Active' and
 *  the Visitor GameObject moves close enough, the Enter() method is called. This method 
 *  sets initial state, starts playback, and sets a transitioning boolean flag to true. 
 *
 *  Some paintings have a staged transition in. This is achieved by a call to TransitionIn()
 *  from the Update() method if the painting is Active and Transitioning. 
 *
 *  Conversely, if the painting is Active and the Visitor moves away from the painting,
 *  the Exit() method is called which begins the process of stopping the soundscape, including
 *  setting the Active flag to false.
 *
 *  As with entering the painting, the Transitioning boolean is set to true. If the painting
 *  is not Active, but is Transitioning, TransitionOut() is repeatedly called from Update() to enable any exit
 *  transitions to be completed.
 *
 *  The music for this painting is based on the folk song of the same name as the painting, and includes 
 *  sung passages from it. There are a number of sounds that comprise this soundscape. They are all attached to GameObjects in the scene,
 *  but with minimal control from here beyond being faded in and out via the TheGirlEntrance FMOD parameter. Due to that, none of them
 *  are references within this script.
 *
 *  These sounds include:
 *  - Hum1 and Hum2. These are two hummed versions of the melody used. They are attached to two GameObjects,
 *    spaced slightly apart, that stand back from the painting, behind the visitor when they are in front
 *    of the painting.
 *  - Singing. This GameObject includes eight objects in the scene that each have a piece of sung dialogue 
 *    attached to them. They are animated to move continuously from right to left in front of the user. This
 *    is handled by the script attached to the Singing GameObject.
 *  - ChildrenL and ChildrenR. These are parts of a stereo recording of children playing from the BBC Sounds
 *    archive. They are simply added as GameObjects in the scene, but include a long fade in and fade out in FMOD.
 *  - drum. This is a recording of a marching snare drum from the BBC Sounds archive. It has a long fade in and 
 *    fade out handled by FMOD.
 *  - MarchingL and MarchingR. This is a stereo recording of troops marching from the BBC Sound archive. It is
 *    faded in and out via FMOD.
 *  - Crowds. These sounds are two stereo recordings, attached to four GameObjects, of crowd noises from the BBC Sound
 *    archive. They are faded in and out via FMOD. 
 *  
 *    The sounds listed above with long fade in and fade out times are all looped, and of varying lengths. This creates
 *    a changing structure over time as they fall into and out of phase with each other.
 */

using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class TheGirl : MonoBehaviour
{
    /**
     *  TheGirl needs a reference to the Visitor GameObject in order to monitor
     *  the distance between Visitor and the TheGirl GameObject.
     */ 
    [SerializeField]
    private Visitor visitor;

    /**
     * distance records the distance between the Visitor GameObject and the painting.
     */
    private float distance;

    /**
     *  If the Visitor GameObject moves closer than EnterDistance to the painting, the
     *  painting's soundscape becomes 'Active'. If the Visitor GameObject then moves further
     *  away than ExitDistance, the Active flag is set to false. 
     *
     *  There is a difference between these two values in order to avoid any jarring
     *  succession of switches in the Active state of the soundscape resulting from 
     *  small shifts in positioning at the boundary.
     */
    [SerializeField]
    private float EnterDistance = 4.0f;

    /**
     *  If the Visitor GameObject then moves further
     *  away than ExitDistance, the Active flag is set to false. 
     */
    [SerializeField]
    private float ExitDistance = 4.5f;

    /**
     *  TheGirl needs a reference to the CommentaryController in order to set
     *  itself as the current location when it becomes Active, and to hand over a reference
     *  to the GameObject that will act as the origin for audio commentary about this 
     *  painting.
     */
    [SerializeField]
    private CommentaryController commentaryController;

    /**
     *  The AudioGuide is the GameObject which will be passed to the 
     *  Commentary Controller for it to attach the audio commentary 
     *  FMOD event instances to. 
     *
     *  Each painting has a different voice for the commentary, so it 
     *  made sense that each would have its own virtual location as the
     *  source of this audio, rather than using the AudioGuide that is
     *  part of the Visitor heirarchy.
     */
    [SerializeField]
    private GameObject AudioGuide;

    /**
     *  With all the paintings, they have a transition phase implemented when both
     *  entering the soundscape and exiting it. The Transitioning boolean is set
     *  to true when the transition starts, and false when it ends.
     */
    private bool Transitioning = false;

    /**
     *  When the Visitor GameObject moves close enough to the painting to 
     *  activate the soundscape, the Active boolean is set to true. When the
     *  Visitor GameObject moves away again, it is set to false.
     */
    private bool Active = false;

    /**
     *  timeStarted is set to the current time when the Visitor GameObject 
     *  enters the painting. This is used to start the commentary a short 
     *  time after entry.
     */
    private DateTime timeStarted;

    /**
     *  Update() is called once per frame.
     *
     *  The distance between the Visitor GameObject and this painting is calculated,
     *  and then compared to the entrance distance threshold to see if the Visitor
     *  has 'entered' the painting. This only happens if the painting is currently
     *  'inactive'. If the distance is smaller than the entrance threshold and the 
     *  painting is currently 'inactive' (Active == false) then it runs the Enter()
     *  method. 
     *
     *  Similarly, if the distance to the Visitor exceeds the exit distance threshold
     *  while the painting is 'active', the Exit() method is called.
     *
     *  If the painting is Active and Transitioning, the TransitionIn() method is called. 
     *
     *  If the painting is Inactive and Transitioning, the TransitionOut() method is called.
     */
    void Update()
    {
      distance = Vector3.Distance(visitor.transform.position, transform.position);
      if(distance < EnterDistance && !Active){
        Enter();
      }

      if(distance > ExitDistance && Active) {
        Exit();
      }

      if(Active && Transitioning) {
        TransitionIn();
      } else if(!Active && Transitioning) {
        TransitionOut();
      }
    }

    /**
     *  The Enter() method is called once when the Visitor GameObject approaches 
     *  close enough to the painting, as defined by EnterDistance.
     *
     *  It sets the Active and Transitioning boolean flags to true. 
     *
     *  The current time is stored in timeStarted, so that the commentary can be started
     *  after a defined delay.
     *
     *  The sounds of this soundscape are faded in by setting the value of the 
     *  TheGirlEntrance parameter to 100.
     *
     *  The Commentary Controller is passed this painting's Audio Guide GameObject, and 
     *  has this painting set as its current location.
     */
    void Enter(){
      Active = true;
      Transitioning = true;

      timeStarted = DateTime.Now;

      FMODUnity.RuntimeManager.StudioSystem.setParameterByName("TheGirlEntrance", 100);

      commentaryController.SetAudioGuide(AudioGuide);
      commentaryController.SetCurrentLocation(CommentaryController.Location.TheGirl);
    }

    /**
     *  The Exit() method is called when the visitor leaves the painting area by moving 
     *  further away than the ExitDistance.
     *
     *  Transitioning is set to true to allow for any transitional actions to be taken, and Active 
     *  set to false. 
     *
     *  Finally, we fade out the sounds of this painting with an FMOD parameter. 
     */
    void Exit(){
      Debug.Log("EXIT");
      Transitioning = true;
      Active = false;
      FMODUnity.RuntimeManager.StudioSystem.setParameterByName("TheGirlEntrance", 0);
    }

    /**
     *  TransisitionIn() is called every frame by Update() while both Active and 
     *  Transitioning are true.
     *  
     *  It is used to handle any transitional effects as the visitor enters the painting.
     *
     *  TheGirl doesn't have any of these. 
     *
     *  TransitionIn() is also used to start the commentary playing five seconds
     *  after the Visitor 'enters' the painting. timeStarted is set to the current time
     *  when Enter() is called, and then compared here to the current time. If it is
     *  later than five seconds after timeStarted, the StartCommentary() method of 
     *  the CommentaryController is called.
     */
    void TransitionIn() {
      if(DateTime.Now > timeStarted.Add(new TimeSpan(0, 0, 5))){
        commentaryController.StartCommentary();
        Transitioning = false;
      }
    }

    /**
     *  TransitionOut() is called every frame from Update() as long as 
     *  Transitioning is true and Active is false. Active is set to false
     *  when the Visitor 'exits' the painting.
     *
     *  It is used for any transitional effects required to close the soundscape,
     *  and for general tidying up of resources.
     *
     *  TheGirl doesn't have any additional exit transisition that need handling here,
     *  so it simply sets Transitioning to false the first time it is called.
     */
    void TransitionOut() {
        Transitioning = false;
    }
}
