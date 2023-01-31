/**
 *  \class RailwaySounds
 *  RailwaySounds is used by RailwayStation, attached to the GameObject that
 *  represents The Railway Station painting. 
 *
 *  It is used to control the movement of ten of the GameObjects to which 
 *  FMOD events are attached, and their audio output. 
 *
 *  Each of these FMOD events has two types of sound. 
 *  1. A sound based on recordings of steam engines.
 *  2. A sound based on recordings of tropical birds.
 *
 *  One of the FMOD events is slightly different in that it has the sound of 
 *  a steam engine being refilled with water vs the sound of a stream running through
 *  woodland. 
 *
 *  When the piece starts, it is the steam engine sounds that are heard. These are very
 *  gradually crossfaded to their natural world counterparts as the piece progresses.
 *
 *  This is achieved through the use of an FMOD parameter. 
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RailwaySounds : MonoBehaviour
{
    /**
     *  Each of the ten GameObjects to which an FMOD event is attached are serialised here.
     */
    [SerializeField]
    private GameObject bird1;
    /**
     *  Each of these ten GameObjects has a corresponding Vector3 that represents the position
     *  to which each will be moved when the soundscape is activated.
     */
    private Vector3 bird1Pos = new Vector3(-1f, 1.8f, 0.0f);

    [SerializeField]
    private GameObject bird2;
    private Vector3 bird2Pos = new Vector3(-1.8f, 1.8f, -1.0f);

    [SerializeField]
    private GameObject bird3;
    private Vector3 bird3Pos = new Vector3(-1.8f, 1.8f, 2.0f);

    [SerializeField]
    private GameObject bird4;
    private Vector3 bird4Pos = new Vector3(-1.3f, 1.8f, 3.0f);

    [SerializeField]
    private GameObject bird5;
    private Vector3 bird5Pos = new Vector3(-0.1f, 1.8f, 3.5f);

    [SerializeField]
    private GameObject bird6;
    private Vector3 bird6Pos = new Vector3(2.1f, 1.8f, 3.0f);

    [SerializeField]
    private GameObject bird7;
    private Vector3 bird7Pos = new Vector3(2.3f, 1.8f, 2.0f);

    [SerializeField]
    private GameObject bird8;
    private Vector3 bird8Pos = new Vector3(2.3f, 1.8f, 1.0f);

    [SerializeField]
    private GameObject bird9;
    private Vector3 bird9Pos = new Vector3(1.2f, 1.8f, 0.0f);

    [SerializeField]
    private GameObject water;
    private Vector3 waterPos = new Vector3(-1f, 0f, 0.5f);

    /**
     *  The clips List will be populated with the ten GameObjects and used to be able to
     *  iterate over them.
     */
    private GameObject[] clips;

    /**
     *  The endPositions List will be populated with the corresponding Vector3 for the end
     *  position of each GameObject.
     */
    private Vector3[] endPositions;

    /**
     *  clipMoving is used to store a boolean for each of the ten audio GameObjects describing
     *  whether each is currently moving or not.
     */
    private bool[] clipMoving = {false, false, false, false, false, false, false, false, false, false};

    /**
     *  State is tracked through the use of these two booleans. When the painting is in 
     *  transition (i.e. the GameObjects are currently moving in or out) transitioning is true. 
     *
     */
    private bool transitioning = false;

    /**
     *  When the painting is being entered, entering is true, when it's being exited, it is false.
     */ 
    private bool entering = true;

    /**
     *  crossFadeValue represents the cross fade between the sounds of engines and the natural world. 
     *  
     */
    private int crossFadeValue = 0;

    /**
     *  A repeating method (CrossFade()) either increases or decreases its value, based on the value
     *  of crossFadeVelocity.
     */
    private int crossFadeVelocity = 5;

    /**
     *  Start() is called before the first frame. 
     *
     *  It is used here to assign the ten GameObjects to the clips List.
     *
     *  The ten corresonding end positions are assigned to the endPositions List, and then
     *  converted from local coordinate space to world coordinate space. 
     */
    void Start()
    {
      clips = new GameObject[] {bird1, bird2, bird3, bird4, bird5, bird6, bird7, bird8, bird9, water};
      endPositions = new Vector3[] {bird1Pos, bird2Pos, bird3Pos, bird4Pos, bird5Pos, bird6Pos, bird7Pos, bird8Pos, bird9Pos, waterPos};
      for(int i = 0; i < endPositions.Length; i++){
        endPositions[i] = transform.TransformPoint(endPositions[i]);
      }
    }

    /**
     *  Update() is called once per frame. 
     *
     *  It is used here to move the GameObjects towards their corresponding end positions when the visitor enters
     *  the painting, or back to their central starting position when the visitor exits the painting. 
     *
     *  If the values of transitioning and entering are both true, the clips List is iterated. For each one, we
     *  check to see if it is still moving according to its corresponding boolean in the clipMoving List. If it
     *  is moving, then it is moved towards its correspinding Vector3 in the endPositions List. 
     *
     *  If the distance between the GameObject and its end position is suffiently small, we set the corresponding
     *  boolean in clipMoving to false.
     *
     *  A similar process occurs to move the GameObjects back to their central position when the visitor leaves the 
     *  painting. 
     *
     *  Finally, regardless of which direction the transition has been in, we check to see if all GameObjects have finished
     *  moving. If they have, we set the value of transitioning to false.
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
        Vector3 startPos = transform.TransformPoint(new Vector3(0f, 0.0f, 0.0f));
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
    }

    /**
     *  onEnter() is a public method called by RailwayStation when the visitor enters the painting.
     *
     *  It initiates the transtional movement of the ten GameObjects by setting transitioning and entering to 
     *  true. All values in clipMoving are set to true.
     *
     *  Finally, InvokeRepeating() is used to start the repeating call to CrossFade().
     */
    public void onEnter() {
      Debug.Log("RAILWAY SOUNDS START");
      transitioning = true;
      entering = true;
      clipMoving = new bool[] {true, true, true, true, true, true, true, true, true, true};

      InvokeRepeating("CrossFade", 0.0f, 10.0f);
    }

    /**
     *  onExit() is a public method called by RailwayStation when the visitor exits the painting.
     *
     *  It initiates the transtional movement of the ten GameObjects by setting transitioning to true,
     *  and entering to false. All values in clipMoving are set to true.
     *
     *  Finally, the repeated call to CrossFade() is cancelled with CancelInvoke().
     */
    public void onExit() {
      transitioning = true;
      entering = false;
      clipMoving = new bool[] {true, true, true, true, true, true, true, true, true, true};

      CancelInvoke("CrossFade");
      FMODUnity.RuntimeManager.StudioSystem.setParameterByName("railwayCrossFade", 0);
      crossFadeValue = 0;
    }

    /**
     *  CrossFade() is repeated called whilst the painting is active.
     *
     *  It adjusts the value of crossFadeValue according to the value of crossFadeVelocity.
     *
     *  If the value of crossFadeValue exceeds 100 or goes below 0, the value of crossFadeVelocity
     *  is inverted. The value of crossFadeValue is then passed to FMOD as a paramenter, which uses it
     *  to cross fade between the engine and natural sounds in each of the ten FMOD events.
     */
    void CrossFade(){
      crossFadeValue += crossFadeVelocity;
      if(crossFadeValue > 100 || crossFadeValue < 0){
        crossFadeVelocity *= -1;
      }
      FMODUnity.RuntimeManager.StudioSystem.setParameterByName("railwayCrossFade", crossFadeValue);
    }
}
