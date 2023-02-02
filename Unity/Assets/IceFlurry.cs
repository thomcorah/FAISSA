/**
 *  \file IceFlurry.cs
 *  \class IceFlurry
 *
 *  The IceFlurry script is attached to each of the IceFlurry prefab Game Objects. 
 *
 *  These GameObjects are added to the scene at runtime by Bosworth. This script is 
 *  responsible for removing the IceFlurry GameObject after a short time.
 */
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class IceFlurry : MonoBehaviour
{
    /**
     *  timeStarted is set to the current time when IceFlurry GameObject
     *  is instantiated. It is used to be able to destroy the GameObject
     *  after a short time.
     */
    private DateTime timeStarted;

    /** 
     *  Start() is called before the first frame update
     *  
     *  It is used here to store the time this GameObject was instantiated in
     *  the timeStarted property.
     */
    void Start()
    {
        timeStarted = DateTime.Now;
    }

    /** 
     *  Update() is called once per frame.
     *
     *  It checks to see if three seconds have elapsed since this GameObject
     *  was instantiated. If so, the GameObject is destroyed. 
     */
    void Update()
    {
        if(DateTime.Now > timeStarted.Add(new TimeSpan(0, 0, 3))){
        Destroy(gameObject);
      }
    }
}
