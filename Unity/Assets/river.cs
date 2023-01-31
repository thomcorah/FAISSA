/**
 *  \class river
 *  river is attached to the River prefab object, used by generateRivers in the 
 *  YellowstoneFalls painting. The prefab has the RiverRunning FMOD event attached.
 *
 *  The code here defines the object's velocity, and destroys the object when it
 *  has ventured beyond its audible range.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class river : MonoBehaviour
{
    /**
     *  speed is the object's velocity in the Z axis.
     */
    private float speed = 2.0f;

    /**
     *  speedX is the object's velocity in the X axis.
     */
    private float speedX = 3.5f;

    /**
     *  rb is a reference to the GameObject's Rigidbody, which we use
     *  to apply velocity to the GameObject.
     */
    private Rigidbody rb;
    
    /**
     *  Start() is called when the object is instantiated by generateRivers.
     *  
     *  It assigns the GameObject's Rigidbody to rb, and then sets its velocity
     *  based on the values of speed and speedX.
     */
    void Start()
    {
        Debug.Log("CREATE RIVER");
        rb = this.GetComponent<Rigidbody>();
        rb.velocity = new Vector3(speedX, 0, speed);
    }

    /**
     *  Update() is called once per frame. 
     *
     *  It monitors the current position of the GameObject in world space and
     *  destroys it when it has ventured beyond the defined thresholds.
     */
    void Update()
    {
      if(transform.position.x < -57.0f || transform.position.x > 20.0f || transform.position.z > 30.0f || transform.position.z < -21f){
        Destroy(gameObject);
      }
    }
}
