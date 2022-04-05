using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class river : MonoBehaviour
{

    private float speed = -5f;
    private float speedX = 3.0f;
    private Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("CREATE RIVER");
        rb = this.GetComponent<Rigidbody>();
        rb.velocity = new Vector3(speedX, 0, speed);
    }

    // Update is called once per frame
    void Update()
    {
      if(transform.position.x < -57.0f || transform.position.x > 56.0f || transform.position.z > 200.0f || transform.position.z < -21f){
        Destroy(gameObject);
      }
    }
}
