using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class new_car_physics : MonoBehaviour
  {
  enum direction
    {
    up,
    down
    }

  Rigidbody body;
  public GameObject start;

  // vertical
  float ground_distance;
  float ground_distance_target = 10f;
  float vertical_speed = 0f;
  const float vertical_speed_target = 0f;         // Speed that keeps car level
  float vertical_speed_margin;                    // Max +/- deviation from vertical_speed_optimal
  const float vertical_speed_margin_min = .005f;  // Slowest vertical speed for level floating
  float vertical_speed_min;// = vertical_speed_optimal - .005f;
  float vertical_speed_max;// = vertical_speed_optimal + .005f;
  const float vertical_acceleration_min = .0001f;
  float vertical_acceleration;
  direction bob = direction.up;

  //// forward
  //float forward_speed = 0f;
  //float forward_speed_max = 70f;
  //float forward_acceleration = 1f;

  //// rotation
  //float turning_rotation = 0f;
  //float rotation_multiplier = 1.3f;
  //float max_roll_speed = 2f;
  //float roll_deceleration_multiplier = .98f;

  void Start ()
    {
    body = GetComponent<Rigidbody> ();

    Vector3 arrow_rotation = start.transform.rotation.eulerAngles;
    //transform.rotation = Quaternion.Euler (new Vector3 (transform.rotation.x, arrow_rotation.y, transform.rotation.z));
    transform.position = start.transform.position;

    //body.angularVelocity = transform.forward * 50f;
    }

  //////////////////////////////////////////////////

  void Update ()
    {
	  }

  //////////////////////////////////////////////////

  void FixedUpdate ()
    {
    //Vector3 new_global_rotation = transform.rotation.eulerAngles;
    //Vector3 new_local_rotation = body.transform.rotation.eulerAngles;

    hover ();
    //drive ();
    //turn ();
    //limit_global_roll_angle ();
    //limit_global_pitch_angle ();
    //stabilize_global_roll ();
    //stabilize_global_pitch ();
    move ();
    }

  //////////////////////////////////////////////////

  void move ()
    {
    body.MovePosition (body.position + body.velocity);
    }

  //////////////////////////////////////////////////

  void hover ()
    {
    Vector3 current_velocity = body.velocity;
    ground_distance = get_ground_distance ();

    float distance_to_target_elevation = Mathf.Abs (ground_distance - ground_distance_target);
    vertical_speed_margin = vertical_speed_margin_min + (distance_to_target_elevation * .01f);
    vertical_speed_min = vertical_speed_target - vertical_speed_margin;
    vertical_speed_max = vertical_speed_target + vertical_speed_margin;

    vertical_acceleration = vertical_acceleration_min;// + (distance_to_target_elevation * .001f);

    if (bob == direction.up)
      {
      if (ground_distance <= ground_distance_target && vertical_speed < vertical_speed_max)
        {
        vertical_speed += vertical_acceleration;
        }
      if (ground_distance >= ground_distance_target) bob = direction.down;
      }
    else if (bob == direction.down)
      {
      if (ground_distance >= ground_distance_target && vertical_speed > vertical_speed_min)
        {
        vertical_speed -= vertical_acceleration;
        }
      if (ground_distance <= ground_distance_target) bob = direction.up;
      }

    Mathf.Clamp (vertical_speed, vertical_speed_min, vertical_speed_max);

    body.velocity = new Vector3 (current_velocity.x, vertical_speed, current_velocity.z);
    }

  //////////////////////////////////////////////////

  void drive ()
    {
    //if (Input.GetButton ("Drive Forward"))
    //  {
    //  if (forward_speed < forward_speed_max) forward_speed += forward_acceleration;
    //  }
    //else if (Input.GetButton ("Drive Backward"))
    //  {
    //  if (forward_speed > -forward_speed_max) forward_speed -= forward_acceleration;
    //  }
    //else
    //  {
    //  if (forward_speed > -.1f && forward_speed < .1f) forward_speed = 0f;
    //  if (forward_speed != 0f) forward_speed = forward_speed * 0.9f;
    //  }

    //Vector3 current_velocity = body.velocity;
    //Vector3 direction = transform.forward;
    //direction.y = 0;
    //Vector3 new_velocity = forward_speed * direction.normalized;
    //new_velocity.y = current_velocity.y;
    //body.velocity = new_velocity;
    }

  //////////////////////////////////////////////////

  void turn ()
    {
    //Vector3 angular_velocity = body.angularVelocity;

    //if (Input.GetButton ("Turn Left") && Input.GetButton ("Turn Right")) turning_rotation = 0;
    //else if (Input.GetButton ("Turn Left")) turning_rotation = -rotation_multiplier;
    //else if (Input.GetButton ("Turn Right")) turning_rotation = rotation_multiplier;
    //else turning_rotation = 0f;
    //if (Input.GetButton ("Drive Backward")) turning_rotation *= -1f;

    //if (Input.GetButton ("Drive Forward") || Input.GetButton ("Drive Backward"))
    //  {
    //  angular_velocity.y = turning_rotation;
    //  body.angularVelocity = angular_velocity;
    //  }
    //else
    //  {
    //  angular_velocity.y = 0f;
    //  body.angularVelocity = angular_velocity;
    //  }
    }

  //////////////////////////////////////////////////

  void limit_global_roll_angle ()
    {
    Vector3 rotation = transform.rotation.eulerAngles;
    Vector3 angular_velocity = body.angularVelocity;

    if (angular_velocity.z > 0 && rotation.z > 90) rotation.z = 90;
    else if (angular_velocity.z < 0 && rotation.z < 270 && rotation.z > 180) rotation.z = 270;
    transform.eulerAngles = rotation;
    }

  //////////////////////////////////////////////////

  void limit_global_pitch_angle ()
    {
    Vector3 rotation = transform.rotation.eulerAngles;
    Vector3 angular_velocity = body.angularVelocity;

    if (angular_velocity.x > 0 && rotation.x > 85) rotation.x = 85;
    else if (angular_velocity.x < 0 && rotation.x < 275 && rotation.x > 180) rotation.x = 275;
    transform.eulerAngles = rotation;
    }

  //////////////////////////////////////////////////

  void stabilize_global_roll ()
    {
    //Vector3 rotation = transform.rotation.eulerAngles;
    //Vector3 angular_velocity = body.angularVelocity;

    //// limit roll speed
    //if (angular_velocity.z > max_roll_speed) angular_velocity.z = max_roll_speed;
    //if (angular_velocity.z < -max_roll_speed) angular_velocity.z = -max_roll_speed;

    //// rolling counter clockwise
    //if (angular_velocity.z > 0f && rotation.z > 0f && rotation.z <= 180)
    //  {
    //  //float angle_diff = rotation.z;
    //  angular_velocity.z *= roll_deceleration_multiplier;
    //  if (angular_velocity.z < 0.05f && angular_velocity.z > -0.05f) angular_velocity.z = 0f;
    //  }

    //body.angularVelocity = angular_velocity;
    }

  //////////////////////////////////////////////////

  void stabilize_global_pitch ()
    {
    //Vector3 rotation = transform.rotation.eulerAngles;
    //Vector3 angular_velocity = body.angularVelocity;

    //if (angular_velocity.x > 5) angular_velocity.x = 5;
    //if (angular_velocity.x < -5) angular_velocity.x = -5;

    //if (rotation.x > 0 && rotation.x < 180)
    //  {
    //  angular_velocity.x = -0.1f;
    //  }
    //else if (rotation.x < 360 && rotation.x >= 180)
    //  {
    //  angular_velocity.x = 0.1f;
    //  }

    //body.angularVelocity = angular_velocity;
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
    //external_force = Vector3.up * force;
    }

  //////////////////////////////////////////////////

  private void OnGUI ()
    {
    GUI.color = UnityEngine.Color.black;

    //GUI.Label (new Rect (10,  20, 300, 20), "forward speed: " + forward_speed.ToString ());
    //GUI.Label (new Rect (10,  40, 300, 20), "vertical speed: " + vertical_speed.ToString ());
    //GUI.Label (new Rect (10,  60, 300, 20), "distance_to_ground: " + ground_distance.ToString ());
    //GUI.Label (new Rect (10,  80, 300, 20), "vertical_speed: " + vertical_speed.ToString ());
    //GUI.Label (new Rect (10, 100, 300, 20), "bob: " + bob.ToString ());
    GUI.Label (new Rect (10, 120, 300, 20), "yaw: " + body.transform.rotation.eulerAngles.y.ToString ());
    GUI.Label (new Rect (10, 140, 300, 20), "roll: " + body.transform.rotation.eulerAngles.z.ToString ());
    GUI.Label (new Rect (10, 160, 300, 20), "pitch: " + body.transform.rotation.eulerAngles.x.ToString ());
    //GUI.Label (new Rect (10, 180, 300, 20), "turning rotation: " + turning_rotation.ToString ());
    }

  //////////////////////////////////////////////////

  }
