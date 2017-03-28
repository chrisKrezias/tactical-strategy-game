using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Threading;

public class AIPlayer : Player {

	private Animator anim;
	private HashIDs hash;


	// Use this for initialization
	void Start () {
		anim = GetComponent<Animator> ();
		hash = GameObject.FindGameObjectWithTag ("Manager").GetComponent<HashIDs> ();
		Health = transform.FindChild ("Canvas").FindChild ("HealthBar").FindChild ("Health").GetComponent<Image> ();
	}
	
	// Update is called once per frame
	public void Update () {
		moveAnimation (animationMoveBool);
		attackAnimation (animationAttackBool);
		deadAnimation (animationDeadBool);
		if (Manager.instance.players [Manager.instance.currentPlayerIndex] == this) {
			transform.GetComponent<Renderer>().material.color = Color.red;
		} else {
			transform.GetComponent<Renderer>().material.color = Color.white;
		}
		if (HP <= 0) {
			Destroy (gameObject);
			if (animationAttackBool) animationAttackBool = false;
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
			if (Vector3.Distance (positionQueue [0], transform.position) <= 0.1f) {
				transform.position = positionQueue [0];
				positionQueue.RemoveAt (0);
				if (positionQueue.Count == 0) {
					actionPoints--;
				}
			}
			} else {
				//priority queu
			List<Tile> attacktilesInRange=TileHighlight.FindHighlight(Manager.instance.map[(int)gridPosition.x][(int)gridPosition.y],attackRange,new Vector2[0], new Vector2[0],false);
			//List<Tile> movementToAttackTilesInRange=TileHighlight.FindHighlight(Manager.instance.map[(int)gridPosition.x][(int)gridPosition.y],movementPerActionPoint+attackRange);
			List<Tile> movementTilesInRange=TileHighlight.FindHighlight(Manager.instance.map[(int)gridPosition.x][(int)gridPosition.y],movementPerActionPoint+1000);
				//attack if in range and with lowest HP
			if (attacktilesInRange.Where(x=>Manager.instance.players.Where(y=>y.GetType()!=typeof(AIPlayer)&&y.HP>0&&y!=this&&y.gridPosition==x.gridPosition).Count()>0).Count()>0){
				if (animationMoveBool) animationMoveBool = false;
				animationAttackBool = true;
				var opponentsInRange = attacktilesInRange.Select (x => Manager.instance.players.Where (y=>y.GetType()!=typeof(AIPlayer)&&y.HP>0 && y != this && y.gridPosition == x.gridPosition).Count() > 0 ? Manager.instance.players.Where (y => y.gridPosition == x.gridPosition).First () : null).ToList();
				Player opponent = opponentsInRange.OrderBy (x =>x!=null? -x.HP:1000).First ();
				Manager.instance.removeTileHighlights ();
				move = false;
				attack = true;
				Manager.instance.highlightTilesAt (gridPosition, Color.red, attackRange, true, false,"red");
				Manager.instance.attackWithCurrentPlayer (Manager.instance.map[(int)opponent.gridPosition.x][(int)opponent.gridPosition.y]);
				Manager.instance.removeTileHighlights ();
			}else if (attacktilesInRange.Where(x=>Manager.instance.doors.Where(y=>y.GetType()!=typeof(AIPlayer)&&y.HP>0&&y.availableToAI==true&&y!=this&&y.gridPosition==x.gridPosition).Count()>0).Count()>0){
				if (animationMoveBool) animationMoveBool = false;
				animationAttackBool = true;
				var opponentsInRange = attacktilesInRange.Select (x => Manager.instance.doors.Where (y=>y.GetType()!=typeof(AIPlayer)&&y.HP>0 && y != this && y.gridPosition == x.gridPosition).Count() > 0 ? Manager.instance.doors.Where (y => y.gridPosition == x.gridPosition).First () : null).ToList();
				Door door = opponentsInRange.OrderBy (x =>x!=null? -x.HP:1000).First ();
				Manager.instance.removeTileHighlights ();
				move = false;
				attack = true;
				Manager.instance.highlightTilesAt (gridPosition, Color.yellow, attackRange, true, true,"yellow");
				Manager.instance.interactWithObject (Manager.instance.map[(int)door.gridPosition.x][(int)door.gridPosition.y]);
				Manager.instance.removeTileHighlights ();
			}
			//move toward nearest attack range of opponent
			/*else if(movementToAttackTilesInRange.Where(x=>Manager.instance.players.Where(y=>y.GetType()!=typeof(AIPlayer)&&y.HP>0&&y!=this&&y.gridPosition==x.gridPosition).Count()>0).Count()>0){
				var opponentsInRange = movementToAttackTilesInRange.Select (x => Manager.instance.players.Where (y=>y.GetType()!=typeof(AIPlayer)&&y.HP>0 && y != this && y.gridPosition == x.gridPosition).Count() > 0 ? Manager.instance.players.Where (y => y.gridPosition == x.gridPosition).First () : null).ToList();
				Player opponent = opponentsInRange.OrderBy (x =>x!=null ? -x.HP:1000).ThenBy(x =>x!=null ? TilePathFinder.FindPath(Manager.instance.map[(int)gridPosition.x][(int)gridPosition.y],Manager.instance.map[(int)x.gridPosition.x][(int)x.gridPosition.y]).Count():1000).First ();
				Manager.instance.removeTileHighlights ();
				move = true;
				attack = false;
				Manager.instance.highlightTilesAt (gridPosition, Color.blue, movementPerActionPoint,false,false,"blue");
				List<Tile> path = TilePathFinder.FindPath (Manager.instance.map [(int)gridPosition.x] [(int)gridPosition.y], Manager.instance.map [(int)opponent.gridPosition.x] [(int)opponent.gridPosition.y],Manager.instance.players.Where(x=>x.gridPosition!=gridPosition && x.gridPosition!=opponent.gridPosition).Select(x=>x.gridPosition).ToArray(),true);
				if (path==null)
					Manager.instance.nextTurn ();
				if (path.Count () > 1) {
					List<Tile> actualMovement = TileHighlight.FindHighlight (Manager.instance.map [(int)gridPosition.x] [(int)gridPosition.y], movementPerActionPoint, Manager.instance.players.Where (x => x.gridPosition != gridPosition).Select (x => x.gridPosition).ToArray (), Manager.instance.doors.Where (x => x.gridPosition != gridPosition).Select (x => x.gridPosition).ToArray ());
					path.Reverse ();
					if (path.Where (x => actualMovement.Contains (x)).Count () > 0)
						Manager.instance.moveCurrentPlayer(path[(int)Mathf.Max(0,path.Count-attackRange)]);
					Manager.instance.removeTileHighlights ();
				} else {
					base.TurnUpdate ();
				}
			}*/
				//move toward nearest opponent
			else if(movementTilesInRange.Where(x=>Manager.instance.players.Where(y=>y.GetType()!=typeof(AIPlayer)&&y.HP>0&&y!=this&&y.gridPosition==x.gridPosition).Count()>0).Count()>0){
				var opponentsInRange = movementTilesInRange.Select (x => Manager.instance.players.Where (y=>y.GetType()!=typeof(AIPlayer)&&y.HP>0 && y != this && y.gridPosition == x.gridPosition).Count() > 0 ? Manager.instance.players.Where (y => y.gridPosition == x.gridPosition).First () : null).ToList();
				Player opponent = opponentsInRange.OrderBy(x =>x!=null ? TilePathFinder.FindPath(Manager.instance.map[(int)gridPosition.x][(int)gridPosition.y],Manager.instance.map[(int)x.gridPosition.x][(int)x.gridPosition.y]).Count():1000).First ();
				Manager.instance.removeTileHighlights ();
				if (animationAttackBool) animationAttackBool = false;
				animationMoveBool = true;
				move = true;
				attack = false;
				Manager.instance.highlightTilesAt (gridPosition, Color.blue, movementPerActionPoint,false,false,"blue");
				List<Tile> path = TilePathFinder.FindPath (Manager.instance.map [(int)gridPosition.x] [(int)gridPosition.y], Manager.instance.map [(int)opponent.gridPosition.x] [(int)opponent.gridPosition.y],Manager.instance.players.Where(x=>x.gridPosition!=gridPosition && x.gridPosition!=opponent.gridPosition).Select(x=>x.gridPosition).ToArray(),false);
				if (path == null)
					Manager.instance.nextTurn ();
				else if (path.Count () > 1) {
					List<Tile> actualMovement = TileHighlight.FindHighlight (Manager.instance.map [(int)gridPosition.x] [(int)gridPosition.y], movementPerActionPoint, Manager.instance.players.Where (x => x.gridPosition != gridPosition).Select (x => x.gridPosition).ToArray (), Manager.instance.doors.Where (x => x.gridPosition != gridPosition).Select (x => x.gridPosition).ToArray ());
					path.Reverse ();
					if (path.Where (x => actualMovement.Contains (x)).Count () > 0)
						Manager.instance.moveCurrentPlayer (path.Where (x => actualMovement.Contains (x)).First ());
					Manager.instance.removeTileHighlights ();
				} else {
					TurnUpdate ();
				}
				/*Manager.instance.moveCurrentPlayer(path[(int)Mathf.Min(Mathf.Max(path.Count-1-1,0),movementPerActionPoint-1)]);
				*/
			}
				//end
			}
		if (actionPoints <= 1 && (attack || move)) {
			move = false;
			attack = false;			
		}
		base.TurnUpdate ();
	}

	public override void TurnOnGUI(){
		base.TurnOnGUI ();
	}

	void OnMouseEnter(){
		if (Manager.instance.players [Manager.instance.currentPlayerIndex].move||Manager.instance.players [Manager.instance.currentPlayerIndex].attack) {
			transform.GetComponent<Renderer> ().material.color = Color.red;
		}
	}

	void OnMouseExit(){
		transform.GetComponent<Renderer>().material.color = Color.white;
	}
}
