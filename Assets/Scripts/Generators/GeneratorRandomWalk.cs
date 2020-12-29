using System;
using UnityEngine;

public class GeneratorRandomWalk : Generator {

	[Tooltip("The cutoff number of steps in case the percent goal is never reached." +
		"\nThe final considered number is the maximum of this number and w * h * 10.")]
	public long walkIterations = 25000;
	[Range(0, 1)]
	public double percentGoal = 0.4;
	[Tooltip("The closer to 1, the closer they stay to the center of the map.")]
	[Range(0, 1)]
	public double weightCenter = 0.15;
	[Tooltip("The closer to 1, the more likely they'll walk straighter.")]
	[Range(0, 1)]
	public double weightTowardPreviousDirection = 0.7;
	[Tooltip("Rather than walk one space at a time, the road fills out in a cross pattern: " +
		"\n         (x, y+1), " +
		"\n(x-1, y), (x, y), (x+1, y)," +
		"\n         (x, y-1)")]
	public bool crossWalk = true;

	private long filled = 0;
	protected override void build() {
		MapGenerator.FillArea(map, new Rect(0, 0, w, h), TILE.WALL);
		RandomWalkAlgorithm();

	}
	public override GENERATOR_TYPE getType() {
		return GENERATOR_TYPE.RANDOM_WALK;
	}


	private void RandomWalkAlgorithm() {
		walkIterations = Math.Max(walkIterations, w * h * 10);
		filled = 0;
		String prevDirection = null, direction = null;

		int dx = 0, dy = 0;
		int drunkardX = MapGenerator.rand.Next(2, w - 2);
		int drunkardY = MapGenerator.rand.Next(2, h - 2);
		long filledGoal = (int)(h * w * percentGoal);
		double n, s, we, e, aim;

		for (int i = 0; i < walkIterations && filled < filledGoal; i++) {
			n = s = e = we = 1.0;

			if (drunkardX < w * 0.25) //left
				e += weightCenter;
			else if (drunkardX > w * 0.75) //right
				we += weightCenter;
			if (drunkardY < h * 0.25) //top
				s += weightCenter;
			else if (drunkardY > h * 0.75) //bottom
				n += weightCenter;

			if (prevDirection == "north")
				n += weightTowardPreviousDirection;
			if (prevDirection == "south")
				s += weightTowardPreviousDirection;
			if (prevDirection == "east")
				e += weightTowardPreviousDirection;
			if (prevDirection == "west")
				we += weightTowardPreviousDirection;

			//normalize them into a range between 0 and 1.
			double total = n + s + e + we;
			n /= total;
			s /= total;
			e /= total;
			we /= total;

			aim = MapGenerator.rand.NextDouble();
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
			} else if (aim < n + s + e + we) {
				dx = -1;
				dy = 0;
				direction = "west";
			} else {
				dx = 0;
				dy = 0;
			}

			///Walk.
			///Stop one tile from the edge.
			if (!pastEdge(drunkardX + dx, drunkardY + dy, 2)) {
				drunkardX += dx;
				drunkardY += dy;
				if (crossWalk) {
					if (crossPattern(drunkardX, drunkardY, isWall)) {
						crossPattern(drunkardX, drunkardY, setFloor);
					}
				} else {
					if (isWall(drunkardX + dx, drunkardY + dy)) {
						setFloor(drunkardX + dx, drunkardY + dy);
					}
				}
			}
			prevDirection = direction;
		}
	}

	private bool pastEdge(int x, int y, int border = 1) {
		return x < border || x >= w - border || y < border || y >= h - border;
	}

	private bool isWall(int x, int y) {
		return (map[x, y] == (int)TILE.WALL);
	}

	private bool setFloor(int x, int y) {
		if (isWall(x, y)) filled++;
		map[x, y] = (int)TILE.FLOOR;
		return false;
	}

	public delegate bool BoolDel(int x, int y);

	private bool crossPattern(int x1, int y1, BoolDel del, int b = 1) {
		for (int x = -b; x <= b; x++) {
			for (int y = -b; y <= b; y++) {
				if (x == y) continue;
				if (del(x1 + x, y1 + y)) return true;
			}
		}
		return false;
	}
}
