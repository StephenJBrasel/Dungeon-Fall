using System;
using System.Collections.Generic;
using UnityEngine;

public class Room : IComparable<Room> {
	public HashSet<Vector2> tiles;
	public HashSet<Vector2> edgeTiles;
	public HashSet<Room> connectedRooms;
	public int roomSize;
	public bool isAccessibleFromMainRoom;
	public bool isMainRoom;

	public Room() { }

	public Room(HashSet<Vector2> roomTiles, int[,] map) {
		tiles = roomTiles;
		roomSize = tiles.Count;
		connectedRooms = new HashSet<Room>();
		edgeTiles = new HashSet<Vector2>();
		foreach (Vector2 tile in tiles) {
			if (MapGenerator.hasCrossNeighbor((int)tile.x, (int)tile.y, map, TILE.WALL)) edgeTiles.Add(tile);
		}
	}

	public void setAccessibleFromMainRoom() {
		if (!isAccessibleFromMainRoom) {
			isAccessibleFromMainRoom = true;
			foreach (Room r in connectedRooms)
				r.setAccessibleFromMainRoom();
		}
	}

	public static void ConnectRooms(Room roomA, Room roomB) {
		roomA.connectedRooms.Add(roomB);
		roomB.connectedRooms.Add(roomA);
		if (roomA.isAccessibleFromMainRoom || roomB.isAccessibleFromMainRoom) 
			roomA.setAccessibleFromMainRoom();
	}

	public int CompareTo(Room other) {
		return other.roomSize.CompareTo(roomSize);
	}

	public bool isConnected(Room otherRoom) {
		return connectedRooms.Contains(otherRoom);
	}
}
