/**
 *  \class BosworthRunning
 *  BosworthRunning controlls the three GameObjects which have underwater
 *  recordings of running water attached in the Battle of Bosworth Field 
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
 *  the visitor GameObject and their audio playback is stopped. The exception here
 *  is running1. This is used as the sound from this painting that plays continuously.
 *  In Unity, it's FMOD event is set to start playing on object start. When the 
 *  visitor leaves the painting, it is returned not to the position of the visitor, but
 *  to a central point in front of the painting, and it's FMOD event continues to play.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BosworthRunning : MonoBehaviour
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
     *  running1 is the first of three GameObjects that BosworthRunning is concerned
     *  with.
     */
    [SerializeField]
    private GameObject running1;

    /**
     *  running1EndPos is the location to which running1 should be moved when the 
     *  Visitor enters the painting.
     */
    private Vector3 running1EndPos = new Vector3(0f, 2f, 1f);

    /**
     *  running1StartPos is the location at which running1 sits when the 
     *  painting is inactive, and therefore where it should return to when the 
     *  Visitor leaves the painting. 
     *
     *  running2 and running3 however originate from and return to the location of the 
     *  the Visitor GameObject. The audio for running1 is left running when the painting
     *  is inactive, acting as the emerging sound that reaches out from this painting
     *  into the gallery space. This is why it must be returned to a central position.
     */
    private Vector3 running1StartPos = new Vector3(1f, 2f, 0f);

    /**
     *  running2 is the second of the three GameObjects that BosworthRunning manages.
     */
    [SerializeField]
    private GameObject running2;

    /**
     *  running2EndPos is the location to which running2 is moved when the painting 
     *  becomes active.
     */
    private Vector3 running2EndPos = new Vector3(4f, 2f, -1f);

    /**
     *  running3 is the third of the three GameObjects that BosworthRunning manages.
     */
    [SerializeField]
    private GameObject running3;

    /**
     *  running3EndPos is the location to which running3 is moved when the Visitor
     *  enters the painting.
     */
    private Vector3 running3EndPos = new Vector3(1f, 2f, -4f);

    /**
     *  clips is a List of the three running water GameObjects. In Start(),
     *  it is populated with the three GameObjects. This is used in order to 
     *  iterate over the three GameObjects.
     */
    private GameObject[] clips;

    /**
     *  endPositions is a List of the Vector3 objects that represent the end position
     *  of each of the running water GameObjects. It is populated with the Vector3s described
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
     *  The Vector3 objects representing start and end positions are converted from 
     *  the local coordinate space to the world coordinate space. 
     *  
     *  The clips List is assigned the three GameObjects that represent the three audio clips.
     *
     *  The endPositions List is assigned the three Vector3 end positions, corresponding to 
     *  each of the clips.
     */
    void Start()
    {
          running1EndPos = transform.TransformPoint(running1EndPos);
          running1StartPos = transform.TransformPoint(running1StartPos);
          running2EndPos = transform.TransformPoint(running2EndPos);
          running3EndPos = transform.TransformPoint(running3EndPos);
          clips = new GameObject[] {running1, running2, running3};
          endPositions = new Vector3[] {running1EndPos, running2EndPos, running3EndPos};
    }

    /**
     *  Update() is called on each frame. 
     *
     *  It is used here to move each of the GameObjects either to their end positions when the 
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
     *  to a central position. 
     *
     *  If this is currently looking at the first GameObject (running1), it is moved towards the Vector3 described
     *  by running1StartPos. The other two GameObjects are moved towards the position of the visitor GameObject. 
     *
     *  The distance from the GameObject to its target position is then measured, and if suffiently close its corresponding
     *  boolean in clipMoving is set to false. In the case of running2 and running3, its FMOD event is also stopped.
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
            float distance = 5f;
            if(i == 0) {
              clips[0].transform.position = Vector3.MoveTowards(clips[0].transform.position, running1StartPos, speed);
              distance =  Vector3.Distance(clips[i].transform.position, running1StartPos);
            } else {
              clips[i].transform.position = Vector3.MoveTowards(clips[i].transform.position, visitor.transform.position, speed);
              distance =  Vector3.Distance(clips[i].transform.position, visitor.transform.position);
            }

            if(distance < 0.5) {
              Debug.Log("Stopping Running", clips[i]);
              clipMoving[i] = false;
              if(i != 0) clips[i].GetComponent<FMODUnity.StudioEventEmitter>().Stop();
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
      Debug.Log("Running Trans In");
      enter = true;
      for(int i = 1; i < clips.Length; i++){
        clips[i].transform.position = new Vector3(visitor.transform.position.x, 3.0f, visitor.transform.position.z);
        clips[i].GetComponent<FMODUnity.StudioEventEmitter>().Play();
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
      Debug.Log("Running Trans Out");
      enter = false;
      clipMoving = new bool[] {true, true, true};
    }
}
