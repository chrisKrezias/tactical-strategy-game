using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class Tile : MonoBehaviour {

	GameObject Prefab;

	public GameObject visual;

	public TileType type=TileType.normal;

	public Vector2 gridPosition = Vector2.zero;

	public int movementCost = 1;
	public bool impassable=false;
	public int tileIndex=0;
	public int tempTileIndex=0;

	public List<Tile> neighbors = new List<Tile> ();

	// Use this for initialization
	void Start () {
		if (SceneManager.GetActiveScene().name=="gameScene"||
			SceneManager.GetActiveScene().name=="gamesScene1"||
			SceneManager.GetActiveScene().name=="gamesScene2"||
			SceneManager.GetActiveScene().name=="gamesScene2newposition"||
			SceneManager.GetActiveScene().name=="gamesScene2weaken")generateNeighbors ();
	}

	void generateNeighbors(){
		neighbors = new List<Tile> ();
		//up
		if (gridPosition.y>0){
			Vector2 n = new Vector2 (gridPosition.x, gridPosition.y - 1);
			neighbors.Add(Manager.instance.map[(int)Mathf.Round(n.x)][(int)Mathf.Round(n.y)]);
		}
		//down
		if (gridPosition.y<Manager.instance.mapSize-1){
			Vector2 n = new Vector2 (gridPosition.x, gridPosition.y + 1);
			neighbors.Add(Manager.instance.map[(int)Mathf.Round(n.x)][(int)Mathf.Round(n.y)]);
		}
		//left
		if (gridPosition.x>0){
			Vector2 n = new Vector2 (gridPosition.x - 1, gridPosition.y);
			neighbors.Add(Manager.instance.map[(int)Mathf.Round(n.x)][(int)Mathf.Round(n.y)]);
		}
		//right
		if (gridPosition.x<Manager.instance.mapSize-1){
			Vector2 n = new Vector2 (gridPosition.x + 1, gridPosition.y);
			neighbors.Add(Manager.instance.map[(int)Mathf.Round(n.x)][(int)Mathf.Round(n.y)]);
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
		case 4:
			visual.transform.GetComponent<Renderer> ().material.color = Color.cyan;
			break;
		default:
			visual.transform.GetComponent<Renderer> ().material.color = Color.white;
			break;
		}
		tempTileIndex = 0;
	}

	void OnMouseDown(){
		if (SceneManager.GetActiveScene().name=="gameScene"||
			SceneManager.GetActiveScene().name=="gamesScene1"||
			SceneManager.GetActiveScene().name=="gamesScene2"||
			SceneManager.GetActiveScene().name=="gamesScene2newposition"||
			SceneManager.GetActiveScene().name=="gamesScene2weaken") {
			if (Manager.instance.players [Manager.instance.currentPlayerIndex].move) {
				Manager.instance.moveCurrentPlayer (this);
			} else if (Manager.instance.players [Manager.instance.currentPlayerIndex].attack) {
				Manager.instance.attackWithCurrentPlayer (this);
			} else if (Manager.instance.players [Manager.instance.currentPlayerIndex].interact) {
				Manager.instance.interactWithObject (this);
				impassable = false;
			} else {
				Manager.instance.healWithCurrentPlayer (this);
			}
			visual.transform.GetComponent<Renderer> ().material.color = Color.white;
		} else if (SceneManager.GetActiveScene().name == "mapCreatorScene") {
			setType(mapCreatorManager.instance.palletSelection);
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
		case TileType.impassable:
			movementCost = 1;
			impassable = true;
			Prefab = PrefabHolder.instance.IMPASSABLE_TILE_PREFAB;
			break;
		case TileType.impassabledoor:
			movementCost = 1;
			impassable = true;
			Prefab = PrefabHolder.instance.DOOR_IMPASSABLE_TILE_PREFAB;
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
