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
			width = mapGen.rand.Next(roomMinSize, roomMaxSize);
			height = mapGen.rand.Next(roomMinSize, roomMaxSize);
			//if (mapGen.rand.Next() % 2 == 0) {
			//	width = mapGen.rand.Next(roomMinSize, roomMaxSize);
			//	height = width + mapGen.rand.Next(-(int)Mathf.Log(width), (int)Mathf.Log(width));
			//} else {
			//	height = mapGen.rand.Next(roomMinSize, roomMaxSize);
			//	width = height + mapGen.rand.Next(-(int)Mathf.Log(height), (int)Mathf.Log(height));
			//}
			int x = mapGen.rand.Next(1, w - width - 1);
			int y = mapGen.rand.Next(1, h - height - 1);

			Rect newRoom = new Rect(x, y, width, height);
			for (j = 0; j < rooms.Count; j++) 
				if (newRoom.Overlaps((Rect)rooms[j])) break;
			if (j >= rooms.Count) {
				MapGenerator.FillArea(map, newRoom, TILE.FLOOR);
				if (rooms.Count != 0) {
					mapGen.CreateHall(map, newRoom.center, ((Rect)rooms[rooms.Count - 1]).center, "L-Hall");
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
