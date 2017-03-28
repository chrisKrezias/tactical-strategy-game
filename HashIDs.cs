using UnityEngine;
using System.Collections;

public class HashIDs : MonoBehaviour {
	public int deadState;
	public int deadBool;
	public int moveState;
	public int moveBool;
	public int attackState;
	public int attackBool;
	void Awake(){
		deadState = Animator.StringToHash ("Base Layer.Dead");
		deadBool = Animator.StringToHash ("Dead");
		moveState = Animator.StringToHash ("Base Layer.Move");
		moveBool = Animator.StringToHash ("Move");
		attackState = Animator.StringToHash ("Base Layer.Attack");
		attackBool = Animator.StringToHash ("Attack");
	}
}
