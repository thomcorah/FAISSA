using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Bosworth : MonoBehaviour
{

    [SerializeField]
    private CommentaryController commentaryController;

    [SerializeField]
    private Visitor visitor;
    private float distance;

    private bool Transitioning = false;
    private bool Active = false;
    private DateTime timeStarted;

    [SerializeField]
    private GameObject AudioGuide;

    [SerializeField]
    private BosworthRunning bosworthRuning;

    [SerializeField]
    private IceWalk iceWalk;

    [SerializeField]
    private Clicks clicks;



    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        distance = Vector3.Distance(visitor.transform.position, transform.position);
        if(distance < 1.5 && !Active){
          Enter();
        }

        if(distance > 2 && Active) {
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
      Debug.Log("Playing Bosworth");
      bosworthRuning.TransitionIn();
      iceWalk.TransitionIn();
      clicks.StartPlaying();

      timeStarted = DateTime.Now;

      commentaryController.SetAudioGuide(AudioGuide);
      commentaryController.SetCurrentLocation(CommentaryController.Location.Bosworth);

    }

    void Exit() {
      Transitioning = true;
      Active = false;
      Debug.Log("Stopping Bosworth");
      bosworthRuning.TransitionOut();
      iceWalk.TransitionOut();
      clicks.StopPlaying();
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
