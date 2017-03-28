using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Threading;

public class UserPlayer : Player {

	private Animator anim;
	private HashIDs hash;

	void Awake(){
		
	}

	// Use this for initialization
	void Start () {
		anim = GetComponent<Animator> ();
		hash = GameObject.FindGameObjectWithTag ("Manager").GetComponent<HashIDs> ();
		Health = transform.FindChild ("Canvas").FindChild ("HealthBar").FindChild ("Health").GetComponent<Image> ();
	}
	
	// Update is called once per frame
	void Update () {
		deadAnimation (animationDeadBool);
		moveAnimation (animationMoveBool);
		attackAnimation (animationAttackBool);
		/*if (Manager.instance.players [Manager.instance.currentPlayerIndex] == this) {
			transform.GetComponent<Renderer>().material.color = Color.blue;
		} else {
			transform.GetComponent<Renderer>().material.color = Color.white;
		}*/
		if (HP <= 0) {
			animationAttackBool = false;
			animationDeadBool = true;
			//transform.GetComponent<Renderer>().material.color = Color.red;
			//transform.rotation = Quaternion.Euler (new Vector3 (90, 0, 0));
		}
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

	public override void TurnUpdate(){
		if (positionQueue.Count > 0) {
				transform.position += (positionQueue[0] - transform.position).normalized * moveSpeed * Time.deltaTime;
				if (Vector3.Distance (positionQueue[0], transform.position) <= 0.1f) {
					transform.position = positionQueue[0];
					positionQueue.RemoveAt (0);
					if (positionQueue.Count == 0) {
						actionPoints--;
						Manager.instance.removeTileHighlights();
					}
				}
		}
		base.TurnUpdate ();
	}


	public override void TurnOnGUI(){
		float buttonHeight = 50;
		float buttonWidth = 150;

	//interact
		Rect buttonRect = new Rect (0, Screen.height - buttonHeight * 4, buttonWidth, buttonHeight);
		if (GUI.Button (buttonRect, "Interact")) {
			if (!interact) {
				Manager.instance.removeTileHighlights();
				move = false;
				attack = false;
				interact = true;
				Manager.instance.highlightTilesAt (gridPosition, Color.yellow, meleeAttackRange, false,true,"yellow");
			} else {
				move = false;
				attack = false;
				interact = false;
				Manager.instance.removeTileHighlights();
			}
		}
	//move
		buttonRect = new Rect (0, Screen.height - buttonHeight * 3, buttonWidth, buttonHeight);
		if (GUI.Button (buttonRect, "Move")) {
			if (!move) {
				Manager.instance.removeTileHighlights();
				move = true;
				attack = false;
				interact = false;
				Manager.instance.highlightTilesAt (gridPosition, Color.blue, movementPerActionPoint,false,false,"blue");
			} else {
				move = false;
				attack = false;
				interact = false;
				Manager.instance.removeTileHighlights();
			}
		}

		// ranged attack
		buttonRect=new Rect(0,Screen.height-buttonHeight*2,buttonWidth,buttonHeight);
		if (GUI.Button (buttonRect, "Attack")) {
			if (!attack) {
				Manager.instance.removeTileHighlights();
				move = false;
				attack = true;
				interact = false;
				Manager.instance.highlightTilesAt (gridPosition, Color.red, rangedAttackRange, true,false,"red");
			} else {
				move = false;
				attack = false;
				interact = false;
				Manager.instance.removeTileHighlights();
			}
		}

	//end turn
		buttonRect=new Rect(0,Screen.height-buttonHeight*1,buttonWidth,buttonHeight);
		if (GUI.Button (buttonRect, "End Turn")) {
			Manager.instance.removeTileHighlights();
			actionPoints = 2;
			move = false;
			attack = false;
			interact = false;
			Manager.instance.nextTurn ();
		}

		if (healer==true) {
			Rect button = new Rect (0, Screen.height - buttonHeight * 5, buttonWidth, buttonHeight);
			if (GUI.Button (button, "Heal")) {
				if (!heal) {
					Manager.instance.removeTileHighlights();
					move = false;
					attack = false;
					interact = false;
					heal=true;
					Manager.instance.highlightTilesAt (gridPosition, Color.cyan, meleeAttackRange+1, true,false,"cyan");
				} else {
					move = false;
					attack = false;
					interact = false;
					heal = false;
					Manager.instance.removeTileHighlights();
				}
			}
		}
		base.TurnOnGUI ();
	}

	void OnMouseEnter(){
		if (Manager.instance.players [Manager.instance.currentPlayerIndex].move||Manager.instance.players [Manager.instance.currentPlayerIndex].attack) {
			transform.GetComponent<Renderer>().material.color = Color.green;
		}
	}

	void OnMouseExit(){
		transform.GetComponent<Renderer>().material.color = Color.white;
	}
}
