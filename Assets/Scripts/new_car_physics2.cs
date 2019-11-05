using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class new_car_physics2 : MonoBehaviour
  {
  enum direction
    {
    up,
    down
    }

  Rigidbody body;
  public GameObject start;
  int phase = 0;

  float vertical_speed = 0f;
  float ground_distance_target = 10f;

  float forward_speed;

  float turning_rotation;

  void Start ()
    {
    body = GetComponent<Rigidbody> ();

    Vector3 arrow_rotation = start.transform.rotation.eulerAngles;
    transform.position = start.transform.position;
    }

  //////////////////////////////////////////////////

  void Update ()
    {
	  }

  //////////////////////////////////////////////////

  void FixedUpdate ()
    {
    hover ();
    drive ();
    turn ();
    move ();
    }

  //////////////////////////////////////////////////

  void move ()
    {
    //body.MovePosition (body.position + transform.forward * body.velocity);
    //body.MoveRotation (body.rotation + Quaternion.Euler(body.angularVelocity));

    body.transform.Translate (Vector3.up * vertical_speed);
    body.transform.Translate (Vector3.forward * forward_speed);
    body.transform.Rotate (body.angularVelocity);
    }

  //////////////////////////////////////////////////

  void hover ()
    {
    //Vector3 current_velocity = body.velocity;
    float ground_distance = get_ground_distance ();
    //float max_rise_speed;
    //float min_rise_speed;

    // above-near
    // hover max
    // target
    // hover min
    // below-near
    // ground

    //if (phase == 0)  // ground to below-near
    //  {
    //  max_rise_speed = .15f;
    //  if (vertical_speed < max_rise_speed) vertical_speed += .001f;
    //  if (ground_distance > ground_distance_target - 5f) phase = 1;
    //  }
    //else if (phase == 1)  // below-near to hover max
    //  {
    //  float hover_max = ground_distance_target + 1;
    //  min_rise_speed = .02f;
    //  if (vertical_speed > min_vertical_speed) vertical_speed -= .001f;
    //  if (ground_distance > hover_max) phase = 2;
    //  }
    //else if (phase == 2)  // hover max to hover min
    //  {
    //  vertical_speed = 1f;
    //  }

    if (ground_distance < ground_distance_target - .1) vertical_speed = .05f;
    else if (ground_distance > ground_distance_target + .1) vertical_speed = -.05f;
    else vertical_speed = 0;

    //body.velocity = new Vector3 (current_velocity.x, vertical_speed, current_velocity.z);
    }

  //////////////////////////////////////////////////

  void drive ()
    {
    //Vector3 current_velocity = body.velocity;
    if (Input.GetButton ("Drive Forward")) forward_speed = .8f;
    else if (Input.GetButton ("Drive Backward")) forward_speed = -.8f;
    else forward_speed = 0;
    //body.velocity = new Vector3 (current_velocity.x, current_velocity.y, forward_speed);
    }

  //////////////////////////////////////////////////

  void turn ()
    {
    Vector3 angular_velocity = body.angularVelocity;

    if (Input.GetButton ("Turn Left") && Input.GetButton ("Turn Right")) turning_rotation = 0;
    else if (Input.GetButton ("Turn Left")) turning_rotation = -1;
    else if (Input.GetButton ("Turn Right")) turning_rotation = 1;
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

  private void OnGUI ()
    {
    GUI.color = UnityEngine.Color.black;

    GUI.Label (new Rect (10, 120, 300, 20), "vertical speed: " + vertical_speed.ToString ());
    //GUI.Label (new Rect (10, 140, 300, 20), "roll: " + body.transform.rotation.eulerAngles.z.ToString ());
    //GUI.Label (new Rect (10, 160, 300, 20), "pitch: " + body.transform.rotation.eulerAngles.x.ToString ());
    }

  //////////////////////////////////////////////////

  }
