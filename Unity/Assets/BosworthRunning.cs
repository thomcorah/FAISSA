using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BosworthRunning : MonoBehaviour
{

    [SerializeField]
    private Visitor visitor;

    [SerializeField]
    private GameObject running1;
    private Vector3 running1EndPos = new Vector3(0f, 2f, 1f);
    private Vector3 running1StartPos = new Vector3(1f, 2f, 0f);

    [SerializeField]
    private GameObject running2;
    private Vector3 running2EndPos = new Vector3(4f, 2f, -1f);

    [SerializeField]
    private GameObject running3;
    private Vector3 running3EndPos = new Vector3(1f, 2f, -4f);

    private GameObject[] clips;
    private Vector3[] endPositions;
    private bool[] clipMoving = {false, false, false};

    private bool enter = true;

    // Start is called before the first frame update
    void Start()
    {
          running1EndPos = transform.TransformPoint(running1EndPos);
          running1StartPos = transform.TransformPoint(running1StartPos);
          running2EndPos = transform.TransformPoint(running2EndPos);
          running3EndPos = transform.TransformPoint(running3EndPos);
          clips = new GameObject[] {running1, running2, running3};
          endPositions = new Vector3[] {running1EndPos, running2EndPos, running3EndPos};

    }

    // Update is called once per frame

    public void TransitionIn() {

      Debug.Log("Running Trans In");
      enter = true;
      for(int i = 1; i < clips.Length; i++){
        clips[i].transform.position = new Vector3(visitor.transform.position.x, 3.0f, visitor.transform.position.z);
        clips[i].GetComponent<FMODUnity.StudioEventEmitter>().Play();
      }
      clipMoving = new bool[] {true, true, true};
    }

    public void TransitionOut() {
      Debug.Log("Running Trans Out");
      enter = false;
      clipMoving = new bool[] {true, true, true};
    }

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
            float distance = 5f;
            if(i == 0) {
              clips[0].transform.position = Vector3.MoveTowards(clips[0].transform.position, running1StartPos, speed);
              distance =  Vector3.Distance(clips[i].transform.position, running1StartPos);
            } else {
              clips[i].transform.position = Vector3.MoveTowards(clips[i].transform.position, visitor.transform.position, speed);
              distance =  Vector3.Distance(clips[i].transform.position, visitor.transform.position);
            }

            if(distance < 0.5) {
              Debug.Log("Stopping Running", clips[i]);
              clipMoving[i] = false;
              if(i != 0) clips[i].GetComponent<FMODUnity.StudioEventEmitter>().Stop();
            }
          }
        }
      }
    }
}
