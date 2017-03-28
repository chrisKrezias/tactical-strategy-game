using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Threading;

public class Door : MonoBehaviour {

	public int HP=1;
	public Vector2 gridPosition=Vector2.zero;
	public string doorName="george";
	public int itemIndex=0;
	public int optionalIndex=0;
	public bool availableToAI=true;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	public void Update () {
		if (HP <= 0) {
			Destroy (gameObject);
		}
	}
}
