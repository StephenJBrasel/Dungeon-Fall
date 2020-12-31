using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField]
    private float speed = 10f;
    public InputActionMap moveActionMap;

    new Rigidbody rigidbody;
    private Vector3 velocity;

    // Start is called before the first frame update
    void Start()
    {
		moveActionMap.Enable();
        rigidbody = GetComponent<Rigidbody>();
        rigidbody.freezeRotation = true;
	}

	// Update is called once per frame
	void Update()
    {
        Vector2 moveDirection = new Vector2();
        foreach (InputAction i in moveActionMap.actions)
            moveDirection += i.ReadValue<Vector2>();
        velocity = new Vector3(moveDirection.x * speed, 0, moveDirection.y * speed);
        if (Mathf.Approximately(velocity.x, 0)) velocity.x = 0;
        if (Mathf.Approximately(velocity.z, 0)) velocity.z = 0;
        //velocity = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical") * speed);

    }

	void FixedUpdate() {
        rigidbody.MovePosition(rigidbody.position + velocity * Time.fixedDeltaTime);
	}
}
