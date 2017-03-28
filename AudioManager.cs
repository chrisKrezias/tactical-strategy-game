using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour {

	public enum AudioChannel {Master,Sfx,Music};

	public float masterVolumePercent { get; private set;}
	public float musicVolumePercent { get; private set;}
	public float sfxVolumePercent { get; private set;}

	AudioSource[] musicSources;
	int activeMusicSourceIndex;

	//public static AudioManager instance;

	Transform audioListener;
	//Transform playerT;
	public static AudioManager instance=null;
	public static AudioManager Instance{
		get { return instance; }
	}
		
	void Awake(){
		if (instance != null && instance != this) {
			Destroy (this.gameObject);
			return;
		} else {
			instance = this;
		}
		DontDestroyOnLoad (this.gameObject);
		musicSources = new AudioSource[2];
		for (int i = 0; i < 2; i++) {
			GameObject newMusicSource = new GameObject ("Music source" + (i + 1));
			musicSources [i] = newMusicSource.AddComponent<AudioSource> ();
			newMusicSource.transform.parent = transform;
		}
		audioListener = FindObjectOfType<AudioListener> ().transform;
		masterVolumePercent = PlayerPrefs.GetFloat ("master vol", 1);
		sfxVolumePercent = PlayerPrefs.GetFloat ("sfx vol", 1);
		musicVolumePercent = PlayerPrefs.GetFloat ("music vol", 1);
	}

	public void SetVolume(float volumePercent, AudioChannel channel){
		switch (channel) {
		case AudioChannel.Master:
			masterVolumePercent=volumePercent;
			break;
		case AudioChannel.Sfx:
			sfxVolumePercent=volumePercent;
			break;
		case AudioChannel.Music:
			musicVolumePercent=volumePercent;
			break;
		}

		musicSources[0].volume=musicVolumePercent*masterVolumePercent;
		musicSources[1].volume=musicVolumePercent*masterVolumePercent;

		PlayerPrefs.SetFloat("master vol",masterVolumePercent);
		PlayerPrefs.SetFloat("sfx vol",sfxVolumePercent);
		PlayerPrefs.SetFloat("music vol",musicVolumePercent);
		PlayerPrefs.Save();
	}

	public void PlayMusic(AudioClip clip, float fadeDuration=1){
		activeMusicSourceIndex = 1 - activeMusicSourceIndex;
		musicSources [activeMusicSourceIndex].clip = clip;
		musicSources [activeMusicSourceIndex].Play ();
		musicSources [activeMusicSourceIndex].loop=true;
		StartCoroutine (AnimateMusicCrossfade (fadeDuration));
	}

	public void Playsound(AudioClip clip, Vector3 pos){
		if (clip != null) {
				AudioSource.PlayClipAtPoint (clip, pos, sfxVolumePercent * masterVolumePercent);
		}
	}

	IEnumerator AnimateMusicCrossfade(float duration){
		float percent = 0;
		while (percent < 1) {
			percent += Time.deltaTime * 1 / duration;
			musicSources [activeMusicSourceIndex].volume = Mathf.Lerp (0, musicVolumePercent * masterVolumePercent, percent);
			musicSources [1-activeMusicSourceIndex].volume = Mathf.Lerp (musicVolumePercent * masterVolumePercent, 0, percent);
			yield return null;
		}
	}

}
