using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour
{
    public Map[] maps; //number of map
    public int mapIndex; //current map, start at 0 for 1st map

    public Transform tilePrefab; //square tile for the map
    public Transform obstaclePrefab; //obstacle
    public Transform backgroundMapFloor; //background of map 
    public Transform navmeshFloor; //navmesh of the map
    public Transform navmeshMaskPrefab; //object to block navmesh
    public Vector2 maxcurrentMap; //maximum map size

    [Range(0, 1)] //give a slider in editor
    public float outlinePercent; //outline of the square line

    public float tileSize; //size of tile, change this will change the size of all map related object

    List<Coord> allTileCoords; //List of all coordinates of tiles
    Queue<Coord> shuffledTileCoords; //List of all shuffled tiles
    Queue<Coord> shuffledOpenTileCoords; //List of all shuffled tiles without obstacle

    Map currentMap; //reference to current "MapGenereator" script
    Transform[,] tileMap; //position of tile map with [x.y] position

    void Awake()
    {
        FindObjectOfType<Spawner>().onNewWave += OnNewWave; //find object with "Spawner" script and when its "onNewWave" function is executed, run "OnNewWave" function
    }

    //function to tell which map to generate
    void OnNewWave(int waveNumber)
    {
        mapIndex = waveNumber - 1; //determine which map to run
        GenerateMap(); //run "GenerateMap" function
    }

    //Generate map
    public void GenerateMap()
    {
        currentMap = maps[mapIndex]; //determine which map to generate
        tileMap = new Transform[currentMap.mapSize.x, currentMap.mapSize.y]; //set map size to tilemap
        System.Random prng = new System.Random(currentMap.seed); //random generate a prng according to seed

        //Generating coordinates
        allTileCoords = new List<Coord>(); //create a new list "alltileCoords"

        //set every coordinates of tiles to the list
        for (int x = 0; x < currentMap.mapSize.x; x++)
        {
            for (int y = 0; y < currentMap.mapSize.y; y++)
            {
                allTileCoords.Add(new Coord(x, y));
            }
        }
        //Using Utility's "shuffleArray" function, randomly queue in all the tile using map seed
        shuffledTileCoords = new Queue<Coord>(Utility.ShuffleArray(allTileCoords.ToArray(), currentMap.seed));

        //Create map holder object for eazy access in Unity3D.exe
        string holderName = "Generated Map"; //name for gmae object holder
        if (transform.FindChild(holderName)) //if there is object in "generated Map"
        {
            DestroyImmediate(transform.FindChild(holderName).gameObject); //destory all in "Generated map" to prevent duplication in map
        }

        Transform mapHolder = new GameObject(holderName).transform; //create new empty game object called "Generated Map"
        mapHolder.parent = transform; //set "Generated Map" a child of map

        //Spawning tiles
        for (int x = 0; x < currentMap.mapSize.x; x++)
        {
            for (int y = 0; y < currentMap.mapSize.y; y++)
            {
                Vector3 tilePosition = CoordToPosition(x, y); //set new position of tile using "CoordToPosition" function
                Transform newTile = Instantiate(tilePrefab, tilePosition, Quaternion.Euler(Vector3.right * 90)) as Transform; //set newTile equal to newly created square tile
                newTile.localScale = Vector3.one * (1 - outlinePercent) * tileSize; //set outline of the new tile
                newTile.parent = mapHolder; //set new tile child of "Generated Map"
                tileMap[x, y] = newTile; //set tileMap [x,y] according to new tile's X and y coordinate
            }
        }

        //Spawning obsticles
        bool[,] obstacleMap = new bool[(int)currentMap.mapSize.x, (int)currentMap.mapSize.y]; //create a bool to store map size X and Y

        int obstacleCount = (int)(currentMap.mapSize.x * currentMap.mapSize.y * currentMap.obstaclePercent); //set number of obstacle in a map
        int currentObstacleCount = 0; //number of obstacle already placed in the map
        List<Coord> allOpenCoords = new List<Coord>(allTileCoords);  //create new list of tiles

        
        for (int i = 0; i < obstacleCount; i++) //if there is obstacle left
        {
            Coord randomCoord = GetRandomCoord(); //get coordinate of obstacle using "GetRandomCoord" function
            obstacleMap[randomCoord.x, randomCoord.y] = true; //The coordinate has a obstacle
            currentObstacleCount++; //increase obstacle placed in the map

            if (randomCoord != currentMap.mapCentre && MapIsFullyAccessible(obstacleMap, currentObstacleCount)) //if the tile is not centre tile and map is fully accessable(no blocked area)
            {
                float obstacleHeight = Mathf.Lerp(currentMap.minObstacleHeight, currentMap.maxObstacleHeight, (float)prng.NextDouble()); //randomly set obstacle height between min and max obstacleHeight
                Vector3 obstaclePosition = CoordToPosition(randomCoord.x, randomCoord.y); //get obstacle position using "CoordToPosition" function

                Transform newObstacle = Instantiate(obstaclePrefab, obstaclePosition + Vector3.up * obstacleHeight/2, Quaternion.identity) as Transform; //create the new obstacle
                newObstacle.parent = mapHolder; //place the obstacle as a child of "Generated Map"
                newObstacle.localScale = new Vector3((1-outlinePercent)*tileSize,obstacleHeight,(1-outlinePercent)*tileSize); //decrease the outine/size of the obstacle to match tile's outline

                Renderer obstacleRenderer = newObstacle.GetComponent<Renderer>(); //get obstacle's renderer component
                Material obstacleMaterial = new Material(obstacleRenderer.sharedMaterial); //get obstacle's material component
                float colorPrecent = randomCoord.y / (float)currentMap.mapSize.y; //color percent to change the obstacle color in z-axis (y-axis in map is z-axis in global world)
                obstacleMaterial.color = Color.Lerp(currentMap.foregroundColor, currentMap.backgroundColor, colorPrecent); //set the color of obstacle depending on z-axis of the obstacle
                obstacleRenderer.sharedMaterial = obstacleMaterial; //applay the color to obstacle

                allOpenCoords.Remove(randomCoord); //remove randomCoord to enable to apply new randomCoord in next loop
            }
            else
            {
                obstacleMap[randomCoord.x, randomCoord.y] = false; //set bool of having obstacle in the tile to false
                currentObstacleCount--; //decrease current obstacle placed in the map
            }
        }

        //shuffled tile coordinate with no obstacle(open) using Utility's "ShuffleArray" function, used in determine map is fully accessable
        shuffledOpenTileCoords = new Queue<Coord>(Utility.ShuffleArray(allOpenCoords.ToArray(), currentMap.seed)); 


        //Creating navmesh Mask by covering navmeshMaskPrefab at the edge of top, bottom, left and right of the map
        Transform maskLeft = Instantiate(navmeshMaskPrefab, Vector3.left * (currentMap.mapSize.x + maxcurrentMap.x) / 4f * tileSize, Quaternion.identity) as Transform;
        maskLeft.parent = mapHolder;
        maskLeft.localScale = new Vector3((maxcurrentMap.x - currentMap.mapSize.x) / 2f, 1, currentMap.mapSize.y) * tileSize;

        Transform maskRight = Instantiate(navmeshMaskPrefab, Vector3.right * (currentMap.mapSize.x + maxcurrentMap.x) / 4f * tileSize, Quaternion.identity) as Transform;
        maskRight.parent = mapHolder;
        maskRight.localScale = new Vector3((maxcurrentMap.x - currentMap.mapSize.x) / 2f, 1, currentMap.mapSize.y) * tileSize;

        Transform maskTop = Instantiate(navmeshMaskPrefab, Vector3.forward * (currentMap.mapSize.y + maxcurrentMap.y) / 4f * tileSize, Quaternion.identity) as Transform;
        maskTop.parent = mapHolder;
        maskTop.localScale = new Vector3(maxcurrentMap.x, 1, (maxcurrentMap.y - currentMap.mapSize.y) / 2f) * tileSize;

        Transform maskBottom = Instantiate(navmeshMaskPrefab, Vector3.back * (currentMap.mapSize.y + maxcurrentMap.y) / 4f * tileSize, Quaternion.identity) as Transform;
        maskBottom.parent = mapHolder;
        maskBottom.localScale = new Vector3(maxcurrentMap.x, 1, (maxcurrentMap.y - currentMap.mapSize.y) / 2f) * tileSize;

        //create a navmesh for the map
        navmeshFloor.localScale = new Vector3(maxcurrentMap.x, maxcurrentMap.y) * tileSize;
        //get the backgrounf of the floor to give colider(prevent player from falling down)
        backgroundMapFloor.localScale = new Vector3(currentMap.mapSize.x * tileSize, currentMap.mapSize.y * tileSize);
    }

    //check wheather the map is fully accessable
    bool MapIsFullyAccessible(bool[,] obstacleMap, int currentObstacleCount)
    {
        bool[,] mapFlags = new bool[obstacleMap.GetLength(0), obstacleMap.GetLength(1)];
        Queue<Coord> queue = new Queue<Coord>(); //create a new list that can store coordinate
        queue.Enqueue(currentMap.mapCentre); //enqueue the centre point
        mapFlags[currentMap.mapCentre.x, currentMap.mapCentre.y] = true; //set the bool of mapFLags to true

        int accessibleTileCount = 1; //accessable tile which is the centre tile is one

        while (queue.Count > 0) //if there is a item in the queue
        {
            Coord tile = queue.Dequeue(); //dequeue the item and save in Coord tile

            /* an algorithm that determines the area connected to a given node in a multi-dimensional array.It is used in the "bucket" fill tool of paint programs to fill connected*/
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    int neighbourX = tile.x + x;
                    int neighbourY = tile.y + y;
                    if (x == 0 || y == 0)
                    {
                        if (neighbourX >= 0 && neighbourX < obstacleMap.GetLength(0) && neighbourY >= 0 && neighbourY < obstacleMap.GetLength(1))
                        {
                            if (!mapFlags[neighbourX, neighbourY] && !obstacleMap[neighbourX, neighbourY]) //check if there have not obstacle in the tile
                            {
                                mapFlags[neighbourX, neighbourY] = true; 
                                queue.Enqueue(new Coord(neighbourX, neighbourY)); //enqueue the new tile coord to the queue
                                accessibleTileCount++; //increase number of accessible tile
                            }
                        }
                    }
                }
            }
        }

        int targetAccessibleTileCount = (int)(currentMap.mapSize.x * currentMap.mapSize.y - currentObstacleCount); //set number of accessible tile a map should have if it is fully accessible
        return targetAccessibleTileCount == accessibleTileCount; //return true if is fully accessible
    }

    //return the coordinate of each tile depending row(x) and column(y) of the time
    Vector3 CoordToPosition(int x, int y)
    {
        return new Vector3(-currentMap.mapSize.x / 2f + 0.5f + x, 0, -currentMap.mapSize.y / 2f + 0.5f + y)* tileSize;
    }

    //Get position of tile player standing depending on player's position
    public Transform GetTileFromPosition (Vector3 position)
    {
        int x = Mathf.RoundToInt(position.x / tileSize + (currentMap.mapSize.x - 1) / 2f);
        int y = Mathf.RoundToInt(position.z / tileSize + (currentMap.mapSize.y - 1) / 2f);
        x = Mathf.Clamp(x, 0, tileMap.GetLength(0)-1);
        y = Mathf.Clamp(y, 0, tileMap.GetLength(1));

        return tileMap[x, y];
    }

    //Dequeue from shuffled tile, save to a variable and enqueue it back, then return the variable
    public Coord GetRandomCoord()
    {
        Coord randomCoord = shuffledTileCoords.Dequeue();
        shuffledTileCoords.Enqueue(randomCoord);
        return randomCoord;
    }

    //Dequeue from shuffled tile, save to a variable and enqueue it back, then return the variable
    public Transform GetRandomOpenTile()
    {
        Coord randomCoord = shuffledOpenTileCoords.Dequeue();
        shuffledOpenTileCoords.Enqueue(randomCoord);
        return tileMap[randomCoord.x,randomCoord.y];
    }

    [System.Serializable]
    public struct Coord
    {
        public int x;
        public int y;

        public Coord(int _x, int _y)
        {
            x = _x;
            y = _y;
        }

        public static bool operator ==(Coord c1, Coord c2)
        {
            return c1.x == c2.x && c1.y == c2.y;
        }

        public static bool operator !=(Coord c1, Coord c2)
        {
            return !(c1 == c2);
        }
    }

    [System.Serializable]
    public class Map
    {
        public Coord mapSize; //map size of x and y
        [Range(0,1)]//slider
        public float obstaclePercent; //number of obstacle
        public int seed; //seed of map
        public float minObstacleHeight; //min height of pbstacle
        public float maxObstacleHeight; //max height of obstacle
        public Color foregroundColor; //front obstacle color
        public Color backgroundColor; //back obstacle color
        public Coord mapCentre //centre of map
        {
            get
            {
                return new Coord(mapSize.x / 2, mapSize.y / 2);
            }
        }
    }
}