using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceWalk : MonoBehaviour
{

    [SerializeField]
    private Visitor visitor;

    [SerializeField]
    private GameObject iceWalk1;
    private Vector3 iceWalk1EndPos = new Vector3(-1f, 0f, -2f);

    [SerializeField]
    private GameObject iceWalk2;
    private Vector3 iceWalk2EndPos = new Vector3(2f, 0f, 2f);

    [SerializeField]
    private GameObject iceWalk3;
    private Vector3 iceWalk3EndPos = new Vector3(3f, 0f, -2f);

    private GameObject[] clips;
    private Vector3[] endPositions;

    private bool[] clipMoving = {false, false, false};
    private bool enter = true;


    // Start is called before the first frame update
    void Start()
    {
          iceWalk1EndPos = transform.TransformPoint(iceWalk1EndPos);
          iceWalk2EndPos = transform.TransformPoint(iceWalk2EndPos);
          iceWalk3EndPos = transform.TransformPoint(iceWalk3EndPos);
          clips = new GameObject[] {iceWalk1, iceWalk2, iceWalk3};
          endPositions = new Vector3[] {iceWalk1EndPos, iceWalk2EndPos, iceWalk3EndPos};

    }
    public void TransitionIn() {
      enter = true;
      foreach(GameObject clip in clips){
        clip.transform.position = new Vector3(visitor.transform.position.x, 3.0f, visitor.transform.position.z);
        clip.GetComponent<FMODUnity.StudioEventEmitter>().Play();
      }
      clipMoving = new bool[] {true, true, true};
    }

    public void TransitionOut() {
      enter = false;
      clipMoving = new bool[] {true, true, true};
    }

    // Update is called once per frame
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
            //Debug.Log("Moving running in");
            clips[i].transform.position = Vector3.MoveTowards(clips[i].transform.position, visitor.transform.position, speed);
            float distance =  Vector3.Distance(clips[i].transform.position, visitor.transform.position);
            if(distance < 0.5) {
              Debug.Log("Stopping iceWalk", clips[i]);
              clipMoving[i] = false;
              clips[i].GetComponent<FMODUnity.StudioEventEmitter>().Stop();
            }
          }
        }
      }
    }
}
