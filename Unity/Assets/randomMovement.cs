using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class randomMovement : MonoBehaviour
{
  Vector3 vel; // Holds the random velocity
  float switchDirection = 3;
  float curTime = 0.0f;

  void Start()
  {
  SetVel();
  }

  void SetVel()
  {
    //FMODUnity.RuntimeManager.PlayOneShot("event:/Robin");
  if (Random.value > .5) {
    vel.x = 5 * Random.value;
  }
  else {
    vel.x = -5 * Random.value;
  }
  if (Random.value > .5) {
    vel.z = 5 * Random.value;
  }
  else {
    vel.z = -5 * Random.value;
  }



  }

  void Update()
  {
  if (curTime < switchDirection) {
    curTime += 1 * Time.deltaTime;
    if (Mathf.Abs(transform.position.x) > 20) {
      vel.x *= -1;
    }

    if(Mathf.Abs(transform.position.z) > 20) {
      vel.z *= -1;
    }
  }
  else {
    SetVel();
    if (Random.value > .5) {
      switchDirection += Random.value;
    } else {
      switchDirection -= Random.value;
    }
    if (switchDirection < 1) {
      switchDirection = 1 + Random.value;
    }
    curTime = 0;
  }
  }

  void FixedUpdate()
  {
  GetComponent<Rigidbody>().velocity = vel;
  }
}
