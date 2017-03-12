using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(WeaponController))]

public class Player : LivingEntity {
    public float v_ButtonCooldown = 0.25f; //horizontal dash cooldown
    public float h_ButtonCooldown = 0.25f; //vertical dash cooldown

    int v_ButtonCount = 0; //times button pressed vertical (up down)
    int h_ButtonCount = 0; //times button pressed horizontal(left right)
    float mainCooldown = 1.0f; //time to re-dash
    float timeStamp;
    float x_dash;
    float y_dash;
    bool dashing;

    string sceneName; //for current scene  name (menu or game)

    public Crosshair crosshair; //custom cursor containing "Crosshair" script

    Camera viewCamera; //creference to camera
    PlayerController controller; //reference to PlayerController script
    WeaponController weaponController; //reference to WeaponController script

	protected override void Start ()
    {
        OnLevelWasLoaded(0); //run function if unity does not auto run the function
        base.Start(); //run LivingEntity's "start" function
    }

    void Awake()
    {
        controller = GetComponent<PlayerController>(); //get reference to PlayerController
        weaponController = GetComponent<WeaponController>(); //get reference to Weapon Controller
        viewCamera = Camera.main; //get reference of main camera
        FindObjectOfType<Spawner>().onNewWave += OnNewWave; //When object with "spawner" script executed "onNewWave" function, run "OnNewWave" function
    }

    //set health and weapon type to equip
    void OnNewWave(int waveNumber)
    {
        health = startingHealth; //health
        weaponController.EquipWeaponType(waveNumber-1); //weapon type
    }

    //check horizontal input for dash
    void checkHorizontal()
    {
        if (Input.GetButtonDown("Horizontal") && Time.time > timeStamp) //check button and cooldown of dash
        {
            if (h_ButtonCount == 1) //double tap key
            {
                Debug.Log("dashingX");
                x_dash = Input.GetAxis("Horizontal");   //get direction of dash
                timeStamp = Time.time + mainCooldown;   //reset cooldown of dash
                dashing = true;
            }
            else
            {   //set cooldown and no. of button tapped
                h_ButtonCooldown = 0.25f;
                h_ButtonCount += 1;
                dashing = false;
            }
        }

        if (h_ButtonCooldown > 0)   
        {   
            h_ButtonCooldown -= 1 * Time.deltaTime; //decrease cooldown
            controller.DashX(x_dash);   //pass direction to playerController.Script
        }

        else if (h_ButtonCooldown < 0)
        {   //reset everything
            h_ButtonCount = 0;
            x_dash = 0;
            dashing = false;
            controller.DashX(x_dash);
        }

        if (Input.GetButtonDown("Vertical"))    //prevent dashing if tap other key
        {   //reset everything 
            h_ButtonCooldown = 0.25f;
            h_ButtonCount = 0;
            x_dash = 0;
            dashing = false;
            controller.DashX(x_dash);
        }
    }

    //check vertical input for dash
    void checkVertical()
    {
        if (Input.GetButtonDown("Vertical") && Time.time > timeStamp) //check button and cooldown of dash
        {
            
            if (v_ButtonCount == 1) //double tap key
            {
                Debug.Log("dashingY");
                y_dash = Input.GetAxis("Vertical");     //get direction of dash
                timeStamp = Time.time + mainCooldown;   //reset cooldown of dash
                dashing = true;
            }
            else
            {   //set cooldown and no. of button tapped
                v_ButtonCooldown = 0.5f;
                v_ButtonCount += 1;
                dashing = false;
            }
        }

        if (v_ButtonCooldown > 0)
        {
            v_ButtonCooldown -= 1 * Time.deltaTime;  //decrease cooldown
            controller.DashY(y_dash);   //pass direction to playerController.Script
        }

        else if (v_ButtonCooldown < 0)
        {    //reset everything
            v_ButtonCount = 0;
            y_dash = 0;
            dashing = false;
            controller.DashY(y_dash);
        }

        if (Input.GetButtonDown("Horizontal"))      //prevent dashing if tap other key
        {   //reset everything
            v_ButtonCooldown = 0.25f;
            v_ButtonCount = 0;
            y_dash = 0;
            dashing = false;
            controller.DashY(y_dash);
        }
    }

	void Update () {
        //movement input
        checkHorizontal();
        checkVertical();
        float x = Input.GetAxisRaw("Horizontal"); //get key input for x
        float y = Input.GetAxisRaw("Vertical");    //get key input for y
        controller.MoveX(x);
        controller.MoveY(y);
        controller.Dashing(dashing);

        //Look input
        Ray ray = viewCamera.ScreenPointToRay(Input.mousePosition); //cast a ray from camera to mouse cursor
        Plane groundPlane = new Plane(Vector3.up, Vector3.up*weaponController.WeaponHeight);
        float rayDistance; //distance from ray to cursor

        if (groundPlane.Raycast(ray, out rayDistance)) //pass in the ray and get the rayDistance
        {
            Vector3 point = ray.GetPoint(rayDistance); //get rayDistance and save to point
            Debug.DrawLine(ray.origin, point, Color.red); 
            controller.LookAt(point); //run PlayerController's "LookAt" function
            crosshair.transform.position = point; //set position of cursor to point 
            crosshair.DetectTargets(ray); //run Crosshair's "DetectTarget" function

            //if cursor position is not close to player(to prevent wierd behaviour when weapon aims to near at player)
            if ((new Vector2 (point.x,point.z)-new Vector2(transform.position.x,transform.position.z)).sqrMagnitude > 1)
            {
                weaponController.Aim(point); //aim weapon at the cursor point
            }
           
        }

        //weapon input
        if (Input.GetMouseButton(0))
        {
            weaponController.OnTriggerHold(); //run WeaponController's "OnTriggerHold" function
        }

        if (Input.GetMouseButtonUp(0))
        {
            weaponController.OnTriggerRelease(); //run WeaponController's "OnTriggerRelease" function
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            weaponController.Reload(); //run WeaponController's "Reload" function
        }

        if(transform.position.y <= -10) //if player fall out the map
        {
            TakeDamage(health); //player die
        }

        if (Input.GetKeyDown(KeyCode.Escape) && sceneName == "Game") //if player press escape at game scene
        {
            Cursor.visible = true; //set cursor to visible
            SceneManager.LoadScene("Menu"); //return to menu
        }
    }

    public override void Die() //overide die function in living entity
    {
        AudioManager.instance.PlaySound("Player Death", transform.position); //play sound when player dies
        base.Die(); //continue function in living entity;
    }

    void OnLevelWasLoaded(int sceneIndex) //unity will automatically run it
    {
        string newSceneName = SceneManager.GetActiveScene().name; //get scene name
        if (newSceneName != sceneName) //already change scene
        {
            sceneName = newSceneName; //reset scene name
        }
    }
}
