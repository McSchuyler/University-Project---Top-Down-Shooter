using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]

public class PlayerController : MonoBehaviour
{
    public float speed = 5;
    public float dashSpeed = 200;
    public float x;
    public float y;
    public float x_dash;
    public float y_dash;
    public bool dashing;
    
    public void MoveX(float _x) //get direction of x
    {
        x = _x;
    }

    public void MoveY(float _y) //get direction of y
    {
        y = _y;
    }

    public void DashX(float _x) //get direction of x dashing
    {
        x_dash = _x;
    }

    public void DashY(float _y) //get direction of y dashing
    {
        y_dash = _y;
    }

    public void LookAt(Vector3 lookPoint) //rotate to face the pointer of mouse
    {
        Vector3 heightCorrectedPoint = new Vector3(lookPoint.x, transform.position.y, lookPoint.z);
        transform.LookAt(heightCorrectedPoint);
    }

    public void Walk() //walking
    {
        if (x <= 0)
            transform.position += Vector3.left * speed * Time.deltaTime;

        if (x >= 0)
            transform.position += Vector3.right * speed * Time.deltaTime;

        if (y <= 0)
            transform.position += Vector3.back * speed * Time.deltaTime;

        if (y >= 0)
            transform.position += Vector3.forward * speed * Time.deltaTime;
    }

    public void Dash() //dashing
    {
        if (x_dash <= 0)
            transform.position += Vector3.left * dashSpeed * Time.deltaTime;

        if (x_dash >= 0)
            transform.position += Vector3.right * dashSpeed * Time.deltaTime;

        if (y_dash <= 0)
            transform.position += Vector3.back * dashSpeed * Time.deltaTime;

        if (y_dash >= 0)
            transform.position += Vector3.forward * dashSpeed * Time.deltaTime;
    }

    public void Dashing(bool _dashing) //get condition of dashing
    {
        dashing = _dashing;
    }

    void FixedUpdate()
    {
        if(dashing == false)    // if is dashing, dont walk
        {
            Walk();
        }
        Dash();
    }
}