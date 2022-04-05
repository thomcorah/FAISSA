using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class TheGirl : MonoBehaviour
{

  [SerializeField]
  private CommentaryController commentaryController;

  [SerializeField]
  private GameObject AudioGuide;

  [SerializeField]
  private GameObject Painting;


  [SerializeField]
  private Visitor visitor;
  private float distance;

  private bool Transitioning = false;
  private bool Active = false;

  private DateTime timeStarted;

    // Start is called before the first frame update
    void Start()
    {
      Debug.Log("THE GIRL STARTED");
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

      timeStarted = DateTime.Now;

      FMODUnity.RuntimeManager.StudioSystem.setParameterByName("TheGirlEntrance", 100);

      commentaryController.SetAudioGuide(AudioGuide);
      commentaryController.SetCurrentLocation(CommentaryController.Location.TheGirl);
    }

    void Exit(){
      Debug.Log("EXIT");
      FMODUnity.RuntimeManager.StudioSystem.setParameterByName("TheGirlEntrance", 0);
      Transitioning = true;
      Active = false;
    }

    void TransitionIn() {
      if(DateTime.Now > timeStarted.Add(new TimeSpan(0, 0, 5))){
        commentaryController.StartCommentary();
        Transitioning = false;
      }

    }

    void TransitionOut() {

        Transitioning = false;

    }
}
