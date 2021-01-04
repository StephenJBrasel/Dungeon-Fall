using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraInterpolate2D : MonoBehaviour
{
    public Transform target;
    [Range(0,1)]
    public float lerpSpeed = 0.5f;
    public float zoomMin = -10f;
    public float zoomMax = -70f;


    private float zoom = -40;
    private Vector3 velocity;

    // Update is called once per frame
    void Update()
    {
        velocity = new Vector3((target.position.x - transform.position.x), (target.position.y - transform.position.y), 0) * lerpSpeed * Time.deltaTime;
        if (Mathf.Approximately(velocity.x, 0)) velocity.x = 0;
        if (Mathf.Approximately(velocity.y, 0)) velocity.y = 0;
        transform.position += velocity;
        zoom = Mathf.Clamp(zoom, zoomMin, zoomMax);

        Vector3 v = transform.position + velocity;
        v.z = Mathf.Max(Mathf.Min(v.z, transform.position.z - zoomMax), transform.position.z - zoomMin);
    }
}
