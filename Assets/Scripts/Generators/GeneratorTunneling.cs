using System.Collections;
using UnityEngine;

public class GeneratorTunneling : Generator {
	public int roomMaxSize = 15;
	public int roomMinSize = 6;
	public int maxRooms = 30;

	protected override void build() {
		MapGenerator.FillArea(map, new Rect(0, 0, w, h), TILE.WALL);
		TunnelingAlgorithm();
	}
	public override GENERATOR_TYPE getType() {
		return GENERATOR_TYPE.TUNNELING;
	}

	private void TunnelingAlgorithm() {
		ArrayList rooms = new ArrayList(maxRooms);

		for (int i = 0, j; i < maxRooms; i++) {
			int width, height;
			width = MapGenerator.rand.Next(roomMinSize, roomMaxSize);
			height = MapGenerator.rand.Next(roomMinSize, roomMaxSize);
			//if (MapGenerator.rand.Next(1) == 0) {
			//	width = MapGenerator.rand.Next(roomMinSize, roomMaxSize);
			//	height = width + MapGenerator.rand.Next(-(int)Mathf.Log(width), (int)Mathf.Log(width));
			//} else {
			//	height = MapGenerator.rand.Next(roomMinSize, roomMaxSize);
			//	width = height + MapGenerator.rand.Next(-(int)Mathf.Log(height), (int)Mathf.Log(height));
			//}
			int x = MapGenerator.rand.Next(1, w - width - 1);
			int y = MapGenerator.rand.Next(1, h - height - 1);

			Rect newRoom = new Rect(x, y, width, height);
			for (j = 0; j < rooms.Count; j++) 
				if (newRoom.Overlaps((Rect)rooms[j])) break;
			if (j >= rooms.Count) {
				MapGenerator.FillArea(map, newRoom, TILE.FLOOR);
				if (rooms.Count != 0) {
					MapGenerator.CreateHall(map, newRoom, (Rect)rooms[rooms.Count - 1]);
				}
				rooms.Add(newRoom);
			}
		}
	}

	[ExecuteInEditMode]
	private void OnValidate() {
		roomMaxSize = Mathf.Clamp(roomMaxSize, roomMinSize, int.MaxValue);
		roomMinSize = Mathf.Clamp(roomMinSize, 1, roomMaxSize);
		maxRooms = Mathf.Clamp(maxRooms, 1, int.MaxValue);
	}
}
