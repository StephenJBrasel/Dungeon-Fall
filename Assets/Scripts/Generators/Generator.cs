using System;
using UnityEngine;

[Serializable]
public abstract class Generator : MonoBehaviour {

	protected int w = 100;
	protected int h = 100;
	protected int[,] map;
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
	public int this[Vector2 v] {
		get {
			return map[(int)v.x, (int)v.y];
		}
	}
}
