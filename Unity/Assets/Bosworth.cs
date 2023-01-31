/**
 *  \file Bosworth.cs
 *  \class Bosworth
 *  Bosworth is attached to the GameObject representing the Battle of Bosworth Field
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
 *  The sounds that comprise the soundscape for Bosworth are:
 *  - bosworthRunning. This is an attached GameObject that handles the playback of three hydrophonic
 *    recordings of running water. 
 *  - iceWalk. This is an attached GameObject that handles the playback of three manipulated recordings
 *    of footsteps and ice cracking in a frozen field.
 *  - iceFlurries. This is a list of prefab GameObjects that each has a short recording of thick 
 *    ice cracking underfoot. They are randomly generated and placed with a density that changes
 *    over time.
 *  - cellos. This is a series of eight short cello sounds loaded into a looping Multi Instrument in FMOD, from
 *    which one is randomly selected for playback on each loop.
 */
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Bosworth : MonoBehaviour
{
    /**
     *  Bosworth needs a reference to the Visitor GameObject in order to monitor
     *  the distance between Visitor and the Bosworth GameObject.
     */ 
    [SerializeField]
    private Visitor visitor; 

    /**
     *  The distance between the visitor and the Bosworth GameObject is monitored and stored in distance.
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
     *  ExitDistance is the distance from the painting that the visitor must move before the
     *  exit transition is triggered and the painting becomes inactive.
     */
    [SerializeField]
    private float ExitDistance = 4.5f;

    /**
     *  Bosworth needs a reference to the CommentaryController in order to set
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
     *  iceFlurries is a list of 10 prefab GameObjects, each with an FMOD event instance 
     *  attached that plays one of 10 short recordings of ice cracking underfoot.
     */
     
    [SerializeField]
    private List<GameObject> iceFlurries = new List<GameObject>();

    /**
     *  The iceFlurry objects are randomly placed at run time according to the value
     *  of iceFlurryDensity.
     */
    private int iceFlurryDensity = 0;

    /**
     *  The value of iceFlurryDensity changes over
     *  time according to the value of iceFlurryDensityDirection.
     */
    private int iceFlurryDensityDirection = 1;

    /**
     *  probChangeIceFlurryDirection describes the probability that the direction
     *  of travel of the value of iceFlurryDensity will invert.
     *  Its value steadily increases over time until an inversion occurs, at whice
     *  point it is set back to 0 again.
     */
    private int probChangeIceFlurryDirection = 0;

    /**
     *  bosworthRunning is a serialised reference to the GameObject that 
     *  manages the three underwater recordings of running water.
     */
    [SerializeField]
    private BosworthRunning bosworthRunning;

    /**
     *  iceWalk is a serialised reference to the GameObject that handles
     *  the three recordings of walking in a frozen field. 
     */
    [SerializeField]
    private IceWalk iceWalk;

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
     *  InvokeRepeating() is used to repeatedly call the AddIceFlurry() method. This method
     *  controlls the addition of the iceFlurry GameObjects with attached sounds. 
     *
     *  Finally, we make sure volume of the cello sounds is set to 0 on entrance using 
     *  an FMOD parameter. Whilst the soundscape is running, the cello volume tracks the
     *  density of ice flurry sounds.
     */
    void Enter() {
      Active = true;
      Transitioning = true;
      Debug.Log("Playing Bosworth");
      bosworthRunning.TransitionIn();
      iceWalk.TransitionIn();

      timeStarted = DateTime.Now;

      commentaryController.SetAudioGuide(AudioGuide);
      commentaryController.SetCurrentLocation(CommentaryController.Location.Bosworth);

      InvokeRepeating("AddIceFlurry", 0.0f, 0.2f);

      FMODUnity.RuntimeManager.StudioSystem.setParameterByName("BosworthCelloVolume", 0.0f);
    }

    /**
     *  The Exit() method is called when the visitor leaves the painting area by moving 
     *  further away than the ExitDistance.
     *
     *  Transitioning is set to true to allow for any transitional actions to be taken, and Active 
     *  set to false. 
     *
     *  The Enter() method initiated the repeated calling of the AddIceFlurry() method to control the 
     *  addition to the scene of iceFlurry GameObjects. This is cancelled here using CancelInvoke()
     *
     *  After a delay, we will want to tidy up the thunder sounds. timeEnded records the 
     *  current time at this point so that the TransitionOut() method can perform the tidy
     *  up after a suitable delay.
     *
     *  Finally, we fade out the sound of the cellos with an FMOD parameter. 
     */
    void Exit() {
      Transitioning = true;
      Active = false;
      Debug.Log("Stopping Bosworth");
      bosworthRunning.TransitionOut();
      iceWalk.TransitionOut();
      CancelInvoke("AddIceFlurry");
      FMODUnity.RuntimeManager.StudioSystem.setParameterByName("BosworthCelloVolume", 0.0f);
    }

    /**
     *  TransisitionIn() is called every frame by Update() while both Active and 
     *  Transitioning are true.
     *  
     *  It is used to handle any transitional effects as the visitor enters the painting.
     *
     *  Bosworth doesn't have any of these. 
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
     *  Bosworth doesn't have any additional exit transisition that need handling here,
     *  so it simply sets Transitioning to false the first time it is called.
     */
    void TransitionOut() {
      Transitioning = false;
    }

    /**
     *  AddIceFlurry() is responsible for controlling the addition to the scene of objects from the iceFlurries list
     *  and changing the volume of the cello in line with the density of ice flurries.
     *
     *  It starts by either increasing or decreasing the value of iceFlurryDensity
     *  according to the value of iceFlurryDensityDirection. This value is restricted to the range
     *  0 - 80. The volume of the cello is then set based on this, using the BosworthCelloVolume
     *  parameter.
     *
     *  If a random number between 0 and 100 is less than the value of iceFlurryDensity, we
     *  add a new ice flurry object to the scene. This new object is randomly selected from the
     *  iceFlurries list, and then randomly placed.
     *
     *  We then increase the value of probChangeIceFlurryDirection and compare it to a randomly 
     *  generated number. If the value is less than the random number, or less than 0, or greater than 100, 
     *  we invert the value of iceFlurryDensityDirection and reset the probability of a direction change
     *  (the value of probChangeIceFlurryDirection) to 0.
     */
    void AddIceFlurry() {
      iceFlurryDensity += iceFlurryDensityDirection;
      iceFlurryDensity = iceFlurryDensity < 0 ? 0 : iceFlurryDensity;
      iceFlurryDensity = iceFlurryDensity > 80 ? 80 : iceFlurryDensity;
      int CelloVolume = iceFlurryDensity + 20;
      FMODUnity.RuntimeManager.StudioSystem.setParameterByName("BosworthCelloVolume", CelloVolume);

      if(UnityEngine.Random.Range(0.0f, 100.0f) < iceFlurryDensity){
        int iceFlurryIndex = UnityEngine.Random.Range(0, 9);
        GameObject iceFlurry = Instantiate(iceFlurries[iceFlurryIndex]) as GameObject;
        float x = UnityEngine.Random.Range(-4.0f, 8.0f);
        float z = UnityEngine.Random.Range(-6.0f, 6.0f);
        iceFlurry.transform.position = transform.TransformPoint(new Vector3(x, 1.0f, z));
      }

      probChangeIceFlurryDirection += 1;
      bool changeDirection = UnityEngine.Random.Range(0, 100) < probChangeIceFlurryDirection;
      if(probChangeIceFlurryDirection < 0 || probChangeIceFlurryDirection > 100 || changeDirection){
        iceFlurryDensityDirection *= -1;
        probChangeIceFlurryDirection = 0;
      }
    }
}
