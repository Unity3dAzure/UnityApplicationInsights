using System.Collections;
using System.Collections.Generic;
using UnityApplicationInsights;
using UnityEngine;
using UnityEngine.XR.WSA.Input;
//using csDelaunay; // Uncomment code to enable Voronoi Selection

[RequireComponent (typeof (Gaze))]
public class Tap : MonoBehaviour
{
  private GestureRecognizer recognizer;

  /// We are using a constant for the sphere cast radius.
  /// Increase this value if your objects are very far away from each other.
  [Tooltip ("Helper to handle taps that are close by an object"), Range (0.1f, 1.0f)]
  public float SphereCastRadius = 0.3f;

  void Awake ()
  {
    recognizer = new GestureRecognizer ();
    recognizer.Tapped += OnTapped;
    recognizer.StartCapturingGestures ();
    recognizer.SetRecognizableGestures (GestureSettings.Tap | GestureSettings.DoubleTap);
  }

  private void Update ()
  {
    /// Enable mouse to simulate tap event
    if (Input.GetMouseButtonDown (0))
    {
      AnalyseTap ();
    }
  }

  void OnDestroy ()
  {
    if (recognizer != null)
    {
      recognizer.Tapped -= OnTapped;
    }
  }

  private void OnTapped (TappedEventArgs obj)
  {
    if (Gaze.Instance.FocusedGameObject != null)
    {
      Debug.Log ("OnTapped:" + Gaze.Instance.FocusedGameObject.name + " Count:" + obj.tapCount + " Source:" + obj.source);
      AnalyseTap (obj.tapCount);
    }
    else
    {
      // User did not tap on an object, but we can check if any objects are near by...
      AnalyseTap ();
    }
  }

  private void AnalyseTap (int taps = 1)
  {
    if (Gaze.Instance == null)
    {
      Debug.LogError ("Gaze required to get current target tapped!");
      return;
    }
    if (Gaze.Instance.FocusedGameObject != null)
    {
      Debug.Log ("Tapped:" + Gaze.Instance.FocusedGameObject.name + " Count:" + taps);
      // Change color when tapped
      ChangeTargetColour (Gaze.Instance.FocusedGameObject, Color.cyan);
      // Log tap hologram event
      RecordTappedObjectEvent (Gaze.Instance.FocusedGameObject.name, taps);
    }
    else
    {
      // Air Tapped in empty space.
      // Maybe the user missed the target - is the hit area hard to tap?
      // We might want to log missed clicks or capture nearest object or objects...
      var obj = GetClosestObject ();
      if (obj != null)
      {
        // Change color when tapped
        ChangeTargetColour (obj.gameObject, Color.blue);
        // Log empty tap near hologram event
        RecordTappedNoneEvent (obj.name, taps);
      }
    }
  }

  private void RecordTappedObjectEvent (string targetName, double value)
  {
    if (ApplicationInsights.Instance == null)
    {
      return;
    }
    ApplicationInsights.Instance.TrackEvent ("Tap", targetName, "Taps", value);
  }

  private void RecordTappedNoneEvent (string targetName, double value)
  {
    if (ApplicationInsights.Instance == null)
    {
      return;
    }
    ApplicationInsights.Instance.TrackEvent ("Tap-Near", targetName, "Taps", value);
  }

  private void ChangeTargetColour (GameObject target, Color selectedColor)
  {
    target.GetComponent<Renderer> ().material.color = selectedColor;
    StartCoroutine (ChangeTargetColourBack (target, Color.white));
  }

  IEnumerator ChangeTargetColourBack (GameObject target, Color originalColor)
  {
    yield return new WaitForSeconds (0.3f);
    target.GetComponent<Renderer> ().material.color = originalColor;
  }

  public Transform GetClosestObject ()
  {
    if (Gaze.Instance == null || Gaze.Instance.Root == null) return null;

    Transform hitObject = null;
    var origin = Gaze.Instance.GazeOrigin ();
    var direction = Gaze.Instance.GazeForward ();
    var maxDistance = Gaze.Instance.MaxGazeCollisionDistance;
    var layer = Gaze.Instance.Root.layer;
    // Bit shift the index of the layer to get a bit mask
    var layerMask = 1 << layer;
    // Cast a sphere to see what nodes are in radius of the head orientation
    var hitObjects = Physics.SphereCastAll (origin, SphereCastRadius, direction, maxDistance, layerMask);
    if (hitObjects.Length == 1)
    {
      // There is only one object in range, no need for Voronoi
      hitObject = hitObjects[0].transform;
    }
    else if (hitObjects.Length > 1)
    {
      // More than one object detected, using Voronoi selection to find closest one
      //hitObject = FindClosestObject(hitObjects); // Uncomment code to enable Voronoi Selection
    }
    else
    {
      Debug.Log ("Found nothing close on layer: " + layer + "\nTry increasing the 'SphereCastRadius' parameter.");
    }
    if (hitObject != null)
    {
      Debug.Log ("Tapped near object: " + hitObject.gameObject.name);
    }
    return hitObject;
  }

  #region Voronoi Selection
  /*

  /// <summary>
  /// Finds the closest hit object using Voronoi Selection
  /// </summary>
  /// <param name="hitObjects"></param>
  /// <returns></returns>
  public Transform FindClosestObject(RaycastHit[] hitObjects)
  {
      var points = new List<Vector2f>();
      foreach (var hit in hitObjects)
      {
          var screenCoordinates = GUICoordinatesWithObject(hit.collider.gameObject);
          points.Add(new Vector2f(screenCoordinates.x, screenCoordinates.y));
          Debug.LogFormat("- Object name: {0} xy: {1},{2} screen: {3},{4}", hit.transform.name, screenCoordinates.x, screenCoordinates.y, Screen.width, Screen.height);
      }
      int objectIndex = VoronoiSelection(points, new Vector2f(Screen.width / 2, Screen.height / 2));

      Debug.Log("nearest object index:" + objectIndex + "/" + hitObjects.Length);
      if (objectIndex > -1)
      {
          return hitObjects[objectIndex].transform;
      }
      return null;
  }

  public static Vector2 GUICoordinatesWithObject(GameObject go)
  {
      var renderer = go.GetComponent<Renderer>();
      if (renderer == null) return go.transform.position;
      Vector3 center = renderer.transform.position;
      return WorldToGUIPoint(center);
  }

  public static Vector2 WorldToGUIPoint(Vector3 world)
  {
      return Camera.main.WorldToScreenPoint(world);
  }

  /// <summary>
  /// Selects the closest node to the center point using the Voronoi diagram
  /// </summary>
  /// <param name="points"></param>
  /// <param name="centerPoint"></param>
  /// <returns></returns>
  public int VoronoiSelection(List<Vector2f> points, Vector2f centerPoint)
  {
      var bounds = new Rectf(0, 0, Screen.width, Screen.height);

      // There are two ways you can create the voronoi diagram: with or without the lloyd relaxation.
      // We do not want lloyd relaxation as it does not represent the real location of the 3D objects.
      Voronoi voronoi = new Voronoi(points, bounds);

      if (voronoi.Edges.Count < 1)
      {
          voronoi.Dispose();
          return -1;
      }

      // Returns the closest site based on a point.
      // The algorithm calculates the closest edge and picks the closest adjacent site of that edge.
      var closestSite = GetClosestSiteByPoint(centerPoint, voronoi.Edges);
      if (closestSite == null)
      {
          voronoi.Dispose();
          return -1;
      }

      // Clear resources
      voronoi.Dispose();

      // The site index does not match the initial points index, we need to check for the correct index.
      for (int i = 0; i < points.Count; i++)
      {
          if (points[i] == closestSite.Coord)
          {
              return i;
          }
      }

      return -1;
  }

  private Site GetClosestSiteByPoint(Vector2f point, List<Edge> edges)
  {
      var minDistance = float.MaxValue;
      var closestEdgeIndex = -1;
      for (int i = 0; i < edges.Count; i++)
      {
          // If clipped ends are null, it means the edge is outside the bounds.
          if (edges[i].ClippedEnds == null)
          {
              continue;
          }
          // Get the minimum distance between the point and edge
          // -1 is returned when the point projection does not lie on the edge
          var currentDistance = MinimumDistanceFromPointToEdge(edges[i], new Vector2(point.x, point.y));
          if (currentDistance != -1 && currentDistance < minDistance)
          {
              minDistance = currentDistance;
              closestEdgeIndex = i;
          }
      }

      if (closestEdgeIndex == -1)
      {
          return null;
      }

      // Get the adjacent sites and return the nearest one.
      var leftSite = edges[closestEdgeIndex].LeftSite;
      var rightSite = edges[closestEdgeIndex].RightSite;
      var distLeft = leftSite.Coord.DistanceSquare(point);
      var distRight = rightSite.Coord.DistanceSquare(point);

      return distLeft < distRight ? leftSite : rightSite;
  }

  private float MinimumDistanceFromPointToEdge(Edge line, Vector2 p)
  {
      var v = new Vector2(line.ClippedEnds[LR.LEFT].x, line.ClippedEnds[LR.LEFT].y);
      var w = new Vector2(line.ClippedEnds[LR.RIGHT].x, line.ClippedEnds[LR.RIGHT].y);

      var vf = new Vector2f(v.x, v.y);
      var wf = new Vector2f(w.x, w.y);
      var pf = new Vector2f(p.x, p.y);

      // Return minimum distance between line segment vw and point p
      float l2 = vf.DistanceSquare(wf);
      if (l2 == 0.0) return pf.DistanceSquare(vf);
      float t = Vector2.Dot((p - v), (w - v)) / l2;

      if (t < 0 || t > 1)
      {
          return -1;
      }

      // Projection falls on the segment
      Vector2 projection = v + t * (w - v);
      var prrojectionF = new Vector2f(projection.x, projection.y);
      return pf.DistanceSquare(prrojectionF);
  }

  //*/
  #endregion
}