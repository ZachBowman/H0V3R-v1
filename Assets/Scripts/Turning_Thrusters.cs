using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turning_Thrusters : MonoBehaviour
  {
  public enum Turning_Physics
    {
    speed_based,
    constant_local_rotation,
    constant_global_rotation,
    relative_torque,
    global_torque,
    angular_velocity,
    strafing
    }

  public Turning_Physics turning_physics;

  Rigidbody body;
  car_physics physics;
  Drive_Engine drive_engine;
  Hover_Engine hover_engine;
  Stabilizers stabilizers;

  // horizontal
  float horizontal_speed;
  float horizontal_speed_max;
  float horizontal_acceleration;

  // yaw
  float turning_rotation = 0f;
  float rotation_multiplier = 1.3f;
  float half_yaw_left = 315f;
  float max_yaw_left = 270f;
  float half_yaw_right = 45f;
  float max_yaw_right = 90f;

  //////////////////////////////////////////////////

  void Start ()
    {
    body = GetComponent<Rigidbody> ();
    physics = GetComponent<car_physics> ();
    drive_engine = GetComponent<Drive_Engine> ();
    hover_engine = GetComponent<Hover_Engine> ();
    stabilizers = GetComponent<Stabilizers> ();

    if (physics.runner_controls) turning_physics = Turning_Physics.strafing;
    else turning_physics = Turning_Physics.angular_velocity;

    // turning
    if (turning_physics == Turning_Physics.constant_local_rotation)
      {
      rotation_multiplier = 1.3f;
      }
    else if (turning_physics == Turning_Physics.constant_global_rotation)
      {
      rotation_multiplier = 1.3f;
      }
    else if (turning_physics == Turning_Physics.relative_torque)
      {
      rotation_multiplier = 500f;
      }
    else if (turning_physics == Turning_Physics.speed_based)
      {
      if (drive_engine.forward_physics == Drive_Engine.Forward_Physics.move_position) rotation_multiplier = 2.5f;
      if (drive_engine.forward_physics == Drive_Engine.Forward_Physics.set_velocity) rotation_multiplier = .03f;
      if (drive_engine.forward_physics == Drive_Engine.Forward_Physics.add_relative_force) rotation_multiplier = .1f;
      }
    else if (turning_physics == Turning_Physics.strafing)
      {
      horizontal_speed_max = 25f;
      horizontal_acceleration = 3f;
      }
    }

  //////////////////////////////////////////////////

  void FixedUpdate ()
    {
    Turn ();
    }

  //////////////////////////////////////////////////

  void Turn ()
    {
    if (turning_physics == Turning_Physics.speed_based)
      {
      if (Input.GetButton ("Turn Left")) turning_rotation = drive_engine.forward_speed * -rotation_multiplier;
      if (Input.GetButton ("Turn Right")) turning_rotation = drive_engine.forward_speed * rotation_multiplier;

      stabilizers.rotation_restriction ();

      if (Input.GetButton ("Drive Forward") || Input.GetButton ("Drive Backward"))
        {
        transform.Rotate (0f, turning_rotation, 0f);
        }
      }
    else if (turning_physics == Turning_Physics.constant_local_rotation)
      {
      if (Input.GetButton ("Turn Right") && Input.GetButton ("Turn Left")) turning_rotation = 0f;
      else if (Input.GetButton ("Turn Left")) turning_rotation = -rotation_multiplier;
      else if (Input.GetButton ("Turn Right")) turning_rotation = rotation_multiplier;
      else turning_rotation = 0f;
      if (Input.GetButton ("Drive Backward")) turning_rotation *= -1f;
      if (!on_ground () && hover_engine.vertical_physics == Hover_Engine.Vertical_Physics.ground_driving) turning_rotation /= 2;

      Vector3 current_angular_velocity = body.angularVelocity;
      current_angular_velocity.y = 0f;
      body.angularVelocity = current_angular_velocity;

      stabilizers.rotation_restriction ();

      if (Input.GetButton ("Drive Forward") || Input.GetButton ("Drive Backward")) transform.Rotate (0f, turning_rotation, 0f);
      }
    else if (turning_physics == Turning_Physics.constant_global_rotation)
      {
      if (Input.GetButton ("Turn Right") && Input.GetButton ("Turn Left")) turning_rotation = 0f;
      else if (Input.GetButton ("Turn Left")) turning_rotation = -rotation_multiplier;
      else if (Input.GetButton ("Turn Right")) turning_rotation = rotation_multiplier;
      else turning_rotation = 0f;
      if (Input.GetButton ("Drive Backward")) turning_rotation *= -1f;
      if (!on_ground () && hover_engine.vertical_physics == Hover_Engine.Vertical_Physics.ground_driving) turning_rotation /= 2;

      Vector3 current_angular_velocity = body.angularVelocity;
      current_angular_velocity.y = 0f;
      body.angularVelocity = current_angular_velocity;

      stabilizers.rotation_restriction ();

      if (Input.GetButton ("Drive Forward") || Input.GetButton ("Drive Backward"))
        {
        transform.Rotate (0f, turning_rotation, 0f, Space.World);
        }
      }
    else if (turning_physics == Turning_Physics.relative_torque)
      {
      if (Input.GetButton ("Turn Left")) turning_rotation = -rotation_multiplier;
      if (Input.GetButton ("Turn Right")) turning_rotation = rotation_multiplier;
      if (Input.GetButton ("Drive Backward")) turning_rotation *= -1f;

      stabilizers.rotation_restriction ();

      if (Input.GetButton ("Drive Forward") || Input.GetButton ("Drive Backward"))
        {
        Vector3 current_torque = transform.InverseTransformDirection (body.angularVelocity);
        body.AddRelativeTorque (-current_torque);
        body.AddRelativeTorque (0f, turning_rotation, 0f);
        }
      }
    else if (turning_physics == Turning_Physics.angular_velocity)
      {
      Vector3 angular_velocity = body.angularVelocity;

      if (Input.GetButton ("Turn Left") && Input.GetButton ("Turn Right")) turning_rotation = 0;
      else if (Input.GetButton ("Turn Left")) turning_rotation = -rotation_multiplier;
      else if (Input.GetButton ("Turn Right")) turning_rotation = rotation_multiplier;
      else turning_rotation = 0f;
      if (Input.GetButton ("Drive Backward")) turning_rotation *= -1f;

      if (Input.GetButton ("Drive Forward") || Input.GetButton ("Drive Backward"))
        {
        angular_velocity.y = turning_rotation;
        body.angularVelocity = angular_velocity;
        }
      else
        {
        angular_velocity.y = 0f;
        body.angularVelocity = angular_velocity;
        }
      }
    else if (turning_physics == Turning_Physics.strafing)
      {
      if (Input.GetButton ("Turn Left") && !Input.GetButton ("Turn Right"))
        {
        if (horizontal_speed > -horizontal_speed_max) horizontal_speed -= horizontal_acceleration;
        }
      else if (Input.GetButton ("Turn Right") && !Input.GetButton ("Turn Left"))
        {
        if (horizontal_speed < horizontal_speed_max) horizontal_speed += horizontal_acceleration;
        }
      else  // not strafing
        {
        if (horizontal_speed > -.1f && horizontal_speed < .1f) horizontal_speed = 0f;
        if (horizontal_speed != 0f) horizontal_speed = horizontal_speed * 0.9f;
        }

      Vector3 current_velocity = body.velocity;
      Vector3 direction = transform.right;
      direction.y = 0;
      Vector3 new_velocity = horizontal_speed * direction.normalized;
      new_velocity.y = current_velocity.y;
      new_velocity.z = current_velocity.z;
      body.velocity = new_velocity;
      }
    }

  //////////////////////////////////////////////////

  // Check if car is grounded (only for wheeled vehicles).
  private bool on_ground ()
    {
    Vector3 ray_start = transform.position + new Vector3 (0f, 0f, 0f);
    return Physics.Raycast (ray_start, Vector3.down, .5f);
    }

  //////////////////////////////////////////////////

  private void OnGUI ()
    {
    GUI.color = UnityEngine.Color.black;

    GUI.Label (new Rect (10, 340, 300, 20), "horizontal speed: " + horizontal_speed.ToString ());
    GUI.Label (new Rect (10, 360, 300, 20), "horizontal speed max: " + horizontal_speed_max.ToString ());
    GUI.Label (new Rect (10, 380, 300, 20), "horizontal acceleration: " + horizontal_acceleration.ToString ());
    }
  }
