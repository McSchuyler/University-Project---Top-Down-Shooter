using UnityEngine;
using System.Collections;

[RequireComponent (typeof(NavMeshAgent))]
public class Enemy : LivingEntity {

    public enum State { Idle,Chasing,Attacking}; //type of state of enemy
    State currentState; //state of enemy

    public ParticleSystem deathEffect; //death effect for enemy
    public static event System.Action OnDeathStatic; //create a static event

    NavMeshAgent pathfinder; //pathfinder mesh
    Transform target; //enemy target
    LivingEntity targetEntity; //reference to Living Entity script
    Material skinMaterial; //material of enemy

    Color originalColor; //color of enemy

    float attackDistanceThreshold = .5f; // maximum attack distance
    float timeBetweenAttacks = 1; //attack cooldown
    float damage = 1; //damage

    float nextAttackTime; //countdown time to next attack
    float myCollisionRadius; //enenmy radious
    float targetCollisionRadius; //player radius

    bool hasTarget; //has target to attack

    void Awake()
    {
        pathfinder = GetComponent<NavMeshAgent>(); //find component of NavMeshAgent of enemy
        
        if (GameObject.FindGameObjectWithTag("Player") != null) //if enemy finds a player
        {
            hasTarget = true; //enemy has a target to attack

            target = GameObject.FindGameObjectWithTag("Player").transform; // set target position
            targetEntity = target.GetComponent<LivingEntity>(); //get target's "LivingEntity" script
     
            myCollisionRadius = GetComponent<CapsuleCollider>().radius; //get enemy radius
            targetCollisionRadius = target.GetComponent<CapsuleCollider>().radius; //get target radius
        }
    }

    //override start in "LivingEntity" script
	protected override void Start () {
        base.Start();  //execute start in "LivingEntity" Script
      
        if(hasTarget) //if enemy finds a player
        {
            currentState = State.Chasing; //enemy is chasing

            targetEntity.OnDeath += OnTargetDeath; //run "OnTargetDeath" function when LivingEntity's "OnDeath" function executed

            StartCoroutine(UpdatePath()); //run "UpdatePath" function
        }
	}

    //set characteristic of enemy differently in each wave
    public void SetCharacteristic(float moveSpeed, int hitsToKillPlayer, float enemyHealth, Color skinColor)
    {
        pathfinder.speed = moveSpeed; //enemy moving speed
        if (hasTarget) //if enemy has target
        {
            damage = Mathf.Ceil(targetEntity.startingHealth / hitsToKillPlayer); //set maximum hit to kill player
        }
        startingHealth = enemyHealth; //set starting health
        skinMaterial = GetComponent<Renderer>().material; //get enemy skin material
        skinMaterial.color = skinColor; //set color for enemy
        originalColor = skinMaterial.color; //save original color for enemy
    }

    //if player dies
    void OnTargetDeath() 
    {
        hasTarget = false; //if enemy has no target
        currentState = State.Idle; //enemy do nothing
    }

    //particle effect when enemy death
    public override void TakeHit(float damage, Vector3 hitPoint, Vector3 HitDirection)
    {
        AudioManager.instance.PlaySound("Impact", transform.position); //play sound when enemy take hit using SoundLibrary and AudioManager
        if(damage >= health && !dead) //if next hit will result in death
        {
            if(OnDeathStatic != null) //if there is a static event call OnDeathStatic
            {
                OnDeathStatic(); //call the event
            }
            AudioManager.instance.PlaySound("Enemy Death", transform.position); //play sound when die using SoundLibrary and AudioManager
            //create death particle effect, wait for "startLifetime" second then destory particle effect 
            Destroy( Instantiate(deathEffect.gameObject, hitPoint, Quaternion.FromToRotation(Vector3.forward, HitDirection)) as GameObject,deathEffect.startLifetime);
        }
        base.TakeHit(damage, hitPoint, HitDirection); //call "TakeHit" function in "LivingEntity" script
    }

    void Update () {
        if (target) //if there is a target or player
        {
            if (Time.time > nextAttackTime) //attack cooldown is off(enemy can attack)
            {
                float sqrDstToTarget = (target.position - transform.position).sqrMagnitude; //find sqr distance from enemy to target (sqr to reduce computer memory as sqr root distance will take lots of memory) 

                if (sqrDstToTarget < Mathf.Pow(attackDistanceThreshold + myCollisionRadius + targetCollisionRadius, 2)) //if enemy is in attack range
                {
                    nextAttackTime = Time.time + timeBetweenAttacks; //reset attack cooldown
                    AudioManager.instance.PlaySound("Enemy Attack", transform.position); //play sound when enemy attack using SoundLibrary and AudioManager
                    StartCoroutine(Attack()); //run "Attack" function
                }
            }
        }  
	}
    IEnumerator Attack()
    {
        currentState = State.Attacking; //enemy is attacking
        pathfinder.enabled = false; //disable enemy find target or player
        Vector3 originalPosition = transform.position; //set original position of enemy
        Vector3 dirToTarget = (target.position - transform.position).normalized; //direction to attack the target
        Vector3 attackPosition = target.position - dirToTarget * (myCollisionRadius + attackDistanceThreshold / 2); //set final attack position

        float attackSpeed = 3; //higher the value faster the enemy attack
        float percent = 0; //percentage of attack animation

        skinMaterial.color = Color.white; //when attacking enemy change to white
        bool hasAppliedDamage = false; //set bool to determine if enemy has attack or not

        while(percent <=1) //if in attack animation
        {
            if(percent >= .5f && !hasAppliedDamage) //if in half of attack animation and haven attack the player
            {
                hasAppliedDamage = true; //set enemy already attack player
                targetEntity.TakeDamage(damage); //run "Take damage" function in "LivingEntity" script
            }
            percent += Time.deltaTime * attackSpeed; //increase the animation percent over time
            float interpolation = (-Mathf.Pow(percent, 2) + percent) * 4; //flactuation for percent between 0 and 1
            transform.position = Vector3.Lerp(originalPosition, attackPosition, interpolation); //move to attack position then back to original position

            yield return null;
        }

        skinMaterial.color = originalColor; //set back enemy color
        currentState = State.Chasing; //change state to chasing
        pathfinder.enabled = true; //enable pathfinding
    }

    IEnumerator UpdatePath()
    {
        float refreshRate = 0.5f; //enemy will update player's position in 0.5 second

        while(hasTarget) //while enemy has a target to attack
        {
            if(currentState == State.Chasing) //if enemy is chasing
            {
                Vector3 dirToTarget = (target.position - transform.position).normalized; //set direction from enemy position to target position
                Vector3 targetPosition = target.position - dirToTarget * (myCollisionRadius + targetCollisionRadius + attackDistanceThreshold / 2); //set real target position (position to attack)
                if (!dead) //if enemy not dead
                {
                    pathfinder.SetDestination(targetPosition); //set enemy's destination
                }
            }
            yield return new WaitForSeconds(refreshRate); //to find position of player every refreshRate(S)
        }
    }
}
