using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using IndoorAtlas;


public class Visitor : MonoBehaviour
{

    [SerializeField]
    private CommentaryController commentaryController;

    [SerializeField]
    private GameObject AudioGuide;
    private bool commentaryStarted = false;

    private Rigidbody rb;

    [SerializeField]
    float mouseSensitivity = 1.0f;

    [SerializeField]
    float MovementSpeed =10.0f;

    private DateTime timeStarted;

    private Vector2 realWorldOrigin = new Vector2(52.62883415f, -1.1403311f);
    private Vector2 realWorldBL = new Vector2(52.62884831f, -1.4043638f);
    private Vector2 realWorldTL = new Vector2(52.62890957f, -1.4029154f);
    private float angleDif = 0f;
    private float latScaler;
    private float longScaler;

    private Vector3 targetLocation;

    private float realWorldWidth = 8.0f;
    private float realWorldLength = 11.9f;

    GUIStyle style = new GUIStyle();
    string debugMsg;

    private void Start()
    {
        rb = this.GetComponent<Rigidbody>();

        timeStarted = DateTime.Now;

        commentaryController.SetAudioGuide(AudioGuide);
        commentaryController.SetCurrentLocation(CommentaryController.Location.Gallery);

        float latDif = realWorldTL.x - realWorldBL.x;
        float longDif = realWorldTL.y - realWorldBL.y;
        angleDif = Mathf.Atan(longDif / latDif);
        Debug.Log("AngleDif: " + angleDif);

        latScaler = 20f / realWorldLength;
        longScaler = 10f /realWorldWidth;

        Debug.Log("LatScaler: " + latScaler);
        Debug.Log("LongScaler: " + longScaler);

        debugMsg = "DEBUG";
        style.fontSize = 60;
    }

    void OnGUI(){
      //GUI.Box(new Rect(0, 200, Screen.width, Screen.height), debugMsg, style);
    }

    // Update is called once per frame
    void Update()
    {
      if(DateTime.Now > timeStarted.Add(new TimeSpan(0, 0, 5)) && !commentaryController.GalleryIntroPlayed && !commentaryStarted){
        Debug.Log("Starting Gallery Commentary from Visitor");
        commentaryStarted = true;
        commentaryController.StartCommentary();
      }



      Cursor.lockState = CursorLockMode.Locked;


      if(Application.isEditor) {
        float horizontal = Input.GetAxis("Horizontal") * MovementSpeed;
        float vertical = Input.GetAxis("Vertical") * MovementSpeed;
        rb.velocity = new Vector3(horizontal * MovementSpeed, 0, vertical * MovementSpeed);

        float rotAmountX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float rotAmountY = Input.GetAxis("Mouse Y") * mouseSensitivity;


        Vector3 rotation;
        rotation.z = 0;


        rotation.x = -rotAmountY;

        rotation.y = rotAmountX;

        transform.Rotate(rotation);
        Vector3 playerRotation = transform.eulerAngles;
        playerRotation.z = 0.0f;
        transform.eulerAngles = playerRotation;

      } else {
        if(Input.touchCount > 0) {
          Vector3 dir = Vector3.zero;
          // we assume that the device is held parallel to the ground
          // and the Home button is in the right hand

          // remap the device acceleration axis to game coordinates:
          // 1) XY plane of the device is mapped onto XZ plane
          // 2) rotated 90 degrees around Y axis

          dir.x = Input.acceleration.x;
          dir.z = Input.acceleration.y + 0.5f;
          dir.y = 0;

          // clamp acceleration vector to the unit sphere
          if (dir.sqrMagnitude > 1)
              dir.Normalize();

          // Make it move 10 meters per second instead of 10 meters per frame...
          dir *= Time.deltaTime;

          // Move object
          transform.Translate(dir * MovementSpeed);
        }
      }


      if(targetLocation != null && !Application.isEditor){
        float step =  1.0f * Time.deltaTime; // calculate distance to move
        transform.position = Vector3.MoveTowards(transform.position, targetLocation, step);
      }

      Vector3 pos = transform.position;
      pos.y = 0.9f;
      // Rotate the cube by converting the angles into a quaternion.
        //Quaternion target = Quaternion.Euler(0, 27, 0);

        // Dampen towards the target rotation
        //transform.rotation = Quaternion.Slerp(transform.rotation, target,  Time.deltaTime * 5.0f);
      transform.position = pos;

      AudioGuide.transform.position = new Vector3(transform.position.x + 2, 1.8f, transform.position.z + 2);

      //debugMsg = "East: " + transform.position.x;
    }

    void IndoorAtlasOnLocationChanged(IndoorAtlas.Location location) {

      Debug.Log("ORIGINAL. Longitude: " + location.position.coordinate.longitude + ". Latitude: " + location.position.coordinate.latitude);

      IndoorAtlas.WGSConversion temp = new IndoorAtlas.WGSConversion ();
      temp.SetOrigin(52.62828928, -1.14032121);

      Vector2 eastNorth = temp.WGStoEN (location.position.coordinate.latitude, location.position.coordinate.longitude);
      Debug.Log ("East-North distance: " + eastNorth.x + ", " + eastNorth.y);

      float eastDif = eastNorth.x + 2.0f;//(float)location.position.coordinate.latitude - realWorldOrigin.x;
      float northDif = eastNorth.y - 4f;//(float)location.position.coordinate.longitude - realWorldOrigin.y;

      /*
      Debug.Log("AngleDif: " + angleDif);
      float sinAngleDif = Mathf.Sin(angleDif);
      float cosAngleDif = Mathf.Cos(angleDif);

      Debug.Log("s: " + sinAngleDif + " c: " + cosAngleDif);

      float newEast = eastDif * cosAngleDif - northDif * sinAngleDif;
      float newNorth = eastDif * sinAngleDif - northDif * cosAngleDif;

      Debug.Log("newEast: " + newEast + " newNorth: " + newNorth);

      newEast *= longScaler;
      newNorth *= latScaler;
      */
      /*

      Debug.Log("ORIGINAL Distance East: " + latDif + " North: " + longDif);

      float realWorldAngleFromOrigin = Mathf.Atan(latDif / longDif);
      Debug.Log("Real world angle from origin: " + realWorldAngleFromOrigin);

      float realWorldDistanceFromOrigin = Mathf.Sqrt((latDif * latDif) + (longDif * longDif));
      Debug.Log("Real world distance from origin: " + realWorldDistanceFromOrigin);

      float gameWorldAngleFromOrigin = realWorldAngleFromOrigin - angleDif;
      Debug.Log("Game world angle from origin: " + gameWorldAngleFromOrigin);

      float adjustedLatDif = Mathf.Cos(gameWorldAngleFromOrigin) * realWorldDistanceFromOrigin;
      float adjustedLongDif = Mathf.Sin(gameWorldAngleFromOrigin) * realWorldDistanceFromOrigin;
      Debug.Log("Rotated East: " + adjustedLatDif + " North: " + adjustedLongDif);
      float gameWorldLatDif = adjustedLatDif * latScaler;
      float gameWorldLongDif = adjustedLongDif * longScaler;

      Debug.Log("GAME COORD. East: " + gameWorldLongDif + ". North: " + gameWorldLatDif);

      */

      Debug.Log("GAME COORD. East: " + eastDif + ". North: " + northDif);
      debugMsg = "East: " + eastDif + ". North: " + northDif;
      Debug.Log("CURRENT Loc: " + transform.position);

      targetLocation = new Vector3(eastDif, 0.9f, northDif);


    }

    void IndoorAtlasOnStatusChanged(IndoorAtlas.Status serviceStatus) {
      Debug.Log(serviceStatus);
    }

    void IndoorAtlasOnHeadingChanged(IndoorAtlas.Heading heading) {
      //Debug.Log(heading);
    }

    void IndoorAtlasOnOrientationChanged(Quaternion rotation) {
      //Debug.Log(rotation);
    }

    void IndoorAtlasOnEnterRegion() {

    }

    void IndoorAtlasOnExitRegion() {

    }

    void IndoorAtlasOnRoute() {

    }



}
