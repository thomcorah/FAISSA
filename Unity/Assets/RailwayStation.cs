/**
 *  \class RailwayStation
 *  RailwayStation is attached to the GameObject representing the The Railway Station
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
 *  The soundscape for this painting is one that slowly changes over time. It consists of:
 *  - 9 GameObjects to which FMOD events are attached that transisition from the sounds of steam engines to 
 *    those of tropical birds.
 *  - 1 GameObject to which an FMOD event is attached that transisitons from the sound of a steam train having
 *    its water refilled to a recording of a stream in a forest.
 *  - The sound of a sliding door, which is used at the point at which the visitor enters or exits the painting.
 */

using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class RailwayStation : MonoBehaviour
{
    /**
     *  RailwayStation needs a reference to the Visitor GameObject in order to monitor
     *  the distance between Visitor and the RailwayStation GameObject.
     */ 
    [SerializeField]
    private Visitor visitor;

    /**
     * distance records the distance between the Visitor GameObject and the painting.
     */
    private float distance;

    /**
     *  If the Visitor GameObject moves closer than EnterDistance to the painting, the
     *  painting's soundscape becomes 'Active'. 
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
     *  RailwayStation needs a reference to the CommentaryController in order to set
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
     *  RailwayStation takes a reference to the GameObject which displays
     *  the painting at the centre of the sound stage. It uses this as the object to 
     *  which to attach the sliding door FMOD event instance.
     */
    [SerializeField]
    private GameObject Painting;

    /**
     *  RailwaySounds is the script attached to the GameObject of which all the 
     *  sounds (except the sliding door) are children. It controls the movement of 
     *  these sounds on entrance and exit from the painting. A reference to it is needed
     *  so that RailwayStation can instruct it to start and stop.
     */
    [SerializeField]
    private RailwaySounds RailwaySounds;

    /**
     *  Wait time is used on exit to allow the sounds to transition back their 
     *  starting points before playing the sliding door event and 'closing' the 
     *  scene.
     */
    private float waitTime = 1f;

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
     *  The Commentary Controller is passed this painting's Audio Guide GameObject, and 
     *  has this painting set as its current location.
     *
     *  A new FMOD Event Instance is created from the SlidingDoor FMOD event. This is attached to the 
     *  GameObject representing the painting in the scene and then played. 
     *
     *  An FMOD parameter (railwayentrance) is connected in FMOD to parameters of an EQ plugin on
     *  all the engine/bird events and the forest event. Before the visitor enters the painting,
     *  this is used broadly to apply a low-pass filter to these sounds, which then opens out
     *  when the visitor enters the painting, to coincide with the opening of the sliding door.
     *
     *  The onEnter() method of RailwaySounds is called so that it can move its pieces into place.
     */
    void Enter(){
      Active = true;
      Transitioning = true;

      timeStarted = DateTime.Now;

      commentaryController.SetAudioGuide(AudioGuide);
      commentaryController.SetCurrentLocation(CommentaryController.Location.RailwayStation);

      FMOD.Studio.EventInstance slidingDoor = FMODUnity.RuntimeManager.CreateInstance("event:/RailwayStation/SlidingDoor");
      FMODUnity.RuntimeManager.AttachInstanceToGameObject(slidingDoor, Painting.GetComponent<Transform>(), Painting.GetComponent<Rigidbody>());
      slidingDoor.start();
      FMODUnity.RuntimeManager.StudioSystem.setParameterByName("railwayentrance", 100);
      RailwaySounds.onEnter();
      Debug.Log("ENTERED RailwayStation");
    }

    /**
     *  The Exit() method is called when the visitor leaves the painting area by moving 
     *  further away than the ExitDistance.
     *
     *  Transitioning is set to true to allow for any transitional actions to be taken, and Active 
     *  set to false. 
     *  
     *  The value of waitTime is set to 0.5. This is then used in TransitionOut() to effect a change
     *  after 0.5s. 
     *
     *  A new instance of the FMOD event SlidingDoor is created, attached to the GameObject representing
     *  the painting, and played. 
     *
     *  The onExit() method of RailwaysSounds is called so that the sounds can be returned to their starting 
     *  positions. 
     */
    void Exit(){
      Transitioning = true;
      Active = false;
      waitTime = 0.5f;
      FMOD.Studio.EventInstance slidingDoor = FMODUnity.RuntimeManager.CreateInstance("event:/RailwayStation/SlidingDoor");
      FMODUnity.RuntimeManager.AttachInstanceToGameObject(slidingDoor, Painting.GetComponent<Transform>(), Painting.GetComponent<Rigidbody>());
      slidingDoor.start();
      RailwaySounds.onExit();

    }

    /**
     *  TransisitionIn() is called every frame by Update() while both Active and 
     *  Transitioning are true.
     *
     *  TransitionIn() is used to start the commentary playing five seconds
     *  after the Visitor 'enters' the painting. timeStarted is set to the current time
     *  when Enter() is called, and then compared here to the current time. If it is
     *  later than five seconds after timeStarted, the StartCommentary() method of 
     *  the CommentaryController is called.
     *
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
     *  In this case, the value of Time.deltaTime is subtracted from that of waitTime on
     *  each call of the method from Update(), until the value of waitTime hits or 
     *  goes below 0. At this point the sounds are all back to their starting positions 
     *  so we reset the value of the FMOD parameter railwayentrance to 0, transitioning 
     *  all the sounds back to the engine noises.
     */
    void TransitionOut() {
      waitTime = waitTime - Time.deltaTime;
      if(waitTime <= 0f){
        Debug.Log("FADE SOUNDS");
        FMODUnity.RuntimeManager.StudioSystem.setParameterByName("railwayentrance", 0);
        Transitioning = false;
      }
    }
}
