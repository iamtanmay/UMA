using UnityEngine;
using System.Collections;

public class BulletTrailNoRigidbody : MonoBehaviour
{

    public float speed = 250;

    void Update()
    {
        // Move the trail
        transform.position = transform.position + transform.forward * Time.deltaTime * speed;
    }
}
