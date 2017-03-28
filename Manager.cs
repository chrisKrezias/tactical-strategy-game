using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
using System.Threading;


public class Manager : MonoBehaviour {

	public static Manager instance;

	public GameObject TilePrefab;
	public GameObject UserPlayerPrefab;
	public GameObject DevaUserPlayerPrefab;
	public GameObject AssaultUserPlayerPrefab;
	public GameObject ApothecaryUserPlayerPrefab;
	public GameObject AIPlayerPrefab;
	public GameObject DoorPrefab;
	public GameObject JammingBeaconPrefab;
	public GameObject shootyAIPlayerPrefab;
	public GameObject bigAIPlayerPrefab;

	public string mapXml;
	public int mapSize;
	public List <List<Tile>> map = new List<List<Tile>>();
	public List <Player> players = new List<Player>();
	public List <Door> doors = new List<Door>();
	public int currentPlayerIndex = 0;
	Transform mapTransform;
	public int numofdeaduserplayers=0;
	public int numofdeadaiplayers=0;
	public int numofobjectives=0;
	public int numofoptional=0;
	public int specialobjectives = 0;

	void Awake(){
		instance = this;
		mapTransform = transform.FindChild ("Map");
	}



	// Use this for initialization
	void Start () {
		generateMap ();
		if (SceneManager.GetActiveScene ().name == "gameScene") {
			generateUserPlayers ();
			generateAIPlayers ();
			generateDoors ();
		} else if (SceneManager.GetActiveScene ().name == "gamesScene1") {
			generateForestUserPlayers ();
			generateForestAIPlayers ();
			generateForestDoors ();
		} else if (SceneManager.GetActiveScene ().name == "gamesScene2") {
			generateBridgeUserPlayers ();
			generateBridgeAIPlayers ();
		} else if (SceneManager.GetActiveScene ().name == "gamesScene2newposition") {
			generateBridgeUserPlayersnewposition ();
			generateBridgeAIPlayers ();
		} else if (SceneManager.GetActiveScene ().name == "gamesScene2weaken") {
			generateBridgeUserPlayersnewposition ();
			generateWeakenBridgeAIPlayers ();
		}
	}

	// Update is called once per frame
	void Update () {
		if (players [currentPlayerIndex].HP > 0) {
			if (players [currentPlayerIndex].playerindex == 1||players [currentPlayerIndex].playerindex == 3) {
				highlightPlayerTile (players [currentPlayerIndex].gridPosition);
			}
			players [currentPlayerIndex].TurnUpdate ();
		} else {
			players [currentPlayerIndex].gridPosition.y = players [currentPlayerIndex].gridPosition.y - 50;
			nextTurn ();
		}
	}

	void OnGUI(){
		if (players [currentPlayerIndex].HP > 0)players [currentPlayerIndex].TurnOnGUI ();
		if (SceneManager.GetActiveScene ().name == "gameScene")
			GUI.Label (new Rect (10, 10, 300, 20), "Interact with the 3 jamming spheres to teleport outside");
		else if (SceneManager.GetActiveScene ().name == "gamesScene1") {
			GUI.Label (new Rect (10, 10, 400, 20), "Open the SouthEast doors to reach the bridge directly");
			GUI.Label (new Rect (10, 20, 300, 20), "OR");
			GUI.Label (new Rect (10, 30, 600, 20), "Interact with the South sphere to release a gas that will weaken the enemies in the bridge");
			GUI.Label (new Rect (10, 40, 300, 20), "BUT");
			GUI.Label (new Rect (10, 50, 600, 20), "Is deadly to you so use the Eastern door that leads to a safe path inside but further");
		} else {
			GUI.Label (new Rect (10, 10, 400, 20), "Purge the bridge of the alien threat");
		}
	}



	public void nextTurn(){
		if (currentPlayerIndex + 1 < players.Count) {
			if (players [currentPlayerIndex].animationMoveBool) players [currentPlayerIndex].animationMoveBool = false;
			if (players [currentPlayerIndex].animationAttackBool) players [currentPlayerIndex].animationAttackBool = false;
			currentPlayerIndex++;
		} else {
			if (players [currentPlayerIndex].animationMoveBool) players [currentPlayerIndex].animationMoveBool = false;
			if (players [currentPlayerIndex].animationAttackBool) players [currentPlayerIndex].animationAttackBool = false;
			currentPlayerIndex = 0;
		}
	}

	public void highlightPlayerTile(Vector2 originLocation){
		map [(int)originLocation.x] [(int)originLocation.y].visual.transform.GetComponent<Renderer>().materials[0].color = Color.green;
	}

	public void highlightTilesAt(Vector2 originLocation, Color highlightColor, int distance, bool ignorePlayers, bool ignoreDoors, string highlightIndex){
		List<Tile> highlightedTiles = new List<Tile> ();
		if (ignorePlayers == true)
			highlightedTiles = TileHighlight.FindHighlight (map [(int)originLocation.x] [(int)originLocation.y], distance, doors.Where (x => x.gridPosition != originLocation).Select (x => x.gridPosition).ToArray ());
		else if (ignoreDoors == true) 
			highlightedTiles = TileHighlight.FindHighlight (map [(int)originLocation.x] [(int)originLocation.y], distance, players.Where (x => x.gridPosition != originLocation).Select (x => x.gridPosition).ToArray (), new Vector2[0],false);
		else
			highlightedTiles = TileHighlight.FindHighlight (map [(int)originLocation.x] [(int)originLocation.y], distance, players.Where (x => x.gridPosition != originLocation).Select (x => x.gridPosition).ToArray (),doors.Where (x => x.gridPosition != originLocation).Select (x => x.gridPosition).ToArray (),highlightColor==Color.red);
		foreach (Tile t in highlightedTiles) {
			t.visual.transform.GetComponent<Renderer>().materials[0].color = highlightColor;
			switch (highlightIndex) {
			case "blue":
				t.tileIndex = 1;	
				break;
			case "yellow":
				t.tileIndex = 2;
				break;
			case "red":
				t.tileIndex = 3;
				break;
			case "cyan":
				t.tileIndex = 4;
				break;
			default:
				t.tileIndex = 0;
				break;
			}
		}
	}

	public void removeTileHighlights(){
		for (int i = 0; i < mapSize; i++) {
			for (int j = 0; j < mapSize; j++) {
				if (!map [i] [j].impassable){
					map [i] [j].visual.transform.GetComponent<Renderer> ().materials [0].color = Color.white;
					map [i] [j].tileIndex = 0;
					map [i] [j].tempTileIndex = 0;
				}
			}
		}
		if (players [currentPlayerIndex].playerindex == 1||players [currentPlayerIndex].playerindex == 3) {
			if (players [currentPlayerIndex].animationMoveBool) players [currentPlayerIndex].animationMoveBool = false;
			if (players [currentPlayerIndex].animationAttackBool) players [currentPlayerIndex].animationAttackBool = false;
		}
	}

	public void moveCurrentPlayer(Tile destTile){
		if (destTile.visual.transform.GetComponent<Renderer>().materials[0].color != Color.white && destTile.visual.transform.GetComponent<Renderer>().materials[0].color != Color.magenta && !destTile.impassable) {
			removeTileHighlights ();
			players [currentPlayerIndex].move = false;
			turnAround (destTile);
			players [currentPlayerIndex].animationMoveBool = true;
			foreach(Tile t in TilePathFinder.FindPath(map [(int)players[currentPlayerIndex].gridPosition.x] [(int)players[currentPlayerIndex].gridPosition.y],destTile,players.Where (x => x.gridPosition != players[currentPlayerIndex].gridPosition).Select (x => x.gridPosition).ToArray (),false)){
				players[currentPlayerIndex].positionQueue.Add(map[(int)t.gridPosition.x][(int)t.gridPosition.y].transform.position+ 1.5f * Vector3.up); 
			}

			players [currentPlayerIndex].gridPosition = destTile.gridPosition;
		}
	}

	public void attackWithCurrentPlayer(Tile destTile){
		turnAround (destTile);
		if (destTile.visual.transform.GetComponent<Renderer>().materials[0].color != Color.white && destTile.visual.transform.GetComponent<Renderer>().materials[0].color != Color.magenta && !destTile.impassable) {
			Player target = null;
			foreach (Player p in players) {
				if (p.gridPosition == destTile.gridPosition) {
					target = p;
				}
			}
			if (target != null) {
				if (players [currentPlayerIndex].playerindex != target.playerindex) {
					players [currentPlayerIndex].animationAttackBool = true;	
					players [currentPlayerIndex].actionPoints--;
					removeTileHighlights ();
					players [currentPlayerIndex].attack = false;
					players [currentPlayerIndex].animationAttackBool = true;	
					//attack logic
					//roll to hit
					bool hit = Random.Range (0.0f, 1.0f) <= players [currentPlayerIndex].attackchance;
					if (hit) {
						//damage logic
						int amountOfDamage = (int)Mathf.Floor (players [currentPlayerIndex].dmg + Random.Range (1, players [currentPlayerIndex].dmgdie));
						target.HP -= amountOfDamage;
						target.Health.fillAmount = target.HP / target.maxHP;
						if (target.HP <= 0) {
							players [currentPlayerIndex].animationAttackBool = false;
						}
						if (target.HP <= 0) {
							if (target.playerindex == 1) {
								target.gridPosition.y = target.gridPosition.y - 50;
								numofdeaduserplayers++;
								if (numofdeaduserplayers == 5) {
									target.animationDeadBool = true;
									gameOver ();
								}
							}
						}
						if (target.playerindex == 2 && target.HP <= 0) {
							target.gridPosition.y = target.gridPosition.y - 50;
							numofdeadaiplayers++;
							if (numofdeadaiplayers == 3 && SceneManager.GetActiveScene ().name == "gameScene") {
								numofdeadaiplayers = 0;
								generateAIPlayers ();
							} else if (numofdeadaiplayers == 4 && SceneManager.GetActiveScene ().name == "gamesScene1") {
								numofdeadaiplayers = 0;
								generate2ndForestAIPlayersWave ();
							} else if (numofdeadaiplayers == 5) {
								if (SceneManager.GetActiveScene ().name == "gamesScene2" ||
								    SceneManager.GetActiveScene ().name == "gamesScene2weaken" ||
								    SceneManager.GetActiveScene ().name == "gamesScene2newposition")
									gameOver ();
							}
						}
					}
				}
			}
		}
	}

	public void healWithCurrentPlayer(Tile destTile){
		turnAround (destTile);
		if (destTile.visual.transform.GetComponent<Renderer>().materials[0].color != Color.white && destTile.visual.transform.GetComponent<Renderer>().materials[0].color != Color.magenta && !destTile.impassable) {
			Player target = null;
			foreach (Player p in players) {
				if (p.gridPosition == destTile.gridPosition) {
					target = p;
				}
			}
			if (target != null) {
					players [currentPlayerIndex].animationAttackBool = true;	
					players [currentPlayerIndex].actionPoints--;
					removeTileHighlights ();
					players [currentPlayerIndex].heal = false;
					players [currentPlayerIndex].animationAttackBool = true;	
					//attack logic
					//roll to hit
				bool hit = true;
				if (hit) {
					if (target.HP<target.maxHP){
						if ((target.maxHP - target.HP) < 6)
							target.HP += (target.maxHP - target.HP);
						else
							target.HP += 6;
						target.Health.fillAmount = target.HP / target.maxHP;
						if (target.HP > 0) {
							players [currentPlayerIndex].animationAttackBool = false;
						}
					}
				}
			}
		}
	}

	public void interactWithObject(Tile destTile){
		turnAround (destTile);
		if (players [currentPlayerIndex].playerindex == 1||players [currentPlayerIndex].playerindex == 3) {
			if (destTile.visual.transform.GetComponent<Renderer> ().materials [0].color != Color.white && destTile.visual.transform.GetComponent<Renderer> ().materials [0].color != Color.magenta && destTile.impassable == false) {
				Door target = null;
				foreach (Door d in doors) {
					if (d.gridPosition == destTile.gridPosition) {
						target = d;
					}
				}
				if (target != null) {
					players [currentPlayerIndex].animationAttackBool = true;	
					players [currentPlayerIndex].actionPoints--;
					removeTileHighlights ();
					players [currentPlayerIndex].attack = false;
					players [currentPlayerIndex].animationAttackBool = true;	
					//attack logic
					//roll to hit
					bool hit = true;
					if (hit) {
						//damage logic
						int amountOfDamage = (int)Mathf.Floor (players [currentPlayerIndex].dmg + Random.Range (1, players [currentPlayerIndex].dmgdie));
						target.HP -= amountOfDamage;
					}
					target.gridPosition.y = target.gridPosition.y - 50;
					if (target.itemIndex <=3 && target.HP <= 0) {
						numofobjectives += target.itemIndex;
						numofoptional += target.optionalIndex;
						if (numofobjectives >= 3) {
							nextLevel ();
						}
					}
				}
			}
		} else {
			if (destTile.impassable == false) {
				Door target = null;
				foreach (Door d in doors) {
					if (d.gridPosition == destTile.gridPosition) {
						target = d;
					}
				}
				if (target != null) {
					players [currentPlayerIndex].animationAttackBool = true;	
					players [currentPlayerIndex].actionPoints--;
					removeTileHighlights ();
					players [currentPlayerIndex].attack = false;
					players [currentPlayerIndex].animationAttackBool = true;	
					//attack logic
					//roll to hit
					bool hit = true;
					if (hit) {
						//damage logic
						int amountOfDamage = (int)Mathf.Floor (players [currentPlayerIndex].dmg + Random.Range (1, players [currentPlayerIndex].dmgdie));
						target.HP -= amountOfDamage;
					}
					//players [currentPlayerIndex].attack = false;
					target.gridPosition.y = target.gridPosition.y - 50;
					if (target.itemIndex <=3 && target.HP <= 0) {
						numofobjectives += target.itemIndex;
						numofoptional += target.optionalIndex;
						if (numofobjectives >= 3) {
							nextLevel ();
						}
					}
				}
			}
		}
	}

	public void turnAround(Tile destTile){
		if (players [currentPlayerIndex].gridPosition.y > destTile.gridPosition.y) {
			players [currentPlayerIndex].transform.rotation =Quaternion.Euler (new Vector3 (0, 0, 0));
		}
		if (players [currentPlayerIndex].gridPosition.x < destTile.gridPosition.x) {
			players [currentPlayerIndex].transform.rotation =Quaternion.Euler (new Vector3 (0, 90, 0));
		}
		if (players [currentPlayerIndex].gridPosition.y < destTile.gridPosition.y) {
			players [currentPlayerIndex].transform.rotation =Quaternion.Euler (new Vector3 (0, 180, 0));
		}
		if (players [currentPlayerIndex].gridPosition.x > destTile.gridPosition.x) {
			players [currentPlayerIndex].transform.rotation =Quaternion.Euler (new Vector3 (0, 270, 0));
		}
		if (players [currentPlayerIndex].gridPosition.x < destTile.gridPosition.x && players [currentPlayerIndex].gridPosition.y > destTile.gridPosition.y) {
			players [currentPlayerIndex].transform.rotation =Quaternion.Euler (new Vector3 (0, 45, 0));
		}
		if (players [currentPlayerIndex].gridPosition.x < destTile.gridPosition.x && players [currentPlayerIndex].gridPosition.y < destTile.gridPosition.y) {
			players [currentPlayerIndex].transform.rotation =Quaternion.Euler (new Vector3 (0, 135, 0));
		}
		if (players [currentPlayerIndex].gridPosition.x > destTile.gridPosition.x && players [currentPlayerIndex].gridPosition.y < destTile.gridPosition.y) {
			players [currentPlayerIndex].transform.rotation =Quaternion.Euler (new Vector3 (0, 225, 0));
		}
		if (players [currentPlayerIndex].gridPosition.x > destTile.gridPosition.x && players [currentPlayerIndex].gridPosition.y > destTile.gridPosition.y) {
			players [currentPlayerIndex].transform.rotation =Quaternion.Euler (new Vector3 (0, 315, 0));
		}
	}


	public void gameOver(){
		SceneManager.LoadScene ("gameOverScene");
	}

	public void nextLevel(){
		if (SceneManager.GetActiveScene ().name == "gameScene")
			SceneManager.LoadScene ("gamesScene1");
		else if (SceneManager.GetActiveScene ().name == "gamesScene1" && numofobjectives == 3) {
			if (numofoptional == 3)
				SceneManager.LoadScene ("gamesScene2weaken");
			else if (numofoptional == 2)
				SceneManager.LoadScene ("gamesScene2newposition");
			else if (numofoptional == 0)
				SceneManager.LoadScene ("gamesScene2");
			else
				gameOver ();
		}
		else
			gameOver();
	}

	void generateMap(){
		loadMapFromXml ();
	}

	void loadMapFromXml(){
		MapXmlContainer container = mapSaveLoad.Load (mapXml);
		mapSize = container.size;

		for (int i = 0; i < mapTransform.childCount; i++) {
			Destroy (mapTransform.GetChild (i).gameObject);
		}
		map = new List<List<Tile>> ();
		for (int i = 0; i < mapSize; i++) {
			List <Tile> row = new List<Tile> ();
			for (int j = 0; j < mapSize; j++) {
				Tile tile = ((GameObject)Instantiate (PrefabHolder.instance.BASE_TILE_PREFAB, new Vector3 (i - Mathf.Floor (mapSize / 2), 0, -j + Mathf.Floor (mapSize / 2)), Quaternion.Euler (new Vector3 ()))).GetComponent<Tile> ();
				tile.transform.parent = mapTransform;
				tile.gridPosition = new Vector2 (i, j);
				tile.setType ((TileType)container.tiles.Where(x=>x.locX==i&&x.locY==j).First().id);
				row.Add (tile);			
			}
			map.Add (row);
		}
	}

	void generateUserPlayers(){
		UserPlayer player;
		player = ((GameObject)Instantiate (DevaUserPlayerPrefab, new Vector3 (16 - Mathf.Floor (mapSize / 2), 1.5f, -25 + Mathf.Floor (mapSize / 2)), Quaternion.Euler (new Vector3 ()))).GetComponent<UserPlayer> ();
		player.gridPosition = new Vector2 (16, 25);
		player.playerName = "Player1";
		player.playerindex = 1;
		player.movementPerActionPoint = 4;
		player.rangedAttackRange = 8;
		player.dmg = 6;
		player.dmgdie = 8;
		players.Add (player);
		player = ((GameObject)Instantiate (UserPlayerPrefab, new Vector3 (17 - Mathf.Floor (mapSize / 2), 1.5f, -25 + Mathf.Floor (mapSize / 2)), Quaternion.Euler (new Vector3 ()))).GetComponent<UserPlayer> ();
		player.gridPosition = new Vector2 (17, 25);
		player.playerName = "Player2";
		player.playerindex = 1;
		players.Add (player);
		player = ((GameObject)Instantiate (UserPlayerPrefab, new Vector3 (16 - Mathf.Floor (mapSize / 2), 1.5f, -24 + Mathf.Floor (mapSize / 2)), Quaternion.Euler (new Vector3 ()))).GetComponent<UserPlayer> ();
		player.gridPosition = new Vector2 (16, 24);
		player.playerName = "Player3";
		player.playerindex = 1;
		players.Add (player);
		player = ((GameObject)Instantiate (AssaultUserPlayerPrefab, new Vector3 (17 - Mathf.Floor (mapSize / 2), 1.5f, -24 + Mathf.Floor (mapSize / 2)), Quaternion.Euler (new Vector3 ()))).GetComponent<UserPlayer> ();
		player.gridPosition = new Vector2 (17, 24);
		player.playerName = "Player4";
		player.playerindex = 1;
		player.movementPerActionPoint = 8;
		player.rangedAttackRange = 2;
		player.dmg = 8;
		players.Add (player);
		player = ((GameObject)Instantiate (ApothecaryUserPlayerPrefab, new Vector3 (16 - Mathf.Floor (mapSize / 2), 1.5f, -23 + Mathf.Floor (mapSize / 2)), Quaternion.Euler (new Vector3 ()))).GetComponent<UserPlayer> ();
		player.gridPosition = new Vector2 (16, 23);
		player.playerName = "Player5";
		player.playerindex = 1;
		player.healer = true;
		players.Add (player);
	}

	void generateAIPlayers(){
		AIPlayer aiplayer;
		aiplayer=((GameObject)Instantiate (AIPlayerPrefab, new Vector3 (1 - Mathf.Floor (mapSize / 2), 0.5f, -20 + Mathf.Floor (mapSize / 2)), Quaternion.Euler (new Vector3 ()))).GetComponent<AIPlayer> ();
		aiplayer.gridPosition = new Vector2 (1, 20);
		aiplayer.playerName = "Bot1";
		aiplayer.playerindex = 2;
		players.Add(aiplayer);
		aiplayer=((GameObject)Instantiate (AIPlayerPrefab, new Vector3 (1 - Mathf.Floor (mapSize / 2), 0.5f, -8 + Mathf.Floor (mapSize / 2)), Quaternion.Euler (new Vector3 ()))).GetComponent<AIPlayer> ();
		aiplayer.gridPosition = new Vector2 (1, 8);
		aiplayer.playerName = "Bot4";
		aiplayer.playerindex = 2;
		players.Add(aiplayer);
		aiplayer=((GameObject)Instantiate (AIPlayerPrefab, new Vector3 (15 - Mathf.Floor (mapSize / 2), 0.5f, -1 + Mathf.Floor (mapSize / 2)), Quaternion.Euler (new Vector3 ()))).GetComponent<AIPlayer> ();
		aiplayer.gridPosition = new Vector2 (15, 1);
		aiplayer.playerName = "Bot7";
		aiplayer.playerindex = 2;
		players.Add(aiplayer);
		aiplayer=((GameObject)Instantiate (AIPlayerPrefab, new Vector3 (23 - Mathf.Floor (mapSize / 2), 0.5f, -28 + Mathf.Floor (mapSize / 2)), Quaternion.Euler (new Vector3 ()))).GetComponent<AIPlayer> ();
		aiplayer.gridPosition = new Vector2 (23, 28);
		aiplayer.playerName = "Bot10";
		aiplayer.playerindex = 2;
		players.Add(aiplayer);
	}

	void generateDoors(){
		Door door;
		door=((GameObject)Instantiate (DoorPrefab, new Vector3 (11 - Mathf.Floor (mapSize / 2), 1.5f, -24 + Mathf.Floor (mapSize / 2)), Quaternion.Euler (new Vector3 (0,90,0)))).GetComponent<Door> ();
		door.gridPosition = new Vector2 (11, 24);
		door.doorName = "Door1";
		doors.Add(door);
		door=((GameObject)Instantiate (DoorPrefab, new Vector3 (11 - Mathf.Floor (mapSize / 2), 1.5f, -23 + Mathf.Floor (mapSize / 2)), Quaternion.Euler (new Vector3 (0,90,0)))).GetComponent<Door> ();
		door.gridPosition = new Vector2 (11, 23);
		door.doorName = "Door2";
		doors.Add(door);
		door=((GameObject)Instantiate (DoorPrefab, new Vector3 (16 - Mathf.Floor (mapSize / 2), 1.5f, -20 + Mathf.Floor (mapSize / 2)), Quaternion.Euler (new Vector3 ()))).GetComponent<Door> ();
		door.gridPosition = new Vector2 (16, 20);
		door.doorName = "Door3";
		doors.Add(door);
		door=((GameObject)Instantiate (DoorPrefab, new Vector3 (17 - Mathf.Floor (mapSize / 2), 1.5f, -20 + Mathf.Floor (mapSize / 2)), Quaternion.Euler (new Vector3 ()))).GetComponent<Door> ();
		door.gridPosition = new Vector2 (17, 20);
		door.doorName = "Door4";
		doors.Add(door);
		door=((GameObject)Instantiate (DoorPrefab, new Vector3 (19 - Mathf.Floor (mapSize / 2), 1.5f, -24 + Mathf.Floor (mapSize / 2)), Quaternion.Euler (new Vector3 (0,90,0)))).GetComponent<Door> ();
		door.gridPosition = new Vector2 (19, 24);
		door.doorName = "Door5";
		doors.Add(door);
		door=((GameObject)Instantiate (DoorPrefab, new Vector3 (19 - Mathf.Floor (mapSize / 2), 1.5f, -23 + Mathf.Floor (mapSize / 2)), Quaternion.Euler (new Vector3 (0,90,0)))).GetComponent<Door> ();
		door.gridPosition = new Vector2 (19, 23);
		door.doorName = "Door6";
		doors.Add(door);
		door=((GameObject)Instantiate (DoorPrefab, new Vector3 (8 - Mathf.Floor (mapSize / 2), 1.5f, -15 + Mathf.Floor (mapSize / 2)), Quaternion.Euler (new Vector3 (0,90,0)))).GetComponent<Door> ();
		door.gridPosition = new Vector2 (8, 15);
		door.doorName = "Door7";
		doors.Add(door);
		door=((GameObject)Instantiate (DoorPrefab, new Vector3 (8 - Mathf.Floor (mapSize / 2), 1.5f, -16 + Mathf.Floor (mapSize / 2)), Quaternion.Euler (new Vector3 (0,90,0)))).GetComponent<Door> ();
		door.gridPosition = new Vector2 (8, 16);
		door.doorName = "Door8";
		doors.Add(door);
		door=((GameObject)Instantiate (DoorPrefab, new Vector3 (11 - Mathf.Floor (mapSize / 2), 1.5f, -10 + Mathf.Floor (mapSize / 2)), Quaternion.Euler (new Vector3 (0,90,0)))).GetComponent<Door> ();
		door.gridPosition = new Vector2 (11, 10);
		door.doorName = "Door9";
		doors.Add(door);
		door=((GameObject)Instantiate (DoorPrefab, new Vector3 (11 - Mathf.Floor (mapSize / 2), 1.5f, -11 + Mathf.Floor (mapSize / 2)), Quaternion.Euler (new Vector3 (0,90,0)))).GetComponent<Door> ();
		door.gridPosition = new Vector2 (11, 11);
		door.doorName = "Door10";
		doors.Add(door);
		door=((GameObject)Instantiate (DoorPrefab, new Vector3 (11 - Mathf.Floor (mapSize / 2), 1.5f, -5 + Mathf.Floor (mapSize / 2)), Quaternion.Euler (new Vector3 (0,90,0)))).GetComponent<Door> ();
		door.gridPosition = new Vector2 (11, 5);
		door.doorName = "Door11";
		doors.Add(door);
		door=((GameObject)Instantiate (DoorPrefab, new Vector3 (11 - Mathf.Floor (mapSize / 2), 1.5f, -4 + Mathf.Floor (mapSize / 2)), Quaternion.Euler (new Vector3 (0,90,0)))).GetComponent<Door> ();
		door.gridPosition = new Vector2 (11, 4);
		door.doorName = "Door12";
		doors.Add(door);
		door=((GameObject)Instantiate (DoorPrefab, new Vector3 (28 - Mathf.Floor (mapSize / 2), 1.5f, -8 + Mathf.Floor (mapSize / 2)), Quaternion.Euler (new Vector3 ()))).GetComponent<Door> ();
		door.gridPosition = new Vector2 (28, 8);
		door.doorName = "Door13";
		doors.Add(door);
		door=((GameObject)Instantiate (DoorPrefab, new Vector3 (27 - Mathf.Floor (mapSize / 2), 1.5f, -8 + Mathf.Floor (mapSize / 2)), Quaternion.Euler (new Vector3 ()))).GetComponent<Door> ();
		door.gridPosition = new Vector2 (27, 8);
		door.doorName = "Door14";
		doors.Add(door);
		door=((GameObject)Instantiate (DoorPrefab, new Vector3 (28 - Mathf.Floor (mapSize / 2), 1.5f, -15 + Mathf.Floor (mapSize / 2)), Quaternion.Euler (new Vector3 ()))).GetComponent<Door> ();
		door.gridPosition = new Vector2 (28, 15);
		door.doorName = "Door15";
		doors.Add(door);
		door=((GameObject)Instantiate (DoorPrefab, new Vector3 (27 - Mathf.Floor (mapSize / 2), 1.5f, -15 + Mathf.Floor (mapSize / 2)), Quaternion.Euler (new Vector3 ()))).GetComponent<Door> ();
		door.gridPosition = new Vector2 (27, 15);
		door.doorName = "Door16";
		doors.Add(door);
		door=((GameObject)Instantiate (DoorPrefab, new Vector3 (24 - Mathf.Floor (mapSize / 2), 1.5f, -12 + Mathf.Floor (mapSize / 2)), Quaternion.Euler (new Vector3 (0,90,0)))).GetComponent<Door> ();
		door.gridPosition = new Vector2 (24, 12);
		door.doorName = "Door17";
		doors.Add(door);
		door=((GameObject)Instantiate (DoorPrefab, new Vector3 (24 - Mathf.Floor (mapSize / 2), 1.5f, -11 + Mathf.Floor (mapSize / 2)), Quaternion.Euler (new Vector3 (0,90,0)))).GetComponent<Door> ();
		door.gridPosition = new Vector2 (24, 11);
		door.doorName = "Door18";
		doors.Add(door);
		door=((GameObject)Instantiate (JammingBeaconPrefab, new Vector3 (4 - Mathf.Floor (mapSize / 2), 1.5f, -2 + Mathf.Floor (mapSize / 2)), Quaternion.Euler (new Vector3 ()))).GetComponent<Door> ();
		door.gridPosition = new Vector2 (4, 2);
		door.doorName = "JammingBeacon1";
		door.itemIndex = 1;
		doors.Add(door);
		door=((GameObject)Instantiate (JammingBeaconPrefab, new Vector3 (27 - Mathf.Floor (mapSize / 2), 1.5f, -12 + Mathf.Floor (mapSize / 2)), Quaternion.Euler (new Vector3 ()))).GetComponent<Door> ();
		door.gridPosition = new Vector2 (27, 12);
		door.doorName = "JammingBeacon2";
		door.itemIndex = 1;
		doors.Add(door);
		door=((GameObject)Instantiate (JammingBeaconPrefab, new Vector3 (11 - Mathf.Floor (mapSize / 2), 1.5f, -15 + Mathf.Floor (mapSize / 2)), Quaternion.Euler (new Vector3 ()))).GetComponent<Door> ();
		door.gridPosition = new Vector2 (11, 15);
		door.doorName = "JammingBeacon3";
		door.itemIndex = 1;
		doors.Add(door);
	}

	void generateForestUserPlayers(){
		UserPlayer player;
		player = ((GameObject)Instantiate (DevaUserPlayerPrefab, new Vector3 (6 - Mathf.Floor (mapSize / 2), 1.5f, -8 + Mathf.Floor (mapSize / 2)), Quaternion.Euler (new Vector3 ()))).GetComponent<UserPlayer> ();
		player.gridPosition = new Vector2 (6, 8);
		player.playerName = "Player1";
		player.playerindex = 1;
		player.movementPerActionPoint = 4;
		player.rangedAttackRange = 8;
		player.dmg = 6;
		player.dmgdie = 8;
		players.Add (player);
		player = ((GameObject)Instantiate (UserPlayerPrefab, new Vector3 (7 - Mathf.Floor (mapSize / 2), 1.5f, -7 + Mathf.Floor (mapSize / 2)), Quaternion.Euler (new Vector3 ()))).GetComponent<UserPlayer> ();
		player.gridPosition = new Vector2 (7, 7);
		player.playerName = "Player2";
		player.playerindex = 1;
		players.Add (player);
		player = ((GameObject)Instantiate (UserPlayerPrefab, new Vector3 (7 - Mathf.Floor (mapSize / 2), 1.5f, -6 + Mathf.Floor (mapSize / 2)), Quaternion.Euler (new Vector3 ()))).GetComponent<UserPlayer> ();
		player.gridPosition = new Vector2 (7, 6);
		player.playerName = "Player3";
		player.playerindex = 1;
		players.Add (player);
		player = ((GameObject)Instantiate (AssaultUserPlayerPrefab, new Vector3 (6 - Mathf.Floor (mapSize / 2), 1.5f, -7 + Mathf.Floor (mapSize / 2)), Quaternion.Euler (new Vector3 ()))).GetComponent<UserPlayer> ();
		player.gridPosition = new Vector2 (6, 7);
		player.playerName = "Player4";
		player.playerindex = 1;
		player.movementPerActionPoint = 8;
		player.rangedAttackRange = 2;
		player.dmg = 8;
		players.Add (player);
		player = ((GameObject)Instantiate (ApothecaryUserPlayerPrefab, new Vector3 (6 - Mathf.Floor (mapSize / 2), 1.5f, -6 + Mathf.Floor (mapSize / 2)), Quaternion.Euler (new Vector3 ()))).GetComponent<UserPlayer> ();
		player.gridPosition = new Vector2 (6, 6);
		player.playerName = "Player5";
		player.playerindex = 1;
		player.healer = true;
		players.Add (player);
	}

	void generateForestAIPlayers(){
		AIPlayer aiplayer;
		aiplayer=((GameObject)Instantiate (AIPlayerPrefab, new Vector3 (8 - Mathf.Floor (mapSize / 2), 0.5f, -20 + Mathf.Floor (mapSize / 2)), Quaternion.Euler (new Vector3 ()))).GetComponent<AIPlayer> ();
		aiplayer.gridPosition = new Vector2 (8, 20);
		aiplayer.playerName = "Bot1";
		aiplayer.playerindex = 2;
		players.Add(aiplayer);
		aiplayer=((GameObject)Instantiate (AIPlayerPrefab, new Vector3 (15 - Mathf.Floor (mapSize / 2), 0.5f, -8 + Mathf.Floor (mapSize / 2)), Quaternion.Euler (new Vector3 ()))).GetComponent<AIPlayer> ();
		aiplayer.gridPosition = new Vector2 (15, 8);
		aiplayer.playerName = "Bot2";
		aiplayer.playerindex = 2;
		players.Add(aiplayer);
		aiplayer=((GameObject)Instantiate (shootyAIPlayerPrefab, new Vector3 (30 - Mathf.Floor (mapSize / 2), 0.5f, -8 + Mathf.Floor (mapSize / 2)), Quaternion.Euler (new Vector3 ()))).GetComponent<AIPlayer> ();
		aiplayer.gridPosition = new Vector2 (30, 8);
		aiplayer.playerName = "Bot5";
		aiplayer.playerindex = 2;
		aiplayer.attackRange = 5;
		players.Add(aiplayer);
		aiplayer=((GameObject)Instantiate (shootyAIPlayerPrefab, new Vector3 (18 - Mathf.Floor (mapSize / 2), 0.5f, -40 + Mathf.Floor (mapSize / 2)), Quaternion.Euler (new Vector3 ()))).GetComponent<AIPlayer> ();
		aiplayer.gridPosition = new Vector2 (18, 40);
		aiplayer.playerName = "Bot6";
		aiplayer.playerindex = 2;
		aiplayer.attackRange = 5;
		players.Add(aiplayer);
	}

	void generate2ndForestAIPlayersWave(){
		AIPlayer aiplayer;
		aiplayer=((GameObject)Instantiate (AIPlayerPrefab, new Vector3 (5 - Mathf.Floor (mapSize / 2), 0.5f, -54 + Mathf.Floor (mapSize / 2)), Quaternion.Euler (new Vector3 ()))).GetComponent<AIPlayer> ();
		aiplayer.gridPosition = new Vector2 (5, 54);
		aiplayer.playerName = "Bot7";
		aiplayer.playerindex = 2;
		players.Add(aiplayer);
		aiplayer=((GameObject)Instantiate (AIPlayerPrefab, new Vector3 (54 - Mathf.Floor (mapSize / 2), 0.5f, -5 + Mathf.Floor (mapSize / 2)), Quaternion.Euler (new Vector3 ()))).GetComponent<AIPlayer> ();
		aiplayer.gridPosition = new Vector2 (54, 5);
		aiplayer.playerName = "Bot8";
		aiplayer.playerindex = 2;
		players.Add(aiplayer);
		aiplayer=((GameObject)Instantiate (shootyAIPlayerPrefab, new Vector3 (5 - Mathf.Floor (mapSize / 2), 0.5f, -5 + Mathf.Floor (mapSize / 2)), Quaternion.Euler (new Vector3 ()))).GetComponent<AIPlayer> ();
		aiplayer.gridPosition = new Vector2 (5, 5);
		aiplayer.playerName = "Bot9";
		aiplayer.playerindex = 2;
		aiplayer.attackRange = 5;
		players.Add(aiplayer);
	}

	void generateForestDoors(){
		Door door;
		door = ((GameObject)Instantiate (DoorPrefab, new Vector3 (50 - Mathf.Floor (mapSize / 2), 1.5f, -51 + Mathf.Floor (mapSize / 2)), Quaternion.Euler (new Vector3 ()))).GetComponent<Door> ();
		door.gridPosition = new Vector2 (50, 51);
		door.itemIndex = 3;
		door.availableToAI = false;
		door.doorName = "Door1";
		doors.Add (door);
		door = ((GameObject)Instantiate (DoorPrefab, new Vector3 (49 - Mathf.Floor (mapSize / 2), 1.5f, -51 + Mathf.Floor (mapSize / 2)), Quaternion.Euler (new Vector3 ()))).GetComponent<Door> ();
		door.gridPosition = new Vector2 (49, 51);
		door.itemIndex = 3;
		door.availableToAI = false;
		door.doorName = "Door2";
		doors.Add (door);
		door = ((GameObject)Instantiate (DoorPrefab, new Vector3 (54 - Mathf.Floor (mapSize / 2), 1.5f, -28 + Mathf.Floor (mapSize / 2)), Quaternion.Euler (new Vector3 (0,90,0)))).GetComponent<Door> ();
		door.gridPosition = new Vector2 (54, 28);
		door.itemIndex = 3;
		door.availableToAI = false;
		door.doorName = "Door3";
		door.optionalIndex = 2;
		doors.Add (door);
		door=((GameObject)Instantiate (JammingBeaconPrefab, new Vector3 (29 - Mathf.Floor (mapSize / 2), 1.5f, -51 + Mathf.Floor (mapSize / 2)), Quaternion.Euler (new Vector3 ()))).GetComponent<Door> ();
		door.gridPosition = new Vector2 (29, 51);
		door.doorName = "JammingBeacon1";
		door.itemIndex = 0;
		door.availableToAI = false;
		door.optionalIndex = 1;
		doors.Add(door);
	}

	void generateBridgeUserPlayers(){
		UserPlayer player;
		player = ((GameObject)Instantiate (DevaUserPlayerPrefab, new Vector3 (3 - Mathf.Floor (mapSize / 2), 1.5f, -4 + Mathf.Floor (mapSize / 2)), Quaternion.Euler (new Vector3 ()))).GetComponent<UserPlayer> ();
		player.gridPosition = new Vector2 (3, 4);
		player.playerName = "Player1";
		player.playerindex = 1;
		player.movementPerActionPoint = 4;
		player.rangedAttackRange = 8;
		player.dmg = 6;
		player.dmgdie = 8;
		players.Add (player);
		player = ((GameObject)Instantiate (UserPlayerPrefab, new Vector3 (2 - Mathf.Floor (mapSize / 2), 1.5f, -4 + Mathf.Floor (mapSize / 2)), Quaternion.Euler (new Vector3 ()))).GetComponent<UserPlayer> ();
		player.gridPosition = new Vector2 (2, 4);
		player.playerName = "Player2";
		player.playerindex = 1;
		players.Add (player);
		player = ((GameObject)Instantiate (UserPlayerPrefab, new Vector3 (2 - Mathf.Floor (mapSize / 2), 1.5f, -2 + Mathf.Floor (mapSize / 2)), Quaternion.Euler (new Vector3 ()))).GetComponent<UserPlayer> ();
		player.gridPosition = new Vector2 (2, 2);
		player.playerName = "Player3";
		player.playerindex = 1;
		players.Add (player);
		player = ((GameObject)Instantiate (AssaultUserPlayerPrefab, new Vector3 (3 - Mathf.Floor (mapSize / 2), 1.5f, -2 + Mathf.Floor (mapSize / 2)), Quaternion.Euler (new Vector3 ()))).GetComponent<UserPlayer> ();
		player.gridPosition = new Vector2 (3, 2);
		player.playerName = "Player4";
		player.playerindex = 1;
		player.movementPerActionPoint = 8;
		player.rangedAttackRange = 2;
		player.dmg = 8;
		players.Add (player);
		player = ((GameObject)Instantiate (ApothecaryUserPlayerPrefab, new Vector3 (2 - Mathf.Floor (mapSize / 2), 1.5f, -3 + Mathf.Floor (mapSize / 2)), Quaternion.Euler (new Vector3 ()))).GetComponent<UserPlayer> ();
		player.gridPosition = new Vector2 (2, 3);
		player.playerName = "Player5";
		player.playerindex = 1;
		player.healer = true;
		players.Add (player);
	}

	void generateBridgeUserPlayersnewposition(){
		UserPlayer player;
		player = ((GameObject)Instantiate (DevaUserPlayerPrefab, new Vector3 (19 - Mathf.Floor (mapSize / 2), 1.5f, -33 + Mathf.Floor (mapSize / 2)), Quaternion.Euler (new Vector3 ()))).GetComponent<UserPlayer> ();
		player.gridPosition = new Vector2 (19, 33);
		player.playerName = "Player1";
		player.playerindex = 1;
		player.movementPerActionPoint = 3;
		player.rangedAttackRange = 7;
		player.dmg = 6;
		player.dmgdie = 8;
		players.Add (player);
		player = ((GameObject)Instantiate (UserPlayerPrefab, new Vector3 (19 - Mathf.Floor (mapSize / 2), 1.5f, -32 + Mathf.Floor (mapSize / 2)), Quaternion.Euler (new Vector3 ()))).GetComponent<UserPlayer> ();
		player.gridPosition = new Vector2 (19, 32);
		player.playerName = "Player2";
		player.playerindex = 1;
		players.Add (player);
		player = ((GameObject)Instantiate (UserPlayerPrefab, new Vector3 (21 - Mathf.Floor (mapSize / 2), 1.5f, -32 + Mathf.Floor (mapSize / 2)), Quaternion.Euler (new Vector3 ()))).GetComponent<UserPlayer> ();
		player.gridPosition = new Vector2 (21, 32);
		player.playerName = "Player3";
		player.playerindex = 1;
		players.Add (player);
		player = ((GameObject)Instantiate (AssaultUserPlayerPrefab, new Vector3 (21 - Mathf.Floor (mapSize / 2), 1.5f, -33 + Mathf.Floor (mapSize / 2)), Quaternion.Euler (new Vector3 ()))).GetComponent<UserPlayer> ();
		player.gridPosition = new Vector2 (21, 33);
		player.playerName = "Player4";
		player.playerindex = 1;
		player.movementPerActionPoint = 8;
		player.rangedAttackRange = 1;
		player.dmg = 8;
		players.Add (player);
		player = ((GameObject)Instantiate (ApothecaryUserPlayerPrefab, new Vector3 (20 - Mathf.Floor (mapSize / 2), 1.5f, -32 + Mathf.Floor (mapSize / 2)), Quaternion.Euler (new Vector3 ()))).GetComponent<UserPlayer> ();
		player.gridPosition = new Vector2 (20, 32);
		player.playerName = "Player5";
		player.playerindex = 1;
		player.healer = true;
		players.Add (player);
	}

	void generateBridgeAIPlayers(){
		AIPlayer aiplayer;
		aiplayer=((GameObject)Instantiate (AIPlayerPrefab, new Vector3 (9 - Mathf.Floor (mapSize / 2), 0.5f, -33 + Mathf.Floor (mapSize / 2)), Quaternion.Euler (new Vector3 ()))).GetComponent<AIPlayer> ();
		aiplayer.gridPosition = new Vector2 (9, 33);
		aiplayer.playerName = "Bot1";
		aiplayer.playerindex = 2;
		players.Add(aiplayer);
		aiplayer=((GameObject)Instantiate (AIPlayerPrefab, new Vector3 (31 - Mathf.Floor (mapSize / 2), 0.5f, -33 + Mathf.Floor (mapSize / 2)), Quaternion.Euler (new Vector3 ()))).GetComponent<AIPlayer> ();
		aiplayer.gridPosition = new Vector2 (31, 33);
		aiplayer.playerName = "Bot2";
		aiplayer.playerindex = 2;
		players.Add(aiplayer);
		aiplayer=((GameObject)Instantiate (shootyAIPlayerPrefab, new Vector3 (16 - Mathf.Floor (mapSize / 2), 0.5f, -18 + Mathf.Floor (mapSize / 2)), Quaternion.Euler (new Vector3 ()))).GetComponent<AIPlayer> ();
		aiplayer.gridPosition = new Vector2 (16, 18);
		aiplayer.playerName = "Bot5";
		aiplayer.playerindex = 2;
		aiplayer.attackRange = 5;
		players.Add(aiplayer);
		aiplayer=((GameObject)Instantiate (shootyAIPlayerPrefab, new Vector3 (24 - Mathf.Floor (mapSize / 2), 0.5f, -18 + Mathf.Floor (mapSize / 2)), Quaternion.Euler (new Vector3 ()))).GetComponent<AIPlayer> ();
		aiplayer.gridPosition = new Vector2 (24, 18);
		aiplayer.playerName = "Bot6";
		aiplayer.playerindex = 2;
		aiplayer.attackRange = 5;
		players.Add(aiplayer);
		aiplayer=((GameObject)Instantiate (bigAIPlayerPrefab, new Vector3 (20 - Mathf.Floor (mapSize / 2), 0.5f, -20 + Mathf.Floor (mapSize / 2)), Quaternion.Euler (new Vector3 ()))).GetComponent<AIPlayer> ();
		aiplayer.gridPosition = new Vector2 (20, 20);
		aiplayer.playerName = "Bot7";
		aiplayer.playerindex = 2;
		aiplayer.maxHP = 25;
		aiplayer.HP = 25;
		aiplayer.actionPoints = 3;
		aiplayer.attackRange = 3;
		players.Add(aiplayer);
	}

	void generateWeakenBridgeAIPlayers(){
		AIPlayer aiplayer;
		aiplayer=((GameObject)Instantiate (AIPlayerPrefab, new Vector3 (9 - Mathf.Floor (mapSize / 2), 0.5f, -33 + Mathf.Floor (mapSize / 2)), Quaternion.Euler (new Vector3 ()))).GetComponent<AIPlayer> ();
		aiplayer.gridPosition = new Vector2 (9, 33);
		aiplayer.playerName = "Bot1";
		aiplayer.playerindex = 2;
		players.Add(aiplayer);
		aiplayer=((GameObject)Instantiate (AIPlayerPrefab, new Vector3 (31 - Mathf.Floor (mapSize / 2), 0.5f, -33 + Mathf.Floor (mapSize / 2)), Quaternion.Euler (new Vector3 ()))).GetComponent<AIPlayer> ();
		aiplayer.gridPosition = new Vector2 (31, 33);
		aiplayer.playerName = "Bot2";
		aiplayer.playerindex = 2;
		players.Add(aiplayer);
		aiplayer=((GameObject)Instantiate (shootyAIPlayerPrefab, new Vector3 (16 - Mathf.Floor (mapSize / 2), 0.5f, -18 + Mathf.Floor (mapSize / 2)), Quaternion.Euler (new Vector3 ()))).GetComponent<AIPlayer> ();
		aiplayer.gridPosition = new Vector2 (16, 18);
		aiplayer.playerName = "Bot5";
		aiplayer.playerindex = 2;
		aiplayer.attackRange = 3;
		aiplayer.actionPoints = 1;
		aiplayer.movementPerActionPoint = 4;
		aiplayer.attackchance = 0.5f;
		players.Add(aiplayer);
		aiplayer=((GameObject)Instantiate (shootyAIPlayerPrefab, new Vector3 (24 - Mathf.Floor (mapSize / 2), 0.5f, -18 + Mathf.Floor (mapSize / 2)), Quaternion.Euler (new Vector3 ()))).GetComponent<AIPlayer> ();
		aiplayer.gridPosition = new Vector2 (24, 18);
		aiplayer.playerName = "Bot6";
		aiplayer.playerindex = 2;
		aiplayer.attackRange = 3;
		aiplayer.actionPoints = 1;
		aiplayer.movementPerActionPoint = 4;
		aiplayer.attackchance = 0.5f;
		players.Add(aiplayer);
		aiplayer=((GameObject)Instantiate (bigAIPlayerPrefab, new Vector3 (20 - Mathf.Floor (mapSize / 2), 0.5f, -20 + Mathf.Floor (mapSize / 2)), Quaternion.Euler (new Vector3 ()))).GetComponent<AIPlayer> ();
		aiplayer.gridPosition = new Vector2 (20, 20);
		aiplayer.playerName = "Bot7";
		aiplayer.playerindex = 2;
		aiplayer.attackRange = 2;
		aiplayer.actionPoints = 2;
		aiplayer.maxHP = 25;
		aiplayer.HP = 25;
		aiplayer.movementPerActionPoint = 4;
		aiplayer.attackchance = 0.5f;
		players.Add(aiplayer);
	}

}
