using System;
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
	RANDOM_WALK,
	ROOM_ADDITION,
	CITY_BUILDINGS,
	ROOMY_MAZE,
	MESSY_BSPTREE
}

public class MapGenerator : MonoBehaviour {

	//public vars
	[Header("Sizing Settings")]
	public bool resizeWithResolution;
	public int width = 100;
	public int height = 100;
	public int verticalBorderStrength = 0;
	public int horizontalBorderStrength = 0;
	public float meshUnitSize = 1f;

	[Header("Randomness Settings")]
	public bool useRandomSeed;
	public String seed;

	[Header("Generator Settings")]
	public GENERATOR_TYPE generationType = GENERATOR_TYPE.CELLULAR_AUTOMATA;

	//private vars
	public static System.Random rand { get; private set; }

	private Generator generator;
	private Screen screen;
	private static int[,] map;
	private MeshGenerator meshGen;

	#region unity
	void Start() {
		map = new int[width, height];
		rand = new System.Random(useRandomSeed ? Time.time.ToString().GetHashCode() : seed.GetHashCode());
		generator = GetComponentInChildren<GeneratorCellularAutomata>();
		meshGen = GetComponent<MeshGenerator>();
		GenerateMap(generationType);
	}

	void Update() {
		Mouse mouse = Mouse.current;
		if (mouse == null) return;
		if (mouse.leftButton.wasReleasedThisFrame) {
			GenerateMap(generationType);
		}
	}

	#endregion

	#region utilities
	private void GenerateMap(GENERATOR_TYPE genType) {
		if (width != map.GetLength(0)) width = map.GetLength(0);
		if (height != map.GetLength(1)) height = map.GetLength(1);

		if (generator.getType() != genType) {
			switch (genType) {
				default:
				case GENERATOR_TYPE.CELLULAR_AUTOMATA:	generator = GetComponentInChildren<GeneratorCellularAutomata>();	break;
				case GENERATOR_TYPE.TUNNELING:			generator = GetComponentInChildren<GeneratorTunneling>();			break;
				case GENERATOR_TYPE.BSPTREE:			generator = GetComponentInChildren<GeneratorBSPTree>();				break;
				case GENERATOR_TYPE.RANDOM_WALK:		generator = GetComponentInChildren<GeneratorRandomWalk>();			break;
				case GENERATOR_TYPE.ROOM_ADDITION:		generator = GetComponentInChildren<GeneratorRoomAddition>();		break;
				case GENERATOR_TYPE.CITY_BUILDINGS:		generator = GetComponentInChildren<GeneratorCityBuildings>();		break;
				case GENERATOR_TYPE.ROOMY_MAZE:			generator = GetComponentInChildren<GeneratorRoomyMaze>();			break;
				case GENERATOR_TYPE.MESSY_BSPTREE:		generator = GetComponentInChildren<GeneratorMessyBSPTree>();		break;
			}
		}
		FillArea(map, new Rect(0, 0, width, height), TILE.WALL);
		FillArea(map, generator.generate(width-(horizontalBorderStrength*2), height-(verticalBorderStrength*2)), horizontalBorderStrength, verticalBorderStrength);
		meshGen.GenerateMesh(map, meshUnitSize);
	}

	public static void FillArea(int[,] m, Rect area, TILE tileType = TILE.WALL) {
		for (int x = (int)area.x; x < (int)(area.x + area.width); x++)
			for (int y = (int)area.y; y < (int)(area.y + area.height); y++)
				m[x, y] = (int)tileType;
	}

	public static void FillArea(int[,] m, int[,] area, int startX = 0, int startY = 0) {
		for (int x = startX; x < area.GetLength(0); x++)
			for (int y = startY; y < area.GetLength(1); y++)
				m[x, y] = area[x-startX, y-startY];
	}

	public static void FillArea(int[,] m, HashSet<Vector2> area, TILE tileType = TILE.WALL) {
		foreach (Vector2 v in area) 
			m[(int)v.x, (int)v.y] = (int)tileType;
	}

	public static void CreateHall(int[,] m, Rect r1, Rect r2) {
		Vector2 newCenter = r1.center;
		Vector2 prevCenter = r2.center;
		if (rand.Next(1) == 1) {
			CreateHorizontally(m, prevCenter.x, newCenter.x, prevCenter.y);
			CreateVertically(m, prevCenter.y, newCenter.y, newCenter.x);
		} else {
			CreateVertically(m, prevCenter.y, newCenter.y, newCenter.x);
			CreateHorizontally(m, prevCenter.x, newCenter.x, prevCenter.y);
		}
	}

	private static void CreateVertically(int[,] m, float y1, float y2, float x, TILE tileType = TILE.FLOOR) {
		for (int y = (int)Math.Min(y1, y2); y <= Math.Max(y1, y2); y++)
			m[(int)x, y] = (int)tileType;
	}

	private static void CreateHorizontally(int[,] m, float x1, float x2, float y, TILE tileType = TILE.FLOOR) {
		for (int x = (int)Math.Min(x1, x2); x <= Math.Max(x1, x2); x++)
			m[x, (int)y] = (int)tileType;
	}

	public static void CreateContainer(int[,] m, Rect area, int borderStrength = 1, TILE inner = TILE.FLOOR, TILE outer = TILE.WALL) {
		int bd = borderStrength - 1;
		int xEnd = (int)(area.x + area.width);
		int yEnd = (int)(area.y + area.height);
		int widthCheck = (int)(area.width - 1 - bd);
		int heightCheck = (int)(area.height - 1 - bd);
		for (int x = (int)area.x; x < xEnd; x++) {
			for (int y = (int)area.y; y < yEnd; y++) {
				if (borderStrength > 0 && (x <= bd || x >= widthCheck || y <= bd || y >= heightCheck)) {
					m[x, y] = (int)outer;
				} else {
					m[x, y] = (int)inner;
				}
			}
		}
	}

	public static void CreateContainer(int[,] m, Rect area, Del delegateFill, float percent, int borderStrength = 1, TILE outer = TILE.WALL) {
		int bd = borderStrength - 1;
		int xEnd = (int)(area.x + area.width);
		int yEnd = (int)(area.y + area.height);
		int widthCheck = (int)(area.width - 1 - bd);
		int heightCheck = (int)(area.height - 1 - bd);
		for (int x = (int)area.x; x < xEnd; x++) {
			for (int y = (int)area.y; y < yEnd; y++) {
				if (borderStrength > 0 && (x <= bd || x >= widthCheck || y <= bd || y >= heightCheck)) {
					m[x, y] = (int)outer;
				} else {
					m[x, y] = (int)delegateFill(x, y, percent);
				}
			}
		}
	}

	public static TILE floorOrWall(int x = 0, int y = 0, float percent = 0.5f) {
		return MapGenerator.rand.NextDouble() < percent ? TILE.FLOOR : TILE.WALL;
	}

	public delegate TILE Del(int x = 0, int y = 0, float percent = 0.5f);
	#endregion

}
