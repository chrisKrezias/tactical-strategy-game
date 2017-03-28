using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MusicManager : MonoBehaviour {

	public AudioClip mainTheme;
	public AudioClip menuTheme;

	void Start(){
		if (SceneManager.GetActiveScene ().name != "introScene") {
			AudioManager.instance.PlayMusic (mainTheme, 2);
		} else {
			AudioManager.instance.PlayMusic (menuTheme, 2);
		}
	}

}
