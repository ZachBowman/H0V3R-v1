using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class car_physics : MonoBehaviour
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

  // public attributes
  public bool alive = true;
  public bool runner_controls;
  public Turning_Physics turning_physics;

  Drive_Engine drive_engine;
  Hover_Engine hover_engine;

  // objects
  Rigidbody body;
  public GameObject start;
  public Vector3 external_force = Vector3.zero;
  //public GameObject missile;

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

  // Used only for runner controls.
  float max_yaw_speed = 2f;
  float min_yaw_speed = .2f;  // The minimum speed at which we consider the car to be pitching.
  float yaw_angle_stop = .2f;
  float yaw_acceleration_far = .07f;
  float yaw_acceleration_near = .03f;

  // roll
  float max_roll_speed = 2f;
  float min_roll_speed = .2f;  // Roll speeds at or below this will be reduced to 0 when rotation is close to target.
  float roll_deceleration_multiplier = .98f;
  float half_roll_left = 45f;
  float max_roll_left = 90f;
  float half_roll_right = 315;
  float max_roll_right = 270f;
  float roll_acceleration_far = .07f;
  float roll_acceleration_near = .03f;
  float roll_angle_stop = .2f;

  // pitch
  float max_pitch_speed = 2f;
  float min_pitch_speed = .2f;  // The minimum speed at which we consider the car to be pitching.
  float half_pitch_forward = 45;
  float max_pitch_forward = 75;
  float half_pitch_backward = 315;
  float max_pitch_backward = 285;
  float pitch_acceleration_far = .07f;
  float pitch_acceleration_near = .03f;
  float pitch_angle_stop = .2f;

  void Start ()
    {
    body = GetComponent<Rigidbody> ();
    hover_engine = GetComponent<Hover_Engine> ();
    drive_engine = GetComponent<Drive_Engine> ();

    if (runner_controls)
      {
      //forward_physics = Forward_Physics.auto_set_velocity;
      turning_physics = Turning_Physics.strafing;
      }
    else
      {
      // DON'T CHANGE THESE ////////
      //forward_physics = Forward_Physics.set_velocity;
      turning_physics = Turning_Physics.angular_velocity;
      //////////////////////////////
      }

    Vector3 arrow_rotation = start.transform.rotation.eulerAngles;
    transform.rotation = Quaternion.Euler (new Vector3 (transform.rotation.x, arrow_rotation.y, transform.rotation.z));
    transform.position = start.transform.position;

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

      rotation_restriction ();

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

      rotation_restriction ();

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

      rotation_restriction ();

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

      rotation_restriction ();

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

  // Only for turning physics that set rotation directly.
  void rotation_restriction ()
    {
    Vector3 angles = transform.rotation.eulerAngles;
    if (angles.x < 360 - max_pitch_forward && angles.x >= 180) angles.x = 360 - max_pitch_forward;
    else if (angles.x > max_pitch_forward && angles.x <= 180) angles.x = max_pitch_forward;
    if (angles.z < 360 - max_roll_left && angles.z >= 180) angles.z = 360 - max_roll_left;
    else if (angles.z > max_roll_left && angles.z <= 180) angles.z = max_roll_left;
    transform.rotation = Quaternion.Euler (angles);
    }

  //////////////////////////////////////////////////

  public void jump_pad_force (float force)
    {
    external_force = Vector3.up * force;
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

    GUI.Label (new Rect (10, 340, 300, 20), "horizontal speed: " + horizontal_speed.ToString ());
    GUI.Label (new Rect (10, 360, 300, 20), "horizontal speed max: " + horizontal_speed_max.ToString ());
    GUI.Label (new Rect (10, 380, 300, 20), "horizontal acceleration: " + horizontal_acceleration.ToString ());
    }

  //////////////////////////////////////////////////

  }
