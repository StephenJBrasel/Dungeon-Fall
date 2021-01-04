using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private MapGenerator mapGenerator;
    [SerializeField]
    private Player player;
    [SerializeField]
    private new Camera camera;

	public void ResetPlayer() {
		Vector2 start = mapGenerator.generator.getPlacementFloorTile();
        float radius = player.GetComponent<SphereCollider>().radius;
		player.GetComponent<Rigidbody>().position = new Vector3(start.x + radius, -5f + radius, start.y + radius);
		camera.transform.SetPositionAndRotation(new Vector3(start.x, 35f, start.y), Quaternion.LookRotation(Vector3.down, Vector3.forward));
	}
}
