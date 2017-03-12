using UnityEngine;
using System.Collections;
using System.Collections.Generic; //use dictionary

public class SoundLibrary : MonoBehaviour
{

    public SoundGroup[] soundGroups; //sound group library

    //create a dictionary which pass in string and pass out audio clip
    Dictionary<string, AudioClip[]> groupDictionary = new Dictionary<string, AudioClip[]>();

    void Awake()
    {
        foreach (SoundGroup soundGroup in soundGroups) //for everything in sound group
        {
            groupDictionary.Add(soundGroup.groupID, soundGroup.group); //assign to dictionary
        }
    }

    public AudioClip GetClipFromName(string name) //get sound clip from the string
    {
        if (groupDictionary.ContainsKey(name)) //if the string contains a keyname in dictionary
        {
            AudioClip[] sounds = groupDictionary[name]; //create a virable to pass the group
            return sounds[Random.Range(0, sounds.Length)]; //select a clip from the group to return
        }
        return null; //return null if find nothing in dictionary
    }

    [System.Serializable]
    public class SoundGroup
    {
        public string groupID; //sound group anme
        public AudioClip[] group; //to contain all sound effect in the group
    }
}