using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Threading;
using UnityEngine.Networking;

public class NetworkUserPlayer : NetworkPlayer {

	private Animator anim;
	private HashIDs hash;

	void Awake(){

	}

	void Start () {
		if (isLocalPlayer) {
			NetManager.instance.gameObject.SetActive (true);
			NetManager.instance.generateMap ();
			NetManager.instance.generatePlayers ();
			do {
				gridPosition = new Vector2 (Random.Range (0, NetManager.instance.mapSize), Random.Range (0, NetManager.instance.mapSize));
			} while(NetManager.instance.players.Where (x => x.gridPosition == gridPosition).Any () || NetManager.instance.map [(int)gridPosition.x] [(int)gridPosition.y].impassable);
			CmdSetGridPosition (gridPosition);
			CmdUpdateTransformPosition ();
			transform.position = new Vector3 (gridPosition.x - Mathf.Floor (NetManager.instance.mapSize / 2), 1.5f, -gridPosition.y + Mathf.Floor (NetManager.instance.mapSize / 2));
			transform.rotation = Quaternion.Euler (Vector3.zero);
			playerName = "Player - " + NetManager.instance.players.Count.ToString ();
			id = (int)GetComponent<NetworkIdentity> ().netId.Value;
			CmdSetId ((int)GetComponent<NetworkIdentity> ().netId.Value);
			NetManager.instance.players.Add (this);
		} else {
			transform.position = new Vector3 (gridPosition.x - Mathf.Floor (NetManager.instance.mapSize / 2), 1.5f, -gridPosition.y + Mathf.Floor (NetManager.instance.mapSize / 2));
			NetManager.instance.players.Add (this);
		}
		anim = GetComponent<Animator> ();
		hash = GameObject.FindGameObjectWithTag ("NetManager").GetComponent<HashIDs> ();
		Health = transform.FindChild ("Canvas").FindChild ("HealthBar").FindChild ("Health").GetComponent<Image> ();
	}

	IEnumerator StartRoutine(){
		while (NetManager.instance == null || !NetManager.instance.gameObject.activeSelf) {
			yield return null;
		}
	}

	public override void Update () {
		if (NetManager.instance.players.Any ()) {
			if (NetManager.instance.players [NetManager.instance.currentPlayerIndex] == this) {
				transform.GetComponent<Renderer> ().material.color = Color.green;
			} else {
				transform.GetComponent<Renderer> ().material.color = Color.white;
			}
		}
		deadAnimation (animationDeadBool);
		moveAnimation (animationMoveBool);
		attackAnimation (animationAttackBool);
		if (NetManager.instance.players [NetManager.instance.currentPlayerIndex] == this) {
			transform.GetComponent<Renderer>().material.color = Color.blue;
		} else {
			transform.GetComponent<Renderer>().material.color = Color.white;
		}
		base.Update();
	}

	public override void TurnUpdate(){
		if (positionQueue.Count > 0) {
			transform.position += (positionQueue[0] - transform.position).normalized * moveSpeed * Time.deltaTime;
			if (Vector3.Distance (positionQueue[0], transform.position) <= 0.1f) {
				transform.position = positionQueue[0];
				positionQueue.RemoveAt (0);
				if (positionQueue.Count == 0) {
					actionPoints--;
					NetManager.instance.removeTileHighlights();
				}
			}
		}
		base.TurnUpdate ();
	}


	public override void OnStartLocalPlayer(){
		GetComponent<MeshRenderer> ().material.color = Color.red;
	}

	public void deadAnimation(bool dead){
		anim.SetBool (hash.deadBool, dead);
	}

	public void moveAnimation(bool move){
		anim.SetBool (hash.moveBool, move);
	}

	public void attackAnimation(bool attack){
		anim.SetBool (hash.attackBool, attack);
	}



	public override void TurnOnGUI(){
		float buttonHeight = 50;
		float buttonWidth = 150;

		//move
		Rect buttonRect = new Rect (0, Screen.height - buttonHeight * 3, buttonWidth, buttonHeight);
		if (GUI.Button (buttonRect, "Move")) {
			if (!move) {
				NetManager.instance.removeTileHighlights();
				move = true;
				attack = false;
				NetManager.instance.highlightTilesAt (gridPosition, Color.blue, movementPerActionPoint,false,"blue");
			} else {
				move = false;
				attack = false;
				NetManager.instance.removeTileHighlights();
			}
		}

		/*buttonRect=new Rect(0,Screen.height-buttonHeight*3,buttonWidth,buttonHeight);
		if (GUI.Button (buttonRect, "Melee Attack")) {
			if (!attack) {
				NetManager.instance.removeTileHighlights();
				move = false;
				attack = true;
				NetManager.instance.highlightTilesAt (gridPosition, Color.red, meleeAttackRange, true, "red");

			} else {
				move = false;
				attack = false;
				NetManager.instance.removeTileHighlights();
			}
		}*/

		// ranged attack
		buttonRect=new Rect(0,Screen.height-buttonHeight*2,buttonWidth,buttonHeight);
		if (GUI.Button (buttonRect, "Attack")) {
			if (!attack) {
				NetManager.instance.removeTileHighlights();
				move = false;
				attack = true;
				NetManager.instance.highlightTilesAt (gridPosition, Color.red, rangedAttackRange, true, "red");
			} else {
				move = false;
				attack = false;
				NetManager.instance.removeTileHighlights();
			}
		}

		//end turn
		buttonRect=new Rect(0,Screen.height-buttonHeight*1,buttonWidth,buttonHeight);
		if (GUI.Button (buttonRect, "End Turn")) {
			NetManager.instance.removeTileHighlights();
			actionPoints = 2;
			move = false;
			attack = false;
			CmdNextTurn ();
		}
		base.TurnOnGUI ();
	}

	void OnMouseEnter(){
		if (NetManager.instance.players [NetManager.instance.currentPlayerIndex].move||NetManager.instance.players [Manager.instance.currentPlayerIndex].attack) {
			transform.GetComponent<Renderer>().material.color = Color.green;
		}
	}

	void OnMouseExit(){
		transform.GetComponent<Renderer>().material.color = Color.white;
	}
}
