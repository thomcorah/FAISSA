/**
 *  \class robin
 *  robin controls the Robin GameObject, to which the looped chirps of a robin are attached.
 *
 *  It moves randomly around the visitor.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class robin : MonoBehaviour
{
    /**
     *  robin needs a reference to the Visitor GameObject as the centre point
     *  around which a new random destination is chosen.
     */
    [SerializeField]
    private Visitor visitor;

    /**
     *  moving tracks the current state of the Robin GameObject. 
     *  While moving is true, the Update() method moves the GameObject
     *  towards the randomly generated end point.
     */
    private bool moving = false;

    /**
     *  endPoint is a randoml generated Vector3 position that the GameObject
     *  will be moved towards.
     */
    private Vector3 endPoint;

    /**
     *  Start() is called before the first frame, and is used to generate the first
     *  random point to move to and start movement.
     */
    void Start()
    {
      endPoint = generatePoint();
      moving = true;
    }

    /**
     *  Update(), called once per frame, is used to move the GameObject towards the endPoint
     *  if moving is true.
     *
     *  Once the GameObject gets close enough to the end point, a new end point is generated by
     *  calling generatePoint().
     */ 
    void Update()
    {
      if(moving){
        float speed = 1.5f * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, endPoint, speed);
        float distance =  Vector3.Distance(transform.position, endPoint);
        if(distance < 0.1) {
          endPoint = generatePoint();
        }
      }
    }

    /**
     *  generatePoint() is called whenever we need to generate a new random end point
     *  for the GameObject to move towards. It generates random numbers between 4 and -4 for
     *  both x and z axis, and then adds these to the current position of the Visitor GameObject
     *  to derive a new end point.
     */
    Vector3 generatePoint(){
      float x;
      do {
        x = Random.Range(4.0f, -4.0f);
      } while(Mathf.Abs(x) >= 2.0f);

      float z;
      do {
        z = Random.Range(4.0f, -4.0f);
      } while(Mathf.Abs(z) >= 2.0f);

      Vector3 NewPoint = new Vector3(visitor.transform.position.x + x, visitor.transform.position.y, visitor.transform.position.z + z);
      return NewPoint;
    }
}
