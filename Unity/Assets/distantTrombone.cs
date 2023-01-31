/**
 *  \class distantTrombone
 *  distantTrombone is attached to the DistantTrombone GameObject, and
 *  is used within the YellowstoneFalls painting. 
 *
 *  It has the DistantTrombone FMOD event attached to it. 
 *
 *  It exposes StartPlaying() which YellowstoneFalls uses to start it 
 *  playing. Whilst it is playing, it moves slowly towards the visitor,
 *  and then away from the visitor, drifting slowly into and out of
 *  audible range. 
 *
 *  YellowstoneFalls can stop playback using the StopPlaying() public method.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class distantTrombone : MonoBehaviour
{

    /**
     *  speedZ is the velocity of the GameObject in the Z axis.
     */
    private float speedZ = 1.5f;

    /**
     *  The value of direction is used to determine whether the GameObject
     *  is moving towards or away from the visitor.
     */
    private float direction = 1.0f;

    /**
     *  rb is a Rigidbody and used to change the velocity of the GameObject.
     */
    private Rigidbody rb;

    /**
     *  When StopPlaying() is called, the ending flag is set to true. This is
     *  then picked up in Update() to perform a reset ready for the next time
     *  the visitor enters YellowstoneFalls.
     */
    private bool ending = false;

    /**
     *  Start() is called before the first frame. It is used here to assign the 
     *  Rigidbody component of the GameObject to the rb property.
     */
    void Start()
    {
      rb = this.GetComponent<Rigidbody>();
    }

    /**
     *  Update() is called once per frame. 
     *
     *  It checks the value of the ending boolean flag. If this is 
     *  true (as set by StopPlaying()) it then checks to see if the 
     *  GameObject has travelled to its most distant point.
     *
     *  If it has, it stops playback of the FMOD event attached to
     *  the object, sets its velocity to 0, and resets the ending 
     *  flag to false.
     *
     *  If the ending flag is false, it checks the position of the GameObject.
     *  If it has ventured too close or too far away from the painting, the 
     *  direction is reversed and a new velocity set accordingly.
     */
    void Update()
    {
      if(ending) {
        if(transform.position.z < -100.0f) {
          this.GetComponent<FMODUnity.StudioEventEmitter>().Stop();
          rb.velocity = new Vector3(0, 0, 0);
          ending = false;
        }
      } else if(transform.position.z > -35.0f || transform.position.z < -100.0f){
        direction *= -1.0f;
        rb.velocity = new Vector3(0, 0, speedZ * direction);
      }
    }

    /**
     *  StartPlaying() is a public method, called by YellowstoneFalls when the 
     *  visitor enters the painting. It sets the position of the DistantTrombone
     *  GameObject to a point in the distane, and its velocity to move slowly in 
     *  towards the visitor. 
     *
     *  It then starts playback of the FMOD event attached.
     */
    public void StartPlaying() {
      Vector3 pos = transform.position;
      transform.position = new Vector3(pos.x, pos.y, -99.0f);
      rb.velocity = new Vector3(0, 0, 3.0f);
      this.GetComponent<FMODUnity.StudioEventEmitter>().Play();
    }

    /**
     *  StopPlaying() is a public method, called by YellowstoneFalls when the 
     *  visitor exits the painting. It sets the ending flag to true, makes sure 
     *  the direction variable is describing travel away from the visitor,
     *  and sets the velocity to a faster pace away from the visitor.
     */
    public void StopPlaying() {
      ending = true;
      direction = -1.0f;
      rb.velocity = new Vector3(0, 0, -20.0f);
    }
}
