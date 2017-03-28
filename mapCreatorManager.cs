using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

public class mapCreatorManager : MonoBehaviour {

	public static mapCreatorManager instance;
	public int mapSize;
	public string mapXml;
	public List <List<Tile>> map = new List<List<Tile>>();
	public TileType palletSelection=TileType.normal;
	Transform mapTransform;
	// Use this for initialization
	void Awake () {
		instance = this;
		mapTransform = transform.FindChild ("Map");
		generateBlankMap(mapSize);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void generateBlankMap(int mSize){
		mapSize = mSize;
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
				tile.setType (TileType.normal);
				row.Add (tile);			
			}
			map.Add (row);
		}
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
				tile.gridPosition = new Vector2 (i, j);
				tile.setType ((TileType)container.tiles.Where(x=>x.locX==i&&x.locY==j).First().id);
				row.Add (tile);			
			}
			map.Add (row);
		}
	}

	void saveMapToXml(){
		mapSaveLoad.Save (mapSaveLoad.CreateMapContainer (map), mapXml);
	}

	void OnGUI(){
		//pallet
		Rect rect = new Rect (10, Screen.height - 80, 100, 60);
		if (GUI.Button (rect, "Normal")) {
			palletSelection = TileType.normal;
		}
		rect = new Rect (10+(100+10)*1, Screen.height - 80, 100, 60);
		if (GUI.Button (rect, "Difficult")) {
			palletSelection = TileType.difficult;
		}
		rect = new Rect (10+(100+10)*2, Screen.height - 80, 100, 60);
		if (GUI.Button (rect, "Very Difficult")) {
			palletSelection = TileType.verydifficult;
		}
		rect = new Rect (10+(100+10)*3, Screen.height - 80, 100, 60);
		if (GUI.Button (rect, "Impassable")) {
			palletSelection = TileType.impassable;
		}
		rect = new Rect (10+(100+10)*4, Screen.height - 80, 100, 60);
		if (GUI.Button (rect, "ImpassableDoor")) {
			palletSelection = TileType.impassabledoor;
		}
		//IO
		rect = new Rect (Screen.width-(10+(100+10)*4), Screen.height - 80, 100, 60);
		if (GUI.Button (rect, "Clear Map")) {
			generateBlankMap (mapSize);
		}
		rect = new Rect (Screen.width-(10+(100+10)*3), Screen.height - 80, 100, 60);
		if (GUI.Button (rect, "Load Map")) {
			loadMapFromXml ();
		}
		rect = new Rect (Screen.width-(10+(100+10)*2), Screen.height - 80, 100, 60);
		if (GUI.Button (rect, "Save Map")) {
			saveMapToXml ();
		}
		rect = new Rect (Screen.width-(10+(100+10)*1), Screen.height - 80, 100, 60);
		if (GUI.Button (rect, "Exit")) {
			SceneManager.LoadScene ("introScene");
		}
	}
}
