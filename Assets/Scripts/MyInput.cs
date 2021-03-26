using System;
using Cinemachine;
using UnityEngine;

interface IMyInput
{
    float X();
    float Y();
    float XRaw();
    float YRaw();
    
    bool GetButton(string btn);
    bool GetButtonUp(string btn);
    bool GetButtonDown(string btn);
}

public class MyInput : MonoBehaviour, IMyInput
{
    public CinemachineVirtualCamera camera;
    private Movement _movement;
    private CinemachineFramingTransposer temp;
    float x;
    float y;
    float xRaw;
    float yRaw;

    private void Start()
    {
        _movement = GetComponent<Movement>();
        temp = camera.GetCinemachineComponent<CinemachineFramingTransposer>();
    }

    private void Update()
    {
        x = Input.GetAxis("Horizontal");
        y = Input.GetAxis("Vertical");
        xRaw = Input.GetAxisRaw("Horizontal");
        yRaw = Input.GetAxisRaw("Vertical");

        float target = _movement.side == -1 ? 0.75f : 0.25f;
        temp.m_ScreenX = Mathf.Lerp(temp.m_ScreenX, target, Time.deltaTime*5);
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
        return Input.GetButton(btn);
    }
    
    public bool GetButtonUp(string btn)
    {
        return Input.GetButtonUp(btn);
    }
    
    public bool GetButtonDown(string btn)
    {
        return Input.GetButtonDown(btn);
    }

}
