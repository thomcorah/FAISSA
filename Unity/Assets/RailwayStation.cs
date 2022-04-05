using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class RailwayStation : MonoBehaviour
{

  [SerializeField]
  private CommentaryController commentaryController;

  [SerializeField]
  private GameObject AudioGuide;

  [SerializeField]
  private GameObject Painting;

  [SerializeField]
  private RailwaySounds RailwaySounds;

  [SerializeField]
  private Visitor visitor;
  private float distance;

  private bool Transitioning = false;
  private bool Active = false;

  private float waitTime = 1f;

  private DateTime timeStarted;

    // Start is called before the first frame update
    void Start()
    {
      Debug.Log("RAILWAY STARTED");
    }

    // Update is called once per frame
    void Update()
    {
      distance = Vector3.Distance(visitor.transform.position, transform.position);
      //Debug.Log("Railway distance: " + distance);
      if(distance < 2 && !Active){
        Enter();
      }

      if(distance > 2.5 && Active) {
        Exit();
      }

      if(Active && Transitioning) {
        TransitionIn();
      } else if(!Active && Transitioning) {
        TransitionOut();
      }
    }

    void Enter(){
      Active = true;
      Transitioning = true;


      FMOD.Studio.EventInstance slidingDoor = FMODUnity.RuntimeManager.CreateInstance("event:/RailwayStation/SlidingDoor");
      FMODUnity.RuntimeManager.AttachInstanceToGameObject(slidingDoor, Painting.GetComponent<Transform>(), Painting.GetComponent<Rigidbody>());
      slidingDoor.start();
      FMODUnity.RuntimeManager.StudioSystem.setParameterByName("railwayentrance", 100);
      RailwaySounds.onEnter();
      Debug.Log("ENTERED RailwayStation");

      timeStarted = DateTime.Now;

      commentaryController.SetAudioGuide(AudioGuide);
      commentaryController.SetCurrentLocation(CommentaryController.Location.RailwayStation);
    }

    void Exit(){
      Debug.Log("EXIT");
      waitTime = 0.5f;
      Transitioning = true;
      Active = false;
      FMOD.Studio.EventInstance slidingDoor = FMODUnity.RuntimeManager.CreateInstance("event:/RailwayStation/SlidingDoor");
      FMODUnity.RuntimeManager.AttachInstanceToGameObject(slidingDoor, Painting.GetComponent<Transform>(), Painting.GetComponent<Rigidbody>());
      slidingDoor.start();
      RailwaySounds.onExit();

    }

    void TransitionIn() {
      if(DateTime.Now > timeStarted.Add(new TimeSpan(0, 0, 5))){
        commentaryController.StartCommentary();
        Transitioning = false;
      }

    }

    void TransitionOut() {
      waitTime = waitTime - Time.deltaTime;
      if(waitTime <= 0f){
        Debug.Log("FADE SOUNDS");
        FMODUnity.RuntimeManager.StudioSystem.setParameterByName("railwayentrance", 0);
        Transitioning = false;
      }
    }
}
