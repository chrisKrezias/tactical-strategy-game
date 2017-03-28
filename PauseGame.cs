using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Networking;

public class PauseGame : MonoBehaviour {
	public Transform canvas;
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.Escape)) {
			Pause ();
		}
	}

	public void Pause(){
		if (canvas.gameObject.activeInHierarchy == false) {
			canvas.gameObject.SetActive (true);
			Time.timeScale = 0;
		} else {
			canvas.gameObject.SetActive (false);
			Time.timeScale = 1;
		}
	}

	public void LoadScene(string name){
		SceneManager.LoadScene (name);
	}

	public void QuitGame(){
		Application.Quit ();
	}

	public void Save(){
		PlayerPrefs.SetString ("currentscenesave", SceneManager.GetActiveScene().name);
		PlayerPrefs.Save ();
	}

	public void Load(){
		SceneManager.LoadScene (PlayerPrefs.GetString ("currentscenesave"));
	}
}
