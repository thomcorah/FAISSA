/**
 *  \class MaryGrant
 *  MaryGrant is attached to the GameObject representing the Mary Isabella Grant
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
 *  - Needles + Chimes: This uses an FMOD event that has recordings which can be transitioned from
 *    from a recording of knitting needles rhymically clacking, to wind chimes playing the same
 *    rhythm. The pace of that tranisiton is controlled by an FMOD parameter.
 *  - Wind. There are four mono recordings of wind. When the visitor enters the painting they
 *    are moved to be positioned around the visitor. As with the needles, these wind sounds 
 *    slowly transition to a set of cello lines.
 *  - Flutes. There are two GameObjects that play an FMOD event with a looping Multi Instrument. This randomly
 *    selects one of eight short flute phrases on each loop. These objects start in the distance, and 
 *    slowly approach the visitor after some time. 
 */
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class MaryGrant : MonoBehaviour
{
    /**
     *  MaryGrant needs a reference to the Visitor GameObject in order to monitor
     *  the distance between Visitor and the MaryGrant GameObject.
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
     *  MaryGrant needs a reference to the CommentaryController in order to set
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
     *  Winds is a reference to the Winds GameObject, of which the four wind 
     *  producing objects are children. It is referenced here so that it can be 
     *  moved when entering and exiting the painting.
     */
    [SerializeField]
    private GameObject Winds;

    /**
     *  Flutes and Flutes2 are GameObjects that each have the same FMOD event attached.
     *  This event has a looping Multi Instrument loaded with 8 short phrases played on 
     *  a flute. Both Flutes objects start in the distance, and are slowly moved towards
     *  the visitor after a period of time. When the visitor exits the painting, they are
     *  transitioned back to their original start position. 
     */
    [SerializeField]
    private GameObject Flutes;

    /**
     *  flutesStartPos records the start position of Flutes, so that the GameObject can be
     *  returned there when the Visitor exits the painting.
     */
    private Vector3 flutesStartPos = new Vector3(3.5f, 0.0f, -1f);

    /**
     *  Flutes2 has the same FMOD event attached as Flutes, but approaches
     *  from a different position.
     */
    [SerializeField]
    private GameObject Flutes2;

    /**
     *  flutes2StartPos is the start position for Flutes2, and used as the point
     *  to which it is returned when the Visitor leaves the painting.
     */
    private Vector3 flutes2StartPos = new Vector3(3.5f, 0f, 3.5f);
    
    /**
     *  The flutes don't start their move towards the visitor until the transition
     *  of other sonic elements is complete. e.g. the needles have become chimes. This
     *  boolean is set to true when that is the case, and then referenced in Update().
     */
    private bool fade_complete = false;

    /**
     *  sceneProgress is incremented from when the visitor enters the painting. It is
     *  the basis for the slow cross fade of needles and chimes, and wind and cellos.
     */
    private int sceneProgress = 0;

    /**
     *  progressVelocity is the rate at which sceneProgress is incremented.
     */
    private int progressVelocity = 5;

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
     *
     *  Finally, if fade_complete is true, the two GameObjects to which the flute sounds are 
     *  attached are slowly moved closer to the position of the visitor.
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

      if(fade_complete){
        float step = Time.deltaTime * 0.07f;
        Flutes.transform.position = Vector3.MoveTowards(Flutes.transform.position, transform.TransformPoint(new Vector3(1.0f, 0f, -1.00f)), step);
        Flutes2.transform.position = Vector3.MoveTowards(Flutes2.transform.position, transform.TransformPoint(new Vector3(1.0f, 0f, 1.0f)), step);
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
     *  The CrossfadeMary() method is called every ten seconds using InvokeRepeating(). This
     *  is the function that increments the value of sceneProgress in order to transition the 
     *  soundscape.
     *
     *  Lastly, an FMOD parameter is used to fade in the sounds of this painting.
     */
    void Enter(){
      fade_complete = false;
      Active = true;
      Transitioning = true;

      timeStarted = DateTime.Now;

      commentaryController.SetAudioGuide(AudioGuide);
      commentaryController.SetCurrentLocation(CommentaryController.Location.Mary);

      InvokeRepeating("CrossfadeMary", 0.0f, 10.0f);

      FMODUnity.RuntimeManager.StudioSystem.setParameterByName("FadeMary", 100);
    }

    /**
     *  The Exit() method is called when the visitor leaves the painting area by moving 
     *  further away than the ExitDistance.
     *
     *  Transitioning is set to true to allow for any transitional actions to be taken, and Active 
     *  set to false. 
     *
     *  We also cancel the repeated call to CrossfadeMary() using CancelInvoke().
     */
    void Exit(){
      Transitioning = true;
      Active = false;
      CancelInvoke("CrossfadeMary");
      
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
     *  It is also used to move the Winds GameObject towards the position of the visitor.
     */
    void TransitionIn() {
      Debug.Log("transitionIn");
      if(DateTime.Now > timeStarted.Add(new TimeSpan(0, 0, 5))){
        commentaryController.StartCommentary();
        Transitioning = false;
      }
      float speed = 1.5f * Time.deltaTime;
      Winds.transform.position = Vector3.MoveTowards(Winds.transform.position, visitor.transform.position, speed);
    }

    /**
     *  TransitionOut() is called every frame from Update() as long as 
     *  Transitioning is true and Active is false. Active is set to false
     *  when the Visitor 'exits' the painting.
     *
     *  It is used for any transitional effects required to close the soundscape,
     *  and for general tidying up of resources.
     *
     *  In the first instance, the Winds GameObject and both of the Flutes GameObjects
     *  are moved back towards their starting locations. Once they are there,
     *  fade_complete is reset to false, sceneProgress is reset to 0, the values of
     *  the MaryCrossfade and FadeMary FMOD parameters are both set to 0, and 
     *  Transitioning is set to false.
     */
    void TransitionOut() {
      Debug.Log("transitionOut");
      float speed = 1.5f * Time.deltaTime;
      Winds.transform.position = Vector3.MoveTowards(Winds.transform.position, transform.TransformPoint(new Vector3(0.5f, 0.0f, 1.5f)), speed);
      float windDistance = Vector3.Distance(Winds.transform.position, transform.TransformPoint(new Vector3(0.5f, 0.0f, 1.5f)));

      float step = Time.deltaTime * 1f;
      Flutes.transform.position = Vector3.MoveTowards(Flutes.transform.position, transform.TransformPoint(flutesStartPos), step);
      Flutes2.transform.position = Vector3.MoveTowards(Flutes2.transform.position, transform.TransformPoint(flutes2StartPos), step);
      float flutesDistance = Vector3.Distance(Flutes.transform.position, transform.TransformPoint(flutesStartPos));

      if(windDistance < 0.2f && flutesDistance < 0.2f){
        fade_complete = false;
        FMODUnity.RuntimeManager.StudioSystem.setParameterByName("MaryCrossfade", 0);
        FMODUnity.RuntimeManager.StudioSystem.setParameterByName("FadeMary", 0);
        sceneProgress = 0;
        Transitioning = false;
      }  
    }

    /**
     *  The CrossfadeMary() method handles the audio transitions of the 
     *  knitting needles and wind sounds in the soundscape. It is called repeatedly via
     *  a call to InvokeRepeating() during the Enter() method.
     *
     *  The value of sceneProgress is increments by the value of progressVelocity. We then
     *  use that as the basis for crossFadeValue, which is capped at 100, before being used
     *  to set the value of the FMOD parameter MaryCrossfade. 
     *
     *  This parameter is used in the needles and wind FMOD events to crossfade between sounds. 
     *
     *  We also set fade_complete to true when the value of sceneProgress exceeds 60. This gets 
     *  picked up in Update() and starts off the process of moving the flute GameObjects closer
     *  to the visitor.
     */
    void CrossfadeMary(){
      sceneProgress += progressVelocity;
      float crossFadeValue = sceneProgress;
      if(sceneProgress > 60){
        fade_complete = true;
      }
      if(sceneProgress > 100){
        crossFadeValue = 100;
      }
      FMODUnity.RuntimeManager.StudioSystem.setParameterByName("MaryCrossfade", crossFadeValue);
    }
}
