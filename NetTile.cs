using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class NetTile : MonoBehaviour {

	GameObject Prefab;

	public GameObject visual;

	public TileType type=TileType.normal;

	public Vector2 gridPosition = Vector2.zero;

	public int movementCost = 1;
	public bool impassable=false;
	public int tileIndex=0;
	public int tempTileIndex=0;

	public List<NetTile> neighbors = new List<NetTile> ();

	// Use this for initialization
	void Start () {
		if (SceneManager.GetActiveScene().name=="multiplayerScene")generateNeighbors ();
	}

	void generateNeighbors(){
		neighbors = new List<NetTile> ();
		//up
		if (gridPosition.y>0){
			Vector2 n = new Vector2 (gridPosition.x, gridPosition.y - 1);
			neighbors.Add(NetManager.instance.map[(int)Mathf.Round(n.x)][(int)Mathf.Round(n.y)]);
		}
		//down
		if (gridPosition.y<NetManager.instance.mapSize-1){
			Vector2 n = new Vector2 (gridPosition.x, gridPosition.y + 1);
			neighbors.Add(NetManager.instance.map[(int)Mathf.Round(n.x)][(int)Mathf.Round(n.y)]);
		}
		//left
		if (gridPosition.x>0){
			Vector2 n = new Vector2 (gridPosition.x - 1, gridPosition.y);
			neighbors.Add(NetManager.instance.map[(int)Mathf.Round(n.x)][(int)Mathf.Round(n.y)]);
		}
		//right
		if (gridPosition.x<NetManager.instance.mapSize-1){
			Vector2 n = new Vector2 (gridPosition.x + 1, gridPosition.y);
			neighbors.Add(NetManager.instance.map[(int)Mathf.Round(n.x)][(int)Mathf.Round(n.y)]);
		}
	}
	// Update is called once per frame
	void Update () {

	}

	void OnMouseEnter(){
		if (SceneManager.GetActiveScene().name == "mapCreatorScene" && Input.GetMouseButton (0)) {
			setType (mapCreatorManager.instance.palletSelection);
		}
		if (!impassable) {
			tempTileIndex = tileIndex;
			tileIndex = 4;
			if (visual.transform.GetComponent<Renderer> ().material.color != Color.white) {
				visual.transform.GetComponent<Renderer> ().material.color = Color.green;
			} else {
				visual.transform.GetComponent<Renderer> ().material.color = Color.magenta;
			}	
		}
	}

	void OnMouseExit(){
		tileIndex = tempTileIndex;
		switch (tileIndex) {
		case 1: 
			visual.transform.GetComponent<Renderer> ().material.color = Color.blue;
			break;
		case 2: 
			visual.transform.GetComponent<Renderer> ().material.color = Color.yellow; 
			break;
		case 3: 
			visual.transform.GetComponent<Renderer> ().material.color = Color.red;
			break;
		default:
			visual.transform.GetComponent<Renderer> ().material.color = Color.white;
			break;
		}
		tempTileIndex = 0;
	}

	void OnMouseDown(){
		if (NetManager.instance.players [NetManager.instance.currentPlayerIndex].isLocalPlayer) {
			if (SceneManager.GetActiveScene ().name == "multiplayerScene") {
				if (SceneManager.GetActiveScene ().name=="multiplayerScene") {
					if (visual.transform.GetComponent<Renderer>().materials[0].color != Color.white && !impassable && NetManager.instance.players[NetManager.instance.currentPlayerIndex].move) {
						NetManager.instance.players[NetManager.instance.currentPlayerIndex].moveCurrentPlayer(this);
					} else if (NetManager.instance.players[NetManager.instance.currentPlayerIndex].attack) {
						NetManager.instance.attackWithCurrentPlayer(this);
					} /*else {
						impassable = impassable ? false : true;
						if (impassable) {
							//visual.transform.GetComponent<Renderer>().materials[0].color = new Color(.5f, .5f, 0.0f);
						} else {
							visual.transform.GetComponent<Renderer>().materials[0].color = Color.white;
						}
					}*/
				} else if (SceneManager.GetActiveScene ().name == "mapCreatorScene") {
					setType(mapCreatorManager.instance.palletSelection);
				}   
			}
		}
	}

	public void setType(TileType t){
		type = t;
		//definitions of TileType properties
		switch(t){
		case TileType.normal:
			movementCost = 1;
			impassable = false;
			Prefab = PrefabHolder.instance.NORMAL_TILE_PREFAB;
			break;
		case TileType.difficult:
			movementCost = 2;
			impassable = false;
			Prefab = PrefabHolder.instance.DIFFICULT_TILE_PREFAB;
			break;
		case TileType.verydifficult:
			movementCost = 3;
			impassable = false;
			Prefab = PrefabHolder.instance.VERYDIFFICULT_TILE_PREFAB;
			break;
		case TileType.impassable:
			movementCost = 1;
			impassable = true;
			Prefab = PrefabHolder.instance.IMPASSABLE_TILE_PREFAB;
			break;
		}
		generateVisuals ();
	}
	public void generateVisuals(){
		GameObject container = transform.FindChild ("Visuals").gameObject;
		//remove all children
		for (int i = 0; i < container.transform.childCount; i++) {
			Destroy (container.transform.GetChild (i).gameObject);
		}
		GameObject newVisual = (GameObject)Instantiate (Prefab, transform.position, Quaternion.Euler (new Vector3 (0,0,0)));
		newVisual.transform.parent = container.transform;
		visual = newVisual;
	}
}
