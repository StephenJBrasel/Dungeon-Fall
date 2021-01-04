using System;
using System.Collections.Generic;
using UnityEngine;

public class Room : IComparable<Room> {
	public bool isAccessibleFromHub { get; private set; }
	public int connectedRoomCount { get { return connectedRooms.Count; } }
	public int roomSize { get; private set; }
	public HashSet<Vector2> edgeTiles { get; private set; }
	private HashSet<Vector2> tiles;
	private HashSet<Room> connectedRooms;
	private Vector2 closestPoint = Vector2.negativeInfinity;

	public Room() {
		connectedRooms = new HashSet<Room>();
		edgeTiles = new HashSet<Vector2>();
	}

	public Room(int[,] m, HashSet<Vector2> roomTiles) {
		tiles = roomTiles;
		Init(m);
	}

	public Room(int[,] m, Rect area) {
		for (float x = area.x; x < (area.x + area.width); x++)
			for (float y = area.y; y < (area.y + area.height); y++)
				tiles.Add(new Vector2(x, y));
		Init(m);
	}

	private void Init(int[,] m) {
		roomSize = tiles.Count;
		connectedRooms = new HashSet<Room>();
		edgeTiles = new HashSet<Vector2>();
		foreach (Vector2 tile in tiles) {
			if (MapGenerator.hasCrossNeighbor((int)tile.x, (int)tile.y, m, TILE.WALL)) edgeTiles.Add(tile);
		}
		
	}

	public Vector2 centralPoint(bool resetCenter = false) {
		if (closestPoint != Vector2.negativeInfinity && !resetCenter) return closestPoint;
		double aggregateDistance, shortestDistance = double.MaxValue, a, b;
		foreach (Vector2 tileA in tiles) {
			aggregateDistance = 0.0f;
			foreach (Vector2 tileB in tiles) {
				a = (tileB.x - tileA.x);
				b = (tileB.y - tileA.y);
				aggregateDistance += a*a + b*b;
				if (aggregateDistance > shortestDistance) break;
			}
			if (shortestDistance > aggregateDistance) {
				shortestDistance = aggregateDistance;
				closestPoint = tileA;
			}
		}
		return closestPoint;
	}

	public void setAccessibleFromHub() {
		if (!isAccessibleFromHub) {
			isAccessibleFromHub = true;
			foreach (Room r in connectedRooms)
				r.setAccessibleFromHub();
		}
	}

	public static void ConnectRooms(Room roomA, Room roomB) {
		roomA.connectedRooms.Add(roomB);
		roomB.connectedRooms.Add(roomA);
		if (roomA.isAccessibleFromHub || roomB.isAccessibleFromHub) 
			roomA.setAccessibleFromHub();
	}

	public int CompareTo(Room other) {
		return other.roomSize.CompareTo(roomSize);
	}

	public bool isConnected(Room otherRoom) {
		return connectedRooms.Contains(otherRoom);
	}
}
