/**
 *  \class YellowstoneFalls
 *  YellowstoneFalls is attached to the GameObject representing the Yellowstone Falls
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
 *  The sounds that comprise the soundscape for Yellowstone Falls are:
 *  - Waterfall. This is a GameObject with an FMOD event attached that plays continuously.
 *  - RiverGenerator. This has the generateRivers class attached and is started and stopped on
 *    entrance and exit from Yellowstone Falls.
 *  - DistantTrombone. This is a prefab with the distantTrombone class attached. It is started and
 *    stopped on entrance and exit from Yellowstone Falls.
 *  - Thunder. This is a long stereo recording of a thunder storm in Yellowstone National Park. The 
 *    left and right channels have been separated and are attached prefabs. These are instantiated and
 *    placed at run time. 
 *  - BullElks and BaldEagles. These are sound objects that are generated and controlled within this
 *    YellowstoneFalls class. 
 */
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class YellowstoneFalls : MonoBehaviour
{

    /**
     *  YellowstoneFalls needs a reference to the Visitor GameObject in order to monitor
     *  the distance between Visitor and the YellowstoneFalls GameObject.
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
     *  YellowstoneFalls needs a reference to the CommentaryController in order to set
     *  itself as the current location when it becomes Active, and to hand over a reference
     *  to the GameObject that will act as the origin for commentary audio about this 
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
     *  timeEnded is set to the current time when the Visitor GameObject
     *  exits the painting. It's used to trigger the tidy up of resources
     *  after sounds have had a chance to fade out.
     */
    private DateTime timeEnded;

    /**
     *  This is a reference to the class attached to the river sound generator object.
     *  
     *  It needs to be started on entrance and stopped on exit.
     */
    [SerializeField]
    private generateRivers rivers;

    /**
     *  This is a reference to the distant trombone sound object.
     *
     *  It needs to be started on entrance and stopped on exit.
     */
    [SerializeField]
    private distantTrombone trombone;

    /**
     *  The serialized ThunderL and ThunderR are prefab GameObjects, to which
     *  are attached the left and right channels of a stereo recording from 
     *  Yellowstone National Park respectively. 
     */
    [SerializeField]
    private GameObject ThunderL;

    /**
     *  thunderL is the private local GameObject that is instantiated from the 
     *  ThunderL prefab.
     */
    private GameObject thunderL;
    
    /**
     *  ThunderR is a serialised reference to the ThunderR prefab object, to which is 
     *  attached an FMOD event that plays the right channel of a recording of thunder
     *  at Yellowstone National Park.
     */
    [SerializeField]
    private GameObject ThunderR;

    /**
     *  thunderR is the private local GameObject which is instantiated from the ThunderR
     *  prefab.
     */
    private GameObject thunderR;

    /**
     *  BullElk is a serialised reference to the BullElk prefab GameObject. It has
     *  an FMOD event attached that plays the BullElk event attached.
     */ 
    [SerializeField]
    private GameObject BullElk;

    /**
     *  bullElks is a List, used to store GameObjects instantiated from the BullElk
     *  prefab. 
     */
    private List<GameObject> bullElks = new List<GameObject>();

    /**
     *  probNewBullElk determines the probability that a new GameObject will be instantiated
     *  from the BullElk prefab. It is used in the AddNewBullElk() method, which is called 
     *  periodically whilst the painting is active.
     */
    private int probNewBullElk = 0;

    /**
     *  bullElkRespawnTime determines the interval, in seconds, between calls to 
     *  AddNewBullElk().
     */
    private float bullElkRespawnTime = 4.0f;

    /**
     *  elkDirection determines whether the value of probNewBullElk is going up or down.
     */
    private int elkDirection = 1;

    /**
     *  BaldEagle is a serialised reference to the BaldEagle prefab GameObject. It has
     *  an FMOD event attached that plays the BaldEagle event attached.
     */ 
    [SerializeField]
    private GameObject BaldEagle;

    /**
     *  baldEagles is a List, used to store GameObjects instantiated from the BaldEagle
     *  prefab. 
     */
    private List<GameObject> baldEagles = new List<GameObject>();

    /**
     *  probNewBaldEagle determines the probability that a new GameObject will be instantiated
     *  from the BaldEagle prefab. It is used in the AddNewBaldEagle() method, which is called 
     *  periodically whilst the painting is active.
     */
    private int probNewBaldEagle = 0;

    /**
     *  baldEagleRespawnTime determines the interval, in seconds, between calls to 
     *  AddNewBaldEagle().
     */
    private float baldEagleRespawnTime = 5.0f;

    /**
     *  eagleDirection determines whether the value of probNewBaldEagle is going up or down.
     */
    private int eagleDirection = 1;

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
     *  Lastly, if the painting is Active, the Update() method iterates over all the BullElk and BaldEagle 
     *  GameObjects, and removes any that have moved away beyond a threshold. 
     *
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

        if(Active){
          List<GameObject> eaglesToDelete = new List<GameObject>();
          for(int i = 0; i < baldEagles.Count; i++){
            if(baldEagles[i].transform.position.x > 8.0f){
              eaglesToDelete.Add(baldEagles[i]);
              Destroy(baldEagles[i]);
            }
          }
          for(int i = 0; i < eaglesToDelete.Count; i++){
            baldEagles.Remove(eaglesToDelete[i]);
          }
          eaglesToDelete = new List<GameObject>();

          List<GameObject> elksToDelete = new List<GameObject>();
          for(int i = 0; i < bullElks.Count; i++){
            if(bullElks[i].transform.position.x > 8.0f){
              elksToDelete.Add(bullElks[i]);
              Destroy(bullElks[i]);
            }
          }
          for(int i = 0; i < elksToDelete.Count; i++){
            bullElks.Remove(elksToDelete[i]);
          }
          elksToDelete = new List<GameObject>();
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
     *  Two of the linked sound producing objects, rivers and trombone are started.
     *
     *  The Commentary Controller is passed this painting's Audio Guide Game Object, and 
     *  has this painting set as its current location.
     *  
     *  The objects that play the thunder sounds are instantiated, placed, and faded in 
     *  using an FMOD global parameter.
     *
     *  InvokeRepeating() is used to repeatedly call two methods, one to handle the generation
     *  of the BullElk sounds and one the generation of the BaldEagle sounds. 
     */
    void Enter() {
      Active = true;
      Transitioning = true;
      Debug.Log("Playing Yellowstone Falls");

      timeStarted = DateTime.Now;

      rivers.StartRiver();

      trombone.StartPlaying();

      commentaryController.SetAudioGuide(AudioGuide);
      commentaryController.SetCurrentLocation(CommentaryController.Location.Yellowstone);

      FMODUnity.RuntimeManager.StudioSystem.setParameterByName("thunderFadeOut", 100);
      thunderL = Instantiate(ThunderL) as GameObject;
      thunderL.transform.position = transform.TransformPoint(new Vector3(-4.0f, 4.0f, -4.0f));
      thunderR = Instantiate(ThunderR) as GameObject;
      thunderR.transform.position = transform.TransformPoint(new Vector3(-4.0f, 4.0f, 4.0f));

      InvokeRepeating("AddBaldEagle", 0.0f, baldEagleRespawnTime);
      InvokeRepeating("AddBullElk", 0.0f, bullElkRespawnTime);
    }

    /**
     *  The Exit() method is called when the visitor leaves the painting area by moving 
     *  further away than the ExitDistance.
     *
     *  Transitioning is set to true to allow for any transitional actions to be taken, and Active 
     *  set to false. 
     *  
     *  The GameObjects that handle the river and trombone sounds are told to stop playing.
     *
     *  The Enter() method initiated the repeated calling of methods to generate BullElk and 
     *  BaldEagle sounds. This repetition is cancelled with CancelInvoke().
     *
     *  The thunder sound is faded out using an FMOD global parameter.
     *
     *  After a delay, we will want to tidy up the thunder sounds. timeEnded records the 
     *  current time at this point so that the TransitionOut() method can perform the tidy
     *  up after a suitable delay.
     */
    void Exit() {
      Transitioning = true;
      Active = false;
      Debug.Log("Stopping Yellowstone Falls");
      rivers.StopRiver();
      trombone.StopPlaying();

      CancelInvoke("AddBaldEagle");
      CancelInvoke("AddBullElk");

      FMODUnity.RuntimeManager.StudioSystem.setParameterByName("thunderFadeOut", 0);
      timeEnded = DateTime.Now;
    }

    /**
     *  TransisitionIn() is called every frame by Update() while both Active and 
     *  Transitioning are true.
     *  
     *  It is used to handle any transitional effects as the visitor enters the painting.
     *
     *  YellowstoneFalls doesn't have any of these. 
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
     *  In this case, when Exit() is called the time at that point is stored in 
     *  timeEnded. This is compared here with the current time. When the current time
     *  is more than two seconds after timeEnded, a number of things occur to tidy up
     *  the resources used for this painting.
     *
     *  1. The GameObjects to which the thunder sounds are attached are destroyed.
     *  2. Any BaldEagle GameObjects that exist are destroyed and the baldEagles List
     *     is reset.
     *  3. Any BullElk GameObjects that exist are destroyed and the bulElks List is
     *     reset.
     */
    void TransitionOut() {
      if(DateTime.Now > timeEnded.Add(new TimeSpan(0, 0, 2))){
        Transitioning = false;
        Destroy(thunderL);
        Destroy(thunderR);
        for(int i = 0; i < baldEagles.Count; i++){
          Destroy(baldEagles[i]);
        }
        baldEagles = new List<GameObject>();
        for(int i = 0; i < bullElks.Count; i++){
          Destroy(bullElks[i]);
        }
        bullElks = new List<GameObject>();
      }
    }

    /**
     *  AddBaldEagle() is called repeatedly via the use of InvokeRepeating() in
     *  the Enter() method, to add new BaldEagle objects to the scene.
     *
     *  The value of the probNewBaldEagle is either incremented or decremented, depending of the value of 
     *  the eagleDirection. 
     *
     *  If the probability is greater than 60%, or less than 0%, the direction is reversed.
     *
     *  A random number between 0 and 100 is generated, and if it is less than the current
     *  value of probNewBaldEagle, a new pair of GameObjects is instantiated from the BaldEagle prefab, positioned,
     *  and then given a velocity so that they move through the scene.
     *
     *  Each of these two new GameObjects is then added to the baldEagles List so that they can be destroyed
     *  when necessary.
     */
    void AddBaldEagle(){
      probNewBaldEagle += eagleDirection;
      if(probNewBaldEagle > 60 || probNewBaldEagle < 0) {
        eagleDirection *= -1;
      }

      if(UnityEngine.Random.Range(0.0f, 100.0f) < probNewBaldEagle){
        GameObject eagleL = Instantiate(BaldEagle) as GameObject;
        eagleL.transform.position = transform.TransformPoint(new Vector3(-4.0f, 5.0f, 0f));
        Rigidbody rbl = eagleL.GetComponent<Rigidbody>();
        rbl.velocity = transform.TransformDirection(new Vector3(0.6f, 0.0f, 0.3f));
        baldEagles.Add(eagleL);
        GameObject eagleR = Instantiate(BaldEagle) as GameObject;
        eagleR.transform.position = transform.TransformPoint(new Vector3(-4.0f, 5.0f, 0f));
        Rigidbody rbr = eagleR.GetComponent<Rigidbody>();
        rbr.velocity = transform.TransformDirection(new Vector3(0.6f, 0.0f, -0.2f));
        baldEagles.Add(eagleR);
      }
    }

    /**
     *  AddBullElk() is called repeatedly via the use of InvokeRepeating() in
     *  the Enter() method, to add new BullElk objects to the scene.
     *
     *  The value of the probNewBullElk is either incremented or decremented, depending of the value of 
     *  the elkDirection. 
     *
     *  If the probability is greater than 60%, or less than 0%, the direction is reversed.
     *
     *  A random number between 0 and 100 is generated, and if it is less than the current
     *  value of probNewBullElk, a new pair of GameObjects is instantiated from the BullElk prefab, positioned,
     *  and then given a velocity so that they move through the scene.
     *
     *  Each of these two new GameObjects is then added to the bullElks List so that they can be destroyed
     *  when necessary.
     */
    void AddBullElk(){
      probNewBullElk += elkDirection;
      if(probNewBullElk > 60 || probNewBullElk < 0) {
        elkDirection *= -1;
      }
      if(UnityEngine.Random.Range(0.0f, 100.0f) < probNewBullElk){
        GameObject elkL = Instantiate(BullElk) as GameObject;
        elkL.transform.position = transform.TransformPoint(new Vector3(-6.0f, 5.0f, 0f));
        Rigidbody rbl = elkL.GetComponent<Rigidbody>();
        rbl.velocity = transform.TransformDirection(new Vector3(0.6f, 0.0f, 0.3f));
        bullElks.Add(elkL);
        GameObject elkR = Instantiate(BullElk) as GameObject;
        elkR.transform.position = transform.TransformPoint(new Vector3(-6.0f, 5.0f, 0f));
        Rigidbody rbr = elkR.GetComponent<Rigidbody>();
        rbr.velocity = transform.TransformDirection(new Vector3(0.6f, 0.0f, -0.2f));
        bullElks.Add(elkR);
      }
    }
}
