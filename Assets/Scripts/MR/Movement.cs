using UnityEngine;
#if UNITY_WSA
using UnityEngine.XR.WSA.Input;
#endif

public class Movement : MonoBehaviour
{
  /// <summary>
  /// The rendered object representing the right controller.
  /// </summary>
  public GameObject Controller;

  /// <summary>
  /// The movement speed of the user.
  /// </summary>
  [Range (1, 10)]
  public float UserSpeed = 5.0f;

  /// <summary>
  /// Provides whether source updates have been registered.
  /// </summary>
  private bool _isAttached = false;

  /// <summary>
  /// Used to calculate and proposes movement translation.
  /// </summary>
  private Vector3 _playerMovementTranslation;

  void Update ()
  {
#if UNITY_WSA && UNITY_2017_2_OR_NEWER
    UpdateControllerState ();
#endif
  }

  #region UNITY_WSA

#if UNITY_WSA && UNITY_2017_2_OR_NEWER

  /// <summary>
  /// Used for tracking the current position and rotation of the controller.
  /// </summary>
  private void UpdateControllerState ()
  {
    // Check for current connected controllers, only if WSA.
    if (InteractionManager.GetCurrentReading ().Length > 0)
    {
      foreach (var sourceState in InteractionManager.GetCurrentReading ())
      {
        if (sourceState.source.kind == InteractionSourceKind.Controller)
        {
          // If a controller source is found, which matches the selected handness,
          // check whether interaction source updated events have been registered.
          if (_isAttached == false)
          {
            // Register events, as not yet registered.
            Debug.Log ("<color=green>Source Found: Registering Controller Source Events</color>");
            _isAttached = true;
            InteractionManager.InteractionSourceUpdated += InteractionManager_InteractionSourceUpdated;
          }

          // Update the position and rotation information for the controller.
          Vector3 newPosition;
          if (sourceState.sourcePose.TryGetPosition (out newPosition, InteractionSourceNode.Pointer) && ValidPosition (newPosition))
          {
            Controller.transform.localPosition = newPosition;
          }

          Quaternion newRotation;
          if (sourceState.sourcePose.TryGetRotation (out newRotation, InteractionSourceNode.Pointer) && ValidRotation (newRotation))
          {
            Controller.transform.localRotation = newRotation;
          }
        }
      }
    }
  }

  /// <summary>
  /// This registered event is triggered when a source state has been updated.
  /// </summary>
  /// <param name="obj"></param>
  private void InteractionManager_InteractionSourceUpdated (InteractionSourceUpdatedEventArgs obj)
  {
    float thumbstickY = obj.state.thumbstickPosition.y;

    // Vertical Input
    if (thumbstickY > 0.3f || thumbstickY < -0.3f)
    {
      _playerMovementTranslation = Camera.main.transform.forward;
      _playerMovementTranslation.y = 0;
      transform.Translate (_playerMovementTranslation * UserSpeed * Time.deltaTime * thumbstickY, Space.World);
      Debug.Log ("User Movement");
    }
  }

#endif

  #endregion

  /// <summary>
  /// Check that controller position is valid.
  /// </summary>
  /// <param name="inputVector3">The Vector3 to check</param>
  /// <returns>The position is valid</returns>
  private bool ValidPosition (Vector3 inputVector3)
  {
    return !float.IsNaN (inputVector3.x) && !float.IsNaN (inputVector3.y) && !float.IsNaN (inputVector3.z) && !float.IsInfinity (inputVector3.x) && !float.IsInfinity (inputVector3.y) && !float.IsInfinity (inputVector3.z);
  }

  /// <summary>
  /// Check that controller rotation is valid.
  /// </summary>
  /// <param name="inputQuaternion">The Quaternion to check</param>
  /// <returns>The rotation is valid</returns>
  private bool ValidRotation (Quaternion inputQuaternion)
  {
    return !float.IsNaN (inputQuaternion.x) && !float.IsNaN (inputQuaternion.y) && !float.IsNaN (inputQuaternion.z) && !float.IsNaN (inputQuaternion.w) && !float.IsInfinity (inputQuaternion.x) && !float.IsInfinity (inputQuaternion.y) && !float.IsInfinity (inputQuaternion.z) && !float.IsInfinity (inputQuaternion.w);
  }
}