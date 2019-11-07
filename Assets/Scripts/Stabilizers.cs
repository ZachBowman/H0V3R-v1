using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stabilizers : MonoBehaviour
  {
  enum Roll
    {
    none,
    half_left,
    full_left,
    half_right,
    full_right
    }

  enum Pitch
    {
    none,
    half_forward,
    full_forward,
    half_backward,
    full_backward
    }

  // Only used for runner controls
  enum Yaw
    {
    none,
    half_left,
    full_left,
    half_right,
    full_right
    }

  Rigidbody body;
  car_physics physics;

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
  float min_pitch_speed = .2f;  // Pitch speeds at or below this will be reduced to 0 when rotation is close to target.
  float half_pitch_forward = 45;
  float max_pitch_forward = 75;
  float half_pitch_backward = 315;
  float max_pitch_backward = 285;
  float pitch_acceleration_far = .07f;
  float pitch_acceleration_near = .03f;
  float pitch_angle_stop = .2f;

  // yaw
  float half_yaw_left = 315f;
  float max_yaw_left = 270f;
  float half_yaw_right = 45f;
  float max_yaw_right = 90f;

  // yaw (used only for runner controls)
  float max_yaw_speed = 2f;
  float min_yaw_speed = .2f;  // The minimum speed at which we consider the car to be pitching.
  float yaw_angle_stop = .2f;
  float yaw_acceleration_far = .07f;
  float yaw_acceleration_near = .03f;

  // action keys
  bool key_roll_left = false;
  bool key_roll_right = false;
  bool key_pitch_forward = false;
  bool key_pitch_backward = false;
  bool key_yaw_left = false;
  bool key_yaw_right = false;

  //////////////////////////////////////////////////

  void Start ()
    {
    body = GetComponent<Rigidbody> ();
    physics = GetComponent<car_physics> ();

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
    }

  //////////////////////////////////////////////////

  void FixedUpdate ()
    {
    limit_local_roll_angle ();
    stabilize_local_roll ();

    limit_local_pitch_angle ();
    stabilize_local_pitch ();

    if (physics.turning_physics == car_physics.Turning_Physics.strafing)
      {
      limit_local_yaw_angle ();
      stabilize_local_yaw ();
      }
    }

  //////////////////////////////////////////////////

  // Helper functions for stabilizer logic.
  // Max is the fastest speed the car should be allowed to rotate.
  // Min is the slowest speed the car should be allowed to rotate if it's not stabilized.

  bool is_rolling_faster_than_left_min (Vector3 angular_velocity) { return angular_velocity.z > min_roll_speed; }
  bool is_rolling_slower_than_left_min (Vector3 angular_velocity) { return angular_velocity.z < min_roll_speed; }
  bool is_rolling_faster_than_right_min (Vector3 angular_velocity) { return angular_velocity.z < -min_roll_speed; }
  bool is_rolling_slower_than_right_min (Vector3 angular_velocity) { return angular_velocity.z > -min_roll_speed; }
  bool is_rolling_faster_than_left_max (Vector3 angular_velocity) { return angular_velocity.z > max_roll_speed; }
  bool is_rolling_slower_than_left_max (Vector3 angular_velocity) { return angular_velocity.z < max_roll_speed; }
  bool is_rolling_faster_than_right_max (Vector3 angular_velocity) { return angular_velocity.z < -max_roll_speed; }
  bool is_rolling_slower_than_right_max (Vector3 angular_velocity) { return angular_velocity.z > -max_roll_speed; }

  bool is_pitching_faster_than_forward_min (Vector3 angular_velocity) { return angular_velocity.x > min_pitch_speed; }
  bool is_pitching_slower_than_forward_min (Vector3 angular_velocity) { return angular_velocity.x < min_pitch_speed; }
  bool is_pitching_faster_than_backward_min (Vector3 angular_velocity) { return angular_velocity.x < -min_pitch_speed; }
  bool is_pitching_slower_than_backward_min (Vector3 angular_velocity) { return angular_velocity.x > -min_pitch_speed; }
  bool is_pitching_faster_than_forward_max (Vector3 angular_velocity) { return angular_velocity.x > max_pitch_speed; }
  bool is_pitching_slower_than_forward_max (Vector3 angular_velocity) { return angular_velocity.x < max_pitch_speed; }
  bool is_pitching_faster_than_backward_max (Vector3 angular_velocity) { return angular_velocity.x < -max_pitch_speed; }
  bool is_pitching_slower_than_backward_max (Vector3 angular_velocity) { return angular_velocity.x > -max_pitch_speed; }

  bool is_yawing_faster_than_left_min (Vector3 angular_velocity) { return angular_velocity.y < -min_yaw_speed; }
  bool is_yawing_slower_than_left_min (Vector3 angular_velocity) { return angular_velocity.y > -min_yaw_speed; }
  bool is_yawing_faster_than_right_min (Vector3 angular_velocity) { return angular_velocity.y > min_yaw_speed; }
  bool is_yawing_slower_than_right_min (Vector3 angular_velocity) { return angular_velocity.y < min_yaw_speed; }
  bool is_yawing_faster_than_left_max (Vector3 angular_velocity) { return angular_velocity.y < -max_yaw_speed; }
  bool is_yawing_slower_than_left_max (Vector3 angular_velocity) { return angular_velocity.y > -max_yaw_speed; }
  bool is_yawing_faster_than_right_max (Vector3 angular_velocity) { return angular_velocity.y > max_yaw_speed; }
  bool is_yawing_slower_than_right_max (Vector3 angular_velocity) { return angular_velocity.y < max_yaw_speed; }

  //////////////////////////////////////////////////

  // Don't let it go past 90 degrees of flip (or whatever max roll is set to).
  void limit_local_roll_angle ()
    {
    Vector3 rotation = transform.rotation.eulerAngles;
    Vector3 angular_velocity = transform.InverseTransformDirection (body.angularVelocity);

    if (angular_velocity.z > 0 && rotation.z > max_roll_left && rotation.z < 180) rotation.z = max_roll_left;
    else if (angular_velocity.z < 0 && rotation.z < max_roll_right && rotation.z > 180) rotation.z = max_roll_right;
    transform.eulerAngles = rotation;
    }

  //////////////////////////////////////////////////

  // Don't let it go past 90 degrees of flip (or whatever max pitch is set to).
  void limit_local_pitch_angle ()
    {
    Vector3 rotation = transform.rotation.eulerAngles;
    Vector3 angular_velocity = transform.InverseTransformDirection (body.angularVelocity);

    if (angular_velocity.x > 0 && rotation.x > max_pitch_forward && rotation.x < 180) rotation.x = max_pitch_forward;
    else if (angular_velocity.x < 0 && rotation.x < max_pitch_backward && rotation.x > 180) rotation.x = max_pitch_backward;
    transform.eulerAngles = rotation;
    }

  //////////////////////////////////////////////////

  // Don't let it go past 90 degrees of spin (or whatever max yaw is set to).
  // Used only for runner control scheme.
  void limit_local_yaw_angle ()
    {
    Vector3 rotation = transform.rotation.eulerAngles;
    Vector3 angular_velocity = transform.InverseTransformDirection (body.angularVelocity);

    if (angular_velocity.y > 0 && rotation.y > max_yaw_right && rotation.y < 180) rotation.y = max_yaw_right;
    else if (angular_velocity.y < 0 && rotation.y < max_yaw_left && rotation.y > 180) rotation.y = max_yaw_left;
    transform.eulerAngles = rotation;
    }

  //////////////////////////////////////////////////

  // Slow angular velocity to prevent wild spinning on the roll.
  void stabilize_local_roll ()
    {
    // convert angular velocity to local axes
    Vector3 rotation = transform.rotation.eulerAngles;
    Vector3 angular_velocity = transform.InverseTransformDirection (body.angularVelocity);

    // limit roll speed regardless of rotation
    if (is_rolling_faster_than_left_max (angular_velocity)) angular_velocity.z = max_roll_speed;
    if (is_rolling_faster_than_right_max (angular_velocity)) angular_velocity.z = -max_roll_speed;

    // stop rolling if it's really close to stabilized
    if ((rotation.z < roll_angle_stop || rotation.z > 360 - roll_angle_stop)
      && is_rolling_slower_than_left_min (angular_velocity) && is_rolling_slower_than_right_min (angular_velocity))
      {
      angular_velocity.z = 0;
      rotation.z = 0;
      transform.eulerAngles = rotation;
      }

    else if (check_roll (rotation) == Roll.half_left)
      {
      if (is_rolling_slower_than_right_min (angular_velocity)) angular_velocity.z -= roll_acceleration_near;  // speed it up a little
      else if (is_rolling_faster_than_right_min (angular_velocity)) angular_velocity.z += roll_acceleration_near;  // slow it down a little
      }
    else if (check_roll (rotation) == Roll.full_left)
      {
      if (is_rolling_slower_than_right_max (angular_velocity)) angular_velocity.z -= roll_acceleration_far;  // speed it up a lot
      }
    else if (check_roll (rotation) == Roll.half_right)
      {
      if (is_rolling_slower_than_left_min (angular_velocity)) angular_velocity.z += roll_acceleration_near;  // speed it up a little
      else if (is_rolling_faster_than_left_min (angular_velocity)) angular_velocity.z -= roll_acceleration_near;  // slow it down a little
      }
    else if (check_roll (rotation) == Roll.full_right)
      {
      if (is_rolling_slower_than_left_max (angular_velocity)) angular_velocity.z += roll_acceleration_far;  // speed it up a lot
      }

    // convert angular velocity back to global and apply
    body.angularVelocity = (transform.forward * angular_velocity.z) + (transform.up * angular_velocity.y) + (transform.right * angular_velocity.x);
    }

  //////////////////////////////////////////////////

  // Slow angular velocity to prevent wild spinning on the pitch.
  void stabilize_local_pitch ()
    {
    // convert angular velocity to local axes
    Vector3 rotation = transform.rotation.eulerAngles;
    Vector3 angular_velocity = transform.InverseTransformDirection (body.angularVelocity);

    // limit pitch speed regardless of rotation
    if (is_pitching_faster_than_forward_max (angular_velocity)) angular_velocity.x = max_pitch_speed;
    if (is_pitching_faster_than_backward_max (angular_velocity)) angular_velocity.x = -max_pitch_speed;

    // stop pitching if it's really close to stabilized
    if ((rotation.x < pitch_angle_stop || rotation.x > 360 - pitch_angle_stop)
      && is_pitching_slower_than_forward_min (angular_velocity) && is_pitching_slower_than_backward_min (angular_velocity))
      {
      angular_velocity.x = 0;
      rotation.x = 0;
      transform.eulerAngles = rotation;
      }

    else if (check_pitch (rotation) == Pitch.half_forward)
      {
      if (is_pitching_slower_than_backward_min (angular_velocity)) angular_velocity.x -= pitch_acceleration_near;  // speed it up a little
      if (is_pitching_faster_than_backward_min (angular_velocity)) angular_velocity.x += pitch_acceleration_near;  // slow it down a little
      }
    else if (check_pitch (rotation) == Pitch.full_forward)
      {
      if (is_pitching_slower_than_backward_max (angular_velocity)) angular_velocity.x -= pitch_acceleration_far;  // speed it up a lot
      }
    else if (check_pitch (rotation) == Pitch.half_backward)
      {
      if (is_pitching_slower_than_forward_min (angular_velocity)) angular_velocity.x += pitch_acceleration_near;  // speed it up a little
      if (is_pitching_faster_than_forward_min (angular_velocity)) angular_velocity.x -= pitch_acceleration_near;  // slow it down a little
      }
    else if (check_pitch (rotation) == Pitch.full_backward)
      {
      if (is_pitching_slower_than_forward_max (angular_velocity)) angular_velocity.x += pitch_acceleration_far;  // speed it up a lot
      }

    // convert angular velocity back to global and apply
    body.angularVelocity = (transform.forward * angular_velocity.z) + (transform.up * angular_velocity.y) + (transform.right * angular_velocity.x);
    }

  //////////////////////////////////////////////////

  // Slow angular velocity to prevent wild spinning on the yaw.
  // Used only for runner control scheme.
  void stabilize_local_yaw ()
    {
    // convert angular velocity to local axes
    Vector3 rotation = transform.rotation.eulerAngles;
    Vector3 angular_velocity = transform.InverseTransformDirection (body.angularVelocity);

    // limit yaw speed regardless of rotation
    if (is_yawing_faster_than_left_max (angular_velocity)) angular_velocity.y = -max_yaw_speed;
    if (is_yawing_faster_than_right_max (angular_velocity)) angular_velocity.y = max_yaw_speed;

    // stop yawing if it's really close to stabilized
    if ((rotation.y < yaw_angle_stop || rotation.z > 360 - yaw_angle_stop)
      && is_yawing_slower_than_left_min (angular_velocity) && is_yawing_slower_than_right_min (angular_velocity))
      {
      angular_velocity.y = 0;
      rotation.y = 0;
      transform.eulerAngles = rotation;
      }

    else if (check_yaw (rotation) == Yaw.half_left)
      {
      if (is_yawing_slower_than_right_min (angular_velocity)) angular_velocity.y += yaw_acceleration_near;  // speed it up a little
      else if (is_yawing_faster_than_right_min (angular_velocity)) angular_velocity.y -= yaw_acceleration_near;  // slow it down a little
      }
    else if (check_yaw (rotation) == Yaw.full_left)
      {
      if (is_yawing_slower_than_right_max (angular_velocity)) angular_velocity.y += yaw_acceleration_far;  // speed it up a lot
      }
    else if (check_yaw (rotation) == Yaw.half_right)
      {
      if (is_yawing_slower_than_left_min (angular_velocity)) angular_velocity.y -= yaw_acceleration_near;  // speed it up a little
      else if (is_yawing_faster_than_left_min (angular_velocity)) angular_velocity.y += yaw_acceleration_near;  // slow it down a little
      }
    else if (check_yaw (rotation) == Yaw.full_right)
      {
      if (is_yawing_slower_than_left_max (angular_velocity)) angular_velocity.y -= yaw_acceleration_far;  // speed it up a lot
      }

    // convert angular velocity back to global and apply
    body.angularVelocity = (transform.forward * angular_velocity.z) + (transform.up * angular_velocity.y) + (transform.right * angular_velocity.x);
    }

  //////////////////////////////////////////////////

  Roll check_roll (Vector3 rotation)
    {
    if (rotation.z > 0f && rotation.z <= half_roll_left) return Roll.half_left;
    else if (rotation.z > half_roll_left && rotation.z <= max_roll_left) return Roll.full_left;
    else if (rotation.z < 360f && rotation.z >= half_roll_right) return Roll.half_right;
    else if (rotation.z < half_roll_right && rotation.z >= max_roll_right) return Roll.full_right;
    else return Roll.none;
    }

  //////////////////////////////////////////////////

  Pitch check_pitch (Vector3 rotation)
    {
    if (rotation.x > 0f && rotation.x <= half_pitch_forward) return Pitch.half_forward;
    else if (rotation.x > half_pitch_forward && rotation.x <= max_pitch_forward) return Pitch.full_forward;
    else if (rotation.x < 360f && rotation.x >= half_pitch_backward) return Pitch.half_backward;
    else if (rotation.x < half_pitch_backward && rotation.x >= max_pitch_backward) return Pitch.full_backward;
    else return Pitch.none;
    }

  //////////////////////////////////////////////////

  Yaw check_yaw (Vector3 rotation)
    {
    if (rotation.y > 0f && rotation.y <= half_yaw_right) return Yaw.half_right;
    else if (rotation.y > half_yaw_right && rotation.y <= max_yaw_right) return Yaw.full_right;
    else if (rotation.y < 360f && rotation.y >= half_yaw_left) return Yaw.half_left;
    else if (rotation.y < half_yaw_left && rotation.y >= max_yaw_left) return Yaw.full_left;
    else return Yaw.none;
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
  }
