using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum TILE {
	FLOOR = 0,
	WALL = 1
}

public enum GENERATOR_TYPE {
	CELLULAR_AUTOMATA,
	TUNNELING,
	BSPTREE, 
	DRUNKARDS_WALK
}

public class MapGenerator : MonoBehaviour {

	//public vars
	[Header("Sizing Settings")]
	public bool resizeWithResolution;
	public int width = 100;
	public int height = 100;

	[Header("Randomness Settings")]
	public bool useRandomSeed;
	public String seed;

	[Header("Generator Settings")]
	public GENERATOR_TYPE generationType = GENERATOR_TYPE.CELLULAR_AUTOMATA;
	
	[Header("Shared")]
	[Tooltip("Shared by Tunneling and BSPTree algorithms")]
	public int roomMaxSize = 15;
	[Tooltip("Shared by Tunneling and BSPTree algorithms")]
	public int roomMinSize = 6;

	[Header("Cellular Automata")]
	[Range(0, 1)]
	public double randomFillPercent = 0.60d;
	public int numSmoothIterations = 5;
	public bool autoCalculateWallDensity = true;
	public int minWallCount = 4;
	public int maxWallCount = 4;

	[Range(0, 10)]
	[Tooltip("ADVANCED USERS ONLY " +
		"\n\nThis is the number of adjacent tiles from the current tile that the smoothing function will search for walls. " +
		"\nThis number is exponential, if 1 is given, 9 tiles will be searched, 2 => 25, 3 => 49, etc.")]
	public int wallCountSearchExpanse = 1;

	[Header("Tunneling")]
	public int maxRooms = 30;

	[Header("BSPTree")]
	public int leafMaxSize = 24;

	[Header("Drunkard's Walk")]
	[Tooltip("The cutoff number of steps in case the percent goal is never reached." +
		"\nThe final considered number is the maximum of this number and width * height * 10.")]
	public long walkIterations = 25000;
	[Range(0, 1)]
	public double percentGoal = 0.4;
	[Tooltip("The closer to 1, the closer they stay to the center of the map.")]
	[Range(0, 1)]
	public double weightCenter = 0.15;
	[Tooltip("The closer to 1, the more likely they'll walk straighter.")]
	[Range(0, 1)]
	public double weightTowardPreviousDirection = 0.7;

	//private vars
	private System.Random pseudoRandom;
	private Screen screen;
	private int[,] map;

	#region unity
	void Start() {
		GenerateMap();
	}

	void Update() {
		var mouse = Mouse.current;
		if (mouse == null) return;
		if (mouse.leftButton.wasReleasedThisFrame) {
			GenerateMap();
		}
		//if(resizeWithResolution && (Screen.width != width || Screen.height != height)) {
		//	width = Screen.width;
		//	height = Screen.height;
		//}
	}
	#endregion

	private void GenerateMap() {

		map = new int[width, height];
		pseudoRandom = new System.Random(
			useRandomSeed ? Time.time.ToString().GetHashCode() : seed.GetHashCode());
		if(generationType == GENERATOR_TYPE.CELLULAR_AUTOMATA) {
			RandomFillMap();
			if (autoCalculateWallDensity) 
				minWallCount = maxWallCount = 2 * wallCountSearchExpanse * (wallCountSearchExpanse + 1);
			for (int i = 0; i < numSmoothIterations; i++)
				SmoothMap();
			return;
		}

		FillArea(new Rect(0, 0, width, height), TILE.WALL);
		switch (generationType) {
			default:
			case GENERATOR_TYPE.TUNNELING:
				TunnelingAlgorithm();
				break;
			case GENERATOR_TYPE.BSPTREE:
				BSPAlgorithm();
				break;
			case GENERATOR_TYPE.DRUNKARDS_WALK:
				DrunkardsWalkAlgorithm();
				break;
		}
	}

	#region drunkards walk
	private void DrunkardsWalkAlgorithm() {
		walkIterations = Math.Max(walkIterations, width * height * 10);
		long filled = 0;
		String prevDirection = null, direction = null;

		int dx = 0, dy = 0;
		int drunkardX = pseudoRandom.Next(2, width - 2);
		int drunkardY = pseudoRandom.Next(2, height - 2);
		long filledGoal = (int)(height * width * percentGoal);
		double n, s, w, e, aim;

		for (int i = 0; i < walkIterations && filled < filledGoal; i++) {
			n = s = e = w = 1.0;

			if (drunkardX < width * 0.25) //left
				e += weightCenter;
			else if (drunkardX > width * 0.75) //right
				w += weightCenter;
			if (drunkardY < height * 0.25) //top
				s += weightCenter;
			else if (drunkardY > height * 0.75) //bottom
				n += weightCenter;

			if (prevDirection == "north")
				n += weightTowardPreviousDirection;
			if (prevDirection == "south")
				s += weightTowardPreviousDirection;
			if (prevDirection == "east")
				e += weightTowardPreviousDirection;
			if (prevDirection == "west")
				w += weightTowardPreviousDirection;

			//normalize them into a range between 0 and 1.
			double total = n + s + e + w;
			n /= total;
			s /= total;
			e /= total;
			w /= total;

			aim = pseudoRandom.NextDouble();
			if (aim < n) {
				dx = 0;
				dy = -1;
				direction = "north";
			} else if (aim < n + s) {
				dx = 0;
				dy = 1;
				direction = "south";
			} else if (aim < n + s + e) {
				dx = 1;
				dy = 0;
				direction = "east";
			} else if (aim < n + s + e + w) {
				dx = -1;
				dy = 0;
				direction = "west";
			} else {
				dx = 0;
				dy = 0;
			}

			///Walk.
			///Stop one tile from the edge.
			if(((drunkardX + dx) > 1) 
				&& ((drunkardX + dx) < (width - 1)) 
				&& ((drunkardY + dy) > 1) 
				&& ((drunkardY + dy) < width - 1)) {
				drunkardX += dx;
				drunkardY += dy;
				if(map[drunkardX, drunkardY] == (int)TILE.WALL) {
					map[drunkardX, drunkardY] = (int)TILE.FLOOR;
					filled++;
				}
			}
			prevDirection = direction;
		}
	}

	#endregion

	#region bsptree
	private void BSPAlgorithm() {
		LinkedList<Leaf> leaves = new LinkedList<Leaf>();
		Leaf rootLeaf = new Leaf(1, 1, width, height);
		leaves.AddFirst(rootLeaf);
		bool splitSuccessfully = true;
		while (splitSuccessfully) {
			splitSuccessfully = false;
			LinkedListNode<Leaf> curr = leaves.First;
			while(curr != null){
				Leaf l = curr.Value;
				if (l.childLeft == null && l.childRight == null) {
					if (l.width > leafMaxSize
						|| l.height > leafMaxSize
						|| pseudoRandom.NextDouble() > 0.8d) {
						if (l.SplitLeaf()) {
							leaves.AddLast(l.childLeft);
							leaves.AddLast(l.childRight);
							splitSuccessfully = true;
						}
					}
				}
				curr = curr.Next;
			}
		}
		CreateRooms(rootLeaf);
	}

	public void CreateRooms(Leaf leaf) {
		if (leaf != null) {
			if (leaf.childLeft != null || leaf.childRight != null) {
				if (leaf.childLeft != null) CreateRooms(leaf.childLeft);
				if (leaf.childRight != null) CreateRooms(leaf.childRight);
				if (leaf.childLeft != null && leaf.childRight != null) {
					if (leaf.childLeft.getRoom() != null && leaf.childRight.getRoom() != null) {
						CreateHall(leaf.childLeft.getRoom().getRect(), leaf.childRight.getRoom().getRect());
					}
				}
			} else {
				//create rooms in the end branches of the bsp tree
				int w = pseudoRandom.Next(roomMinSize, (Math.Min(roomMaxSize, leaf.width - 1)));
				int h = pseudoRandom.Next(roomMinSize, (Math.Min(roomMaxSize, leaf.height - 1)));
				int x = pseudoRandom.Next(leaf.x, leaf.x + (leaf.width - 1) - w);
				int y = pseudoRandom.Next(leaf.y, leaf.y + (leaf.height - 1) - h);
				leaf.setRoom(new Rectangle(x, y, w, h));
				FillArea(leaf.getRoom().getRect(), TILE.FLOOR);
			}
		}
	}
	#endregion

	#region tunneling
	private void TunnelingAlgorithm() {
		ArrayList rooms = new ArrayList(maxRooms);

		for (int i = 0; i < maxRooms; i++) {
			int w = pseudoRandom.Next(roomMinSize, roomMaxSize);
			int h = pseudoRandom.Next(roomMinSize, roomMaxSize);
			int x = pseudoRandom.Next(0, width - w - 1);
			int y = pseudoRandom.Next(0, height - h - 1);

			Rect newRoom = new Rect(x, y, w, h);
			int j;
			for (j = 0; j < rooms.Count; j++) {
				if (newRoom.Overlaps((Rect)rooms[j])) {
					break;
				}
			}
			if (j >= rooms.Count) {
				FillArea(newRoom, TILE.FLOOR);
				Vector2 newCenter = newRoom.center;
				//All rooms after the first room connect to the previous room.
				if (rooms.Count != 0) {
					CreateHall(newRoom, (Rect)rooms[rooms.Count - 1]);
				}
				rooms.Add(newRoom);
			}
		}
	}
	#endregion

	#region utilities
	private void FillArea(Rect newRoom, TILE tileType) {
		for (int x = (int)newRoom.x; x < (int)(newRoom.x + newRoom.width); x++)
			for (int y = (int)newRoom.y; y < (int)(newRoom.y + newRoom.height); y++)
				map[x, y] = (int)tileType;
	}

	public void CreateHall(Rect r1, Rect r2) {
		switch (generationType) {
			default:
			case GENERATOR_TYPE.BSPTREE:
				Vector2 newCenter = r1.center;
				Vector2 prevCenter = r2.center;
				if (pseudoRandom.Next(1) == 1) {
					CreateHorizontalTunnel(prevCenter.x, newCenter.x, prevCenter.y);
					CreateVerticalTunnel(prevCenter.y, newCenter.y, newCenter.x);
				} else {
					CreateVerticalTunnel(prevCenter.y, newCenter.y, newCenter.x);
					CreateHorizontalTunnel(prevCenter.x, newCenter.x, prevCenter.y);
				}
				break;
		}
	}

	private void CreateVerticalTunnel(float y1, float y2, float x) {
		for (int y = (int)Math.Min(y1, y2); y <= Math.Max(y1, y2); y++)
			map[(int)x, y] = (int)TILE.FLOOR;
	}

	private void CreateHorizontalTunnel(float x1, float x2, float y) {
		for (int x = (int)Math.Min(x1, x2); x <= Math.Max(x1, x2); x++)
			map[x, (int)y] = (int)TILE.FLOOR;
	}
	#endregion

	#region Cellular Automata
	void RandomFillMap() {

		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				if (x == 0 || x == width - 1 || y == 0 || y == height - 1) {
					map[x, y] = (int)TILE.WALL;
				} else {
					map[x, y] = (int)(pseudoRandom.NextDouble() < randomFillPercent ? TILE.FLOOR : TILE.WALL);
				}
			}
		}
	}

	void SmoothMap() {
		int[,] next = new int[width, height];
		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				if (x == 0 || x == width - 1 || y == 0 || y == height - 1) {
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
	int GetSurroundingWallCount(int x, int y, int offset = 1) {
		int wallCount = 0;
		for (int neighborX = x - offset; neighborX <= x + offset; neighborX++) {
			for (int neighborY = y - offset; neighborY <= y + offset; neighborY++) {
				if (neighborX >= 0 && neighborX < width && neighborY >= 0 && neighborY < height) {
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

	#endregion

	#region EditorStuff
	[ExecuteInEditMode]
	private void OnValidate() {
		roomMaxSize = Mathf.Clamp(roomMaxSize, roomMinSize, (width < height ? width : height) - 2);
		roomMinSize = Mathf.Clamp(roomMinSize, 1, roomMaxSize);
		maxRooms = Mathf.Clamp(maxRooms, 1, (width * height) / (roomMinSize));
		minWallCount = Mathf.Clamp(minWallCount, 1, maxWallCount);
		maxWallCount = Mathf.Clamp(maxWallCount, minWallCount, (4 * wallCountSearchExpanse * (wallCountSearchExpanse + 1)));
	}

	void OnDrawGizmos() {
		if (map != null) {
			for (int x = 0; x < width; x++) {
				for (int y = 0; y < height; y++) {
					Gizmos.color = (map[x, y] == 0) ? Color.black : Color.white;
					Vector3 pos = new Vector3(-width / 2 + x + 0.5f, 0, -height / 2 + y + 0.5f);
					Gizmos.DrawCube(pos, Vector3.one);
				}
			}
		}
	}
	#endregion
}
