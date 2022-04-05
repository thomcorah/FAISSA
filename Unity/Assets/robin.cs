using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class robin : MonoBehaviour
{

    [SerializeField]
    private Visitor visitor;

    private bool moving = false;
    private Vector3 endPoint;

    // Start is called before the first frame update
    void Start()
    {
      endPoint = generatePoint();
      moving = true;
    }

    // Update is called once per frame
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
