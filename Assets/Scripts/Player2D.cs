using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player2D : MonoBehaviour
{
    [SerializeField]
    private float speed = 10f;
    public InputActionMap moveActionMap;

    new Rigidbody2D rigidbody;
    private Vector2 velocity;

    // Start is called before the first frame update
    void Start()
    {
		moveActionMap.Enable();
        rigidbody = GetComponent<Rigidbody2D>();
        rigidbody.freezeRotation = true;
	}

	// Update is called once per frame
	void Update()
    {
        Vector2 moveDirection = new Vector2();
        foreach (InputAction i in moveActionMap.actions)
            moveDirection += i.ReadValue<Vector2>();
        velocity = new Vector2(moveDirection.x, moveDirection.y).normalized * speed;
        if (Mathf.Approximately(velocity.x, 0)) velocity.x = 0;
        if (Mathf.Approximately(velocity.y, 0)) velocity.y = 0;
        //velocity = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical") * speed);

    }

	void FixedUpdate() {
        rigidbody.MovePosition(rigidbody.position + velocity * Time.fixedDeltaTime);
	}
}
