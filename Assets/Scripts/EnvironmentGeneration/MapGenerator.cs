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
	public int hallMaxWidth = 6;

	[Header("Randomness Settings")]
	public bool useRandomSeed;
	public String seed;

	[Header("Generator Settings")]
	public GENERATOR_TYPE generationType = GENERATOR_TYPE.CELLULAR_AUTOMATA;

	//private vars
	public Noise rand { get; private set; }

	private Generator generator;

	private Screen screen;
	public int[,] map { get; private set; }
	private MeshGenerator meshGen;

	#region unity
	void Start() {
		map = new int[width, height];
		rand = new Noise((uint)(useRandomSeed
			? Time.time.ToString().GetHashCode() + this.GetHashCode()
			: seed.GetHashCode()));

		Generator[] generators = GetComponentsInChildren<Generator>();
		foreach (Generator g in generators) 
			g.mapGen = this;

		generator = GetComponentInChildren<GeneratorCellularAutomata>();
		meshGen = GetComponent<MeshGenerator>();
		GenerateMap(generationType);
	}

	void Update() {
		Mouse mouse = Mouse.current;
		Keyboard keyboard = Keyboard.current;
		if (mouse == null || keyboard == null) return;
		if ((mouse.leftButton.wasReleasedThisFrame && mouse.rightButton.isPressed) || 
			(mouse.leftButton.isPressed && mouse.rightButton.wasReleasedThisFrame) || 
			(keyboard.shiftKey.isPressed && mouse.leftButton.isPressed && mouse.rightButton.isPressed)) {
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
		//testGen();
		FillArea(map, generator.generate(width - (horizontalBorderStrength * 2), height - (verticalBorderStrength * 2)), horizontalBorderStrength, verticalBorderStrength);
		meshGen.GenerateMesh(map, meshUnitSize);
	}

	private void testGen() {
		for (int x = 0, i = 0; x + width / 6 < width; x += width / 3) {
			for (int y = 0, j = 0; y + height / 6 < height; y += height / 3) {
				int tileX = x + width / 6;
				int tileY = y + height / 6;
				map[tileX, tileY] = (int)TILE.FLOOR;
				int toX = tileX + ((i - 1) * (width / 6));
				int toY = tileY + ((j - 1) * (height / 6));
				map[toX, toY] = (int)TILE.FLOOR;
				CreateMeanderingly(map, tileX, tileY, toX, toY);
				j++;
			}
			i++;
		}
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

	public HashSet<Vector2> GetLine(Vector2 from, Vector2 to) {
		HashSet<Vector2> line = new HashSet<Vector2>();
		int x = (int)from.x;
		int y = (int)from.y;
		int dx = (int)to.x - x;
		int dy = (int)to.y - y;

		bool inverted = false;
		int step = Math.Sign(dx);
		int gradient = Math.Sign(dy);

		int longest = Mathf.Abs(dx);
		int shortest = Mathf.Abs(dy);

		if(longest < shortest) {
			inverted = true;
			longest = Mathf.Abs(dy);
			shortest = Mathf.Abs(dx);

			step = Math.Sign(dy);
			gradient = Math.Sign(dx);
		}

		int gradientAccumulation = longest / 2;
		for (int i = 0; i < longest; i++) {
			line.Add(new Vector2(x, y));
			if (inverted) {
				y += step;
			} else {
				x += step;
			}

			gradientAccumulation += shortest;
			if(gradientAccumulation >= longest) {
				if (inverted) {
					x += gradient;
				} else {
					y += gradient;
				}
				gradientAccumulation -= longest;
			}
		}

		return line;
	}

	public void CreateHall(int[,] m, Vector2 v1, Vector2 v2, string method) {
		if (method.Equals("random")) {
			switch(rand.Next(0, 2)) {
				case 0:
					method = "dottedLine";
					break;
				case 1:
					method = "meander";
					break;
				case 2:
					method = "L-Hall";
					break;
			}
		}
		switch (method) {
			case "dottedLine":
				HashSet<Vector2> line = GetLine(v1, v2);
				foreach (Vector2 v in line) {
					DrawCircle(m, v, rand.Next(1, hallMaxWidth/2));
				}
				break;
			case "meander":
				CreateMeanderingly(m, v1, v2);
				break;
			default:
			case "L-Hall":
				if (rand.Next() % 2 == 1) {
					CreateHorizontally(m, v2.x, v1.x, v2.y);
					CreateVertically(m, v2.y, v1.y, v1.x);
				} else {
					CreateVertically(m, v2.y, v1.y, v1.x);
					CreateHorizontally(m, v2.x, v1.x, v2.y);
				}
				break;
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

	private void CreateMeanderingly(int[,] m, Vector2 v1, Vector2 v2, TILE tileType = TILE.FLOOR) {
		CreateMeanderingly(m, v1.x, v1.y, v2.x, v2.y);
	}
	private void CreateMeanderingly(int[,] m, float x1, float y1, float x2, float y2, TILE tileType = TILE.FLOOR) {
		if (x1 == x2) {
			CreateVertically(m, y1, y2, x1, tileType);
		} else if (y1 == y2) {
			CreateHorizontally(m, x1, x2, y1, tileType);
		} else {
			int xMin = (int)Math.Min(x1, x2);
			int xMax = (int)Math.Max(x1, x2);
			int yMin = (int)Math.Min(y1, y2);
			int yMax = (int)Math.Max(y1, y2);
			//diagonal going from lower left to upper right
			if (x1 < x2 && y1 < y2 || x2 < x1 && y2 < y1) {
				for(;;) {
					if (xMin < xMax && yMin < yMax) {
						if (rand.Next() % 2 == 1) {
							m[xMin++, yMin] = (int)tileType; // yMin instead of yMax
						} else {
							m[xMin, yMin++] = (int)tileType; // yMin++ instead of yMax--
						}
					} else if (xMin <= xMax) {
						m[xMin++, yMin] = (int)tileType; // yMin instead of yMax
					} else if (yMin <= yMax) {
						m[xMin, yMin++] = (int)tileType; // yMin++ instead of yMax--
					} else break;
				}
			} else {
				for(;;) {
					if (xMin < xMax && yMin < yMax) {
						if (rand.Next() % 2 == 1) {
							m[xMin++, yMax] = (int)tileType;
						} else {
							m[xMin, yMax--] = (int)tileType;
						}
					} else if (xMin <= xMax) {
						m[xMin++, yMax] = (int)tileType;
					} else if (yMin <= yMax) {
						m[xMin, yMax--] = (int)tileType;
					} else break;
				}
			}
		}

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

	void DrawCircle(int[,] m, Vector2 v, int r) {
		for (int x = -r; x < r; x++) {
			for (int y = -r; y < r; y++) {
				if (x * x + y * y <= r * r) {
					int realX = (int)v.x + x;
					int realY = (int)v.y + y;
					if (realX >= horizontalBorderStrength 
						&& realX < width - horizontalBorderStrength 
						&& realY >= verticalBorderStrength 
						&& realY < height - verticalBorderStrength) {
						m[realX, realY] = (int)TILE.FLOOR;
					}
				}
			}
		}
	}

	public TILE floorOrWall(int x = 0, int y = 0, float percent = 0.5f) {
		return rand.NextDouble() < percent ? TILE.FLOOR : TILE.WALL;
	}

	public static bool hasCrossNeighbor(int x, int y, int[,] map, TILE type = TILE.WALL) {
		if (map[x+1, y] == (int)type) return true;
		if (map[x-1, y] == (int)type) return true;
		if (map[x, y+1] == (int)type) return true;
		if (map[x, y-1] == (int)type) return true;
		//for (int x = (int)v.x - 1; x <= v.x + 1; x++) {
		//	for (int y = (int)v.y - 1; y <= v.y + 1; y++) {
		//		if ((x == (int)v.x || y == (int)v.y) && map[x, y] == (int)tile.wall) {
		//			return true;
		//		}
		//	}
		//}
		return false; 
	}

	public delegate TILE Del(int x = 0, int y = 0, float percent = 0.5f);
	#endregion
	//void OnDrawGizmos() {
	//	if (map != null) {
	//		for (int x = 0; x < width; x++) {
	//			for (int y = 0; y < height; y++) {
	//				Gizmos.color = (map[x, y] == 1) ? Color.black : Color.white;
	//				Vector3 pos = new Vector3(-width / 2 + x + .5f, 0, -height / 2 + y + .5f);
	//				Gizmos.DrawCube(pos, Vector3.one);
	//			}
	//		}
	//	}
	//}
}
