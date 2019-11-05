using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class section_script : MonoBehaviour
  {
  public GameObject car;
  public GameObject next_section;
  public int number;
  private bool track_extended;
  private bool passed;
  private float vanishing_distance = 512;
  private Vector3 size;

  void Start ()
    {
    track_extended = false;
    passed = false;

    size = GetComponent<Collider> ().bounds.size;
    }

  void Update ()
    {
    // Extend track
    if (!track_extended && transform.position.z - car.transform.position.z < vanishing_distance)
      {
      GameObject new_section = Instantiate (next_section,
        new Vector3 (this.transform.position.x, this.transform.position.y, this.transform.position.z + size.z),
        Quaternion.identity);

      section_script new_section_script = new_section.GetComponent<section_script> ();
      new_section_script.number = number + 1;
      new_section.name = "section" + new_section_script.number.ToString();
      track_extended = true;
      }


    if (transform.position.z < car.transform.position.z)
      {
      passed = true;
      }

    // Remove track
    if (passed && transform.position.z - car.transform.position.z < -vanishing_distance) Destroy (gameObject);
    }
  }
