using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
using System.Threading;
using UnityEngine.Networking;


public class NetManager : NetworkBehaviour {

	public static NetManager instance;

	public GameObject TilePrefab;
	public GameObject UserPlayerPrefab;
	public int mapSize;
	Transform mapTransform;
	public List <List<NetTile>> map = new List<List<NetTile>>();
	public List <NetworkPlayer> players = new List<NetworkPlayer>();
	public int numofdeadplayers=0;
	public string mapXml;

	[SyncVar]
	public int currentPlayerIndex = 0;
	void Awake(){
		instance = this;
		mapTransform = transform.FindChild ("Map");
	}

	void Start () {

	}

	void Update () {
		if (players.Any ()) {
			if (players [currentPlayerIndex].HP > 0) {
				highlightPlayerTile (players [currentPlayerIndex].gridPosition);
				players [currentPlayerIndex].TurnUpdate ();
			}
			else
				players.Where (x => x.isLocalPlayer).First ().callNextTurn ();
		}
	}

	void OnGUI(){
		if (players.Any ()) {
			if (players [currentPlayerIndex].HP > 0)players [currentPlayerIndex].TurnOnGUI ();
		}
	}

	public void highlightPlayerTile(Vector2 originLocation){
		map [(int)originLocation.x] [(int)originLocation.y].visual.transform.GetComponent<Renderer>().materials[0].color = Color.green;
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

	public void highlightTilesAt (Vector2 originLocation,Color highlightColor, int distance, bool ignorePlayers, string highlightIndex){
		List<NetTile> highlightedTiles = new List<NetTile> ();
		if (ignorePlayers) {
			highlightedTiles = NetTileHighlight.FindHighlight (map [(int)originLocation.x] [(int)originLocation.y], distance);
		} else {
			highlightedTiles = NetTileHighlight.FindHighlight (map [(int)originLocation.x] [(int)originLocation.y], distance, players.Where (x => x.gridPosition != originLocation).Select (x => x.gridPosition).ToArray (), highlightColor == Color.red);
		}foreach (NetTile t in highlightedTiles) {
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
			default:
				t.tileIndex = 0;
				break;
			}
		}
	}

	public void removeTileHighlights(){
		for (int i = 0; i < mapSize; i++) {
			for (int j = 0; j < mapSize; j++) {
				if (!map[i][j].impassable)
					map [i] [j].visual.transform.GetComponent<Renderer>().materials[0].color = Color.white;
					map [i] [j].tileIndex = 0;
					map [i] [j].tempTileIndex = 0;
			}
		}
		if (players [currentPlayerIndex].animationMoveBool) players [currentPlayerIndex].animationMoveBool = false;
		if (players [currentPlayerIndex].animationAttackBool) players [currentPlayerIndex].animationAttackBool = false;
	}

	public void moveCurrentPlayer(string serializedTile){
		Vector2 deserializedTile = JsonUtility.FromJson<Vector2> (serializedTile);
		NetTile destTile = map [(int)deserializedTile.x] [(int)deserializedTile.y];
		if (destTile.visual.transform.GetComponent<Renderer>().materials[0].color != Color.white && destTile.visual.transform.GetComponent<Renderer>().materials[0].color != Color.magenta && !destTile.impassable) {
			removeTileHighlights ();
			players [currentPlayerIndex].move = false;
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
			players [currentPlayerIndex].animationMoveBool = true;
			foreach(NetTile t in NetTilePathFinder.FindPath(map [(int)players[currentPlayerIndex].gridPosition.x] [(int)players[currentPlayerIndex].gridPosition.y],destTile,players.Where (x => x.gridPosition != players[currentPlayerIndex].gridPosition).Select (x => x.gridPosition).ToArray ())){
				players[currentPlayerIndex].positionQueue.Add(map[(int)t.gridPosition.x][(int)t.gridPosition.y].transform.position+ 1.5f * Vector3.up); 
			}
			players [currentPlayerIndex].gridPosition = destTile.gridPosition;
			players [currentPlayerIndex].SetGridPosition (players [currentPlayerIndex].gridPosition);
		}
	}

	public void attackWithCurrentPlayer(NetTile destTile){
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
		if (destTile.visual.transform.GetComponent<Renderer>().materials[0].color != Color.white && destTile.visual.transform.GetComponent<Renderer>().materials[0].color != Color.magenta && !destTile.impassable) {
			NetworkPlayer target = null;
			foreach (NetworkPlayer p in players) {
				if (p.gridPosition == destTile.gridPosition) {
					target = p;
				}
			}
			if (target != null) {
				players [currentPlayerIndex].actionPoints--;
				removeTileHighlights();
				players [currentPlayerIndex].attack = false;
				players [currentPlayerIndex].animationAttackBool = true;	
				bool hit = Random.Range (0.0f, 1.0f) <= players [currentPlayerIndex].attackchance;
				if (hit) {
					int amountOfDamage = (int)Mathf.Floor (players [currentPlayerIndex].dmg + Random.Range (1, players [currentPlayerIndex].dmgdie));
					target.HP -= amountOfDamage;
					target.Health.fillAmount = target.HP/target.maxHP;
					if (target.HP <= 0) {
						players [currentPlayerIndex].animationAttackBool = false;
					}
					if (target.userplayerindex == 1 && target.HP <= 0) {
						numofdeadplayers++;
						if (numofdeadplayers == 2) {
							target.animationDeadBool = true;
							gameOver ();
						}
					}
				}
			}
		}
	}

	public void gameOver(){
		SceneManager.LoadScene ("gameOverScene");
	}

	public void generateMap(){
		loadMapFromXml ();
	}

	void loadMapFromXml(){
		MapXmlContainer container = mapSaveLoad.Load (mapXml);
		mapSize = container.size;
		for (int i = 0; i < mapTransform.childCount; i++) {
			Destroy (mapTransform.GetChild (i).gameObject);
		}
		map = new List<List<NetTile>> ();
		for (int i = 0; i < mapSize; i++) {
			List <NetTile> row = new List<NetTile> ();
			for (int j = 0; j < mapSize; j++) {
				NetTile tile = ((GameObject)Instantiate (PrefabHolder.instance.BASE_TILE_PREFAB, new Vector3 (i - Mathf.Floor (mapSize / 2), 0, -j + Mathf.Floor (mapSize / 2)), Quaternion.Euler (new Vector3 ()))).GetComponent<NetTile> ();
				tile.transform.parent = mapTransform;
				tile.gridPosition = new Vector2 (i, j);
				tile.setType ((TileType)container.tiles.Where(x=>x.locX==i&&x.locY==j).First().id);
				row.Add (tile);			
			}
			map.Add (row);
		}
	}

	public void generatePlayers(){

	}
}
