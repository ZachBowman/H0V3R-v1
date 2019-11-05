using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class obstacle_script : MonoBehaviour
  {
  bool obstacles_on = true;

  public GameObject car;

  public GameObject obstacle1;
  public GameObject obstacle2;
  public List<GameObject> obstacles = new List<GameObject> ();

  void Start ()
    {
    // Remove any existing obstacle objects from the placement zone.
    foreach (Transform child in this.transform)
      {
      Destroy (child.gameObject);
      }

    // Create a new random obstacle.
    if (obstacles_on && Random.Range (0, 1000) >= 500)
      {
      GameObject obstacle_to_instantiate;
      int r = Random.Range (0, 2);
      if (r == 0) obstacle_to_instantiate = obstacle1;
      else obstacle_to_instantiate = obstacle2;

      GameObject new_obstacle = Instantiate (obstacle_to_instantiate, this.transform.position, Quaternion.identity);
      Vector3 rotation = new_obstacle.transform.rotation.eulerAngles;
      rotation.x -= 90;
      rotation.z += 180;
      new_obstacle.transform.eulerAngles = rotation;
      new_obstacle.transform.parent = this.transform;
      }
    }

  // Update is called once per frame
  void Update ()
    {
  	}
  }
