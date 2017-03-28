using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour {
	public Transform mainMenu, optionsMenu, soundMenu, graphicsMenu, controlsMenu;
	public Slider[] volumeSliders;
	public Toggle[] resolutionToggles;
	public Toggle fullscreenToggle;
	public int[] screenWidths;
	int activeScreenResIndex;

	void Start(){
		activeScreenResIndex = PlayerPrefs.GetInt ("screen res index");
		bool isFullscreen = (PlayerPrefs.GetInt ("fullscreen") == 1) ? true : false;
//		volumeSliders [0].value = AudioManager.instance.masterVolumePercent;
//		volumeSliders [1].value = AudioManager.instance.musicVolumePercent;
//		volumeSliders [2].value = AudioManager.instance.sfxVolumePercent;
		for (int i = 0; i < resolutionToggles.Length; i++) {
			resolutionToggles [i].isOn = i == activeScreenResIndex;
		}
		fullscreenToggle.isOn=isFullscreen;
	}

	public void LoadScene(string name){
		SceneManager.LoadScene (name);
	}

	public void MultiplayerScene(string name){
		SceneManager.LoadScene (name);
	}

	public void QuitGame(){
		Application.Quit ();
	}

	public void Load(){
		SceneManager.LoadScene (PlayerPrefs.GetString ("currentscenesave"));
	}

	public void OptionsMenu(bool clicked){
		if (clicked == true) {
			optionsMenu.gameObject.SetActive (clicked);
			mainMenu.gameObject.SetActive (false);
		} else {
			optionsMenu.gameObject.SetActive (clicked);
			mainMenu.gameObject.SetActive (true);
		}
	}

	public void SoundMenu(bool clicked){
		if (clicked == true) {
			soundMenu.gameObject.SetActive (clicked);
			optionsMenu.gameObject.SetActive (false);
		} else {
			soundMenu.gameObject.SetActive (clicked);
			optionsMenu.gameObject.SetActive (true);
		}
	}

	public void ControlsMenu(bool clicked){
		if (clicked == true) {
			controlsMenu.gameObject.SetActive (clicked);
			optionsMenu.gameObject.SetActive (false);
		} else {
			controlsMenu.gameObject.SetActive (clicked);
			optionsMenu.gameObject.SetActive (true);
		}
	}

	public void GraphicsMenu(bool clicked){
		if (clicked == true) {
			graphicsMenu.gameObject.SetActive (clicked);
			optionsMenu.gameObject.SetActive (false);
		} else {
			graphicsMenu.gameObject.SetActive (clicked);
			optionsMenu.gameObject.SetActive (true);
		}
	}

	public void SetScreenResolution(int i){
		if (resolutionToggles [i].isOn) {
			activeScreenResIndex = i;
			float aspectRatio = 16 / 9f;
			Screen.SetResolution (screenWidths [i], (int)(screenWidths [i] / aspectRatio), false);
			PlayerPrefs.SetInt ("screen res index", activeScreenResIndex);
			PlayerPrefs.Save ();
		}
	}

	public void SetFullscreen(bool isFullscreen){
		for (int i = 0; i < resolutionToggles.Length; i++) {
			resolutionToggles [i].interactable = !isFullscreen;
		}
		if (isFullscreen) {
			Resolution[] allResolutions = Screen.resolutions;
			Resolution maxResolution = allResolutions [allResolutions.Length - 1];
			Screen.SetResolution (maxResolution.width, maxResolution.height, true);
		} else {
			SetScreenResolution (activeScreenResIndex);
		}
		PlayerPrefs.SetInt ("fullscreen", ((isFullscreen) ? 1 : 0));
		PlayerPrefs.Save ();
	}

	public void SetMasterVolume(float value){
		AudioManager.instance.SetVolume (value, AudioManager.AudioChannel.Master);
	}

	public void SetMusicVolume(float value){
		AudioManager.instance.SetVolume (value, AudioManager.AudioChannel.Music);
	}

	public void SetSfxVolume(float value){
		AudioManager.instance.SetVolume (value, AudioManager.AudioChannel.Sfx);
	}
		
}
