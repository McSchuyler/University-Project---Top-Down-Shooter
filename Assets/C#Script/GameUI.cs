using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameUI : MonoBehaviour {

    //game over
    public Image fadePlane; //Game Over Background plane
    public GameObject gameOverUI; //Game object that contain text and button for game over

    //wave banner
    public RectTransform newWaveBanner; //background image for banner
    public Text newWaveTitle; //title of each wave
    public Text newWaveEnemyCount; //enemy count of each wave
    public Text scoreUI; //text for scoring
    public RectTransform healthBar; //player's health

    Spawner spawner; //reference to the spawner script
    Player player; //get reference of player

	void Start () {
        player = FindObjectOfType<Player>();//find obejct with player script
        player.OnDeath += OnGameOver;  //if player's "Ondeath" function is executed, apply "OnGameOver" function
	}

    void Awake()
    {
        spawner = FindObjectOfType<Spawner>(); //find object with spawner script
        spawner.onNewWave += OnNewWave; //apply "OnNewWave" function when spawner's "onNewWave" function is executed
    }

    void Update()
    {
        scoreUI.text = ScoreKeeper.score.ToString("D6"); //set score to 6digit (D6)

        float healthPercent = 0; //set percent to 0
        if(player != null) //if player is not dead
        {
            healthPercent = player.health / player.startingHealth; //get remaining of player health in percent
        }
        healthBar.localScale = new Vector3(healthPercent, 1, 1); //set the player health bar
    }


    //Change and write title and enemy count on the banner depending on each wave
    void OnNewWave(int waveNumber)
    {
        string[] numbers = { "One", "Two", "Three", "Four", "Five" }; //string for wave 1.2.3.4 and 5
        string[] weaponType = { "SMG", "Assault Rifle", "Assault Rifle", "Shotgun", "Gatling Gun" }; //string for weapon type of wave 1.2.3.4 and 5
        newWaveTitle.text = "-Wave" + numbers[waveNumber - 1] + "- " + weaponType[waveNumber - 1]; //write the wave title with wave number and weapon type player are using

        string enemyCountString = ((spawner.waves[waveNumber - 1].infinite) ? "Infinite" : spawner.waves[waveNumber - 1].enemyCount + ""); //string of enemy count, last wave to "infinite"
        newWaveEnemyCount.text = "Enemies: " + enemyCountString; //write the enemy count depending on the wave count
        StartCoroutine(AnimateNewWaveBanner()); //call "AnimateNewWaveBanner" function
    }

    IEnumerator AnimateNewWaveBanner()
    {
        float delayTime = 2f; //time for banner to wait
        float speed = 2.5f; //speed banner rising
        float animatePercent = 0; //to determine the percentage of animation done (rise up)
        int dir = 1; //value to determine the direction (rise up and fall down)

        float endDelayTime = Time.time + 1 / speed + delayTime; //time for end of delay

        while (animatePercent >= 0) //animatePercent is running
        {
            animatePercent += Time.deltaTime * speed * dir; //increase animatePercent over time

            if(animatePercent >= 1) //finished animation
            {
                animatePercent = 1; 
                if(Time.time > endDelayTime) //finished waiting delay time
                {
                    dir = -1; //decrease direction 
                }
            }

            newWaveBanner.anchoredPosition = Vector2.up * Mathf.Lerp(-124, 24, animatePercent); //animate the banner **(lowest position, highest position, percentage of position)**
            yield return null;
        }
    }

    //set gave over UI
    void OnGameOver()
    {
        Cursor.visible = true; //set cursur to visible
        StartCoroutine(Fade(Color.clear, Color.black, 1)); //call "Fade" function 
        gameOverUI.SetActive(true); //activate gameobject with "game over" text and restart button
    }

    IEnumerator Fade(Color from, Color to, float time)
    {
        float speed = 1 / time; //fade speed
        float percent = 0; //fade percentage

        while (percent < 1) //if fade animation is not complete
        {
            percent += Time.deltaTime * speed; //increase fade percent over time
            fadePlane.color = Color.Lerp(from, to, percent); //animate the fade effect **(original clear color, black color, percentage of fade)
            yield return null;
        }
    }

    //start new game
    public void StartNewGame()
    {
       
        SceneManager.LoadScene("Game"); //run new game scene
    }
	
    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("Menu"); //menu scene
    }
}
