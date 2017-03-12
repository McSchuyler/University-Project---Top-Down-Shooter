using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{

    public AudioClip mainTheme; //1st BGM for main
    public AudioClip menuTheme; //2nd BGM for menu

    string sceneName; //for current scene  name (menu or game)

    void Start()
    {
        OnLevelWasLoaded(0); //run function if unity does not auto run the function
    }

    void OnLevelWasLoaded(int sceneIndex) //unity will automatically run it
    {
        string newSceneName = SceneManager.GetActiveScene().name; //get scene name
        if (newSceneName != sceneName) //already change scene
        {
            sceneName = newSceneName; //reset scene name
            Invoke("PlayMusic", .2f); //wait then play music
        }
    }

    void PlayMusic()
    {
        AudioClip clipToPlay = null; 

        if (sceneName == "Menu") //if at menu
        {
            clipToPlay = menuTheme; //play menu sound clip
        }
        else if (sceneName == "Game") //if at game
        {
            clipToPlay = mainTheme; //play game sound clip
        }

        if (clipToPlay != null) //if there is clip to play
        {
            AudioManager.instance.PlayMusic(clipToPlay, 2); //play sound clip 
            Invoke("PlayMusic", clipToPlay.length); //repeat the music
        }

    }

}