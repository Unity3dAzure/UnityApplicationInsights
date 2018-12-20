using System;
using UnityApplicationInsights;
using UnityEngine;
using UnityEngine.Rendering;

public class Gaze : MonoBehaviour
{
  /// <summary>
  /// Provides Singleton-like behaviour to this class.
  /// </summary>
  public static Gaze Instance;

  /// <summary>
  /// Provides a reference to the object the user is currently looking at.
  /// </summary>
  public GameObject FocusedGameObject { get; private set; }

  /// <summary>
  /// Provides whether an object has been successfully hit by the raycast.
  /// </summary>
  public bool Hit { get; private set; }

  /// <summary>
  /// Provides a reference to compare whether the user is still looking at the same object (and has not looked away).
  /// </summary>
  private GameObject _oldFocusedObject = null;

  /// <summary>
  /// Max Ray Distance
  /// </summary>
  [Range (4.0f, 100.0f)]
  public float MaxGazeCollisionDistance = 15;

  private float _gazeTimeCounter = 0;

  /// <summary>
  /// The cursor object will be created when the app is running, this will store its values.
  /// </summary>
  private RaycastHit _hitInfo;
  private GameObject _cursor;
  private Vector3 _cursorPosition = new Vector3 (0.0f, 0.0f, 5.0f);
  private Vector3 _cursorSize = new Vector3 (0.02f, 0.02f, 0.02f);
  private Vector3 _cursorSizeHover = new Vector3 (0.01f, 0.01f, 0.01f);

  /// <summary>
  /// Container for holograms
  /// </summary>
  public GameObject Root;

  void Awake ()
  {
    // Set this class to behave similar to singleton
    Instance = this;
    _cursor = CreateCursor ();
  }

  void Start ()
  {
    FocusedGameObject = null;
  }

  void Update ()
  {
    // Set the old focused gameobject.
    _oldFocusedObject = FocusedGameObject;

    // Check whether raycast has hit.
    Hit = Physics.Raycast (Camera.main.transform.position, Camera.main.transform.forward, out _hitInfo, MaxGazeCollisionDistance);
    if (Hit == true)
    {
      // Check whether the hit has a collider.
      if (_hitInfo.collider != null)
      {
        // Set the focused object with what the user just looked at.
        FocusedGameObject = _hitInfo.collider.gameObject;

        // Lerp the cursor to the hitpoint
        _cursor.transform.position = new Vector3 (_hitInfo.point.x, _hitInfo.point.y, _hitInfo.point.z - _cursor.transform.localScale.z);
        CursorRollover ();
      }
      else
      {
        // Object looked on is not valid, set focused gameobject to null.
        FocusedGameObject = null;
        CursorRollout ();
      }
    }
    else
    {
      // No object looked upon, set focused gameobject to null.
      FocusedGameObject = null;
      CursorRollout ();
    }

    // Reset to default cursor position if no hit detected
    if (FocusedGameObject == null)
    {
      _cursor.transform.localPosition = _cursorPosition;
    }

    // Check whether the previous focused object is this same object. If so, reset the focused object.
    if (FocusedGameObject != _oldFocusedObject)
    {
      ResetFocusedObject ();
    }
    // If they are the same, but are null, reset the counter.
    else if (FocusedGameObject == null && _oldFocusedObject == null)
    {
      _gazeTimeCounter = 0;
    }
    // Count whilst the user continues looking at the same object.
    else
    {
      _gazeTimeCounter += Time.deltaTime;
    }
  }

  /// <summary>
  /// Create a cursor object, to provide what the user is looking at.
  /// </summary>
  /// <returns></returns>
  private GameObject CreateCursor ()
  {
    GameObject newCursor = GameObject.CreatePrimitive (PrimitiveType.Sphere);
    // Remove the collider, so it doesn't block raycast.
    Destroy (newCursor.GetComponent<SphereCollider> ());
    newCursor.name = "cursor";
    newCursor.transform.localScale = _cursorSize;
    newCursor.transform.position = _cursorPosition;
    newCursor.transform.parent = gameObject.transform;
    var renderer = newCursor.GetComponent<MeshRenderer> ();
    renderer.material.color = Color.blue;
    renderer.receiveShadows = false;
    renderer.shadowCastingMode = ShadowCastingMode.Off;
    return newCursor;
  }

  private void CursorRollout ()
  {
    _cursor.GetComponent<MeshRenderer> ().material.color = Color.grey;
    _cursor.transform.localScale = _cursorSize;
  }

  private void CursorRollover ()
  {
    _cursor.GetComponent<MeshRenderer> ().material.color = Color.blue;
    _cursor.transform.localScale = _cursorSizeHover;
  }

  /// <summary>
  /// Reset the old focused object, stop the gaze timer, and send data if it is greater than zero.
  /// </summary>
  public void ResetFocusedObject ()
  {
    // Ensure the old focused object is not null.
    if (_oldFocusedObject != null)
    {
      // Only looking for objects with the correct root game object.
      if (_oldFocusedObject.transform.root.gameObject == Root)
      {
        // Turn the timer into an int, and ensure that more than zero time has passed.
        double value = Math.Round (_gazeTimeCounter, 2);
        if (value > 0)
        {
          // Record the object gazed and duration of gaze for Analytics
          RecordGazeMetrics (_oldFocusedObject.name, value);
        }
        // Reset timer
        _gazeTimeCounter = 0;
      }
    }
  }

  private void RecordGazeMetrics (string targetName, double value)
  {
    if (ApplicationInsights.Instance == null)
    {
      return;
    }
    ApplicationInsights.Instance.TrackMetric ("Gaze", targetName, value);
  }

  public Vector3 GazeForward ()
  {
    return Camera.main.transform.forward;
  }

  public Vector3 GazeOrigin ()
  {
    return Camera.main.transform.position;
  }

}