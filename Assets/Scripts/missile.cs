using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class missile : MonoBehaviour
  {
  float speed = 6f;

  Rigidbody body;

  // Use this for initialization
  void Start ()
    {
    body = GetComponent<Rigidbody> ();
    }

  // Update is called once per frame
  void FixedUpdate ()
    {
    Vector3 movement = transform.forward * speed;
    body.MovePosition (body.position + movement);
    }

  void OnCollisionEnter (Collision collision)
    {
    Destroy (gameObject);
    }
  }
