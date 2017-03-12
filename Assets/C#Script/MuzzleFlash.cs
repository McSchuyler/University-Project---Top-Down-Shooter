using UnityEngine;
using System.Collections;

public class MuzzleFlash : MonoBehaviour {

	public GameObject flashHolder; //gameobject containing the muzzle flash sprite
	public Sprite[] flashSprites; //array of muzzle flash sprite
	public SpriteRenderer[] spriteRenderers; //position of sprite

	public float flashTime; //time the flash flashes

	void Start() {
		Deactivate (); //run "Deactivate" function
	}

	public void Activate() {
		flashHolder.SetActive (true); //set game object "flash holder" to active
		int flashSpriteIndex = Random.Range (0, flashSprites.Length); //randomize the sprite to show
		for (int i =0; i < spriteRenderers.Length; i ++) //change each sprite renderer's sprite
        {
			spriteRenderers[i].sprite = flashSprites[flashSpriteIndex]; //to the randomzied sprite to show
		}

		Invoke ("Deactivate", flashTime); //wait for flash time, the run "Deacvitate" function
	}

	void Deactivate() {
		flashHolder.SetActive (false); //set game object "flash holder" to disactive
	}
}
