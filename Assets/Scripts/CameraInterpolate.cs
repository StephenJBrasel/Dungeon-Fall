using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraInterpolate : MonoBehaviour
{
    public Transform target;
    [Range(0,1)]
    public float lerpSpeed = 0.5f;

    private Vector3 velocity;

    // Update is called once per frame
    void Update()
    {
        velocity = new Vector3((target.position.x - transform.position.x), 0, (target.position.z - transform.position.z)) * lerpSpeed;
    }

	void FixedUpdate() {
        if (Mathf.Approximately(velocity.x, 0)) velocity.x = 0;
        if (Mathf.Approximately(velocity.z, 0)) velocity.z = 0;
        transform.position += velocity * Time.fixedDeltaTime;
	}
}
