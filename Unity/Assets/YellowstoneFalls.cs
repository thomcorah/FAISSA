using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class YellowstoneFalls : MonoBehaviour
{

    [SerializeField]
    private Visitor visitor;
    private float distance;

    [SerializeField]
    private CommentaryController commentaryController;

    [SerializeField]
    private GameObject YellowstoneRoom;
    private GameObject roomInstance;

    [SerializeField]
    private generateRivers rivers;

    [SerializeField]
    private distantTrombone trombone;

    [SerializeField]
    private GameObject waterfall;
    private float waterfallStartDistance = 0.0f;
    private float waterfallVelocity = -20;
    private Vector3 waterfallEndPoint = new Vector3(-30f, 0f, 0f);

    private bool Transitioning = false;
    private bool Active = false;
    private DateTime timeStarted;

    [SerializeField]
    private GameObject AudioGuide;



    // Start is called before the first frame update
    void Start()
    {
      waterfallEndPoint = transform.TransformPoint(waterfallEndPoint);
    }

    // Update is called once per frame
    void Update()
    {

        distance = Vector3.Distance(visitor.transform.position, transform.position);
        if(distance < 2 && !Active){
          Enter();
        }

        if(distance > 2.5 && Active) {
          Debug.Log("Calling exit on Yellowstone Falls");
          Exit();
        }

        if(Active && Transitioning) {
          TransitionIn();
        } else if(!Active && Transitioning) {
          TransitionOut();
        }

    }

    void Enter() {
      Active = true;
      Transitioning = true;
      Debug.Log("Playing Yellowstone Falls");

      roomInstance = Instantiate(YellowstoneRoom) as GameObject;
      roomInstance.transform.position = new Vector3(0, 0, 0);

      timeStarted = DateTime.Now;

      //waterfall.GetComponent<FMODUnity.StudioEventEmitter>().SetParameter("Mode", 1, false);
      //waterfall.GetComponent<Rigidbody>().velocity = new Vector3(waterfallVelocity, 0, 0);
      //waterfallStartDistance = waterfall.GetComponent<Rigidbody>().transform.position.x;

      rivers.StartRiver();

      trombone.StartPlaying();

      commentaryController.SetAudioGuide(AudioGuide);
      commentaryController.SetCurrentLocation(CommentaryController.Location.Yellowstone);

    }

    void Exit() {
      Transitioning = true;
      Active = false;
      Debug.Log("Stopping Yellowstone Falls");
      rivers.StopRiver();
      trombone.StopPlaying();
    }

    void TransitionIn() {
      float speed = 3f * Time.deltaTime;
      Rigidbody rb = waterfall.GetComponent<Rigidbody>();
      float dist = Vector3.Distance(waterfall.transform.position, visitor.transform.position);
      waterfall.GetComponent<FMODUnity.StudioEventEmitter>().SetParameter("WaterfallDistance", dist, false);
      //Debug.Log("Distance: " + xDelta);
      //waterfall.transform.position = Vector3.MoveTowards(waterfall.transform.position, waterfallEndPoint, speed);
      float distance =  Vector3.Distance(waterfall.transform.position, waterfallEndPoint);


      if(DateTime.Now > timeStarted.Add(new TimeSpan(0, 0, 5))){
        commentaryController.StartCommentary();
        Transitioning = false;
      }


    }

    void TransitionOut() {
      waterfall.GetComponent<Rigidbody>().velocity = new Vector3(-waterfallVelocity, 0, 0);
      Rigidbody rb = waterfall.GetComponent<Rigidbody>();
      float xDelta = Mathf.Abs(rb.transform.position.x - waterfallStartDistance);
      waterfall.GetComponent<FMODUnity.StudioEventEmitter>().SetParameter("WaterfallDistance", xDelta + 6.0f, false);
      if(rb.transform.position.x > -3){
        Debug.Log("End of Yellowstone Transition out");
        rb.velocity = new Vector3(0, 0, 0);
        Transitioning = false;
        waterfall.GetComponent<FMODUnity.StudioEventEmitter>().SetParameter("Mode", 0, false);
        Destroy(roomInstance);
      }
    }

}
