using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clicks : MonoBehaviour
{

    //private FMOD.Studio.EventInstance instance;
    //private string fmodEvent = "event:/Bosworth/AmbiClicks";
    // Start is called before the first frame update
    void Start()
    {
      //instance = FMODUnity.RuntimeManager.CreateInstance(fmodEvent);
      //instance.start();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void StartPlaying() {
      //instance.start();
      //Debug.Log("Playing Bosworth Clicks");
      //instance.setParameterByName("BosworthClicksVolume", 100);
      GetComponent<FMODUnity.StudioEventEmitter>().Play();
    }

    public void StopPlaying() {
      //instance.setParameterByName("BosworthClicksVolume", 0);
      //instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
      GetComponent<FMODUnity.StudioEventEmitter>().Stop();
    }
}
