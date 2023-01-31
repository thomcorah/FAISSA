/**
 *  \class WollastonPeople
 *  WollastonPeople is used by Wollaston, attached to the GameObject representing the painting
 *  The Wollaston Family. 
 *
 *  The soundscape includes four GameObjects that each represent one of four of the people in the 
 *  painting. WollastonPeople is used to control these four GameObjects.
 *
 *  It has two responsibilities.
 *
 *  1. The GameObjects sit in a central position in front of the painting while the soundscape 
 *     is inactive. When the visitor enters the painting, they are moved to four points surrounding
 *     the visitor.
 *  2. Each of the GameObjects has an FMOD event attached that includes two pieces of audio. One is 
 *     a rhythmically edited recording of the character's whispers. The other is a single intrument playing
 *     part of an extra from Vivaldi's Four Seasons: Summer. The crossfade between these two sounds is
 *     controlled by the degree to which the visitor is looking at the location of the GameObject. When
 *     the visitor is looking directly towards the GameObject, they will only hear the instrument audio. 
 *     As their gaze moves away from the GameObject, this will crossfade to the whispers audio. 
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WollastonPeople : MonoBehaviour
{
    /**
     *  WollastonPeople requires a reference to the visitor GameObject, in order to know the direction
     *  in which it is facing. 
     */
    [SerializeField]
    public Visitor visitor;

    /**
     *  When the visitor leaves the painting, these four GameObjects return to their central position
     *  in front of the painting. This is described here as startPos.
     */
    private Vector3 startPos = new Vector3(-1.0f, 0f, 0f);

    /**
     *  Each of the four GameObjects is passed to WollastonPeople - William, Godshall, Elizabeth, and 
     *  Francis. 
     *  This GameObject represents William Wollaston.
     */
    [SerializeField]
    private GameObject William;

    /**
     *  Each of the four GameObjects has its own position when the soundscape is running.
     *  These are recorded here.
     */
    private Vector3 WilliamPos = new Vector3(0f, 0f, 0f);

    /**
     *  Godshall is the GameObject associated with Lady Godschall.
     */
    [SerializeField]
    private GameObject Godshall;

    /**
     *  GodshallPos is the target location for Godshall when the soundscape is running.
     */
    private Vector3 GodshallPos = new Vector3(-1.4f, 0f, -1.5f);

    /**
     *  Elizabeth is the GameObject associated with Elizabeth Wollaston.
     */
    [SerializeField]
    private GameObject Elizabeth;

    /**
     *  ElizabethPos is the position to which Elizabeth will be moved when the soundscape
     *  becomes active.
     */
    private Vector3 ElizabethPos = new Vector3(-2.5f, 0f, -2.5f);

    /**
     *  Francis represents Francis Wollaston.
     */
    [SerializeField]
    private GameObject Francis;

    /**
     *  FrancisPos is the location to which Francis will be moved when the soundscape
     *  becomes active.
     */
    private Vector3 FrancisPos = new Vector3(-1.55f, 0f, 2.4f);

    /**
     *  The clips List will be given these four GameObjects in order to be able to iterate over them.
     */
    private GameObject[] clips;

    /**
     *  The end position for each GameObject will be stored in the endPositions List in an order corresponding
     *  to the order of those GameObjects in the clips List.
     */
    private Vector3[] endPositions;

    /**
     *  clipMoving is a List of booleans used to monitor if each GameObject is currently in movement.
     */
    private bool[] clipMoving = {false, false, false, false};

    /**
     *  The crossfade on each FMOD event is enacted with the use of an FMOD parameter for each one. The names of these 
     *  parameters are recorded in this string List, in an order that corresponds to the GameObjects in the clips List.
     */
    private string[] FMODParameters = {"WollastonWilliam", "WollastonGodshall", "WollastonElizabeth", "WollastonFrancis"};

    /**
     *  The transitioning, entering, and active booleans are used to track state. transitioning will be set to true when
     *  the visitor enters or exits the painting, and returned to false when all four GameObjects have reached their 
     *  destination. 
     */ 
    private bool transitioning = false;

    /**
     *  entering is true if that transition is from the visitor entering the painting, and false if they are exiting.
     */
    private bool entering = true;

    /**
     *  active is true while the visitor is in the painting, and false when they're not. 
     */
    private bool active = false;

    /**
     *  Start() is called before the first frame. 
     *
     *  It is used here to assign the four GameObjects to the clips List, and the end position Vector3s to 
     *  the endPositions List. 
     * 
     *  startPos is converted from local coordinate space to world coordinate space. Each of the four Vector3s in 
     *  endPositions is also converted from local coordinate space to world coordinate space. 
     */
    void Start()
    {
      clips = new GameObject[] {William, Godshall, Elizabeth, Francis};
      endPositions = new Vector3[] {WilliamPos, GodshallPos, ElizabethPos, FrancisPos};

      startPos = transform.TransformPoint(startPos);
      for(int i = 0; i < endPositions.Length; i++){
        endPositions[i] = transform.TransformPoint(endPositions[i]);
      }
    }

    /**
     *  Update(), called once per frame, is used to both move the four GameObjects during transitions,
     *  and adjust the balance of sounds in each FMOD event.
     *
     *  If the painting is transitioning in (transitioning == true, entering == true), each of the four
     *  GameObjects is iterated over. For each, if it is still moving (its corresponding boolean in the 
     *  clipMoving List is true) it is moved towards its end position.
     *  If the distance from the GameOject to its end position is sufficiently small, its corresponding boolean
     *  flag in the clipMoving List is set to false. 
     *
     *  A similar process occurs if the painting is transitioning out (transitioning == true, entering == false),
     *  except the GameObjects are moved from their current location towards startPos.
     *
     *  Irrespective of whether the transition is in or out, if all the booleans in clipMoving are false, then 
     *  transitioning is set to false.
     *
     *  Update() also handles the crossfade on each of the four GameObjects. 
     *
     *  It first checks that the visitor parameter isn't null, and that the painting is active.
     *
     *  It then gets the normalised direction in which the visitor GameObject is facing. This is represented
     *  by the Vector3 nose.
     *
     *  It then iterates over each of the four GameObjects. For each, it:
     *
     *  1. Creates and normalises a Vector3 (personDirection) representing the direction from the 
     *     visitor GameObject to the person GameObject in question.
     *  2. Calculates the angular difference between the direction to the person, and the direction
     *     in which the visitor is looking. This is done using the dot product. This produces a number 
     *     from 1 if the visitor is looking directly towards the GameObject, to -1 if they are looking
     *     180deg away from it. 
     *  3. The dot product is clamped and scaled, and then passed to FMOD via the appropriate parameter
     *     drawn from the FMODParemeters List.
     */

    void Update()
    {
      float speed = 1.5f * Time.deltaTime;
      if(transitioning && entering){
        for(int i = 0; i < clips.Length; i++){
          if(clipMoving[i]){
            clips[i].transform.position = Vector3.MoveTowards(clips[i].transform.position, endPositions[i], speed);
            float distance =  Vector3.Distance(clips[i].transform.position, endPositions[i]);
            if(distance < 0.1) {
              clipMoving[i] = false;
            }
          }
        }
      } else if(transitioning && !entering){
        for(int i = 0; i < clips.Length; i++){
          if(clipMoving[i]){
            clips[i].transform.position = Vector3.MoveTowards(clips[i].transform.position, startPos, speed);
            float distance =  Vector3.Distance(clips[i].transform.position, startPos);
            if(distance < 0.1) {
              clipMoving[i] = false;
            }
          }
        }
      }
      bool stopTransition = true;
      for(int i = 0; i < clipMoving.Length; i++){
        if(clipMoving[i]){
          stopTransition = false;
        }
      }
      if(stopTransition) transitioning = false;

      if(visitor && active){
        Vector3 nose = Vector3.Normalize(visitor.transform.forward);
        nose.y = 0;

        for(int i = 0; i < clips.Length; i++){
          Vector3 personDirection = visitor.transform.position - clips[i].transform.position;
          personDirection = Vector3.Normalize(personDirection);
          personDirection.y = 0;
          float dotProduct = Vector3.Dot(nose, personDirection);
          
          dotProduct = (dotProduct * -1.0f);
          if(dotProduct < 0.0f) {
            dotProduct = 0.0f;
          }

          int scaler = (int)(dotProduct * 95.0f + 0.5f);
          FMODUnity.RuntimeManager.StudioSystem.setParameterByName(FMODParameters[i], scaler);
        }
      }
    }

    /**
     *  onEnter() is a public method called by Wollaston when the visitor enters the painting.
     *
     *  It enables the movement of the GameObjects to their active positions by setting transitioning
     *  to true, entering to true, active to true, and the four booleans in clipMoving to true.
     */
    public void onEnter() {
      transitioning = true;
      entering = true;
      clipMoving = new bool[] {true, true, true, true};
      active = true;
    }

    /**
     *  onExit() is a public method called by Wollaston when the visitor exits the painting. 
     *
     *  It enables the return of the four GameObjects to their central position by setting transitioning
     *  to true, entering to false, active to false, and the four booleans of clipMoving to true.
     */
    public void onExit() {
      transitioning = true;
      entering = false;
      clipMoving = new bool[] {true, true, true, true};
      active = false;
    }
}
