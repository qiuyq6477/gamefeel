using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{

    public static InputManager Instance;
    CharacterController _characterController;
    [Header("控制是否使用自定义按键")]
    public bool keyIsSet;
    [Header("左移动")]
    public KeyCode LeftMoveKey;
    [Header("右移动")]
    public KeyCode RightMoveKey;
    [Header("开火按键")]
    public KeyCode Fire;
    [Header("跳跃按键")]
    public KeyCode Jump;
    [Header("冲刺按键")]
    public KeyCode Dash;
    [Header("爬墙按键")]
    public KeyCode Climb;
    
    //=============================================
    [HideInInspector]
    public bool FireKey { get {return Input.GetKey(Fire); } }
    [HideInInspector]
    public bool FireKeyDown { get { return Input.GetKeyDown(Fire); } }
    [HideInInspector]
    public bool FireKeyUp { get { return Input.GetKeyUp(Fire); } }
    //=============================================
    [HideInInspector]
    public bool ClimbKey { get {return Input.GetKey(Climb); } }
    [HideInInspector]
    public bool ClimbKeyDown { get { return Input.GetKeyDown(Climb); } }
    [HideInInspector]
    public bool ClimbKeyUp { get { return Input.GetKeyUp(Climb); } }
    //=============================================
    [HideInInspector]
    public bool JumpKey { get { return Input.GetKey(Jump); } }
    [HideInInspector]
    public bool JumpKeyDown {
        get
        {
            if(Input.GetKeyDown(Jump))
            {
				return true;
            }
            else if(JumpFrame > 0)
            {
				return true;
            }
            return false;
        }
    }
    [HideInInspector]
    public bool JumpKeyUp { get { return Input.GetKeyUp(Jump); } }
    //=============================================
    [HideInInspector]
    public bool DashKey { get { return Input.GetKey(Dash); } }
    [HideInInspector]
    public bool DashKeyDown { get { return Input.GetKeyDown(Dash); } }
    [HideInInspector]
    public bool DashKeyUp { get { return Input.GetKeyUp(Dash); } }
    //=============================================
    [HideInInspector]
    public float v = 0;
    public float h = 0;
    public AnimationCurve MoveStartCurve;
    public AnimationCurve MoveEndCurve;
    [SerializeField]
    float MoveStartTime;
    [SerializeField]
    float MoveEndTime;
    [SerializeField]
    public int MoveDir;

    int JumpFrame;
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        _characterController = GetComponent<CharacterController>();
        KeyInit();
    }
    public void KeyInit()
    {
        if (!keyIsSet)
        {
	        Fire = KeyCode.J;
            Jump = KeyCode.W;
            Dash = KeyCode.K;
            Climb = KeyCode.L;
            LeftMoveKey = KeyCode.A;
            RightMoveKey = KeyCode.D;
        }
    }

    private void FixedUpdate()
    {
        if(JumpFrame >= 0)
        {
            JumpFrame--;
        }
    }

    private void Update()
    {
        CheckHorzontalMove();
        v = Input.GetAxisRaw("Vertical");
		h = Input.GetAxisRaw("Horizontal");
		if (Input.GetKeyDown(Jump))
        {
            JumpFrame = 3;       //在落地前3帧按起跳仍然能跳
        }
    }

    void CheckHorzontalMove()
    {
		if (Input.GetKeyDown(RightMoveKey) && h<= 0)
		{
				MoveDir = 1;
		}
		else if (Input.GetKeyDown(LeftMoveKey) && h >= 0)
		{
		
				MoveDir = -1;
		}
		else if (Input.GetKeyUp(RightMoveKey))
		{
			if (Input.GetKey(LeftMoveKey))  //放开右键的时候仍按着左键
			{
				MoveDir = -1;
				MoveStartTime = Time.time;
			}
			else
			{
				MoveDir = 0;
			}
		}
		else if (Input.GetKeyUp(LeftMoveKey))
		{
			if (Input.GetKey(RightMoveKey))
			{
				MoveDir = 1;
			}
			else
			{
				MoveDir = 0;
			}
		}
	}

}
