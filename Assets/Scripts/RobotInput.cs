using System;
using System.Collections;
using UnityEngine;

public class RobotInput : MonoBehaviour, IMyInput
{
    public float x;
    public float y;
    public float xRaw;
    public float yRaw;
    private Collision coll;
    private void Start()
    {
        coll = GetComponent<Collision>();
        
    }

    private void Update()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, new Vector2(xRaw, 0), 0.5f, LayerMask.GetMask("Ground"));
        if (hit.collider != null)
        {
            x = -x;
            xRaw = -xRaw;
        }
    }
    public float X()
    {
        return x;
    }

    public float Y()
    {
        return y;
    }

    public float XRaw()
    {
        return xRaw;
    }

    public float YRaw()
    {
        return yRaw;
    }
    public bool GetButton(string btn)
    {
        return false;
    }
    
    public bool GetButtonUp(string btn)
    {
        return false;
    }
    
    public bool GetButtonDown(string btn)
    {
        return false;
    }

}