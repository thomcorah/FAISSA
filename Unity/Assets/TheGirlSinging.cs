using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheGirlSinging : MonoBehaviour
{

    [SerializeField]
    private GameObject Singing1;

    [SerializeField]
    private GameObject Singing2;

    [SerializeField]
    private GameObject Singing3;

    [SerializeField]
    private GameObject Singing4;

    [SerializeField]
    private GameObject Singing5;

    [SerializeField]
    private GameObject Singing6;

    [SerializeField]
    private GameObject Singing7;

    [SerializeField]
    private GameObject Singing8;

    private GameObject[] Singers;

    private Vector3 StartPos = new Vector3(5f, 1.6f, 0f);
    private Vector3 EndPos = new Vector3(-5f, 1.6f, 0f);



    // Start is called before the first frame update
    void Start()
    {
        Singers = new GameObject[] {Singing1, Singing2, Singing3, Singing4, Singing5, Singing6, Singing7, Singing8};
        StartPos = transform.TransformPoint(StartPos);
        EndPos = transform.TransformPoint(EndPos);
    }

    // Update is called once per frame
    void Update()
    {
        float speed = 1.0f * Time.deltaTime;
        for(int i = 0; i < Singers.Length; i++){
          Singers[i].transform.position = Vector3.MoveTowards(Singers[i].transform.position, EndPos, speed);
          float distance =  Vector3.Distance(Singers[i].transform.position, EndPos);
          if(distance < 0.1) {
            Singers[i].transform.position = StartPos;
          }
        }
    }
}
