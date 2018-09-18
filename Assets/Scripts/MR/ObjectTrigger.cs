using UnityApplicationInsights;
using UnityEngine;

public class ObjectTrigger : MonoBehaviour
{
    [Range(0.1f,4.0f)]
    public float ColliderRadius = 1.0f;
    /// <summary>
    /// Container for holograms
    /// </summary>
    public GameObject Root;

    private void Start()
    {
        // Add the Collider and Rigidbody components, and set their respective settings. This allows for collision.
        var collider = gameObject.AddComponent<SphereCollider>();
        collider.isTrigger = true;
        collider.radius = ColliderRadius;
        
        var rigidbody = gameObject.AddComponent<Rigidbody>();
        rigidbody.useGravity = false;
        rigidbody.isKinematic = true;
    }

    /// <summary>
    /// Triggered when an object with a collider enters this objects trigger collider.
    /// </summary>
    /// <param name="collision">Collided Object</param>
    private void OnTriggerEnter(Collider collider)
    {
        CompareTriggerEvent(collider, true);
    }

    /// <summary>
    /// Triggered when an object with a collider exits this objects trigger collider.
    /// </summary>
    /// <param name="collision">Collided Object</param>
    private void OnTriggerExit(Collider collider)
    {
        CompareTriggerEvent(collider, false);
    }

    /// <summary>
    /// Method for providing debug message, and sending event information to InsightsTracker.
    /// </summary>
    /// <param name="other">Collided Object</param>
    /// <param name="enter">Enter = true, Exit = False</param>
    private void CompareTriggerEvent(Collider collider, bool enter)
    {
        // Only looking for objects with the correct root game object.
        if (collider.transform.root.gameObject == Root)
        {
            // Change color when in proximity
            collider.gameObject.GetComponent<Renderer>().material.color = enter ? Color.green : Color.white;

            if (enter == true)
            {
                RecordProximityEvent(collider.gameObject.name);
            }
            Debug.LogFormat("User is{0}near <b>{1}</b>", enter == true ? " " : " no longer ", collider.gameObject.name);
        }
    }

    private void RecordProximityEvent(string targetName)
    {
        if (ApplicationInsights.Instance == null)
        {
            return;
        }
        ApplicationInsights.Instance.TrackEvent("Near", targetName);
    }
}