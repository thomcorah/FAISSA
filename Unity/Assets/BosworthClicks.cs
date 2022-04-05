using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BosworthClicks : MonoBehaviour
{

    [SerializeField]
    private Visitor visitor;

    [SerializeField]
    private GameObject click1;
    private Vector3 click1OuterPos = new Vector3(-7.22f, 2, 6.71f);

    [SerializeField]
    private GameObject click2;
    private Vector3 click2OuterPos = new Vector3(-4.33f, 2, -0.38f);

    [SerializeField]
    private GameObject click3;
    private Vector3 click3OuterPos = new Vector3(-0.84f, 2, -0.67f);

    [SerializeField]
    private GameObject click4;
    private Vector3 click4OuterPos = new Vector3(0.87f, 2, 5.88f);

    [SerializeField]
    private GameObject click5;
    private Vector3 click5OuterPos = new Vector3(-4.36f, 2, 9.49f);

    private GameObject[] allClicks;
    private bool[] clickDirections = {true, false, false, true, false};
    private Vector3[] clickStartPositions;



    // Start is called before the first frame update
    void Start()
    {
      allClicks = new GameObject[] {click1, click2, click3, click4, click5};
      clickStartPositions = new Vector3[] {click1OuterPos, click2OuterPos, click3OuterPos, click4OuterPos, click5OuterPos};
    }

    public void StartPlaying(){
      foreach(GameObject click in allClicks) {
        click.GetComponent<FMODUnity.StudioEventEmitter>().Play();
      }
    }

    public void StopPlaying(){
      foreach(GameObject click in allClicks) {
        click.GetComponent<FMODUnity.StudioEventEmitter>().Stop();
      }
    }

    // Update is called once per frame
    void Update()
    {
      for(int i = 0; i < allClicks.Length; i++){
        float distance = Vector3.Distance(visitor.transform.position, allClicks[i].transform.position);
        float speed = 1f * Time.deltaTime;
        allClicks[i].transform.position = Vector3.MoveTowards(allClicks[i].transform.position, visitor.transform.position, speed);
        if(distance < 2) {
          allClicks[i].transform.position = clickStartPositions[i];
        }
      }
    }
}
