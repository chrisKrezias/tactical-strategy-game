using UnityEngine;
using System.Collections;

public class FlyCamera : MonoBehaviour {

	public float speed = 50.0f;
	public int xposition1= -15;
	public int xposition2= 15;
	public int yposition1= 10;
	public int yposition2= 20;
	public int zposition1= -15;
	public int zposition2= 15;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 dir = new Vector3 ();
		if (Input.GetKey (KeyCode.W))
			dir.y += 1.0f;
		if (Input.GetKey (KeyCode.S))
			dir.y -= 1.0f;
		if (Input.GetKey (KeyCode.A))
			dir.x -= 1.0f;
		if (Input.GetKey (KeyCode.D))
			dir.x += 1.0f;
		if (Input.GetKey (KeyCode.Q))
			dir.z -= 1.0f;
		if (Input.GetKey (KeyCode.E))
			dir.z += 1.0f;
		dir.Normalize ();
		transform.Translate (dir * speed * Time.deltaTime);
		transform.position = new Vector3 (Mathf.Clamp (transform.position.x, xposition1, xposition2),
			Mathf.Clamp (transform.position.y, yposition1, yposition2), Mathf.Clamp (transform.position.z, zposition1, zposition2));
	}
}
