using UnityEngine;
using System.Collections;

public class Crosshair : MonoBehaviour
{

    public LayerMask targetMask; //enemy layer
    public SpriteRenderer dot; //cursor's dot
    public Color dotHighlightColour; //custom color for cursor's dot
    Color originalDotColour; //original color for cursor's dot

    void Start()
    {
        Cursor.visible = false; //set normal cursor to not visible
        originalDotColour = dot.color; //get the original color of the custom cursor's dot
    }

    void Update()
    {
        transform.Rotate(Vector3.forward * -60 * Time.deltaTime); //give custom cursor a rotating animation
    }

    public void DetectTargets(Ray ray)
    {
        if (Physics.Raycast(ray, 100, targetMask)) //if the cursor's racast hit a target mask, where target mask is enemy layer
        {
            dot.color = dotHighlightColour; // change custom cursor's dot to custom color
        }
        else
        {
            dot.color = originalDotColour; //change back to original color of the custom cursor's dot
        }
    }
}