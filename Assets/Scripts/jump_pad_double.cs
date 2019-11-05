using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class jump_pad_double : MonoBehaviour {

  void Start ()
    {

    }

  void Update ()
    {
    }

  private void OnTriggerEnter (Collider other)
    {
    other.transform.parent.GetComponent<car_physics> ().jump_pad_force (345f);
    }
  }
