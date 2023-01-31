/**
 *  \class Wollaston
 *  Wollaston is attached to the GameObject representing the The Wollaston Family
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
 *  The soundscape for this piece is broadly constructed of two types of event. Within the scene are four 
 *  GameObjects that represent each of four people depicted in the painting. For each of these a series of
 *  whispered thoughts have been recorded, chopped up, and arranged rhythmically. Each of these people
 *  also has a single instrument part based on a short extract from Vivaldi's Four Seasons: Summer attached.
 *  The sound emenating from each person's GameObject is therefore a changing balance of the whispers and the
 *  instrumental. This is handled by WollastonPeople.
 *  
 *  The other type of sounds are background drones that are triggered with varying probability.
 */
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Wollaston : MonoBehaviour
{
    /**
     *  Wollaston needs a reference to the Visitor GameObject in order to monitor
     *  the distance between Visitor and the Wollaston GameObject.
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
     *  Wollaston needs a reference to the CommentaryController in order to set
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
    private float timeStarted;

    /**
     *  WollastonPeople is a reference to the GameObject that manages the four 
     *  'people' based sound sources. It is referenced here so that Wollaston
     *  can be used to tell it when to start or stop.
     */
    [SerializeField]
    private WollastonPeople WollastonPeople;

    /** 
     *  The playback of each of the four drone-like sounds is probability-based - 
     *  based on the value of currentProbability. This changes
     *  incrementally over time. 
     */
    private int currentProbability = 0;
    /**
     *  This probability changes over time, either rising or falling, as described by
     *  the value of droneProbabilityDirection.
     */
    private int droneProbabilityDirection = 1;

    /**
     *  As time goes on, the value of probabilityOfDirectionChange increases, and is 
     *  used to randomly invert the value of droneProbabilityDirection, and thereby
     *  change the direction of travel of currentProbability.
     */
    private int probabilityOfDirectionChange = 0;
   
    /**
     *  probInterval is multiplied by droneProbabilityDirection in order to get the 
     *  amount by which to increase or descrease the value of currentProbability.
     */
    private int probInterval = 10;

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
     *  Each of the four people sounds has a parameter that is used to control the balance
     *  between thier whisper and intrumental sounds. They are each set to 5, ready for
     *  further change as the piece progresses. This introduces a little of the instrumental
     *  aspect from the start.
     *  
     *  The WollastonAllSounds parameter automates the volume across the four drone sounds. This
     *  is set to 100 to fade them all in. 
     *
     *  The people sounds are started through a call to the onEnter() method of WollastonPeople.
     *
     *  The Commentary Controller is passed this painting's Audio Guide GameObject, and 
     *  has this painting set as its current location.
     *
     *  The AdjustProbabilities() method is called every 10 seconds using InvokeRepeating().
     */
    void Enter(){
      Debug.Log("Enter Wollaston");
      WollastonPeople.visitor = visitor;
      Active = true;
      Transitioning = true;

      timeStarted = Time.time;

      FMODUnity.RuntimeManager.StudioSystem.setParameterByName("WollastonWilliam", 5);
      FMODUnity.RuntimeManager.StudioSystem.setParameterByName("WollastonGodshall", 5);
      FMODUnity.RuntimeManager.StudioSystem.setParameterByName("WollastonElizabeth", 5);
      FMODUnity.RuntimeManager.StudioSystem.setParameterByName("WollastonFrancis", 5);

      FMODUnity.RuntimeManager.StudioSystem.setParameterByName("WollastonAllSounds", 100);

      WollastonPeople.onEnter();

      commentaryController.SetAudioGuide(AudioGuide);
      commentaryController.SetCurrentLocation(CommentaryController.Location.Wollaston);

      InvokeRepeating("AdjustProbabilities", 0.0f, 10.0f);
    }

    /**
     *  The Exit() method is called when the visitor leaves the painting area by moving 
     *  further away than the ExitDistance.
     *
     *  Transitioning is set to true to allow for any transitional actions to be taken, and Active 
     *  set to false. 
     *
     *  All the 'people' sounds are returned to 100% whispers, and the drone sounds are faded out.
     *
     *  WollastonPeople has its onExit() method called to bring the sources back to a central point.
     *
     *  We stop the repeating call to AdjustProbabilities() using CancelInvoke().
     */
    void Exit(){
      Debug.Log("EXIT");
      Transitioning = true;
      Active = false;
      FMODUnity.RuntimeManager.StudioSystem.setParameterByName("WollastonWilliam", 0);
      FMODUnity.RuntimeManager.StudioSystem.setParameterByName("WollastonGodshall", 0);
      FMODUnity.RuntimeManager.StudioSystem.setParameterByName("WollastonElizabeth", 0);
      FMODUnity.RuntimeManager.StudioSystem.setParameterByName("WollastonFrancis", 0);

      FMODUnity.RuntimeManager.StudioSystem.setParameterByName("WollastonAllSounds", 0);

      WollastonPeople.onExit();

      CancelInvoke("AdjustProbabilities");
    }

    /**
     *  TransisitionIn() is called every frame by Update() while both Active and 
     *  Transitioning are true.
     *  
     *  It is used to handle any transitional effects as the visitor enters the painting.
     *
     *  Wollaston doesn't have any of these. 
     *
     *  TransitionIn() is also used to start the commentary playing five seconds
     *  after the Visitor 'enters' the painting. timeStarted is set to the current time
     *  when Enter() is called, and then compared here to the current time. If it is
     *  later than five seconds after timeStarted, the StartCommentary() method of 
     *  the CommentaryController is called.
     */
    void TransitionIn() {
      if(Time.time - timeStarted > 5.0f){
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
     *  Wollaston doesn't have any additional exit transisition that need handling here,
     *  so it simply sets Transitioning to false the first time it is called.
     */
    void TransitionOut() {
        Transitioning = false;
    }

    /**
     *  AdjustProbabilities() is called every ten seconds via InvokeRepeating in
     *  Enter(). It either increases or decreases the value of currentProbability, 
     *  based on the value of droneProbabilityDirection.
     *
     *  The value of currentProbability is restricted to the range 0 - 100.
     *
     *  The value of probabilityOfDirectionChange is inceased with each call, and then
     *  used to randomly change the direction of currentProbability by inverting the value
     *  of droneProbabilityDirection. When this occurs, the probability of a direction change
     *  is reset to 0.
     *
     *  The new value for currentProbability is then sent to FMOD via the WollastonOngoing
     *  parameter and used by FMOD to automate the chance of playback of a looping Single
     *  Instrument on each of the four drone sounds. 
     */
    void AdjustProbabilities(){
      currentProbability += (probInterval * droneProbabilityDirection);
      if(currentProbability < 0){
        currentProbability = 0;
      }
      if(currentProbability > 100) {
        currentProbability = 100;
      }
      probabilityOfDirectionChange += probInterval;
      if(UnityEngine.Random.Range(0.0f, 100.0f) < probabilityOfDirectionChange){
        droneProbabilityDirection *= -1;
        probabilityOfDirectionChange = 0;
      }
      FMODUnity.RuntimeManager.StudioSystem.setParameterByName("WollastonOngoing", currentProbability);
    }
}
