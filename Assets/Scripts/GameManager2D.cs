using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager2D : MonoBehaviour
{
    [SerializeField]
    private MapGenerator mapGenerator;
    [SerializeField]
    private Player2D player;
    [SerializeField]
    private new Camera camera;

	public void ResetPlayer() {
		Vector2 start = mapGenerator.generator.getPlacementFloorTile();
        float radius = player.GetComponent<CircleCollider2D>().radius;
		player.GetComponent<Rigidbody2D>().position = new Vector3(start.x + radius, start.y + radius, 0);
		camera.transform.SetPositionAndRotation(new Vector3(start.x, start.y, -40), Quaternion.LookRotation(Vector3.forward, Vector3.up));
	}
}
