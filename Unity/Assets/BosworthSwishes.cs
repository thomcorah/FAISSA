using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BosworthSwishes: MonoBehaviour
{

    [SerializeField]
    private Visitor visitor;

    [SerializeField]
    private GameObject swish1;

    [SerializeField]
    private GameObject swish2;

    [SerializeField]
    private GameObject swish3;

    [SerializeField]
    private GameObject swish4;

    [SerializeField]
    private GameObject swish5;

    private GameObject[] allSwishes;



    // Start is called before the first frame update
    void Start()
    {
      allSwishes = new GameObject[] {swish1, swish2, swish3, swish4, swish5};
    }

    public void StartPlaying(){
      foreach(GameObject swish in allSwishes) {
        swish.GetComponent<FMODUnity.StudioEventEmitter>().Play();
      }
    }

    public void StopPlaying(){
      foreach(GameObject swish in allSwishes) {
        swish.GetComponent<FMODUnity.StudioEventEmitter>().Stop();
      }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
