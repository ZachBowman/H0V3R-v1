using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class car_physics : MonoBehaviour
  {
  // public attributes
  public bool alive = true;
  public bool runner_controls;

  // objects
  Rigidbody body;
  public GameObject start;
  public Vector3 external_force = Vector3.zero;
  //public GameObject missile;

  void Start ()
    {
    body = GetComponent<Rigidbody> ();

    // Move car to starting point
    Vector3 arrow_rotation = start.transform.rotation.eulerAngles;
    transform.rotation = Quaternion.Euler (new Vector3 (transform.rotation.x, arrow_rotation.y, transform.rotation.z));
    transform.position = start.transform.position;
    }

  //////////////////////////////////////////////////

  void Update ()
    {
    //if (Input.GetButtonDown ("Fire1"))
    //{
    //Transform spawn = transform.Find ("player_missile_spawn");
    //Instantiate (missile, spawn.position, transform.rotation);
    //}

    //if (Input.GetButtonDown ("Flip") && !button_roll)
    //  {
    //  float z_rotation = transform.rotation.eulerAngles.z;
    //  if (z_rotation > 45f || z_rotation < -45f)
    //    {
    //    button_roll = true;
    //    roll_direction *= -1f;
    //    flip_vertical_acceleration = 0.6f;
    //    flip_rotation_acceleration = 4f;
    //    deliberate_flip = true;
    //    }
    //  }
    //if (Input.GetButtonUp ("Flip") && button_roll) button_roll = false;
    }

  //////////////////////////////////////////////////

  public void jump_pad_force (float force)
    {
    external_force = Vector3.up * force;
    }

  //////////////////////////////////////////////////

  private void OnGUI ()
    {
    GUI.color = UnityEngine.Color.black;

    Vector3 localangularvelocity = transform.InverseTransformDirection (body.angularVelocity);
    GUI.Label (new Rect (10,  20, 300, 20), "local angular velocity x: " + localangularvelocity.x.ToString ());
    GUI.Label (new Rect (10,  40, 300, 20), "local angular velocity y: " + localangularvelocity.y.ToString ());
    GUI.Label (new Rect (10,  60, 300, 20), "local angular velocity z: " + localangularvelocity.z.ToString ());

    GUI.Label (new Rect (10, 160, 300, 20), "yaw: " + body.transform.rotation.eulerAngles.y.ToString ());
    GUI.Label (new Rect (10, 180, 300, 20), "roll: " + body.transform.rotation.eulerAngles.z.ToString ());
    GUI.Label (new Rect (10, 200, 300, 20), "pitch: " + body.transform.rotation.eulerAngles.x.ToString ());
    //GUI.Label (new Rect (10, 180, 300, 20), "turning rotation: " + turning_rotation.ToString ());

    //GUI.Label (new Rect (10, 240, 300, 20), "global angular velocity x: " + body.angularVelocity.x.ToString ());
    //GUI.Label (new Rect (10, 260, 300, 20), "global angular velocity y: " + body.angularVelocity.y.ToString ());
    //GUI.Label (new Rect (10, 280, 300, 20), "global angular velocity z: " + body.angularVelocity.z.ToString ());
    }
  }
