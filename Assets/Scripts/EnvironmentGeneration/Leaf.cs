using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rectangle {
	public int x;
	public int y;
	public int w;
	public int h;

	public Rectangle(int x, int y, int w, int h) {
		this.x = x;
		this.y = y;
		this.w = w;
		this.h = h;
	}

	public Vector2 center() {
		int centerX = (int)((x + w)*0.5f);
		int centerY = (int)((y + h)*0.5f);
		return new Vector2(centerX, centerY);
	}

	public bool intersect(Rectangle other) {
		return (x <= other.w && w >= other.x
			&& y <= other.h && h >= other.y);
	}

	public Rect getRect() {
		return new Rect(x, y, w, h);
	}
} 

public class Leaf {
	public int x { get; private set; }
	public int y { get; private set; }
	public int width { get; private set; }
	public int height { get; private set; }
	public int MIN_LEAF_SIZE {
		get;
		private set;
	} = 10;
	public Leaf childLeft { get; private set; }
	public Leaf childRight { get; private set; }
	private Rect hall;
	private System.Random rand = new System.Random();
	private Rectangle room;
	public MapGenerator generator;

	public Leaf(int x, int y, int width, int height) {
		this.x = x;
		this.y = y;
		this.width = width;
		this.height = height;
	}

	public Rectangle getRoom() {
		if (room != null) return room;
		else {
			Rectangle room1 = childLeft != null ? childLeft.getRoom() : null;
			Rectangle room2 = childRight != null ? childRight.getRoom() : null;
			if (childLeft == null && childRight == null)
				return null; //TODO figure out if this has any unintended consequences.
			else if (!Rect.Equals(null, room1) && !Rect.Equals(null, room2)) {
				return rand.Next(0, 1) == 1 ? room1 : room2;
			} else if (Rect.Equals(null, room2)) return room1;
			else if (Rect.Equals(null, room1)) return room2;
			return null;
		}
	}

	public void setRoom(Rectangle room) {
		this.room = room;
	}

	public bool SplitLeaf() {
		if (childLeft != null || childRight != null)
			return false; // this leaf has already been split

		bool splitHorizontally = rand.Next() % 2 == 0;
		if (width / height >= 1.25f)
			splitHorizontally = false;
		else if (height / width >= 1.25f)
			splitHorizontally = true;

		int max = splitHorizontally ? height - MIN_LEAF_SIZE : width - MIN_LEAF_SIZE;
		if (max <= MIN_LEAF_SIZE) return false;

		int split = rand.Next(MIN_LEAF_SIZE, max);

		if (splitHorizontally) {
			childLeft = new Leaf(x, y, width, split);
			childRight = new Leaf(x, y + split, width, height - split);
		} else {
			childLeft = new Leaf(x, y, split, height);
			childRight = new Leaf(x + split, y, width - split, height);
		}
		return true;
	}
}
