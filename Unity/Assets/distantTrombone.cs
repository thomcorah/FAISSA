using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class distantTrombone : MonoBehaviour
{

    private float speedZ = 3.0f;
    private float direction = 1.0f;
    private Rigidbody rb;

    private bool ending = false;

    // Start is called before the first frame update
    void Start()
    {
      rb = this.GetComponent<Rigidbody>();
    }

    public void StartPlaying() {
      Vector3 pos = transform.position;
      transform.position = new Vector3(pos.x, pos.y, -99.0f);
      rb.velocity = new Vector3(0, 0, 3.0f);
      this.GetComponent<FMODUnity.StudioEventEmitter>().Play();
    }

    public void StopPlaying() {
      ending = true;
      direction = -1.0f;
      rb.velocity = new Vector3(0, 0, -20.0f);


    }

    // Update is called once per frame
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
}
