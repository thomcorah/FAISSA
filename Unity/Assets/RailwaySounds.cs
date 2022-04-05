using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RailwaySounds : MonoBehaviour
{

    [SerializeField]
    private GameObject bird1;
    private Vector3 bird1Pos = new Vector3(-1f, 1.8f, -1.5f);

    [SerializeField]
    private GameObject bird2;
    private Vector3 bird2Pos = new Vector3(-1.8f, 1.8f, -0.5f);

    [SerializeField]
    private GameObject bird3;
    private Vector3 bird3Pos = new Vector3(-1.8f, 1.8f, 0.5f);

    [SerializeField]
    private GameObject bird4;
    private Vector3 bird4Pos = new Vector3(-1.3f, 1.8f, 1.5f);

    [SerializeField]
    private GameObject bird5;
    private Vector3 bird5Pos = new Vector3(-0.1f, 1.8f, 2f);

    [SerializeField]
    private GameObject bird6;
    private Vector3 bird6Pos = new Vector3(2.1f, 1.8f, 1.5f);

    [SerializeField]
    private GameObject bird7;
    private Vector3 bird7Pos = new Vector3(2.3f, 1.8f, 0.5f);

    [SerializeField]
    private GameObject bird8;
    private Vector3 bird8Pos = new Vector3(2.3f, 1.8f, -0.5f);

    [SerializeField]
    private GameObject bird9;
    private Vector3 bird9Pos = new Vector3(1.2f, 1.8f, -1.5f);

    [SerializeField]
    private GameObject water;
    private Vector3 waterPos = new Vector3(-1f, 0f, -1f);

    private GameObject[] clips;
    private Vector3[] endPositions;
    private bool[] clipMoving = {false, false, false, false, false, false, false, false, false, false};

    private bool transitioning = false;
    private bool entering = true;



    // Start is called before the first frame update
    void Start()
    {
      clips = new GameObject[] {bird1, bird2, bird3, bird4, bird5, bird6, bird7, bird8, bird9, water};
      endPositions = new Vector3[] {bird1Pos, bird2Pos, bird3Pos, bird4Pos, bird5Pos, bird6Pos, bird7Pos, bird8Pos, bird9Pos, waterPos};
      for(int i = 0; i < endPositions.Length; i++){
        endPositions[i] = transform.TransformPoint(endPositions[i]);
      }
    }

    // Update is called once per frame
    void Update()
    {
      float speed = 1.5f * Time.deltaTime;
      if(transitioning && entering){
        for(int i = 0; i < clips.Length; i++){
          if(clipMoving[i]){
            clips[i].transform.position = Vector3.MoveTowards(clips[i].transform.position, endPositions[i], speed);
            float distance =  Vector3.Distance(clips[i].transform.position, endPositions[i]);
            if(distance < 0.1) {
              clipMoving[i] = false;
            }
          }
        }
      } else if(transitioning && !entering){
        Vector3 startPos = new Vector3(0f, 1.8f, -10f);
        for(int i = 0; i < clips.Length; i++){
          if(clipMoving[i]){
            clips[i].transform.position = Vector3.MoveTowards(clips[i].transform.position, startPos, speed);
            float distance =  Vector3.Distance(clips[i].transform.position, startPos);
            if(distance < 0.1) {
              clipMoving[i] = false;
            }
          }
        }
      }
      bool stopTransition = true;
      for(int i = 0; i < clipMoving.Length; i++){
        if(clipMoving[i]){
          stopTransition = false;
        }
      }
      if(stopTransition) transitioning = false;
    }

    public void onEnter() {
      Debug.Log("RAILWAY SOUNDS START");
      transitioning = true;
      entering = true;
      clipMoving = new bool[] {true, true, true, true, true, true, true, true, true, true};
    }

    public void onExit() {
      transitioning = true;
      entering = false;
      clipMoving = new bool[] {true, true, true, true, true, true, true, true, true, true};
    }
}
