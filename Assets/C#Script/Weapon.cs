using UnityEngine;
using System.Collections;

public class Weapon : MonoBehaviour {

    public enum FireMode { Auto,Burst,Single}; //fire mode
    public FireMode fireMode; //type of fire mode

    public int burstCount; //bullet hot in a burst
    public Transform[] projectileSpawn; //muzzle to shot bullet
    public Projectile projectile; //bullet game object
    public float msBetweenShots = 100; //delay between each shot
    public float muzzleVelocity = 35; //bullet speed
    public int projectilePerMag; //number of bullet avaliable before reload
    public float reloadTime = 0.3f; //reload time

    [Header("Recoil")]
    public Vector2 kickMinMax = new Vector2(0.2f, 0.75f); //recoil animation transform
    public Vector2 recoilAngleMinMax = new Vector2(3, 5); //recoil animation torque
    public float recoilMoveSettleTime =0.1f; //recoil time transform
    public float recoilRotationSettleTime =0.1f; //recoil time torque

    [Header("Effects")]
    public Transform shell; //shell game object
    public Transform shellEjection; //position of shell eject
    public AudioClip shootAudio; //audio for shooting
    public AudioClip reloadAudio; //audio for reloading

    MuzzleFlash muzzleflash; //reference to "MuzzleFlash" script

    float nextShotTime; //next bullet shot cooldown

    bool triggerReleaseSinceLastShot; //is trigger released since last shot
    int shotsRemainingInBurst; //remaining no of bullet able to burst
    int projectileRemainingInMag; //remaining bullet before reload

    Vector3 recoilSmoothDampVelocity; //velocity of transform recoil
    float recoilRotSmoothDampVelocity; //velocity of torque recoil
    float recoilAngle; //recoil angle

    bool isReloading; //is player reloading

    void Start()
    {
        muzzleflash = GetComponent<MuzzleFlash>(); //get reference to "MuzzleFlash" Script
        shotsRemainingInBurst = burstCount; //set bullet able to burst
        projectileRemainingInMag = projectilePerMag; //set max bullet before reload
    }

    //use LateUpdate to prevent overide of this function byy other function
    void LateUpdate()
    {
        //animate recoil
        transform.localPosition = Vector3.SmoothDamp(transform.localPosition, Vector3.zero, ref recoilSmoothDampVelocity, recoilMoveSettleTime); //recoil animation in transform
        recoilAngle = Mathf.SmoothDamp(recoilAngle, 0, ref recoilRotSmoothDampVelocity, recoilRotationSettleTime); //find recoil angle
        transform.localEulerAngles = transform.localEulerAngles + Vector3.left * recoilAngle; //recoil animation in torque

        if(!isReloading && projectileRemainingInMag == 0) //if is not reloading and there is no bullet avaliable
        {
            Reload(); //run "Reload" function
        }
    }

    //shoot bullet
    void Shoot()
    {
        if (!isReloading && Time.time > nextShotTime && projectileRemainingInMag >0) //if is not reloading, cooldown of next shot is ready, and have bullet to shoot
        {
            if(fireMode == FireMode.Burst) //if is burst fire mode
            {
                if(shotsRemainingInBurst == 0) //if there is not enough bullet avaliable to sohot
                {
                    return; //jump out
                }
                shotsRemainingInBurst--; //decrease bullet remaining to burst
            }
            else if (fireMode == FireMode.Single) //if is single fire mode
            {
                if (!triggerReleaseSinceLastShot) //if is holding the trigger after bullet shoot
                {
                    return; //jump out
                }
            }

            //Mag
            for(int i=0; i<projectileSpawn.Length; i++) //for each projectile spawner
            {
                if(projectileRemainingInMag == 0) //if there is no bullet to shoot
                {
                    break; //jump out from loop
                }
                projectileRemainingInMag--; //decrese bullet remaining in to shoot
                nextShotTime = Time.time + msBetweenShots / 1000; //set cooldown for next shot
                Projectile newProjectile = Instantiate(projectile, projectileSpawn[i].position, projectileSpawn[i].rotation) as Projectile; //create bullet
                newProjectile.SetSpeed(muzzleVelocity); //set speed of bullet using projectile's "SetSpeed" function
            }
            //shell animation
            Instantiate(shell, shellEjection.position, shellEjection.rotation); //create shell
            muzzleflash.Activate(); //run MuzzleFlash's "Activate" function
            transform.localPosition -= Vector3.forward * Random.Range(kickMinMax.x,kickMinMax.y); //launch the shell
            recoilAngle += Random.Range(recoilAngleMinMax.x,recoilAngleMinMax.y); //random set recoil angle
            recoilAngle = Mathf.Clamp(recoilAngle, 0, 30); //clamp the recoil angle from 0 to 30

            AudioManager.instance.PlaySound(shootAudio, transform.position); // play shooting sound slip using audio manager script
        }
    }

    //reloading
    public void Reload()
    {
        if(!isReloading && projectileRemainingInMag != projectilePerMag) //if is not reloading and is not full mag
        {
            StartCoroutine(AnimateReload()); //run "AnimateReload" function
            AudioManager.instance.PlaySound(reloadAudio, transform.position); //play reloading sound slip using audio manager script
        }
    }

    //reload animation
    IEnumerator AnimateReload()
    {
        isReloading = true; //in reloading animation
        yield return new WaitForSeconds(0.2f); //wait for 0.2 second

        float reloadSpeed = 1/reloadTime; //set reload speed
        float percent = 0; //set reload animation percent
        Vector3 initialRot = transform.localEulerAngles; //set initial position rotation for reload animation
        float maxReloadAngle = 30; //set max reload angle

        while(percent < 1) //if in reload animation
        {
            percent += Time.deltaTime * reloadSpeed; //increase reload animation percent over time
            float interpolation = (-Mathf.Pow(percent, 2) + percent) * 4; //flactuation for percent between 0 and 1
            float reloadAngle = Mathf.Lerp(0, maxReloadAngle, interpolation); //Lerp the angle for reload animation
            transform.localEulerAngles = initialRot + Vector3.left * reloadAngle; //animate reload animation

            yield return null;
        }

        isReloading = false; //finished reloading
        projectileRemainingInMag = projectilePerMag; //get full mag
    }

    //weapon aim at cursor accurately
    public void Aim (Vector3 aimPoint)
    {
        if (!isReloading) //if is not reloading(to not disturb animation of reload)
        {
            transform.LookAt(aimPoint); //weapon will look at the cursor point
        }
    }

    //holding mouse button 1
    public void OnTriggerHold()
    {
        Shoot();//run "Shoot" function
        triggerReleaseSinceLastShot = false; //is not holding the trigger after bullet shoot
    }
    
    //release mouse button 1
    public void OnTriggerRelease()
    {
        triggerReleaseSinceLastShot = true;  //is holding the trigger after bullet shoot
        shotsRemainingInBurst = burstCount; //reset full number of bullet avaliable to burst
    }
}