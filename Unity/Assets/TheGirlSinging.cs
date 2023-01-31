/**
 *  \class TheGirlSinging
 *  TheGirlSinging is used TheGirl, attached to the object representing the painting
 *  The Girl I Left Behind Me.
 *
 *  This soundscape includes 8 GameObjects to which are attached recordings of a 
 *  sung portion of the folk song on which this painting is based. These GameObjects
 *  pass continuously in front of the visitor from right to left, matching the motion
 *  described in the painting. 
 *
 *  This script controlls this motion. 
 *
 *  When a GameObject reaches the far left side, it is repositioned to the far right, and 
 *  continues. 
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheGirlSinging : MonoBehaviour
{

    /**
     *  Each of the eight GameObjects is represented using a serialised GameObject property, 
     *  Singing1 - Singing8.
     */
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

    /**
     *  In order to be able it iterate over all eight GameObjects, the Singers List
     *  is populated with them during Start().
     */
    private GameObject[] Singers;

    /**
     *  The StartPos and EndPos Vector3 properties represent the far right (StartPos) and
     *  far left (EndPos) positions between which the GameObjects will move. 
     */
    private Vector3 StartPos = new Vector3(5f, 1.6f, 0f);

    /**
     *  EndPos is the far left location to which the singing GameObjects move towards.
     */
    private Vector3 EndPos = new Vector3(-5f, 1.6f, 0f);

    /**
     *  Start() is called before the first frame. 
     *
     *  The Singers List is populated with the eight GameObjects representing the sung recordings.
     *
     *  StartPos and EndPos are converted from the local coordinate space to the world coordinate space.
     */
    void Start()
    {
        Singers = new GameObject[] {Singing1, Singing2, Singing3, Singing4, Singing5, Singing6, Singing7, Singing8};
        StartPos = transform.TransformPoint(StartPos);
        EndPos = transform.TransformPoint(EndPos);
    }

    /**
     *  Update() is called each frame. 
     *
     *  The List of Singer objects is iterated over. For each, it is moved closer to the position described by
     *  EndPos. If the distance between the GameObject and EndPos is sufficiently small, the GameObject is repositioned
     *  at the location described by StartPos.
     */
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
