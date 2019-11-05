using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class camera_movement : MonoBehaviour
  {
  public GameObject target;
  Vector3 position_offset = new Vector3 (0f, 2f, -8f);

  void LateUpdate ()
    {
    Quaternion rotation = Quaternion.Euler (0, target.transform.eulerAngles.y, 0);
    transform.position = target.transform.position + (rotation * position_offset);
    transform.LookAt (target.transform);
    }
  }
