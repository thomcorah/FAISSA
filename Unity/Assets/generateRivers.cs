using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class generateRivers : MonoBehaviour
{

    public GameObject riverPrefab;
    public float respawnTime = 1.0f;

    public IEnumerator coroutine;
    private List<GameObject> riverObjects = new List<GameObject>();

    private bool creatingRivers = false;

    private DateTime timeLastRiver;

    // Start is called before the first frame update
    void Start()
    {

      //StartCoroutine(riverWave());
      //riverObjects = new GameObject[10];
      coroutine = riverWave();
    }

    void Update(){
      if(DateTime.Now > timeLastRiver.Add(new TimeSpan(0, 0, 1)) && creatingRivers){
        spawnRiver();
      }
    }

    public void StartRiver() {
      timeLastRiver = DateTime.Now;
      creatingRivers = true;

      Debug.Log("StartRiver");
      GameObject r1 = Instantiate(riverPrefab) as GameObject;

      Vector3 r1Pos = new Vector3(20, -2, -3);
      r1.transform.position = transform.TransformPoint(r1Pos);
      riverObjects.Add(r1);

      GameObject r2 = Instantiate(riverPrefab) as GameObject;
      Vector3 r2Pos = new Vector3(0, -2, 3);
      r2.transform.position = transform.TransformPoint(r2Pos);
      riverObjects.Add(r2);

      StartCoroutine(coroutine);
    }

    public void StopRiver() {
      Debug.Log("StopRiver");
      Debug.Log(riverObjects);
      creatingRivers = false;
      StopCoroutine(coroutine);
      Debug.Log(riverObjects);
      for(int i = 0; i < riverObjects.Count; i++) {
        Debug.Log("River object");
        Debug.Log(riverObjects[i]);
        if(riverObjects[i]){
          Rigidbody thisRiverObject = riverObjects[i].GetComponent<Rigidbody>();
          Vector3 vel = thisRiverObject.velocity;
          thisRiverObject.velocity = new Vector3(vel.x, 0f, 20f);
        }
      }
    }

    private void spawnRiver() {
      Debug.Log("RIVER");
      timeLastRiver = DateTime.Now;
      /*GameObject r = Instantiate(riverPrefab) as GameObject;
      Vector3 newPos = new Vector3(30, -2, -4);
      r.transform.position = transform.TransformPoint(newPos);
      riverObjects.Add(r);
      Debug.Log("RiverObjects: " + riverObjects);*/

    }

    public IEnumerator riverWave() {
      while(true){
        yield return new WaitForSeconds(respawnTime);
        spawnRiver();
      }
    }

}
