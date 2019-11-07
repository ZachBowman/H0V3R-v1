using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hover_Engine : MonoBehaviour
  {
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

  public Vertical_Physics vertical_physics;

  Rigidbody body;
  car_physics physics;

  float ground_distance;
  float ground_distance_target = 10f;
  float ground_distance_min;
  float ground_distance_max;
  float deceleration_below_target = 6f;
  float deceleration_above_target = 14f;
  float hover_max = 10.4f;
  float hover_min = 9.6f;
  float vertical_speed;
  static float vertical_speed_hover;
  float rise_speed_max = vertical_speed_hover + 4f;
  float sink_speed_max = vertical_speed_hover - 4f;
  float rise_speed_min = vertical_speed_hover + 1f;
  float sink_speed_min = vertical_speed_hover - 1f;
  float vertical_acceleration_large;
  float vertical_acceleration_small;
  float vertical_speed_min;
  float vertical_speed_max;
  Direction vertical_direction = Direction.up;

  //////////////////////////////////////////////////

  void Start ()
    {
    // TODO: Add vertical movement for runner controls
    //if (physics.runner_controls)
    vertical_physics = Vertical_Physics.hover_velocity;

    body = GetComponent<Rigidbody> ();
    physics = GetComponent<car_physics> ();

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
    }

  //////////////////////////////////////////////////

  void FixedUpdate ()
    {
    Hover ();
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

    //if (jump) body.AddForce (Vector3.up * 10f, ForceMode.Acceleration);
    //if (external_force.y > 0f) external_force.y += Physics.gravity.y;
    //if (external_force.y < 0f) external_force.y = 0f;
    //if (external_force.y > 0f)
    //  {
    //  body.AddForce (external_force, ForceMode.Acceleration);
    //  external_force.y -= 10f;
    //  }
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

    GUI.Label (new Rect (10, 100, 300, 20), "vertical speed: " + vertical_speed.ToString ());
    GUI.Label (new Rect (10, 120, 300, 20), "distance_to_ground: " + ground_distance.ToString ());
    //GUI.Label (new Rect (10, 100, 300, 20), "bob: " + bob.ToString ());
    }
  }
