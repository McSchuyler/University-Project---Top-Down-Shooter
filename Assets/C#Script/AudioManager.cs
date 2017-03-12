using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public enum AudioChannel { Master, Sfx, Music}; //self define variable for Master volume, sound effect volume and BGM volume

    public float masterVolumePercent { get; private set; } //master volume (can get the value but cannot set in other script)
    public float sfxVolumePercent { get; private set; } //sound effect volume (can get the value but cannot set in other script)
    public float musicVolumePercent { get; private set; } //BG music volume (can get the value but cannot set in other script)

    AudioSource sfx2DSource; //2D music for sfx
    AudioSource[] musicSources; //array for BGM
    int activeMusicSourceIndex; //current active BGM

    public static AudioManager instance; //to let other script access to audio manager

    Transform audioListener; //Listener position
    Transform playerT; //player position

    SoundLibrary library; //reference to SoundLibrary script

    void Awake()
    {
        if(instance != null){ 
            Destroy(gameObject); //when reload the scene, if there is already a instance, destory this instance to prevent duplicate
        }
        else
        {
            instance = this; //create an instance to enable other script to access audio manager
            DontDestroyOnLoad(gameObject); //dont destory this even reload the game scene

            library = GetComponent<SoundLibrary>(); //get reference of SoundLibrary script

            musicSources = new AudioSource[2]; //array contain 2 slot for AudioSource
            for (int i = 0; i < 2; i++) //loop through the 2 BGM
            {
                GameObject newMusicSource = new GameObject("Music source " + (i + 1)); //create a game object for the BGM
                musicSources[i] = newMusicSource.AddComponent<AudioSource>(); //add reference to AudioSource
                newMusicSource.transform.parent = transform; //arrange newMusicSource to be a child of Audio Manager
            }
            GameObject newSfx2Dsource = new GameObject("2D sfx source"); //create new game object for 2d sfx
            sfx2DSource = newSfx2Dsource.AddComponent<AudioSource>(); //add reference to audio source
            newSfx2Dsource.transform.parent = transform; //arrage newSfx2Dsource ito be child of Audio Manager

            audioListener = FindObjectOfType<AudioListener>().transform; //find position of audio listener

            if(FindObjectOfType<Player>() != null) //if there is a player
            {
                playerT = FindObjectOfType<Player>().transform; //find position of player
            }
           
            masterVolumePercent = PlayerPrefs.GetFloat("master vol", 1); //load the setting of master volume
            sfxVolumePercent = PlayerPrefs.GetFloat("sfx vol", 1); //load the setting of sfx volume
            musicVolumePercent = PlayerPrefs.GetFloat("music vol", 1); //load the setting of bgm volume
        }       
    }

    void Update()
    {
        if (playerT != null) //if there is a player
        {
            audioListener.position = playerT.position; //position of audio listener is position of player
        }
    }

    public void SetVolume(float volumePercent, AudioChannel channel) //function to set volume 
    {
        switch (channel)
        {
            case AudioChannel.Master: //for master channel
                masterVolumePercent = volumePercent;
                break;
            case AudioChannel.Sfx: //for sound effect channel
                sfxVolumePercent = volumePercent;
                break;
            case AudioChannel.Music: //for BGM channel
                musicVolumePercent = volumePercent;
                break;
        }
        musicSources[0].volume = musicVolumePercent * masterVolumePercent; //make change of the volume if the volume is changed
        musicSources[1].volume = musicVolumePercent * masterVolumePercent; //make change of the volume if the volume is changed

        PlayerPrefs.SetFloat("master vol", masterVolumePercent); //determine the setting for master volume;
        PlayerPrefs.SetFloat("sfx vol", sfxVolumePercent);//determine the setting for sfx volume;
        PlayerPrefs.SetFloat("music vol", musicVolumePercent);//determine the setting for bgm volume;
        PlayerPrefs.Save(); //save the setting
    }

    public void PlayMusic(AudioClip clip, float fadeDuration = 1) //plays BGM with 1s fade in/ fade out
    {
        activeMusicSourceIndex = 1 - activeMusicSourceIndex; //determine the BGM it should play (array start from 0 and music index start from 1)
        musicSources[activeMusicSourceIndex].clip = clip; //get the BGM
        musicSources[activeMusicSourceIndex].Play(); //play

        StartCoroutine(AnimateMusicCrossfade(fadeDuration)); //run function to fade the BGM
    }

    public void PlaySound(AudioClip clip, Vector3 pos) //function to play sfx
    {
        if (clip != null)//if is playing a clip
        {
            AudioSource.PlayClipAtPoint(clip, pos, sfxVolumePercent * masterVolumePercent); //play the sound at certain position and certain volume
        }
    }

    public void PlaySound2D(string soundName) //function to play 2d sound
    {
        sfx2DSource.PlayOneShot(library.GetClipFromName(soundName), sfxVolumePercent * masterVolumePercent); //play the sound at certain volume
    }

    public void PlaySound(string soundName, Vector3 pos) //function to get clip to play using string
    {
        PlaySound(library.GetClipFromName(soundName), pos); //call the function to play the clip
    }

    IEnumerator AnimateMusicCrossfade(float duration)//function to fade BGM
    {
        float percent = 0; //percent of fading

        while (percent < 1) //if is fading
        {
            percent += Time.deltaTime * 1 / duration; //increase completed fading over time
            musicSources[activeMusicSourceIndex].volume = Mathf.Lerp(0, musicVolumePercent * masterVolumePercent, percent); //fade in the BGM that should play
            musicSources[1 - activeMusicSourceIndex].volume = Mathf.Lerp(musicVolumePercent * masterVolumePercent, 0, percent); //fade out another BGM to stop it
            yield return null;
        }
    }

    void OnLevelWasLoaded(int index)
    {
        if (playerT == null)
        {
            if (FindObjectOfType<Player>() != null)
            {
                playerT = FindObjectOfType<Player>().transform;
            }
        }
    }
}