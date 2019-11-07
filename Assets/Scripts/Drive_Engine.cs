using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drive_Engine : MonoBehaviour
  {
  public enum Forward_Physics
    {
    move_position,
    set_velocity,
    auto_set_velocity,  // forward drive is autonomous
    add_relative_force  // with VelocityChange
    }

  public Forward_Physics forward_physics;
  public float forward_speed;

  Rigidbody body;
  car_physics physics;
  Hover_Engine hover_engine;

  float forward_speed_max;
  float forward_speed_max_adjusted;
  float forward_acceleration;
  float forward_acceleration_adjusted;
  float analog_drive_input_raw = 0;
  float analog_drive_input = 0;

  //////////////////////////////////////////////////

  void Start ()
    {
    body = GetComponent<Rigidbody> ();
    physics = GetComponent<car_physics> ();
    hover_engine = GetComponent<Hover_Engine> ();

    if (physics.runner_controls) forward_physics = Forward_Physics.auto_set_velocity;
    else forward_physics = Forward_Physics.set_velocity;

    if (forward_physics == Forward_Physics.move_position)
      {
      forward_speed_max = .9f;
      forward_acceleration = .02f;
      }
    else if (forward_physics == Forward_Physics.set_velocity)
      {
      forward_speed_max = 70f;
      forward_acceleration = 1f;
      }
    else if (forward_physics == Forward_Physics.auto_set_velocity)
      {
      forward_speed_max = 70f;
      forward_acceleration = .1f;
      }
    else if (forward_physics == Forward_Physics.add_relative_force)
      {
      forward_speed_max = 30f;
      forward_acceleration = .5f;
      }

    forward_speed = 0f;
    forward_speed_max_adjusted = forward_speed_max;
    forward_acceleration_adjusted = forward_acceleration;
    }

  //////////////////////////////////////////////////

  void FixedUpdate ()
    {
    Drive ();
    }

  //////////////////////////////////////////////////

  void Drive ()
    {
    calculate_forward_speed ();

    if (forward_physics == Forward_Physics.set_velocity || forward_physics == Forward_Physics.auto_set_velocity)
      {
      Vector3 current_velocity = body.velocity;
      Vector3 direction = transform.forward;
      direction.y = 0;
      Vector3 new_velocity = forward_speed * direction.normalized;
      new_velocity.y = current_velocity.y;
      body.velocity = new_velocity;
      }
    else if (forward_physics == Forward_Physics.move_position)
      {
      Mathf.Clamp (forward_speed, -forward_speed_max, forward_speed_max);
      Vector3 forward_movement = transform.forward * forward_speed;
      body.MovePosition (body.position + forward_movement);
      }
    else if (forward_physics == Forward_Physics.add_relative_force)
      {
      Mathf.Clamp (forward_speed, -forward_speed_max, forward_speed_max);
      float forward_speed_adjustment = forward_speed - transform.InverseTransformDirection (body.velocity).z;
      float side_speed_adjustment = -transform.InverseTransformDirection (body.velocity).x;
      body.AddRelativeForce (side_speed_adjustment, 0f, forward_speed + forward_speed_adjustment, ForceMode.VelocityChange);
      }

    check_forward_collision ();
    }

  //////////////////////////////////////////////////

  void calculate_forward_speed ()
    {
    // 1 -> 1
    // .9 -> .8
    // .8 -> .6
    // .7 -> .4
    // .6 -> .2
    // .5 -> 0

    // Axis input will be in range of 0 to 1, 0 to -1.
    analog_drive_input_raw = Input.GetAxis ("Analog Drive Speed");

    // Input does not become reliable until outside the range of -0.5 to 0.5.
    if (analog_drive_input_raw > 0.5 || analog_drive_input_raw < -0.5)
      {
      if (analog_drive_input_raw > 0) analog_drive_input = Convert.ToSingle (analog_drive_input_raw - 0.5) * 2;
      else analog_drive_input = Convert.ToSingle (analog_drive_input_raw + 0.5) * 2;

      // Negative is forward stick.
      if (analog_drive_input_raw < 0)
        {
        forward_acceleration_adjusted = forward_acceleration * analog_drive_input * -1;
        forward_speed_max_adjusted = forward_speed_max * analog_drive_input * -1;
        if (forward_speed < forward_speed_max_adjusted) forward_speed += forward_acceleration_adjusted;
        }

      // Positive is backward stick.
      else if (analog_drive_input_raw > 0)
        {
        forward_acceleration_adjusted = forward_acceleration * analog_drive_input;
        forward_speed_max_adjusted = forward_speed_max * analog_drive_input;
        if (forward_speed > -forward_speed_max_adjusted) forward_speed -= forward_acceleration_adjusted;
        }
      }
    else  // keyboard input
      {
      forward_speed_max_adjusted = forward_speed_max;

      if ((Input.GetButton ("Drive Forward") || forward_physics == Forward_Physics.auto_set_velocity) && physics.alive)
        {
        if (forward_speed < forward_speed_max && can_drive ()) forward_speed += forward_acceleration;
        }
      else if (Input.GetButton ("Drive Backward") && physics.alive)
        {
        if (forward_speed > -forward_speed_max) forward_speed -= forward_acceleration;
        }
      else  // not driving
        {
        if (forward_speed > -.1f && forward_speed < .1f) forward_speed = 0f;
        if (forward_speed != 0f) forward_speed = forward_speed * 0.9f;
        }
      }
    }

  //////////////////////////////////////////////////

  private bool can_drive ()
    {
    if (hover_engine.vertical_physics != Hover_Engine.Vertical_Physics.ground_driving) return true;
    if (on_ground ()) return true;
    return false;
    }

  //////////////////////////////////////////////////

  // Check if car is grounded (only for wheeled vehicles).
  private bool on_ground ()
    {
    Vector3 ray_start = transform.position + new Vector3 (0f, 0f, 0f);
    return Physics.Raycast (ray_start, Vector3.down, .5f);
    }

  //////////////////////////////////////////////////

  void check_forward_collision ()
    {
    bool collision_front = false;
    Vector3 ray_start = transform.position + new Vector3 (0f, .5f, 0f);
    RaycastHit hit;

    collision_front = Physics.Raycast (ray_start, transform.forward, out hit, 3f);

    if (collision_front)// && forward_speed > .3f)
      {
      hit_obstacle (hit.transform.gameObject);
      }
    }

  //////////////////////////////////////////////////

  void hit_obstacle (GameObject obj)
    {
    Debug.Log ("Hit " + obj.name);
    if (physics.runner_controls) physics.alive = false;
    }

  //////////////////////////////////////////////////

  private void OnGUI ()
    {
    GUI.color = UnityEngine.Color.black;

    GUI.Label (new Rect (10, 240, 300, 20), "analog_drive_input_raw: " + analog_drive_input_raw.ToString ());
    GUI.Label (new Rect (10, 260, 300, 20), "analog_drive_input: " + analog_drive_input.ToString ());
    GUI.Label (new Rect (10, 280, 300, 20), "forward speed: " + forward_speed.ToString ());
    GUI.Label (new Rect (10, 300, 300, 20), "forward speed max adjusted: " + forward_speed_max_adjusted.ToString ());
    }
  }
