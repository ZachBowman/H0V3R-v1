using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class car_physics : MonoBehaviour
  {
  public enum Forward_Physics
    {
    move_position,
    set_velocity,
    auto_set_velocity,  // forward drive is autonomous
    add_relative_force  // with VelocityChange
    }

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

  public enum Vertical_Physics
    {
    ground_driving,
    hover_velocity,
    hover_global_velocity
    }

  enum Direction
    {
    up,
    down
    }

  // Only used for runner controls
  //enum Yaw
  //  {
  //  none,
  //  half_left,
  //  full_left,
  //  half_right,
  //  full_right
  //  }

  // input physics
  public bool runner_controls;
  public Forward_Physics forward_physics;
  public Vertical_Physics vertical_physics;
  public Turning_Physics turning_physics;

  // action keys
  bool key_roll_left = false;
  bool key_roll_right = false;
  bool key_pitch_forward = false;
  bool key_pitch_backward = false;
  bool key_yaw_left = false;
  bool key_yaw_right = false;

  // objects
  Rigidbody body;
  public GameObject start;
  public Vector3 external_force = Vector3.zero;
  //public GameObject missile;

  // vertical
  float ground_distance;
  float ground_distance_target = 10f;
  float ground_distance_min;
  float ground_distance_max;
  float deceleration_below_target = 6f;
  float deceleration_above_target = 14f;
  float hover_max = 10.4f;
  float hover_min = 9.6f;
  float vertical_speed;// = 0f;
  static float vertical_speed_hover;// = .2f;
  float rise_speed_max = vertical_speed_hover + 4f;//.6f;
  float sink_speed_max = vertical_speed_hover - 4f;//.6f;
  float rise_speed_min = vertical_speed_hover + 1f;
  float sink_speed_min = vertical_speed_hover - 1f;
  float vertical_acceleration_large;// = .02f;
  float vertical_acceleration_small;// = .02f;
  float vertical_speed_min;
  float vertical_speed_max;
  Direction vertical_direction = Direction.up;

  // forward
  float forward_speed;
  float forward_speed_max;
  float forward_speed_max_adjusted;
  float forward_acceleration;
  float forward_acceleration_adjusted;
  float analog_drive_input_raw = 0;
  float analog_drive_input = 0;

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

  bool alive = true;

  void Start ()
    {
    if (runner_controls)
      {
      forward_physics = Forward_Physics.auto_set_velocity;
      vertical_physics = Vertical_Physics.hover_velocity;
      turning_physics = Turning_Physics.strafing;

      //forward_physics = Forward_Physics.set_velocity;
      }
    else
      {
      // DON'T CHANGE THESE ////////
      forward_physics = Forward_Physics.set_velocity;
      vertical_physics = Vertical_Physics.hover_velocity;
      turning_physics = Turning_Physics.angular_velocity;
      //////////////////////////////

      //forward_physics = Forward_Physics.move_position;
      //vertical_physics = Vertical_Physics.hover_global_velocity;
      }

    body = GetComponent<Rigidbody> ();

    Vector3 arrow_rotation = start.transform.rotation.eulerAngles;
    transform.rotation = Quaternion.Euler (new Vector3 (transform.rotation.x, arrow_rotation.y, transform.rotation.z));
    transform.position = start.transform.position;

    // forward
    if (forward_physics == Forward_Physics.move_position)
      {
      forward_speed_max = .9f;
      forward_acceleration = .02f;
      if (turning_physics == Turning_Physics.speed_based) rotation_multiplier = 2.5f;
      }
    else if (forward_physics == Forward_Physics.set_velocity)
      {
      forward_speed_max = 70f;
      forward_acceleration = 1f;
      if (turning_physics == Turning_Physics.speed_based) rotation_multiplier = .03f;
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
      if (turning_physics == Turning_Physics.speed_based) rotation_multiplier = .1f;
      }

    forward_speed = 0f;
    forward_speed_max_adjusted = forward_speed_max;
    forward_acceleration_adjusted = forward_acceleration;

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
    else if (turning_physics == Turning_Physics.strafing)
      {
      horizontal_speed_max = 25f;
      horizontal_acceleration = 3f;
      }

    // vertical
    if (vertical_physics == Vertical_Physics.hover_velocity)
      {
      vertical_speed = 0f;
      vertical_speed_hover = .2f;
      vertical_speed_min = vertical_speed_hover - .6f;
      vertical_speed_max = vertical_speed_hover + .6f;
      vertical_acceleration_large = .02f;
      vertical_acceleration_small = .02f;
      ground_distance_min = 10f;
      ground_distance_max = 11f;
      vertical_direction = Direction.up;
      }
    else
      {
      ground_distance_min = 0f;
      ground_distance_max = 0f;
      }

    //test_stabilizers ();
    }

  //////////////////////////////////////////////////

  void Update ()
    {
    if (Input.GetButton ("Roll Left") && !key_roll_left)
      {
      key_roll_left = true;
      test_left_roll_stabilizer ();
      }
    else if (key_roll_left) key_roll_left = false;

    if (Input.GetButton ("Roll Right") && !key_roll_right)
      {
      key_roll_right = true;
      test_right_roll_stabilizer ();
      }
    else if (key_roll_right) key_roll_right = false;

    if (Input.GetButton ("Pitch Forward") && !key_pitch_forward)
      {
      key_pitch_forward = true;
      test_forward_pitch_stabilizer ();
      }
    else if (key_pitch_forward) key_pitch_forward = false;

    if (Input.GetButton ("Pitch Backward") && !key_pitch_backward)
      {
      key_pitch_backward = true;
      test_backward_pitch_stabilizer ();
      }
    else if (key_pitch_backward) key_pitch_backward = false;

    if (Input.GetButton ("Yaw Left") && !key_yaw_left)
      {
      key_yaw_left = true;
      test_left_yaw_stabilizer ();
      }
    else if (key_yaw_left) key_yaw_left = false;

    if (Input.GetButton ("Yaw Right") && !key_yaw_right)
      {
      key_yaw_right = true;
      test_right_yaw_stabilizer ();
      }
    else if (key_yaw_right) key_yaw_right = false;

    //if (Input.GetKeyDown (KeyCode.Space) && !key_space)
    //  {
    //  key_space = true;
    //  jump = true;
    //  external_force = Vector3.up * 320f;
    //  }
    //if (Input.GetKeyUp (KeyCode.Space) && key_space) key_space = false;

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
    Hover ();
    Drive ();
    Turn ();

    //if (turning_physics == Turning_Physics.strafing)
    //  {
    //  limit_local_yaw_angle ();
    //  stabilize_local_yaw ();
    //  }
    }

  //////////////////////////////////////////////////

  void Hover ()
    {
    Vector3 current_velocity_vector = body.velocity;
    ground_distance = get_ground_distance ();

    if (vertical_physics == Vertical_Physics.hover_velocity)
      {
      if (vertical_direction == Direction.up)
        {
        if (ground_distance <= deceleration_below_target)
          {
          if (vertical_speed < rise_speed_max) vertical_speed += vertical_acceleration_large;
          }
        else if (ground_distance >= deceleration_below_target && ground_distance <= hover_max)
          {
          if (vertical_speed > rise_speed_min) vertical_speed -= vertical_acceleration_small;
          else if (vertical_speed < rise_speed_min) vertical_speed += vertical_acceleration_small;
          }
        else if (ground_distance >= hover_max) vertical_direction = Direction.down;
        }
      else if (vertical_direction == Direction.down)
        {
        if (ground_distance >= deceleration_above_target)
          {
          if (vertical_speed > sink_speed_max) vertical_speed -= vertical_acceleration_large;
          }
        else if (ground_distance <= deceleration_above_target && ground_distance >= hover_min)
          {
          if (vertical_speed < sink_speed_min) vertical_speed += vertical_acceleration_small;
          else if (vertical_speed > sink_speed_min) vertical_speed -= vertical_acceleration_small;
          }
        else if (ground_distance <= hover_min) vertical_direction = Direction.up;
        }

      Mathf.Clamp (vertical_speed, sink_speed_max, rise_speed_max);

      body.velocity = new Vector3 (current_velocity_vector.x, vertical_speed, current_velocity_vector.z);
      }

    else if (vertical_physics == Vertical_Physics.hover_global_velocity)
      {
      if (vertical_direction == Direction.up)
        {
        if (ground_distance <= ground_distance_max && vertical_speed < vertical_speed_max)
          {
          vertical_speed += vertical_acceleration_large;
          }
        if (ground_distance >= ground_distance_max) vertical_direction = Direction.down;
        }
      else if (vertical_direction == Direction.down)
        {
        if (ground_distance >= ground_distance_min && vertical_speed > vertical_speed_min)
          {
          vertical_speed -= vertical_acceleration_large;
          }
        if (ground_distance <= ground_distance_min) vertical_direction = Direction.up;
        }

      Mathf.Clamp (vertical_speed, vertical_speed_min, vertical_speed_max);
      current_velocity_vector = transform.InverseTransformDirection (body.velocity);
      current_velocity_vector.y = vertical_speed;
      body.velocity = transform.TransformDirection (current_velocity_vector);
      }
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

      //if (jump) body.AddForce (Vector3.up * 10f, ForceMode.Acceleration);
      //if (external_force.y > 0f) external_force.y += Physics.gravity.y;
      //if (external_force.y < 0f) external_force.y = 0f;
      //if (external_force.y > 0f)
      //  {
      //  body.AddForce (external_force, ForceMode.Acceleration);
      //  external_force.y -= 10f;
      //  }
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

      if ((Input.GetButton ("Drive Forward") || forward_physics == Forward_Physics.auto_set_velocity) && alive)
        {
        if (forward_speed < forward_speed_max && can_drive ()) forward_speed += forward_acceleration;
        }
      else if (Input.GetButton ("Drive Backward") && alive)
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

  void Turn ()
    {
    if (turning_physics == Turning_Physics.speed_based)
      {
      if (Input.GetButton ("Turn Left")) turning_rotation = forward_speed * -rotation_multiplier;
      if (Input.GetButton ("Turn Right")) turning_rotation = forward_speed * rotation_multiplier;

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
      if (!on_ground () && vertical_physics == Vertical_Physics.ground_driving) turning_rotation /= 2;

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
      if (!on_ground () && vertical_physics == Vertical_Physics.ground_driving) turning_rotation /= 2;

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

  // Don't let it go past 90 degrees of spin (or whatever max yaw is set to).
  //void limit_local_yaw_angle ()
  //  {
  //  Vector3 rotation = transform.rotation.eulerAngles;
  //  Vector3 angular_velocity = transform.InverseTransformDirection (body.angularVelocity);

  //  if (angular_velocity.y > 0 && rotation.y > max_yaw_right && rotation.y < 180) rotation.y = max_yaw_right;
  //  else if (angular_velocity.y < 0 && rotation.y < max_yaw_left && rotation.y > 180) rotation.y = max_yaw_left;
  //  transform.eulerAngles = rotation;
  //  }

  //////////////////////////////////////////////////

  // Helper functions for stabilizer logic.
  // Max is the fastest speed the car should be allowed to rotate.
  // Min is the slowest speed the car should be allowed to rotate if it's not stabilized.

  //bool is_yawing_faster_than_left_min (Vector3 angular_velocity) { return angular_velocity.y < -min_yaw_speed; }
  //bool is_yawing_slower_than_left_min (Vector3 angular_velocity) { return angular_velocity.y > -min_yaw_speed; }
  //bool is_yawing_faster_than_right_min (Vector3 angular_velocity) { return angular_velocity.y > min_yaw_speed; }
  //bool is_yawing_slower_than_right_min (Vector3 angular_velocity) { return angular_velocity.y < min_yaw_speed; }
  //bool is_yawing_faster_than_left_max (Vector3 angular_velocity) { return angular_velocity.y < -max_yaw_speed; }
  //bool is_yawing_slower_than_left_max (Vector3 angular_velocity) { return angular_velocity.y > -max_yaw_speed; }
  //bool is_yawing_faster_than_right_max (Vector3 angular_velocity) { return angular_velocity.y > max_yaw_speed; }
  //bool is_yawing_slower_than_right_max (Vector3 angular_velocity) { return angular_velocity.y < max_yaw_speed; }

  //////////////////////////////////////////////////

  // Slow angular velocity to prevent wild spinning on the yaw.
  // Used only for runner control scheme.
  //void stabilize_local_yaw ()
  //  {
  //  // convert angular velocity to local axes
  //  Vector3 rotation = transform.rotation.eulerAngles;
  //  Vector3 angular_velocity = transform.InverseTransformDirection (body.angularVelocity);

  //  // limit yaw speed regardless of rotation
  //  if (is_yawing_faster_than_left_max (angular_velocity)) angular_velocity.y = -max_yaw_speed;
  //  if (is_yawing_faster_than_right_max (angular_velocity)) angular_velocity.y = max_yaw_speed;

  //  // stop yawing if it's really close to stabilized
  //  if ((rotation.y < yaw_angle_stop || rotation.z > 360 - yaw_angle_stop)
  //    && is_yawing_slower_than_left_min (angular_velocity) && is_yawing_slower_than_right_min (angular_velocity))
  //    {
  //    angular_velocity.y = 0;
  //    rotation.y = 0;
  //    transform.eulerAngles = rotation;
  //    }

  //  else if (check_yaw (rotation) == Yaw.half_left)
  //    {
  //    if (is_yawing_slower_than_right_min (angular_velocity)) angular_velocity.y += yaw_acceleration_near;  // speed it up a little
  //    else if (is_yawing_faster_than_right_min (angular_velocity)) angular_velocity.y -= yaw_acceleration_near;  // slow it down a little
  //    }
  //  else if (check_yaw (rotation) == Yaw.full_left)
  //    {
  //    if (is_yawing_slower_than_right_max (angular_velocity)) angular_velocity.y += yaw_acceleration_far;  // speed it up a lot
  //    }
  //  else if (check_yaw (rotation) == Yaw.half_right)
  //    {
  //    if (is_yawing_slower_than_left_min (angular_velocity)) angular_velocity.y -= yaw_acceleration_near;  // speed it up a little
  //    else if (is_yawing_faster_than_left_min (angular_velocity)) angular_velocity.y += yaw_acceleration_near;  // slow it down a little
  //    }
  //  else if (check_yaw (rotation) == Yaw.full_right)
  //    {
  //    if (is_yawing_slower_than_left_max (angular_velocity)) angular_velocity.y -= yaw_acceleration_far;  // speed it up a lot
  //    }

  //  // convert angular velocity back to global and apply
  //  body.angularVelocity = (transform.forward * angular_velocity.z) + (transform.up * angular_velocity.y) + (transform.right * angular_velocity.x);
  //  }

  //////////////////////////////////////////////////

  //Yaw check_yaw (Vector3 rotation)
  //  {
  //  if (rotation.y > 0f && rotation.y <= half_yaw_right) return Yaw.half_right;
  //  else if (rotation.y > half_yaw_right && rotation.y <= max_yaw_right) return Yaw.full_right;
  //  else if (rotation.y < 360f && rotation.y >= half_yaw_left) return Yaw.half_left;
  //  else if (rotation.y < half_yaw_left && rotation.y >= max_yaw_left) return Yaw.full_left;
  //  else return Yaw.none;
  //  }

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

  float get_ground_distance ()
    {
    Ray ray = new Ray (transform.position, Vector3.down);
    RaycastHit hit_info;

    if (Physics.Raycast (ray, out hit_info, 20))
      {
      return hit_info.distance;
      }
    else return 20;
    }

  //////////////////////////////////////////////////

  public void jump_pad_force (float force)
    {
    external_force = Vector3.up * force;
    }

  //////////////////////////////////////////////////

  private bool can_drive ()
    {
    if (vertical_physics != Vertical_Physics.ground_driving) return true;
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

  void test_stabilizers ()
    {
    test_left_roll_stabilizer ();
    //test_right_roll_stabilizer ();
    //body.angularVelocity = transform.right * -20f;  // test pitch
    //body.angularVelocity = (transform.forward * 20f) + (transform.right * -20f);
    }

  //////////////////////////////////////////////////

  void test_left_roll_stabilizer ()
    {
    body.angularVelocity = transform.forward * 20f;
    }

  //////////////////////////////////////////////////

  void test_right_roll_stabilizer ()
    {
    body.angularVelocity = transform.forward * -20f;
    }

  //////////////////////////////////////////////////

  void test_forward_pitch_stabilizer ()
    {
    body.angularVelocity = transform.right * 20f;
    }

  //////////////////////////////////////////////////

  void test_backward_pitch_stabilizer ()
    {
    body.angularVelocity = transform.right * -20f;
    }

  //////////////////////////////////////////////////

  void test_left_yaw_stabilizer ()
    {
    body.angularVelocity = transform.up * -20f;
    }

  //////////////////////////////////////////////////

  void test_right_yaw_stabilizer ()
    {
    body.angularVelocity = transform.up * 20f;
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
    if (runner_controls) alive = false;
    }

  //////////////////////////////////////////////////

  private void OnGUI ()
    {
    GUI.color = UnityEngine.Color.black;

    Vector3 localangularvelocity = transform.InverseTransformDirection (body.angularVelocity);
    GUI.Label (new Rect (10,  20, 300, 20), "local angular velocity x: " + localangularvelocity.x.ToString ());
    GUI.Label (new Rect (10,  40, 300, 20), "local angular velocity y: " + localangularvelocity.y.ToString ());
    GUI.Label (new Rect (10,  60, 300, 20), "local angular velocity z: " + localangularvelocity.z.ToString ());

    GUI.Label (new Rect (10,  100, 300, 20), "vertical speed: " + vertical_speed.ToString ());
    GUI.Label (new Rect (10,  120, 300, 20), "distance_to_ground: " + ground_distance.ToString ());
    //GUI.Label (new Rect (10, 100, 300, 20), "bob: " + bob.ToString ());

    GUI.Label (new Rect (10, 160, 300, 20), "yaw: " + body.transform.rotation.eulerAngles.y.ToString ());
    GUI.Label (new Rect (10, 180, 300, 20), "roll: " + body.transform.rotation.eulerAngles.z.ToString ());
    GUI.Label (new Rect (10, 200, 300, 20), "pitch: " + body.transform.rotation.eulerAngles.x.ToString ());
    //GUI.Label (new Rect (10, 180, 300, 20), "turning rotation: " + turning_rotation.ToString ());

    //GUI.Label (new Rect (10, 240, 300, 20), "global angular velocity x: " + body.angularVelocity.x.ToString ());
    //GUI.Label (new Rect (10, 260, 300, 20), "global angular velocity y: " + body.angularVelocity.y.ToString ());
    //GUI.Label (new Rect (10, 280, 300, 20), "global angular velocity z: " + body.angularVelocity.z.ToString ());

    GUI.Label (new Rect (10, 240, 300, 20), "analog_drive_input_raw: " + analog_drive_input_raw.ToString ());
    GUI.Label (new Rect (10, 260, 300, 20), "analog_drive_input: " + analog_drive_input.ToString ());
    GUI.Label (new Rect (10, 280, 300, 20), "forward speed: " + forward_speed.ToString ());
    GUI.Label (new Rect (10, 300, 300, 20), "forward speed max adjusted: " + forward_speed_max_adjusted.ToString ());

    GUI.Label (new Rect (10, 340, 300, 20), "horizontal speed: " + horizontal_speed.ToString ());
    GUI.Label (new Rect (10, 360, 300, 20), "horizontal speed max: " + horizontal_speed_max.ToString ());
    GUI.Label (new Rect (10, 380, 300, 20), "horizontal acceleration: " + horizontal_acceleration.ToString ());
    }

  //////////////////////////////////////////////////

  }
