using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Linq;

public class NetworkPlayer : NetworkBehaviour {
	[SyncVar]
	public Vector2 gridPosition=Vector2.zero;

	[SyncVar]
	public int id;

	public static int syncStaticPlayerIndex=0;
	public float moveSpeed = 10.0f;
	public int movementPerActionPoint = 5;
	public int attackRange = 1;
	public Vector3 moveDestination;
	public bool move=false;
	public bool attack=false;
	public bool animationDeadBool=false;
	public bool animationMoveBool=false;
	public bool animationAttackBool=false;
	public float HP = 25;
	public float maxHP = 25;
	public float attackchance = 0.75f;
	public float AC = 0.15f;
	public int dmg = 5;
	public float dmgdie = 6;//d6
	public string playerName="george";
	public int actionPoints = 2;
	public int userplayerindex=0;
	public Image Health;
	public int meleeAttackRange = 1;
	public int rangedAttackRange = 5;

	[Command]
	public void CmdSetId(int newId){
		id = newId;
	}

	[ClientRpc]
	public void RpcSyncNetManagerPlayerIds(string serializedList){
		var lst = JsonUtility.FromJson<int[]> (serializedList);
		var newOrderedList = new List<NetworkPlayer>();
		foreach (var id in lst) {
			var added = GameObject.FindObjectsOfType<NetworkPlayer> ().Where (x => x.id == id).First ();
			newOrderedList.Add (added);
		}
		NetManager.instance.players = newOrderedList;
	}

	[Command]
	public void CmdSyncNetManagerPlayerIds(string serializedList){
		var lst = JsonUtility.FromJson<int[]> (serializedList);
		var newOrderedList = new List<NetworkPlayer> ();
		foreach (var id in lst) {
			var added = GameObject.FindObjectsOfType<NetworkPlayer> ().Where (x => x.id == id).First ();
			newOrderedList.Add (added);
		}
		NetManager.instance.players = newOrderedList;
	}

	public void moveCurrentPlayer(NetTile t){
		CmdMoveCurrentPlayer (JsonUtility.ToJson (t.gridPosition));
	}
	[Command]
	public void CmdMoveCurrentPlayer(string serializedGridPosition){
		RpcMoveCurrentPlayer (serializedGridPosition);
		NetManager.instance.moveCurrentPlayer (serializedGridPosition);
	}
	[ClientRpc]
	public void RpcMoveCurrentPlayer(string serializedGridPosition){
		NetManager.instance.moveCurrentPlayer (serializedGridPosition);
	}

	public List<Vector3> positionQueue = new List<Vector3> ();

	void Awake(){
		moveDestination = transform.position;
	}

	void Start () {

	}

	public virtual void Update () {
		if (HP <= 0) {
			animationAttackBool = false;
			animationDeadBool = true;
		}
	}

	public virtual void TurnUpdate(){
		if (actionPoints <= 0) {
			actionPoints = 2;
			move = false;
			attack = false;
			if (isLocalPlayer) {
				CmdNextTurn ();
			}
		}
	}

	[Command]
	public void CmdNextTurn(){
		if (NetManager.instance.currentPlayerIndex + 1 < NetManager.instance.players.Count) {
			NetManager.instance.currentPlayerIndex++;
		} else {
			NetManager.instance.currentPlayerIndex = 0;
		}
	}

	public void SetGridPosition(Vector2 newPos){
		if (isLocalPlayer) {
			CmdSetGridPosition (newPos);
		}
	}
	public void callNextTurn(){
		CmdNextTurn ();
	}
	[Command]
	public void CmdSetGridPosition(Vector2 newPos){
		gridPosition = newPos;
	}
	[Command]
	public void CmdUpdateTransformPosition(){
		transform.position = new Vector3 (gridPosition.x - Mathf.Floor (NetManager.instance.mapSize / 2), 1.5f, -gridPosition.y + Mathf.Floor (NetManager.instance.mapSize / 2));
	}

	public virtual void TurnOnGUI(){

	}
}
