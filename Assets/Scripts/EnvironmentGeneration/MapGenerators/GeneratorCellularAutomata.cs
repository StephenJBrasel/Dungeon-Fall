using System;
using System.Collections.Generic;
using UnityEngine;

public class GeneratorCellularAutomata : Generator {

	public int numSmoothIterations = 5;
	public bool autoCalculateWallDensity = true;
	[ConditionalHide("autoCalculateWallDensity", true, false)]
	public int minWallCount = 4;
	[ConditionalHide("autoCalculateWallDensity", true, false)]
	public int maxWallCount = 4;
	[Tooltip("ADVANCED USERS ONLY " +
		"\n\nThis is the number of adjacent tiles from the current tile that the smoothing function will search for walls. " +
		"\nThis number is exponential, if 1 is given, 9 tiles will be searched, 2 => 25, 3 => 49, etc.")]
	public int wallCountSearchExpanse = 1;
	[Range(0.0f, 1.0f)]
	public double randomFillPercent = 0.40d;
	[Tooltip("")]
	public bool willCleanMap = true;
	[ConditionalHide("willCleanMap", true, true)]
	[Tooltip("The number of tries to generate a desired room.")]
	public int cleanMapMaxAttemptCount = 5;
	[ConditionalHide("willCleanMap", true, true)]
	[Tooltip("True by default, if false will return an unfloodfilled cellular automata generation." +
		"\nOtherwise, resets the attempt count, decreases both minRooms and minArea (iteratively, never less than 1), and restarts the generation process")]
	public bool degradeAttempts = true;
	[ConditionalHide("willCleanMap", true, true)]
	[Tooltip("The minimum number of rooms to be left with. Must be greater than 1.")]
	public int minRooms = 1;
	[ConditionalHide("willCleanMap", true, true)]
	[Tooltip("The (exclusive) minimum number of floor tiles that the room with the largest number of floor tiles has to have.")]
	public int minRoomArea = 25;
	[ConditionalHide("willCleanMap", true, true)]
	[Tooltip("The (exclusive) minimum number of wall tiles that the pillar/island with the largest number of floor tiles has to have.")]
	public int minIslandArea = 50;

	private int originalminRooms;
	private int originalminArea;
	private int eliminateSmallRoomsAttempts = 0;

	public override GENERATOR_TYPE getType() {
		return GENERATOR_TYPE.CELLULAR_AUTOMATA;
	}

	public GeneratorCellularAutomata() {
	}

	protected override void build() {
		originalminArea = minRoomArea;
		originalminRooms = minRooms;
		eliminateSmallRoomsAttempts = 0;
		bool ConditionsMet = false;
		if (mapGen == null) mapGen = GetComponentInParent<MapGenerator>();
		while (!ConditionsMet) {
			// Initialize Paramteres
			if (autoCalculateWallDensity)
				minWallCount = maxWallCount = 2 * wallCountSearchExpanse * (wallCountSearchExpanse + 1);
			// Randomize the map
			MapGenerator.CreateContainer(map, new Rect(0, 0, w, h), mapGen.floorOrWall, (float)randomFillPercent);
			// Smooth the map
			for (int i = 0; i < numSmoothIterations; i++)
				SmoothMap();
			// Floodfill Smaller areas
			if (willCleanMap){
				if(++eliminateSmallRoomsAttempts >= cleanMapMaxAttemptCount) {
					if (degradeAttempts) {
						eliminateSmallRoomsAttempts = 0;
						minRooms = Math.Max(minRooms - 1, 1);
						minRoomArea = Math.Max(minRoomArea - 1, 1);
						if (minRooms + minRoomArea <= 2) break;
					} else break;
				}
				ConditionsMet = CleanMap();
			}
			else ConditionsMet = true;
		}
		minRoomArea = originalminArea;
		minRooms = originalminRooms;
	}

	private void SmoothMap() {
		int[,] next = new int[w, h];
		for (int x = 0; x < w; x++) {
			for (int y = 0; y < h; y++) {
				if (x == 0 || x == w - 1 || y == 0 || y == h - 1) {
					next[x, y] = (int)TILE.WALL;
					continue;
				}
				int neighborWallTiles = GetSurroundingWallCount(x, y, wallCountSearchExpanse);
				if (neighborWallTiles < minWallCount) {
					next[x, y] = (int)TILE.FLOOR;
				} else if (neighborWallTiles > maxWallCount) {
					next[x, y] = (int)TILE.WALL;
				}
			}
		}
		map = next;
	}

	//TODO: Optimize to use 4 double four loops (?) to avoid the self-check (neighborX != x || neighborY != y).
	// Returns a number between 0 and wallCountSearcHExpanse*2
	private int GetSurroundingWallCount(int x, int y, int offset = 1) {
		int wallCount = 0;
		for (int neighborX = x - offset; neighborX <= x + offset; neighborX++) {
			for (int neighborY = y - offset; neighborY <= y + offset; neighborY++) {
				if (IsInMap(neighborX, neighborY)) {
					if (neighborX != x || neighborY != y) {
						wallCount += map[neighborX, neighborY] == (int)TILE.WALL ? 1 : 0;
					}
				} else {
					wallCount++;
				}
			}
		}
		return wallCount;
	}

	private bool IsInMap(int x, int y) {
		return x >= 0 && x < w && y >= 0 && y < h;
	}

	private bool IsInMap(Vector2 v) {
		return IsInMap((int)v.x, (int)v.y);
	}

	public override Vector2 getStartPoint() {
		throw new NotImplementedException();
	}

	private bool CleanMap() {
		List<HashSet<Vector2>> regions = GetRegions();
		List<HashSet<Vector2>> rooms = GetTypedRegions(regions, TILE.FLOOR);
		List<HashSet<Vector2>> islands = GetTypedRegions(regions, TILE.WALL);
		//fill the appropriate floor regions in.
		int i;
		if (rooms.Count >= minRooms) {
			int j, indexMin, roomsCount = rooms.Count;
			for (i = roomsCount - 1; i >= 0; i--)
				if (rooms[i].Count < minRoomArea) break;
			indexMin = Math.Max(0, i);
			if (indexMin < roomsCount - 1 && (roomsCount - indexMin) >= minRooms) {
				for (j = 0; j <= i; j++) {
					MapGenerator.FillArea(map, rooms[0]);
					rooms.RemoveAt(0);
				}
				List<Room> survivingRooms = new List<Room>();
				for(j = 0; j < rooms.Count; j++) {
					survivingRooms.Add(new Room(rooms[j], map));
				}
				survivingRooms.Sort();
				survivingRooms[0].isMainRoom = true;
				survivingRooms[0].isAccessibleFromMainRoom = true;

				ConnectClosestRooms(survivingRooms);
			} else return false;
		} else return false;
		i = islands.Count-2; // Always leave the outermost group of walls.
		if (i >= 0) {
			do {
				if (islands[i].Count <= minIslandArea)
					MapGenerator.FillArea(map, islands[i], TILE.FLOOR);
			} while (--i >= 0);
		}
		return true;
	}

	void ConnectClosestRooms(List<Room> allRooms, bool forceAccessibilityFromMainRoom = false) {

		List<Room> roomListA = new List<Room>();
		List<Room> roomListB = new List<Room>();

		if (forceAccessibilityFromMainRoom) {
			foreach (Room r in allRooms) {
				if (r.isAccessibleFromMainRoom) {
					roomListB.Add(r);
				} else {
					roomListA.Add(r);
				}
			}
		} else {
			roomListA = allRooms;
			roomListB = allRooms;
		}

		int bestDistance = int.MaxValue;
		Vector2 bestTileA = new Vector2();
		Vector2 bestTileB = new Vector2();
		Room bestRoomA = new Room();
		Room bestRoomB = new Room();
		bool possibleConnectionFound = false;
		foreach (Room roomA in roomListA) {
			if (!forceAccessibilityFromMainRoom) {
				possibleConnectionFound = false;
				if(roomA.connectedRooms.Count > 0) {
					continue;
				}
			}

			foreach (Room roomB in roomListB) {

				if (roomA == roomB || roomA.isConnected(roomB)) {
					continue;
				}

				foreach(Vector2 tileA in roomA.edgeTiles) {
					foreach (Vector2 tileB in roomB.edgeTiles) {
						int distanceBetweenRooms = (int)(Mathf.Pow(tileA.x - tileB.x, 2f) + Mathf.Pow(tileA.y - tileB.y, 2f));
						if(distanceBetweenRooms < bestDistance || !possibleConnectionFound) {
							bestDistance = distanceBetweenRooms;
							possibleConnectionFound = true;
							bestTileA = tileA;
							bestTileB = tileB;
							bestRoomA = roomA;
							bestRoomB = roomB;
						}
					}
				}
			}
			if (possibleConnectionFound && !forceAccessibilityFromMainRoom) {
				CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
			}
		}
		if (possibleConnectionFound && forceAccessibilityFromMainRoom) {
			CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
			ConnectClosestRooms(allRooms, true);
		}

		if (!forceAccessibilityFromMainRoom) {
			ConnectClosestRooms(allRooms, true);
		}

	}

	private void CreatePassage(Room roomA, Room roomB, Vector2 tileA, Vector2 tileB) {
		Room.ConnectRooms(roomA, roomB);
		mapGen.CreateHall(map, tileA, tileB, "dottedLine");
	}

	Vector3 Vector2ToWorldPoint(Vector2 v) {
		return new Vector3(-w / 2 + 0.5f + v.x, 2, -h / 2 + 0.5f + v.y);
	}

	private List<HashSet<Vector2>> GetRegions() {
		List<HashSet<Vector2>> rooms = new List<HashSet<Vector2>>();
		int[,] mapFlags = new int[w, h];
		for (int x = 0; x < w; x++) {
			for (int y = 0; y < h; y++) {
				if (mapFlags[x,y] == 1) continue;
				HashSet<Vector2> room = GetRegion(mapFlags, x, y);
				if(room != null) rooms.Add(room);
			}
		}
		rooms.Sort(delegate (HashSet<Vector2> r1, HashSet<Vector2> r2) {
			return r1.Count.CompareTo(r2.Count);
		});
		return rooms;
	}

	private List<HashSet<Vector2>> GetTypedRegions(List<HashSet<Vector2>> regions, TILE type) {
		List<HashSet<Vector2>> typedRegion = new List<HashSet<Vector2>>();
		foreach(HashSet<Vector2> region in regions) {
			foreach(Vector2 v in region) {
				if (this[v] == (int)type) typedRegion.Add(region);
				break;
			}
		}
		return typedRegion;
	}

	private HashSet<Vector2> GetRegion(int[,] mapFlags, int x, int y) {
		TILE type = (TILE)map[x, y];
		LinkedList<Vector2> currFlood = new LinkedList<Vector2>();
		currFlood.AddFirst(new Vector2(x, y));
		mapFlags[x, y] = 1;
		LinkedListNode<Vector2> curr = currFlood.First;
		while (curr != null) {
			Vector2 val = curr.Value;
			ConditionallyAddToList(currFlood, mapFlags, val.x, val.y + 1, type);
			ConditionallyAddToList(currFlood, mapFlags, val.x, val.y - 1, type);
			ConditionallyAddToList(currFlood, mapFlags, val.x + 1, val.y, type);
			ConditionallyAddToList(currFlood, mapFlags, val.x - 1, val.y, type);
			curr = curr.Next;
		}
		return new HashSet<Vector2>(currFlood);
	}

	private void ConditionallyAddToList(LinkedList<Vector2> flood, int[,] mapFlags, float x, float y, TILE type) {
		if (IsInMap((int)x, (int)y) && map[(int)x, (int)y] == (int)type && mapFlags[(int)x,(int)y] == 0) {
			mapFlags[(int)x, (int)y] = 1;
			flood.AddLast(new Vector2(x, y)); // add it to the flood
		}
	}

	[ExecuteInEditMode]
	private void OnValidate() {
		minWallCount = Mathf.Clamp(minWallCount, 1, maxWallCount);
		maxWallCount = Mathf.Clamp(maxWallCount, minWallCount, (4 * wallCountSearchExpanse * (wallCountSearchExpanse + 1)));
		wallCountSearchExpanse = Mathf.Clamp(wallCountSearchExpanse, 1, int.MaxValue);
		minRooms = Mathf.Clamp(minRooms, 1, int.MaxValue);
	}
}
