using UnityEngine;
using System.Collections;

public class Shell : MonoBehaviour
{

    public Rigidbody myRigidbody;
    public float forceMin; //min force for shell to pop out
    public float forceMax; //max force for shell to pop out

    float lifeTime = 4; //time for shell to show
    float fadeTime = 2; //time for shell to fade

    // Use this for initialization
    void Start()
    {
        float force = Random.Range(forceMin, forceMax); //random force between min force and max force
        myRigidbody.AddForce(transform.right * force); //add force to the shell
        myRigidbody.AddTorque(Random.insideUnitSphere * force); //random the torque the shell will pop out

        StartCoroutine(Fade()); //run "Fade" function
    }

    //fades the shell
    IEnumerator Fade()
    {
        yield return new WaitForSeconds(lifeTime); //wait for "lifeTime" seconds

        float percent = 0; //fade animation percent
        float fadeSpeed = 1 / fadeTime; //fade speed
        Material mat = GetComponent<Renderer>().material; //get reference to the shell material
        Color initialColor = mat.color; //set initial color

        while(percent < 1) //if is in fade animation
        {
            percent += Time.deltaTime * fadeSpeed; //increase fade animation precent over time
            mat.color = Color.Lerp(initialColor, Color.clear, percent); //change color to clear(invisible) over time
            yield return null;
        }
    }
}