using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class MaryGrant : MonoBehaviour
{
    [SerializeField]
    private CommentaryController commentaryController;

    [SerializeField]
    private GameObject Winds;

    [SerializeField]
    private GameObject AudioGuide;

    [SerializeField]
    private Visitor visitor;
    private float distance;

    private bool Transitioning = false;
    private bool Active = false;

    private DateTime timeStarted;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
      distance = Vector3.Distance(visitor.transform.position, transform.position);
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

      commentaryController.SetAudioGuide(AudioGuide);
      commentaryController.SetCurrentLocation(CommentaryController.Location.Mary);
    }

    void Exit(){
      Transitioning = true;
      Active = false;
    }

    void TransitionIn() {
      if(DateTime.Now > timeStarted.Add(new TimeSpan(0, 0, 5))){
        commentaryController.StartCommentary();
        Transitioning = false;
      }
      float speed = 1.5f * Time.deltaTime;
      Winds.transform.position = Vector3.MoveTowards(Winds.transform.position, visitor.transform.position, speed);
      /*float distance =  Vector3.Distance(Winds.transform.position, visitor.transform.position);
      if(distance < 0.2) {
        clipMoving[i] = false;
      }*/
    }

    void TransitionOut() {
      Transitioning = false;
    }
}
