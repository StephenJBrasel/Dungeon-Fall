using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

public class GeneratorRoomAddition : Generator {
	
	public int roomMaxSize = 15;
	public int roomMinSize = 6;
	public int maxNumRooms = 30;

	public int squareRoomMaxSize = 12;
	public int squareRoomMinSize = 6;
	public int crossRoomMaxSize = 12;
	public int crossRoomMinSize = 6;
	
	public double cavernChance = 0.4d;
	public int cavernMaxSize = 35;

	public double wallProbability = 0.40d;
	public int neighbors = 4;

	public double squareRoomChance = 0.2d;
	public double crossRoomChance = 0.15;

	public int buildRoomAttempts = 500;
	public int placeRoomAttempts = 20;
	public int maxTunnelLength = 12;

	public bool includeShortcuts = true;
	public int shortcutAttempts = 500;
	public int shortCutLength = 5;
	public int minPathFindingDistance = 50;

	private ArrayList rooms;

	protected override void build() {
		MapGenerator.FillArea(map, new Rect(0, 0, w, h), TILE.WALL);
		RoomAddition();
	}

	public override GENERATOR_TYPE getType() {
		return GENERATOR_TYPE.ROOM_ADDITION;
	}

	public override Vector2 getStartPoint() {
		throw new NotImplementedException();
	}

	private void RoomAddition() {
		rooms = new ArrayList(maxNumRooms);
		int[,] room = generateRoom();
		Vector2 widthHeight = getRoomDimensions(room);
		int roomX = (int)((w / 2 - widthHeight.x / 2) - 1);
		int roomY = (int)((h / 2 - widthHeight.y / 2) - 1);
		addRoom(roomX, roomY, room);

		for (int i = 0; i < buildRoomAttempts; i++) {
			room = generateRoom();
			int[] res = placeRoom(room, w, h);
			roomX = res[0];
			roomY = res[1];
			int wallTile = res[2];
			int direction = res[3];
			int tunnelLength = res[4];
			if(roomX >= 1 && roomX < w - 1 && roomY >= 1 && roomY < h - 1) {
				addRoom(roomX, roomY, room);
				addTunnel(wallTile, direction, tunnelLength);
				if (rooms.Count >= maxNumRooms) break;
			}
		}
		if (includeShortcuts) addShortcuts(w, h);
	}

	private void addShortcuts(int w, int h) {
		throw new NotImplementedException();
	}

	private void addTunnel(int wallTile, int direction, int tunnelLength) {
		throw new NotImplementedException();
	}

	private int[] placeRoom(int[,] room, int w, int h) {
		int[] res = new int[5];
		int roomX = 0, roomY = 0, wallTile = 0, direction = 0, tunnelLength = 0;


		res[0] = roomX;
		res[1] = roomY;
		res[2] = wallTile;
		res[3] = direction;
		res[4] = tunnelLength;
		return res;
	}

	private void addRoom(int roomX, int roomY, int[,] room) {
		throw new NotImplementedException();
	}

	private Vector2 getRoomDimensions(int[,] room) {
		throw new NotImplementedException();
	}

	private int[,] generateRoom() {
		int[,] room;
		double choice = mapGen.rand.NextDouble();
		if(rooms.Count > 1) {
			if(choice < squareRoomChance) 
				room = generateRoomSquare();
			else if (choice < (squareRoomChance + crossRoomChance)) 
				room = generateRoomCross();
			else room = generateRoomCellularAutomata();
		} else {
			if (choice < cavernChance) room = generateRoomCavern();
			else room = generateRoomSquare();
		}
		return room;
	}

	private int[,] generateRoomCross() {
		int HorizontalWidth = mapGen.rand.Next(crossRoomMinSize + 2, crossRoomMaxSize) / 2 * 2;
		int VerticalHeight = mapGen.rand.Next(crossRoomMinSize + 2, crossRoomMaxSize) / 2 * 2;
		int HorizontalHeight = mapGen.rand.Next(crossRoomMinSize, VerticalHeight - 2) / 2 * 2;
		int VerticalWidth = mapGen.rand.Next(crossRoomMinSize, HorizontalWidth - 2) / 2 * 2;
		int[,] room = new int[HorizontalWidth, VerticalHeight];
		MapGenerator.FillArea(room, new Rect(0, 0, room.GetLength(0), room.GetLength(1)), TILE.WALL);

		int VerticalOffset = (VerticalHeight / 2 - HorizontalHeight / 2);
		MapGenerator.FillArea(room, new Rect(0, VerticalOffset, HorizontalWidth, HorizontalHeight), TILE.FLOOR);

		int HorizontalOffset = (HorizontalWidth / 2 - VerticalWidth / 2);
		MapGenerator.FillArea(room, new Rect(HorizontalOffset, 0, VerticalWidth, VerticalHeight), TILE.FLOOR);

		return room;
	}

	private int[,] generateRoomSquare() {
		int roomWidth = mapGen.rand.Next(squareRoomMinSize, squareRoomMaxSize);
		int roomHeight = mapGen.rand.Next(
			Math.Max((int)(roomWidth*0.5f), squareRoomMinSize), 
			Math.Min((int)(roomWidth*0.5), squareRoomMaxSize));
		int[,] room = new int[roomWidth, roomHeight];
		MapGenerator.CreateContainer(room, new Rect(0, 0, roomWidth, roomHeight));
		return room;
	}

	private int[,] generateRoomCellularAutomata() {
		GeneratorCellularAutomata generator = FindObjectOfType<GeneratorCellularAutomata>();
		if (generator == null) return new int[roomMinSize,roomMinSize];
		int[,] ret;

		int numSmoothIterations = generator.numSmoothIterations;
		bool autoCalculateWallDensity = generator.autoCalculateWallDensity;
		int wallCountSearchExpanse = generator.wallCountSearchExpanse;
		double randomFillPercent = generator.randomFillPercent;
		bool willCleanMap = generator.willCleanMap;
		int cleanMapMaxAttemptCount = generator.cleanMapMaxAttemptCount;
		bool degradeAttempts = generator.degradeAttempts;
		int minRooms = generator.minRooms;
		int minArea = generator.minRoomArea;

		generator.numSmoothIterations = 5;
		generator.autoCalculateWallDensity = true;
		generator.wallCountSearchExpanse = 1;
		generator.randomFillPercent = wallProbability;
		generator.willCleanMap = true;
		generator.cleanMapMaxAttemptCount = 5;
		generator.degradeAttempts = true;
		generator.minRooms = 1;
		generator.minRoomArea = 4;
		
		ret = generator.generate(roomMaxSize, roomMaxSize);

		generator.numSmoothIterations = numSmoothIterations;
		generator.autoCalculateWallDensity = autoCalculateWallDensity;
		generator.wallCountSearchExpanse = wallCountSearchExpanse;
		generator.randomFillPercent = randomFillPercent;
		generator.willCleanMap = willCleanMap;
		generator.cleanMapMaxAttemptCount = cleanMapMaxAttemptCount;
		generator.degradeAttempts = degradeAttempts;
		generator.minRooms = minRooms;
		generator.minRoomArea = minArea;

		return ret;
	}

	private int[,] generateRoomCavern() {
		throw new NotImplementedException();
	}
}
