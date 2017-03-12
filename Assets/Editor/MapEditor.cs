using UnityEngine;
using System.Collections;
using UnityEditor;

//No Gameplay purpose, just for editor

//To allow real time update the map without running the game
[CustomEditor(typeof(MapGenerator))]
public class MapEditor : Editor {

    public override void OnInspectorGUI()
    {
        MapGenerator map = target as MapGenerator; //targeted script (MapGenerator.cs)

        if (DrawDefaultInspector()){ //if there is something change in map editor
            map.GenerateMap(); //update the map
        }

        if (GUILayout.Button("Generate Map")) //crete a button in map editor 
        {
            map.GenerateMap(); //when button is pressed, update the map
        }
    }
}
