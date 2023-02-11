/**
 *  \class Visitor
 *  The script attached to the object representing the location and orientation of the user within the gallery space.  
 *
 *  The Visitor Game Object represents the user within the virtual gallery space. The principle task of this script
 *  is to move the Visitor object within the virtual space. 
 *  
 *  When played in the Unity Editor, this is achieved by responding to key presses (arrow keys) and mouse movement.
 *  The keyboard arrow keys create movement, while the mouse controls rotation around the Y axis.
 *
 *  When played on a device, this script consumes location data from the IndoorAtlas API, and maps this to 
 *  the World space coordinates of this virtual environment.
 *
 *  In addition, when played on a device, the device itself can be tilted in order to simulate a change in 
 *  positioning data.
 *
 *  This class is also responsible for starting the clip of audio commentary that first welcomes a user to the 
 *  gallery, if they've not already heard it, via the Commentary Controller.
 */

using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using IndoorAtlas;

public class Visitor : MonoBehaviour
{
    /** 
     *  A reference to the CommentaryController class, responsible for controlling 
     *  playback and interaction with the spoken commentary.
     */
    [SerializeField]
    private CommentaryController commentaryController;

    /** 
     *  A reference to the GameObject to which the audio commentary will be attached. This AudioGuide GameObject is
     *  kept close to the Visitor GameObject, representing a guide walking with you in the gallery.
     */
    [SerializeField]
    private GameObject audioGuide;

    /** 
     *  commentaryStarted is a boolean to track if the commentary has started or not.
     */
    private bool commentaryStarted = false;

    /** 
     *  rb will be used as a reference to the Rigidbody component of the Visitor GameObject, used
     *  in the movement of the Visitor GameObject.
     */
    private Rigidbody rb;

    /** 
     *  mouseSensitivity (serialized to appear as settings in the Unity UI) is a
     *  parameter used when calculating rotation.
     */
    [SerializeField]
    float mouseSensitivity = 1.0f;

    /** 
     *  movementSpeed (serialized to appear as settings in the Unity UI) is a
     *  parameter used when calculating movement.
     */
    [SerializeField]
    float movementSpeed = 20.0f;

    /** 
     *  timeStarted is used to log the time when the Visitor GameObject is created. This allows
     *  for the commentary to be started after a defined delay.
     */
    private DateTime timeStarted;

    /**
     * latLong is the raw latitude and longitude supplied by the Indoor Atlas SDK.
     */
    private Vector2 latLong;

    /**
     *  eastNorth stores the distance of the position supplied by the Indoor Atlas SDK from a defined 
     *  origin point, in terms of metres north and metres east. 
     */
    private Vector2 eastNorth = new Vector2(0.0f, 0.0f);

    /**
     *  targetLocation is the Vector3 position in the world coordinate system derived from eastNorth.
     */
    private Vector3 targetLocation;
    
    /** 
     *  A simple on-screen text readout of the East and North distances is used for debugging. The string
     *  to be displayed in this message is stored in debugMsg.
     */
    GUIStyle style = new GUIStyle();

    /**
     *  debugMsg is the string that is displayed in the GUI message box.
     */
    string debugMsg;

    /**
     *  visitorRotationAdjustment allows for an adjustment of the rotation about the Y axis supplied by the 
     *  BOSE AR SDK. This allows for compensation of the orientation drift that can occur over time. 
     */
    private float visitorRotationAdjustment = 0.0f;

    /** 
     *  Start() is run once when the Visitor GameObect is initialised. 
     *
     *  It: 
     *  - assigns the Visitor GameObject's Rigidbody component to the rb property.
     *  - records the current time in timeStarted.
     *  - passes the audioGuide GameObject to the Commentary Controller to be used as the source of audio commentary
     *  - tells the Commentary Controller that the current location is the Gallery (as opposed to a painting). The 
     *    Commentary Controller uses this to play the correct commentary.
     *  - Initialises the debugMsg with "DEBUG", and sets an appropriate font size for the GUI text.
     */
    private void Start()
    {
        rb = this.GetComponent<Rigidbody>();

        timeStarted = DateTime.Now;

        commentaryController.SetAudioGuide(audioGuide);
        commentaryController.SetCurrentLocation(CommentaryController.Location.Gallery);

        debugMsg = "";
        style.fontSize = 60;
    }

    /** 
     *  OnGUI() is run whenever the GUI needs updating. It redraws the simple text display with the current
     *  value of debugMsg.
     */
    void OnGUI(){
      GUI.Box(new Rect(0, 200, Screen.width, Screen.height), debugMsg, style);
    }

    /** 
     *  Update() is called once per frame.
     *
     *  It is responsible for:
     *  - Starting the audio commentary 5 seconds after initialisation
     *  - Handling movement and rotation of the Visitor GameObject via the mouse and keyboard when being played
     *    in the Unity editor.
     *  - Enabling control via mobile device tilting when being run on a mobile device.
     *  - Moving the Visitor GameObject towards the targetLocation (derived from indoor positioning system or device tilting)
     *    when being run on a mobile device.
     *  - Keeping the audio guide a consistent distance from the Visitor GameObject.
     */
    void Update()
    {
      if(DateTime.Now > timeStarted.Add(new TimeSpan(0, 0, 5)) && !commentaryController.GalleryIntroPlayed && !commentaryStarted){
        commentaryStarted = true;
        commentaryController.StartCommentary();
      }

      Cursor.lockState = CursorLockMode.Locked;
      
      if(Application.isEditor) {
        float horizontal = Input.GetAxis("Horizontal") * movementSpeed * Time.deltaTime;
        float vertical = Input.GetAxis("Vertical") * movementSpeed * Time.deltaTime;
        rb.velocity = transform.TransformVector(new Vector3(horizontal * movementSpeed, 0, vertical * movementSpeed));

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
          dir.x = Input.acceleration.x; // Tilting phone side to side
          dir.z = Input.acceleration.y + 0.5f; // Tilting phone forwards and backwards.
          dir.y = 0;

          Debug.Log("dir.x: " + dir.x);
          Debug.Log("dir.z: " + dir.z);

          // clamp acceleration vector to the unit sphere
          if (dir.sqrMagnitude > 1)
              dir.Normalize();

          // Make it move 10 meters per second instead of 10 meters per frame...
          dir *= Time.deltaTime * movementSpeed;

          eastNorth.x = eastNorth.x + dir.z; // add forward/backwards tilt to target east location
          eastNorth.y = eastNorth.y + dir.x; // add side to side tilt to target north location

          debugMsg = "East: " + eastNorth.x + ". North: " + eastNorth.y;
          targetLocation = new Vector3(-eastNorth.y, 0.9f, eastNorth.x);

          Touch touch = Input.GetTouch(0);
          if(touch.position.x < (Screen.width / 2.0f)) {
            visitorRotationAdjustment-= 10f * Time.deltaTime;
          } else {
            visitorRotationAdjustment+= 10f * Time.deltaTime;
          }
        }
      }
      
      if(targetLocation != null && !Application.isEditor){
        //Debug.Log("LatLong: " + latLong);
        //Debug.Log("eastNorth: " + eastNorth);
        //Debug.Log("Target Location: " + targetLocation);
        float step =  1.0f * Time.deltaTime; // calculate distance to move
        transform.position = Vector3.MoveTowards(transform.position, targetLocation, step);
      }

      Vector3 pos = transform.position;
      pos.y = 0.9f;
      transform.position = pos;

      Vector3 nrotation;
        nrotation.z = 0f;
        nrotation.x = 0f;
        nrotation.y = visitorRotationAdjustment;

        transform.Rotate(nrotation);

      audioGuide.transform.position = new Vector3(transform.position.x + 2, 1.8f, transform.position.z + 2);

      //Debug.Log("Current loc: " + transform.position);
    }

    /** 
     *  IndoorAtlasOnLocationChanged() is part of the indoor positioning SDK. It is called whenever 
     *  IndoorAtlas received a location change notification, and is passed that location information.
     *
     *  latLong is given the latitude and longitude from IndoorAtlas. This is purely so that they can 
     *  be written to the debug log with other information in Update().
     *
     *  The IndoorAtlas SDK includes WGSConversion, providing a way to convert from absolute latitude and longitude
     *  values to an East/North distance from a provided origin point.
     *
     *  The virtual gallery room in Unity has been sized and rotated to match the real world. This makes translation 
     *  of this East/North distance relatively trivial. The origin has been set to be the top-left (North-West) corner of the 
     *  room as viewed in Unity. This is the case for the conversion from lat/long to East/North, as well as in the Unity environment 
     *  itself. This means that the East/North distance translates directly to game world coordinates, with the exception that the North/South
     *  axis in the game world is inverted.
     */
    void IndoorAtlasOnLocationChanged(IndoorAtlas.Location location) {
      // *** UNCOMMENT THIS TO USE TARGETLOCATION FROM INDOORATLAS SESSION
      
      /*
      Debug.Log("New Coordinates Received");
      latLong = new Vector2((float)location.position.coordinate.latitude, (float)location.position.coordinate.longitude);

      IndoorAtlas.WGSConversion temp = new IndoorAtlas.WGSConversion ();
      temp.SetOrigin(52.62888686, -1.12777658);

      eastNorth = temp.WGStoEN(location.position.coordinate.latitude, location.position.coordinate.longitude);

      float eastDif = eastNorth.x;// + 2.0f;
      float northDif = eastNorth.y;// - 4f;

      debugMsg = "East: " + eastDif + ". North: " + northDif;
      
      targetLocation = new Vector3(-northDif, 0.9f, eastDif);
      */
    }

    /**
     *  IndoorAtlasOnStatusChanged() is part of the Indoor Atlas indoor positioning SDK.
     *
     *  It is not used here.
     */
    void IndoorAtlasOnStatusChanged(IndoorAtlas.Status serviceStatus) {
      Debug.Log(serviceStatus);
    }

    /**
     *  IndoorAtlasOnHeadingChanged() is part of the Indoor Atlas indoor positioning SDK.
     *
     *  It is not used here.
     */
    void IndoorAtlasOnHeadingChanged(IndoorAtlas.Heading heading) {
      //Debug.Log(heading);
    }

    /**
     *  IndoorAtlasOnOrientationChanged() is part of the Indoor Atlas indoor positioning SDK.
     *
     *  It is not used here.
     */
    void IndoorAtlasOnOrientationChanged(Quaternion rotation) {
      //Debug.Log(rotation);
    }

    /**
     *  IndoorAtlasOnEnterRegion() is part of the Indoor Atlas indoor positioning SDK.
     *
     *  It is not used here.
     */
    void IndoorAtlasOnEnterRegion() {

    }

    /**
     *  IndoorAtlasOnExitRegion() is part of the Indoor Atlas indoor positioning SDK.
     *
     *  It is not used here.
     */
    void IndoorAtlasOnExitRegion() {

    }

    /**
     *  IndoorAtlasOnRoute() is part of the Indoor Atlas indoor positioning SDK.
     *
     *  It is not used here.
     */
    void IndoorAtlasOnRoute() {

    }



}
