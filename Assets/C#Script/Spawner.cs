using UnityEngine;
using System.Collections;

public class Spawner : MonoBehaviour {

    public Wave[] waves; //array of waves
    public Enemy enemy; //enemy

    LivingEntity playerEntity; //reference to "LivingEntity" script
    Transform PlayerT; //reference to player's transform

    Wave currentWave; //current wave
    int currentWaveNumber; //current wave number

    int enemiesRemainingToSpawn; //remaining enemy
    int enemiesRemainingAlive; //remaining enemy alive
    float nextSpawnTime; //next enemy spawn time

    MapGenerator map; //reference to "MapGenerator" script

    float timeBetweenCampingChecks = 2; //time between check for player camping
    float campThresholdDistance = 1.5f; //minimum distance to move before consifer camping
    float nextCampCheckTime; //time for next camping check
    Vector3 campPositionOld; //old camping position
    bool isCamping; //is player camping

    bool isDisable; //is the game disable

    public event System.Action<int> onNewWave; //event for onNewWave

    void Start() //start the wave
    {
        playerEntity = FindObjectOfType<Player>(); //get reference to the player
        PlayerT = playerEntity.transform; //save player's transform

        nextCampCheckTime = timeBetweenCampingChecks + Time.time; //update camp time check
        campPositionOld = PlayerT.position; //update player's camping position
        playerEntity.OnDeath += OnPlayerDeath; //run "OnPlayerDeath" function when LivingEntity's "OnDeath" function is executed

        map = FindObjectOfType<MapGenerator>(); //ger reference to "MapGenerator" script
        NextWave(); //run "NextWave" function
    }

    void Update()
    {
        if (!isDisable) //if the game is running, player is alive
        {
            if (Time.time > nextCampCheckTime) //is time for camping check
            {
                nextCampCheckTime = Time.time + timeBetweenCampingChecks; //update checking time

                isCamping = (Vector3.Distance(PlayerT.position, campPositionOld) < campThresholdDistance); //check wheather player have move more then camp distance threshold
                campPositionOld = PlayerT.position; //set player's new position to campPositionOld
            }

            if ((enemiesRemainingToSpawn > 0 || currentWave.infinite) && Time.time > nextSpawnTime) //if there is enemy to spawn or current wave is infinite, and it is time to spawn enemy
            {
                enemiesRemainingToSpawn--; //decerease enemies remain to spawn
                nextSpawnTime = Time.time + currentWave.timeBetweenSpawns; //reset cooldown between each spawn

                StartCoroutine(SpawnEnemy()); //run "SpawnEnemy" function
            }
        }
    }

    //spawn enemy
    IEnumerator SpawnEnemy()
    {
        float spawnDelay = 1; //delay before spawn
        float tileFlashSpeed = 4; //tile flashing speed
        Transform spawnTile = map.GetRandomOpenTile(); //get a spawn tile randomly using "GetRandomOpenTile" function in "MapGenerator" script

        if (isCamping) //if player is camping
        {
            spawnTile = map.GetTileFromPosition(PlayerT.position); //tile to spawn enemy equals to player position using "GetTileFromPosition" function in "MapGenerator" script
        }

        Material tileMat = spawnTile.GetComponent<Renderer>().material; //get reference to the tilemap material
        Color initialColor = tileMat.color; //set original tile mat color
        Color flashColor = Color.red; //set color for tile map to flash
        float spawnTimer = 0; //set spawn timer

        while(spawnTimer < spawnDelay) //when spawn timer < delay before spawn
        {
            tileMat.color = Color.Lerp(initialColor, flashColor, Mathf.PingPong(spawnTimer * tileFlashSpeed, 1)); //create flash from red to original color of the tile map
            spawnTimer += Time.deltaTime; //increase spawn timer over time
            yield return null;
        }

        Enemy spawnedEnemy = Instantiate(enemy, spawnTile.position + Vector3.up, Quaternion.identity) as Enemy; //spawn enemy
        spawnedEnemy.OnDeath += OnEnemyDeath; //run "OnEnemyDeath" function when Enemy's "OnEnemyDeath" function is executed
        spawnedEnemy.SetCharacteristic(currentWave.moveSpeed, currentWave.hitsToKillPlayer, currentWave.enemyHealth, currentWave.skinColor); //run Enemy's "SetCharacteristic" function
    }

    //when player is dead
    void OnPlayerDeath()
    {
        isDisable = true; //game is disable
    }

    //when enemy dead
    void OnEnemyDeath()
    {
        enemiesRemainingAlive--; //decrease number of enemy remaining alive

        if(enemiesRemainingAlive == 0) //if no enemy aive
        {
            NextWave(); //spawn next wave
        }
    }

    //reset player position in each new wave
    void ResetPlayerPosition()
    {
        PlayerT.position = map.GetTileFromPosition(Vector3.zero).position + Vector3.up * 3; //set new position in centre of new map
    }

    //count remaining enemy avaliable to spawn and remaining waves avaliable
    void NextWave() 
    {
        if(currentWaveNumber > 0)
        {
            AudioManager.instance.PlaySound2D("Level Complete"); //play sound if player complete a wave
        }
        currentWaveNumber++; //increase current wave number
        if(currentWaveNumber -1 < waves.Length) //if this is not the last wave
        {
            currentWave = waves[currentWaveNumber - 1]; //set current wave which wave is it now

            enemiesRemainingToSpawn = currentWave.enemyCount; //reset enemy to spawn
            enemiesRemainingAlive = enemiesRemainingToSpawn; //reset enemy alive

            if(onNewWave != null) //if there is an event called onNewWave
            {
                onNewWave(currentWaveNumber); //create next wave
            }

            ResetPlayerPosition(); //reset player position to the centre of the map
        }
    }

    [System.Serializable]
    public class Wave
    {
        public bool infinite; //is this wave have infinite enemy
        public int enemyCount; //number of enemy in the wave
        public float timeBetweenSpawns; //delays between each enemy spawn

        public float moveSpeed; //moving speed of enemy
        public int hitsToKillPlayer; //damage of enemy
        public float enemyHealth; //hits to kill enemy
        public Color skinColor; //color of enemy
    }
}
