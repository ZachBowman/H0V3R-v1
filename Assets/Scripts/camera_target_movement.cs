using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class camera_target_movement : MonoBehaviour
  {
  public GameObject player;
  Vector3 position_offset = new Vector3 (0f, 0f, 0f);
  float camera_move_x_speed;
  float camera_move_y_speed;
  float camera_move_z_speed;
  float camera_move_speed;

  private void Start ()
    {
    car_physics car_physics_script = player.GetComponent<car_physics> ();
    if (car_physics_script.runner_controls)
      {
      camera_move_x_speed = 6f;
      camera_move_y_speed = 30f;
      camera_move_z_speed = 30f;
      }
    else
      {
      camera_move_x_speed = 30f;
      camera_move_y_speed = 30f;
      camera_move_z_speed = 30f;
      }
    }

  void FixedUpdate ()
    {
    // old movement
    //transform.position = Vector3.Lerp (transform.position, player.transform.position + position_offset,
    //  camera_move_speed * Time.deltaTime);

    // new movement
    Vector3 position = transform.position;
    position.x = Mathf.Lerp (transform.position.x, player.transform.position.x + position_offset.x, camera_move_x_speed * Time.deltaTime);
    position.y = Mathf.Lerp (transform.position.y, player.transform.position.y + position_offset.y, camera_move_y_speed * Time.deltaTime);
    position.z = Mathf.Lerp (transform.position.z, player.transform.position.z + position_offset.z, camera_move_z_speed * Time.deltaTime);
    transform.position = position;

    Vector3 rotation = player.transform.rotation.eulerAngles;
    rotation.x = 0;
    rotation.z = 0;

    transform.rotation = Quaternion.Lerp (transform.rotation, Quaternion.Euler (rotation.x, rotation.y, rotation.z),
      6f * Time.deltaTime);
    }
  }
