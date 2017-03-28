using UnityEngine;
using System.Collections;

public class PrefabHolder : MonoBehaviour {
	public static PrefabHolder instance;
	public GameObject BASE_TILE_PREFAB;
	public GameObject NORMAL_TILE_PREFAB;
	public GameObject DIFFICULT_TILE_PREFAB;
	public GameObject VERYDIFFICULT_TILE_PREFAB;
	public GameObject IMPASSABLE_TILE_PREFAB;
	public GameObject DOOR_IMPASSABLE_TILE_PREFAB;

	void Awake(){
		instance=this;
	}
}
