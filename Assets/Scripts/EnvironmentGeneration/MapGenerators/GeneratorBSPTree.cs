using System;
using System.Collections.Generic;
using UnityEngine;

public class GeneratorBSPTree : Generator {

	public int roomMaxSize = 15;
	public int roomMinSize = 6;
	public int leafMaxSize = 24;

	public override GENERATOR_TYPE getType() {
		return GENERATOR_TYPE.BSPTREE;
	}

	protected override void build() {
		MapGenerator.FillArea(map, new Rect(0, 0, w, h), TILE.WALL);
		BSPAlgorithm();
	}
	// hi! 

	public override Vector2 getStartPoint() {
		throw new NotImplementedException();
	}

	#region bsptree
	private void BSPAlgorithm() {
		LinkedList<Leaf> leaves = new LinkedList<Leaf>();
		Leaf rootLeaf = new Leaf(1, 1, w, h);
		leaves.AddFirst(rootLeaf);
		bool splitSuccessfully = true;
		while (splitSuccessfully) {
			splitSuccessfully = false;
			LinkedListNode<Leaf> curr = leaves.First;
			while (curr != null) {
				Leaf l = curr.Value;
				if (l.childLeft == null && l.childRight == null) {
					if (l.width > leafMaxSize
						|| l.height > leafMaxSize
						|| mapGen.rand.NextDouble() > 0.8d) {
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
						mapGen.CreateHall(map, leaf.childLeft.getRoom().getRect().center, leaf.childRight.getRoom().getRect().center, "L-Hall");
					}
				}
			} else {
				//create rooms in the end branches of the bsp tree
				int w = mapGen.rand.Next(roomMinSize, (Math.Min(roomMaxSize, leaf.width - 1)));
				int h = mapGen.rand.Next(roomMinSize, (Math.Min(roomMaxSize, leaf.height - 1)));
				int x = mapGen.rand.Next(leaf.x, leaf.x + (leaf.width - 1) - w);
				int y = mapGen.rand.Next(leaf.y, leaf.y + (leaf.height - 1) - h);
				leaf.setRoom(new Rectangle(x, y, w, h));
				MapGenerator.FillArea(map, leaf.getRoom().getRect(), TILE.FLOOR);
			}
		}
	}

	#endregion
}
