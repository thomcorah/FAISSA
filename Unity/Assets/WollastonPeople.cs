using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WollastonPeople : MonoBehaviour
{
    [SerializeField]
    private Visitor visitor;

    [SerializeField]
    private GameObject William;
    private Vector3 WilliamPos = new Vector3(0f, 0f, 0f);

    [SerializeField]
    private GameObject Anne;
    private Vector3 AnnePos = new Vector3(-1f, 0f, -2.5f);

    [SerializeField]
    private GameObject Cathryn;
    private Vector3 CathrynPos = new Vector3(-2f, 0f, -1.5f);

    [SerializeField]
    private GameObject Elizabeth;
    private Vector3 ElizabethPos = new Vector3(-1.5f, 0f, -2f);

    [SerializeField]
    private GameObject Francis;
    private Vector3 FrancisPos = new Vector3(1f, 0f, 2f);

    [SerializeField]
    private GameObject Nicholas;
    private Vector3 NicholasPos = new Vector3(-1f, 0f, 2.5f);

    private GameObject[] clips;
    private Vector3[] endPositions;
    private bool[] clipMoving = {false, false, false, false, false};

    private bool transitioning = false;
    private bool entering = true;





    // Start is called before the first frame update
    void Start()
    {

      clips = new GameObject[] {Anne, Cathryn, Elizabeth, Francis, Nicholas};
      endPositions = new Vector3[] {AnnePos, CathrynPos, ElizabethPos, FrancisPos, NicholasPos};
      WilliamPos = transform.TransformPoint(WilliamPos);
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
            clips[i].transform.position = Vector3.MoveTowards(clips[i].transform.position, WilliamPos, speed);
            float distance =  Vector3.Distance(clips[i].transform.position, WilliamPos);
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
      transitioning = true;
      entering = true;
      clipMoving = new bool[] {true, true, true, true, true};
    }

    public void onExit() {
      transitioning = true;
      entering = false;
      clipMoving = new bool[] {true, true, true, true, true};
    }
}
