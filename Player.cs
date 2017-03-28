using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class Player : MonoBehaviour {

	public Vector2 gridPosition=Vector2.zero;
	public float moveSpeed = 10.0f;
	public int movementPerActionPoint = 5;
	public int meleeAttackRange = 1;
	public int rangedAttackRange = 5;
	public Vector3 moveDestination;
	public bool move=false;
	public bool attack=false;
	public bool interact=false;
	public bool heal=false;
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
	public int playerindex=0;
	public bool healer=false;
	public int attackRange=2;
	public Image Health;
	//movement animation
	public List<Vector3>positionQueue=new List<Vector3>();

	//
	//private Vector3 position;

	void Awake(){
		moveDestination = transform.position;
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
		/*if (Input.GetMouseButtonDown (0)) {
			locatePosition();
		}
		rotatePosition ();*/
	}
		

	/*void locatePosition(){
		RaycastHit hit;
		Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
		if (Physics.Raycast (ray, out hit, 1000)) {
			position = new Vector3 (hit.point.x, hit.point.y, hit.point.z);
		}
	}

	void rotatePosition(){
		if (Vector3.Distance (transform.position, position) > 1) {
			Quaternion newRotation = Quaternion.LookRotation (position - transform.position, Vector3.forward);
			newRotation.x = 0f;
			newRotation.z = 0f;
			transform.rotation = Quaternion.Slerp (transform.rotation, newRotation, Time.deltaTime * 10);
		}
	}*/

	public virtual void TurnUpdate(){
		if (actionPoints <= 0) {
			actionPoints = 2;
			move = false;
			attack = false;
			interact = false;
			heal = false;
			Manager.instance.nextTurn ();
		}
	}

	public virtual void TurnOnGUI(){

	}
		

	/*public void OnGUI(){
		//display HP
		Vector3 location=Camera.main.WorldToScreenPoint(transform.position)+Vector3.up*35;
		GUI.TextArea (new Rect (location.x, Screen.height - location.y, 30, 20), HP.ToString ());
	}*/


		
}
