using UnityEngine;
using System.Collections;

public class LivingEntity : MonoBehaviour,iDamageable {

    public float startingHealth; //starting health
    public float health { get; protected set; }//current health
    protected bool dead; //is dead or not

    public event System.Action OnDeath; //event call OnDeath

    //for player and enemy

    protected virtual void Start()
    {
        health = startingHealth; //set max health
    }

    public virtual void TakeHit(float damage, Vector3 hitPoint, Vector3 HitDirection)
    {
        TakeDamage(damage);  //run "TakeDamage" function
    }

    public virtual void TakeDamage(float damage)
    {
        health -= damage; //decrease helth according to damage

        if (health <= 0 && !dead) //if health less then 0 and no death
        {
            Die(); //run "Die" function
        }
    }

    public virtual void Die()
    {
        dead = true; //is dead 
        if(OnDeath != null) //if there is event "OnDeath"
        {
            OnDeath(); //run "OnDeath" funtion according to type(player or enemy)
        }
        GameObject.Destroy(gameObject); //destory dead game object
    }

}
