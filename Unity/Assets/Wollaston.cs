using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Wollaston : MonoBehaviour
{

  [SerializeField]
  private CommentaryController commentaryController;

  [SerializeField]
  private GameObject AudioGuide;

  [SerializeField]
  private GameObject Painting;

  [SerializeField]
  private WollastonPeople WollastonPeople;

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
      Debug.Log("WOllaston");
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
      Debug.Log("Enter Wollaston");
      Active = true;
      Transitioning = true;

      timeStarted = DateTime.Now;

      FMODUnity.RuntimeManager.StudioSystem.setParameterByName("WollastonAnne", 100);
      FMODUnity.RuntimeManager.StudioSystem.setParameterByName("WollastonCathryn", 100);
      FMODUnity.RuntimeManager.StudioSystem.setParameterByName("WollastonElizabeth", 100);
      FMODUnity.RuntimeManager.StudioSystem.setParameterByName("WollastonFrancis", 100);
      FMODUnity.RuntimeManager.StudioSystem.setParameterByName("WollastonNicholas", 100);

      WollastonPeople.onEnter();

      commentaryController.SetAudioGuide(AudioGuide);
      commentaryController.SetCurrentLocation(CommentaryController.Location.Wollaston);
    }

    void Exit(){
      Debug.Log("EXIT");
      Transitioning = true;
      Active = false;
      FMODUnity.RuntimeManager.StudioSystem.setParameterByName("WollastonAnne", 0);
      FMODUnity.RuntimeManager.StudioSystem.setParameterByName("WollastonCathryn", 0);
      FMODUnity.RuntimeManager.StudioSystem.setParameterByName("WollastonElizabeth", 0);
      FMODUnity.RuntimeManager.StudioSystem.setParameterByName("WollastonFrancis", 0);
      FMODUnity.RuntimeManager.StudioSystem.setParameterByName("WollastonNicholas", 0);

      WollastonPeople.onExit();
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
