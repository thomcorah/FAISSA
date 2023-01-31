/**
 *  \class IceWalk
 *  IceWalk controlls the three GameObjects which have slowed down
 *  recordings of footsteps in a frozen field attached in the Battle of Bosworth Field 
 *  soundscape. 
 *
 *
 *  Initially, these three GameObjects are together in front of the painting. 
 *  When the visitor enters the painting they are first moved to the position
 *  of the visitor GameObject before their respective FMOD events are played.
 *  They are then transitioned to three points surrounding
 *  the position of the visitor.
 *
 *  When the visitor exits the painting, they are moved back to the position of
 *  the visitor GameObject and their audio playback is stopped. 
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceWalk : MonoBehaviour
{
    /**
     *  visitor is a serialised reference to the visitor GameObject. It is used
     *  as the point from which to move the running water GameObjects when the 
     *  visitor enters the painting, and the point to which to return them when 
     *  the visitor leaves the painting.
     */
    [SerializeField]
    private Visitor visitor;

    /**
     *  There are three GameObjects in the scene that are effected here. For each,
     *  there is a serialised reference to the GameObject - iceWalk1, iceWalk2, and 
     *  iceWalk3. 
     */
    [SerializeField]
    private GameObject iceWalk1;

    /**
     *  Each of the iceWalk GameObjects also has a Vector3 describing the location it should be moved
     *  to when the Bosworth soundscape is active. iceWalk1EndPos is the location to which iceWalk1
     *  should be moved.
     */
    private Vector3 iceWalk1EndPos = new Vector3(-1f, 0f, -2f);

    /**
     *  iceWalk2 is a reference to the second of three GameObjects that IceWalk
     *  is concerned with.
     */
    [SerializeField]
    private GameObject iceWalk2;

    /**
     *  iceWalk2EndPos is the location to which iceWalk2 should moved when the Visitor
     *  enters the Bosworth soundscape.
     */
    private Vector3 iceWalk2EndPos = new Vector3(2f, 0f, 2f);

    /**
     *  iceWalk3 is the third of three GameObjects that IceWalk is concerned with.
     */
    [SerializeField]
    private GameObject iceWalk3;

    /**
     *  iceWalk3EndPos is the target location for iceWalk3 when the painting is active.
     */
    private Vector3 iceWalk3EndPos = new Vector3(3f, 0f, -2f);

    /**
     *  clips is a List of the three GameObjects. In Start(),
     *  it is populated with the three GameObjects. This is used in order to 
     *  iterate over the three GameObjects.
     */
    private GameObject[] clips;

    /**
     *  endPositions is a List of the Vector3 objects that represent the end position
     *  of each of the three GameObjects. It is populated with the Vector3s described
     *  above in Start(), in an order that corresponds with the order of GameObjects in clips.
    */
    private Vector3[] endPositions;

    /**
     *  We need to monitor the position of each of the GameObjects during transitions in order
     *  to know when they have all completed their move. clipMoving is a List of booleans that is
     *  used to toggle the overall movement state of these three GameObjects.
     */
    private bool[] clipMoving = {false, false, false};

    /**
     *  enter is a boolean flag which is set to true when the visitor enters the painting,
     *  and false when they exit. It is used in Update() to move the GameObjects in the 
     *  correct direction.
     */
    private bool enter = true;


    /**
     *  Start() is called before first frame, and is used here to perform initial setup.
     *  
     *  The Vector3 objects representing the end positions are converted from 
     *  the local coordinate space to the world coordinate space. 
     *  
     *  The clips List is assigned the three GameObjects that represent the three audio clips.
     *
     *  The endPositions List is assigned the three Vector3 end positions, corresponding to 
     *  each of the clips.
     */
    void Start()
    {
          iceWalk1EndPos = transform.TransformPoint(iceWalk1EndPos);
          iceWalk2EndPos = transform.TransformPoint(iceWalk2EndPos);
          iceWalk3EndPos = transform.TransformPoint(iceWalk3EndPos);
          clips = new GameObject[] {iceWalk1, iceWalk2, iceWalk3};
          endPositions = new Vector3[] {iceWalk1EndPos, iceWalk2EndPos, iceWalk3EndPos};
    }

    /**
     *  Update() is called on each frame. 
     *
     *  It is used here to move each of the GameObjects to their end positions when the 
     *  visitor enters the painting, or to return them to their central positions when the visitor
     *  leaves the painting. 
     *
     *  It first loops over each of the GameObjects in the clips List. For each, it checks the clipMoving
     *  List to see if the current GameObject is moving. 
     *
     *  If it is moving, it checks whether we are transitioning in or out of the soundscape, based on the 
     *  value of the enter boolean.
     *
     *  If the scene is transitioning in, the GameObject is moved towards its respective end position. The distance
     *  between the GameObject and its end position is measured. If this distance is sufficiently small, it is considered
     *  to have arrived, and its respective boolean in clipMoving is set to false.
     *
     *  If the scene is transitioning out (i.e. the visitor has left the painting) then the GameObjects must return
     *  to the location of the visitor GameObject.
     *
     *  The distance from the GameObject to its target position is then measured, and if suffiently close its corresponding
     *  boolean in clipMoving is set to false. Its FMOD event is also stopped at this point.
     */
    void Update()
    {
      for(int i = 0; i < clips.Length; i++){
        if(clipMoving[i]){
          float speed = 1.5f * Time.deltaTime;
          if(enter){
            clips[i].transform.position = Vector3.MoveTowards(clips[i].transform.position, endPositions[i], speed);
            float distance =  Vector3.Distance(clips[i].transform.position, endPositions[i]);
            if(distance < 0.2) {
              clipMoving[i] = false;
            }
          } else {
            clips[i].transform.position = Vector3.MoveTowards(clips[i].transform.position, visitor.transform.position, speed);
            float distance =  Vector3.Distance(clips[i].transform.position, visitor.transform.position);
            if(distance < 0.5) {
              Debug.Log("Stopping iceWalk", clips[i]);
              clipMoving[i] = false;
              clips[i].GetComponent<FMODUnity.StudioEventEmitter>().Stop();
            }
          }
        }
      }
    }

    /**
     *  TransitionIn() is a public method called by Bosworth when the visitor enters the painting.
     *
     *  It sets enter to true, moves each of the three GameObjects to the location of the visitor,
     *  starts their FMOD event playing, and sets all three booleans in clipMoving to true. This
     *  is then picked up in Update() to move the GameObjects to their end positions. 
     */
    public void TransitionIn() {
      enter = true;
      foreach(GameObject clip in clips){
        clip.transform.position = new Vector3(visitor.transform.position.x, 3.0f, visitor.transform.position.z);
        clip.GetComponent<FMODUnity.StudioEventEmitter>().Play();
      }
      clipMoving = new bool[] {true, true, true};
    }

    /**
     *  TransitionOut() is a public method called by Bosworth when the visitor leaves the painting.
     *
     *  It sets enter to false and all three booleans in clipMoving to true. This is then picked up by
     *  Update() resulting in all three GameObjects moving back towards their central position.
     */
    public void TransitionOut() {
      enter = false;
      clipMoving = new bool[] {true, true, true};
    }
}
