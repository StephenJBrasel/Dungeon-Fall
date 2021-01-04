using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class Generator : MonoBehaviour {

	[HideInInspector]
	public MapGenerator mapGen;
	
	protected int w = 100;
	protected int h = 100;
	protected int[,] map;
	protected List<Vector2> floorTiles = null;
	void Start() {
		mapGen = GetComponentInParent<MapGenerator>();
	}
	public int[,] generate(int w, int h) {
		map = new int[w, h];
		this.w = w;
		this.h = h;
		build();
		return map;
	}

	public int[,] regenerate(int[,] map) {
		this.map = map;
		w = map.GetLength(0);
		h = map.GetLength(1);
		build();
		return map;
	}

	protected abstract void build();
	public abstract GENERATOR_TYPE getType();
	public virtual Vector2 getPlacementFloorTile() {
		GetFloorTiles(true);
		floorTiles.Sort(delegate (Vector2 v1, Vector2 v2) {
			return neighborCount((int)v1.x, (int)v1.y, 2).CompareTo(neighborCount((int)v2.x, (int)v2.y, 2));
		});
		int i = 0;
		for (; i < floorTiles.Count-1; i++) {
			Vector2 test = floorTiles[i + 1];
			if (neighborCount((int)test.x, (int)test.y) > 0) break;
		}
		int index = mapGen.rand.Next(i);
		Vector2 v = floorTiles[index];
		float unitSize = mapGen.meshUnitSize;
		return new Vector2(-w / 2f + v.x * unitSize + unitSize / 2f, -h / 2f + v.y * unitSize + unitSize / 2f);
	}

	private List<Vector2> GetFloorTiles(bool resetFloorTiles = false) {
		if (floorTiles != null && !resetFloorTiles) return floorTiles;
		floorTiles = new List<Vector2>();
		for (int x = 0; x < w; x++)
			for (int y = 0; y < h; y++)
				if (map[x, y] == (int)TILE.FLOOR)
					floorTiles.Add(new Vector2(x, y));
		return floorTiles;
	}

	public int this[Vector2 v] {
		get {
			return map[(int)v.x, (int)v.y];
		}
	}

	//TODO: Optimize to use 4 double four loops (?) to avoid the self-check (neighborX != x || neighborY != y).
	// Returns a number between 0 and wallCountSearcHExpanse*2
	protected int neighborCount(int x, int y, int offset = 1, TILE type = TILE.WALL) {
		int count = 0;
		for (int neighborX = x - offset; neighborX <= x + offset; neighborX++) {
			for (int neighborY = y - offset; neighborY <= y + offset; neighborY++) {
				if (IsInMap(neighborX, neighborY)) { 
					if (neighborX != x || neighborY != y) {
						count += map[neighborX, neighborY] == (int)type ? 1 : 0;
					} 
				} else count++;
			}
		}
		return count;
	}

	protected bool IsInMap(int x, int y, int border = 0) {
		return x >= border && x < w-border && y >= border && y < h-border;
	}
}
