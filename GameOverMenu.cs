using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverMenu : MonoBehaviour {
	public Transform canvas;

	public void MainMenuScene(){
		SceneManager.LoadScene ("introScene");
	}

	public void QuitGame(){
		Application.Quit ();
	}

	public void Load(){
		SceneManager.LoadScene (PlayerPrefs.GetString ("currentscenesave"));
	}

}
