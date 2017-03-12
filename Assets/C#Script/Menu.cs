using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement; //to access scene manager
using UnityEngine.UI; //to use slider and toggle

public class Menu : MonoBehaviour
{

    public GameObject mainMenuHolder; //reference to main menu
    public GameObject optionsMenuHolder; //reference to option menu

    public Slider[] volumeSliders; //array for sliders in menu
    public Toggle[] resolutionToggles; //array of toggle in menu
    public Toggle fullscreenToggle; //toggle for fullscreen
    public int[] screenWidths; //array to save screen width for resolution
    int activeScreenResIndex; //keep track of active toggle resolution

    void Start()
    {
        activeScreenResIndex = PlayerPrefs.GetInt("screen res index"); //load resolution
        bool isFullscreen = (PlayerPrefs.GetInt("fullscreen") == 1) ? true : false; //load full screen or not

        //set respective volume
        volumeSliders[0].value = AudioManager.instance.masterVolumePercent;
        volumeSliders[1].value = AudioManager.instance.musicVolumePercent;
        volumeSliders[2].value = AudioManager.instance.sfxVolumePercent;

        for (int i = 0; i < resolutionToggles.Length; i++)
        {
            resolutionToggles[i].isOn = i == activeScreenResIndex; //toggle the resolution if current resolution is equal to saved resolution
        }

        fullscreenToggle.isOn = isFullscreen; //call full screen function if is fullscreen
    }


    public void Play() //function to start game
    {
        SceneManager.LoadScene("Game"); //start "Game" scene
    }

    public void Quit() //function to quit game
    {
        Application.Quit(); //Quit game
    }

    public void OptionsMenu() //function to show option menu
    {
        mainMenuHolder.SetActive(false); //hide main menu
        optionsMenuHolder.SetActive(true); //show option menu
    }

    public void MainMenu() //function to show main menu
    {
        mainMenuHolder.SetActive(true); //show main menu
        optionsMenuHolder.SetActive(false); //hide option menu
    }

    public void SetScreenResolution(int i)
    {
        if (resolutionToggles[i].isOn) //if any off the resolution toggle is on
        {
            activeScreenResIndex = i;
            float aspectRatio = 16 / 9f; //aspect ratio
            Screen.SetResolution(screenWidths[i], (int)(screenWidths[i] / aspectRatio), false); //set resolution
            PlayerPrefs.SetInt("screen res index", activeScreenResIndex); //determine what index to save
            PlayerPrefs.Save(); //save the index for resolution
        }
    }

    public void SetFullscreen(bool isFullscreen)
    {
        for (int i = 0; i < resolutionToggles.Length; i++) //loop through all resolution toggle
        {
            resolutionToggles[i].interactable = !isFullscreen; //make resolution toggle uninteractable if is fullscreen
        }

        if (isFullscreen) //if is fullscreen
        {
            Resolution[] allResolutions = Screen.resolutions; // get array of all resolution support by monitor
            Resolution maxResolution = allResolutions[allResolutions.Length - 1]; //get the last resolution as the last is the biggest resolution
            Screen.SetResolution(maxResolution.width, maxResolution.height, true); //set resolution
        }
        else //if set to not fullscreen
        {
            SetScreenResolution(activeScreenResIndex); //set the active resolution toggle before fullscreen
        }

        PlayerPrefs.SetInt("fullscreen", ((isFullscreen) ? 1 : 0)); //if is fullscreen set to 1 else to 0
        PlayerPrefs.Save(); //save the index for fullscreen or not
    }

    public void SetMasterVolume(float value) //function to set master volume
    {
        AudioManager.instance.SetVolume(value, AudioManager.AudioChannel.Master); //run function in audio manager script to set respective volume
    }

    public void SetMusicVolume(float value) //function to set BGM volume
    {
        AudioManager.instance.SetVolume(value, AudioManager.AudioChannel.Music); //run function in audio manager script to set respective volume
    }

    public void SetSfxVolume(float value) //function to set sfx volume
    {
        AudioManager.instance.SetVolume(value, AudioManager.AudioChannel.Sfx); //run function in audio manager script to set respective volume
    }
    
}