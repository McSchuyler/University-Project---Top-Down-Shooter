using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour {

    public LayerMask collisionMask; //layer that of effect the bullet
    public Color trailColor; //color of the bulet trail
    float damage = 1; //damage of bullet
    float speed = 10; //speed of bullet

    float lifeTime = 3; //bullet decay time
    float skinWidth = .1f; //width of bullet

    void Start()
    {
        Destroy(gameObject, lifeTime); //destory bullet after lifeTime(s)

        Collider[] initialCollisions = Physics.OverlapSphere(transform.position, .1f, collisionMask); //create a collider array that return array with collider touching or inside the sphere
        if(initialCollisions.Length > 0)
        {
            OnHitObject(initialCollisions[0],transform.position); //run "OnHitObject" function
        }

        GetComponent<TrailRenderer>().material.SetColor("_TintColor", trailColor); //get component of the trail renderer's material and set the color
    }

    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed; //set bullet speed
    }

	void Update () {
        float moveDistance = speed * Time.deltaTime; //distance of the bullet should move over time
        CheckCollisions(moveDistance); //run "CheckCollision" function
        transform.Translate(Vector3.forward * moveDistance); //move bullet
	}

    //check if bullet collide
    void CheckCollisions (float moveDistance)
    {
        Ray ray = new Ray(transform.position, transform.forward); //cast a ray from bullet's position to in front of it
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, moveDistance + skinWidth, collisionMask, QueryTriggerInteraction.Collide)) //if the bullet hit an object
        {
            OnHitObject(hit.collider, hit.point); //run "OnHitObject" function
        }

    }

    void OnHitObject(Collider c, Vector3 hitPoint)
    {
        iDamageable damageableObject = c.GetComponent<iDamageable>(); //set reference to "iDamageable" script
        if (damageableObject != null) //if is damageable object
        {
            damageableObject.TakeHit(damage,hitPoint,transform.forward); //run iDamageable's "TakeHit" function
        }
        GameObject.Destroy(gameObject); //destory bullet
    }
}
