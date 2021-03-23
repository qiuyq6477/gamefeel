using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

/// <summary>
/// 玩家现在的方向
/// </summary>
public enum PlayDir
{
    Right,
    Left,
}
/// <summary>
/// 状态枚举
/// </summary>
enum PlayState
{
    Normal,
    Jump,
    Climb,
    Dash,
    Fall,
}
public class CharacterController : MonoBehaviour
{
    [Header("当前速度")]
    public Vector3 Velocity;
    [Header("当前状态")]
    [SerializeField]
    PlayState playState;
    [Header("当前方向")]
    [SerializeField]
    PlayDir playDir;
    [Header("玩家是否存活")]
    [SerializeField]
    bool isAlive;
    [Header("碰撞盒大小")]
    [SerializeField]
    Vector2 boxSize;
    [Header("中心点位置")]
    [SerializeField]
    Vector3 CenterPos;
    [Header("子弹类型")]
    [SerializeField]
    string BulletType;
    [Header("枪口")]
    [SerializeField]
    private Transform GunPoint;
    [Header("开火间隔")]
    [SerializeField]
    private float interval = 0.1f;

    [Header("子弹速度")]
    [SerializeField]
    private float bulletSpeed;
    [Header("子弹速度Y的偏移")]
    [SerializeField]
    private float bulletSpeedY = 1f;
    
    Rigidbody2D rig;
    InputManager input;
    public float MoveSpeed;
    public Animator playAnimator;

    bool isIntroJump;              //是否是刚进入跳跃的状态
    bool isCanControl = true;      //是否允许控制
    bool isMove = true;            //是否允许左右移动
    int playerLayerMask;           //玩家层级，射线检测时忽略玩家自身    
    private RaycastHit2D HitEnemyHead;
    RaycastHit2D DownBox;
    RaycastHit2D[] UpBox;
    RaycastHit2D[] RightBox;
    RaycastHit2D[] LeftBox;
    RaycastHit2D[] HorizontalBox;
    [Header("Jump")]
    float startJumpPos;           //开始跳跃时的位置
    public float JumpMax;         //跳跃的最大高度
    public float JumpMin;         //跳跃的最小高度
    public float JumpSpeed;         
    public float ClimbSpeed;
    //爬墙耐力相关
    private float CurStamina;
	private float ClimbMaxStamina = 110;
	private float ClimbUpCost = 100 / 2.2f;
    private float ClimbStillCost = 100 / 10f;
    private float ClimbJumpCost = 110 / 4f;

	Vector2 DashDir;

	int dashCount = 0;
	int CoyotetimeFram = 0;

	float moveH;         //横向位移减速时的速度
    int introDir;        //横向位移减速时的方向
    void Start()
    {
        input = InputManager.Instance;
        rig = GetComponent<Rigidbody2D>();
        playerLayerMask = LayerMask.GetMask("Player");
        playerLayerMask = ~playerLayerMask;             //获得当前玩家层级的mask值，让射线忽略玩家层检测
        playAnimator = GetComponentInChildren<Animator>();
    }

    private void FixedUpdate()
    {
		if(CoyotetimeFram > 0)
		{
			CoyotetimeFram--;
		}
        HorizontalMove();
        rig.MovePosition(transform.position + Velocity * Time.fixedDeltaTime);
    }

    // Update is called once per frame
    void Update()
    {
        RayCastBox();
        playAnimator.SetBool("isGround", isGround);
        playAnimator.SetFloat("speedy", Velocity.y);
        CheckDir();
		if(Velocity.x >= MoveSpeed)
		{
			CheckHorizontalMove();
		}
		if(Velocity.y >6)
		{
			CheckUpMove();
		}
		//碰撞点位debug
		if (HorizontalBox != null && HorizontalBox.Length > 0 && HorizontalBox[0])
        {
            Debug.DrawLine(Position, HorizontalBox[0].point, Color.yellow);
        }
        if (input.DashKeyDown && dashCount > 0)
        {
            Dash();
        }

        if (input.FireKey)
        {
	        Fire();
        }
        switch (playState)
        {
            case PlayState.Normal:
                Normal();
                break;
            case PlayState.Climb:
                Climb();
                break;
            case PlayState.Fall:
                Fall();
                break;
			case PlayState.Dash:
				CheckDashJump();
				break;
        }
    }

    private float _interval = 0;
    private void Fire()
    {
	    _interval -= Time.deltaTime;
	    if (_interval <= 0)
	    {
		    _interval = interval;
		    AudioManager.instance.PlaySound("fire");
		    var go = PoolManager.Spawn(BulletType, GunPoint.transform.position, Quaternion.identity);
		    var bullet = go.GetComponent<Bullet>();
		    bullet.dir = GetDirInt;
		    bullet.moveSpeed = bulletSpeed;
		    bullet.speedOffsetY = bulletSpeedY;
		    PoolManager.Spawn("gunEffect", GunPoint.transform.position, Quaternion.identity, 0.05f);
	    }
    }

    /// <summary>
    /// 横向移动
    /// </summary>
    void HorizontalMove()
    {
        if (isCanMove())
        {
            //减速速阶段
            if ((Velocity.x > 0 && input.MoveDir == -1) || (Velocity.x < 0 && input.MoveDir == 1) || input.MoveDir == 0 ||
				(isGround && input.v < 0) || Mathf.Abs(Velocity.x) > MoveSpeed)
            {
                introDir = Velocity.x > 0 ? 1 : -1;
                moveH = Mathf.Abs(Velocity.x);
				if(isGround)
				{
					moveH -= MoveSpeed / 3;
				}
				else
				{
					moveH -= MoveSpeed / 6;
				}
				if (moveH < 0.01f)
                {
                    moveH = 0;
                }
                Velocity.x = moveH * introDir;
            }
			else
			{
				//蹲下不允许移动
				if (isGround && input.v < 0)
					return;

				if (input.MoveDir == 1 && !(isGround && input.v < 0))
				{
					if (isGround)
					{
						Velocity.x += MoveSpeed / 6;
					}
					else
					{
						Velocity.x += MoveSpeed / 15f;
					}
					if (Velocity.x > MoveSpeed)
						Velocity.x = MoveSpeed;
				}
				else if (input.MoveDir == -1 && !(isGround && input.v < 0))
				{
					if (isGround)
					{
						Velocity.x -= MoveSpeed / 6;
					}
					else
					{
						Velocity.x -= MoveSpeed / 12f;
					}
					if (Velocity.x < -MoveSpeed)
						Velocity.x = -MoveSpeed;
				}
			}
			
            playAnimator.SetFloat("speedx", Math.Abs(Velocity.x));
        }
        else
        {
	        playAnimator.SetFloat("speedx", 0);
        }
    }

    bool isCanClimb()
    {
        return (playState != PlayState.Dash && playState != PlayState.Jump) && BoxCheckCanClimb() && isCanControl && !isIntroJump;
    }

    bool isCanJump()
    {
        return playState == PlayState.Normal || playState == PlayState.Climb && isCanControl;
    }

    bool isCanFall()
    {
        return playState != PlayState.Dash && playState != PlayState.Jump && playState != PlayState.Climb;
    }

    bool isCanMove()
    {
        return playState != PlayState.Dash && playState != PlayState.Climb && isCanControl && isMove;
    }

    /// <summary>
    /// 四个方向上的射线检测
    /// </summary>
    void RayCastBox()
    {
        RightBox = Physics2D.BoxCastAll(Position, boxSize, 0, Vector3.right, 0.1f, LayerMask.GetMask("Ground"));
        LeftBox = Physics2D.BoxCastAll(Position, boxSize, 0, Vector3.left, 0.1f, LayerMask.GetMask("Ground"));
        UpBox = Physics2D.BoxCastAll(Position, boxSize, 0, Vector3.up, 0.05f, LayerMask.GetMask("Ground"));
        DownBox = Physics2D.BoxCast(Position, boxSize, 0, Vector3.down, 0.05f, LayerMask.GetMask("Ground"));
        HitEnemyHead = Physics2D.BoxCast(Position, boxSize, 0, Vector3.down, 0.05f, LayerMask.GetMask("Enemy"));
        if (HitEnemyHead.collider != null && Velocity.y < 0)
        {
	        Jump(new Vector2(Velocity.x, JumpSpeed/2), new Vector2(MoveSpeed, JumpSpeed));
        }
    }

    private void OnDrawGizmos()
    {
	    // Gizmos.color = Color.cyan;
	    // foreach (var box in RightBox)	
	    // {
		   //  Gizmos.DrawWireCube(box.collider.bounds., box.collider.bounds.size);
	    // }
	    // Gizmos.color = Color.green;
	    // foreach (var box in LeftBox)	
	    // {
		   //  Gizmos.DrawWireCube(box.collider.bounds.center, box.collider.bounds.size);
	    // }
	    // Gizmos.color = Color.magenta;
	    // foreach (var box in UpBox)	
	    // {
		   //  Gizmos.DrawWireCube(box.collider.bounds.center, box.collider.bounds.size);
	    // }
	    //
	    // if (DownBox.collider != null)
	    // {
		   //  Gizmos.color = Color.red;
		   //  Gizmos.DrawWireCube(DownBox.collider.bounds.center, DownBox.collider.bounds.size);
	    // }
    }

    /// <summary>
    /// 普通陆地状态
    /// </summary>
    public void Normal()
    {
        if (!isGround)
        {
			CoyotetimeFram = 4;
			playState = PlayState.Fall;
            return;
        }

		Velocity.y = 0;
		CurStamina = ClimbMaxStamina;
		dashCount = 1;
		if (input.JumpKeyDown)
		{
			Jump();
		}
    }

    /// <summary>
    /// 落下状态
    /// </summary>
    public void Fall()
    {
        if (isGround)
        {
            playState = PlayState.Normal;
            return;
        }
		if(CoyotetimeFram > 0 && input.JumpKeyDown)
		{
			CoyotetimeFram = 0;
			Velocity.y = 0;
			Jump();
			return;
		}
		//落下时如果在处在可以爬墙的位置，按下跳跃键即使不爬墙仍能进行小型蹬墙跳
		if (input.JumpKeyDown && BoxCheckCanClimb() && !input.ClimbKey && !CheckIsClimb())
		{
			Velocity.y = 0;
			Velocity.x = 0;
			Jump(new Vector2(4 * -GetClimpDirInt, 0), new Vector2(24 , 0));
			return;
		}
		if (isCanFall())
        {
            Velocity.y -= 150f * Time.deltaTime;
            Velocity.y = Mathf.Clamp(Velocity.y, -25, Velocity.y);
            if (isCanClimb() && (CheckIsClimb() || input.ClimbKey))
            {
                playState = PlayState.Climb;
            }
        }
    }

    #region 爬墙
    /// <summary>
    /// 检测是周围是否有墙壁，既是否可以爬墙。
    /// </summary>
    /// <returns></returns>
    bool BoxCheckCanClimbDash()
    {
		RightBox = Physics2D.BoxCastAll(Position, boxSize, 0, Vector3.right, 0.4f, playerLayerMask);
		LeftBox = Physics2D.BoxCastAll(Position, boxSize, 0, Vector3.left, 0.4f, playerLayerMask);
		if (RightBox.Length > 0)
        {
            HorizontalBox = RightBox;
        }
        else if (LeftBox.Length > 0)
        {
			HorizontalBox = LeftBox;
        }
        return RightBox.Length > 0 || LeftBox.Length > 0;
    }

	bool BoxCheckCanClimb()
	{
		if (RightBox.Length > 0)
		{
			HorizontalBox = RightBox;
		}
		else if (LeftBox.Length > 0)
		{
			HorizontalBox = LeftBox;
		}
		return RightBox.Length > 0 || LeftBox.Length > 0;
	}

	/// <summary>
	/// 蔚蓝中，紧贴着墙壁并且按住朝向墙壁的方向键，会减缓下落速度，这里是检测是否按了朝向墙壁的按键
	/// </summary>
	/// <returns></returns>
	bool CheckIsClimb()
    {
        return (input.MoveDir < 0 && LeftBox.Length > 0) || (input.MoveDir > 0 && RightBox.Length > 0);
    }

    /// <summary>
    /// 攀爬主方法
    /// </summary>
    void Climb()
    {
		var CheckBox = BoxCheckCanClimb();
		if (!input.ClimbKey || !CheckBox)
        {
            if(isGround)
            {
                playState = PlayState.Normal;
                return;
            }
            if (!CheckIsClimb())
            {

                playState = PlayState.Fall;
                return;
            }
        }
        Velocity.x = 0;
		playDir = HorizontalBox == RightBox ? PlayDir.Right : PlayDir.Left;
		//爬墙时，检测是否接近墙的最上端，小于一定距离时自动跳到平台上
		if (isCanClimb())
        {
            if (UpBox.Length == 0)
            {
                if (input.v > 0 && transform.position.y - HorizontalBox[0].point.y > 0.9f)
                {
                    StartCoroutine("ClambAutoJump");
                    return;
                }
            }
            //如果爬在墙的最上端要么自动跳到平台上，要么滑落一段距离
            if (input.v <= 0 && transform.position.y - HorizontalBox[0].point.y > 0.7f || !input.ClimbKey)
            {
                Velocity.y = -ClimbSpeed;
            }
            else if (transform.position.y - HorizontalBox[0].point.y <= 0.7f || input.ClimbKey)
            {
                Velocity.y = input.v * ClimbSpeed;
            }
        }
		//蹬墙跳
		if(input.JumpKeyDown)
		{
			if(input.ClimbKey)
			{
				if((input.h > 0 && GetDirInt < 0) || (input.h < 0 && GetDirInt > 0))
				{
					Jump(new Vector2(8 * -GetDirInt, 0), new Vector2(24 , 0));
				}
				else
				{
					Jump();
				}
			}
			else
			{
				Jump(new Vector2(8 * -GetDirInt, 0), new Vector2(24 , 0));
			}
		}

    }

    /// <summary>
    /// 攀爬到墙壁最上沿时如果有可跳跃平台，则自动跳跃到平台上
    /// </summary>
    IEnumerator ClambAutoJump()
    {
        var posY = Mathf.Ceil(transform.position.y);
        isCanControl = false;
        Velocity = Vector3.zero;
        while (posY + 1f - transform.position.y > 0)
        {
            Velocity.y = JumpSpeed;
            Velocity.x = GetDirInt * 15;
            yield return null;
        }
        Velocity = Vector3.zero;
        playState = PlayState.Fall;
        isCanControl = true;
    }
    #endregion;

    #region 跳跃
    /// <summary>
    /// 记录初始位置和计算最高能跳到的位置，根据按键时间进行跳跃高度判断
    /// </summary>
    void Jump(Vector2 vel, Vector2 maxVel)
    {
		playState = PlayState.Jump;
		startJumpPos = transform.position.y;
		isIntroJump = true;
		if (vel.y >= 0)
			Velocity.y = vel.y;
		StartCoroutine(IntroJump(vel, maxVel));
	}
    void Jump()
    {
		playState = PlayState.Jump;
		startJumpPos = transform.position.y;
		isIntroJump = true;
		StartCoroutine(IntroJump(Vector2.zero, Vector2.zero));
	}

    IEnumerator IntroJump(Vector2 vel, Vector2 maxVel)
    {
        float dis = 0;
		// move up
		float curJumpMin = JumpMin * (vel.y + JumpSpeed)/ JumpSpeed;
		float curJumpMax = JumpMax * (vel.y + JumpSpeed) / JumpSpeed;
		float curJumpSpeed = JumpSpeed + vel.y;
		while (playState == PlayState.Jump && dis <= curJumpMin && Velocity.y < curJumpSpeed)
        {
			if (vel.x != 0 && Mathf.Abs(Velocity.x) < maxVel.x)
			{
				isMove = false;
				Velocity.x += vel.x;
				if(Mathf.Abs(Velocity.x) > maxVel.x)
				{
					Velocity.x = maxVel.x * GetDirInt;
				}
			}
			if (!CheckUpMove())   //返回false说明撞到墙，结束跳跃
			{
                Velocity.y = 0;
                isIntroJump = false;
				isMove = true;
				yield break;
            }
			//获取当前角色相对于初始跳跃时的高度
			dis = transform.position.y - startJumpPos;
			if (vel.y <= 0)
			{
				Velocity.y += 240 * Time.fixedDeltaTime;
			}
			yield return new WaitForFixedUpdate();
        }
		Velocity.y = curJumpSpeed;
		isMove = true;
		while (playState == PlayState.Jump && input.JumpKey && dis < curJumpMax)
        {
	        if (!CheckUpMove())
            {
                Velocity.y = 0;
                isIntroJump = false;
				yield break;
            }
			if (input.JumpKeyDown && BoxCheckCanClimb() && !input.ClimbKey && !CheckIsClimb())
			{
				Velocity.y = 0;
				isIntroJump = false;
				Jump(new Vector2(4 * -GetDirInt, 0), new Vector2(24 , 0));
				yield break;
			}

			dis = transform.position.y - startJumpPos;
            Velocity.y = curJumpSpeed;
            yield return new WaitForFixedUpdate();
        }
		// slow down
		while (playState == PlayState.Jump && Velocity.y > 0 )
        {
			if (!CheckUpMove())
            {
                break;
            }
			if (input.JumpKeyDown && BoxCheckCanClimb() && !input.ClimbKey && !CheckIsClimb())
			{
				Velocity.y = 0;
				isIntroJump = false;
				Jump(new Vector2(4 * -GetDirInt, 0), new Vector2(24, 0));
				yield break;
			}
			if (dis > JumpMax)
            {
                Velocity.y -= 100 * Time.fixedDeltaTime;
            }
            else
            {
                Velocity.y -= 200 * Time.fixedDeltaTime;
            }
            yield return new WaitForFixedUpdate();
        }
        // fall down
        Velocity.y = 0;
        playAnimator.SetFloat("speedy", Velocity.y);
        yield return 0.1f;
        isIntroJump = false;
        playState = PlayState.Fall;
    }
	#endregion

	#region 冲刺
	void Dash()
    {
        Velocity = Vector2.zero;
		dashCount--;
		playState = PlayState.Dash;
		StopAllCoroutines();
		StartCoroutine("IntroDash");
    }

    IEnumerator IntroDash()
    {
		//获取输入时的按键方向
		float verticalDir;
		if(isGround && input.v < 0)  //在地面上并且按住下时不应该有垂直方向
		{
			verticalDir = 0;
		}
		else
		{
			verticalDir = input.v;
		}
		//冲刺方向注意归一化
		DashDir = new Vector2(input.MoveDir, verticalDir).normalized;
        if(DashDir == Vector2.zero)
        {
			DashDir = Vector3.right * GetDirInt;
        }
        int i = 0;
        isCanControl = false;
        FixHorizon = false;
        while (i < 9)
        {
			if(playState == PlayState.Dash)
			{
				Velocity = DashDir * 20f;
			}
			i++;
			CheckHorizontalMove();
			yield return new WaitForFixedUpdate();
        }
        isCanControl = true;
		if (playState == PlayState.Dash)
		{
			if (DashDir.y > 0)
			{
				Velocity.y = 24;
			}
			if(isGround)
			playState = PlayState.Normal;
			else
			playState = PlayState.Fall;
		}
	}

	void CheckDashJump()
	{
		if (playState == PlayState.Dash)
		{
			if (input.JumpKeyDown)
			{
				if (DashDir == Vector2.up && BoxCheckCanClimbDash())
				{
					Jump(new Vector2(4 * -GetClimpDirInt, 24 - JumpSpeed + 6), new Vector2(24, 0));
				}
				else if (isGround)
				{
					Velocity.y = 0;
					if(input.v < 0)
					{
						if(input.MoveDir != 0)
						{
							dashCount = 1;
							Velocity = new Vector3(30 * input.MoveDir, 0);
							Jump(new Vector2(4 * input.MoveDir, -4), new Vector2(42, 0));
						}
						else
						{
							Jump(new Vector2(0, -4), new Vector2(0, 0));
						}
					}
					else
					{
						Jump();
					}
				}
			}
		}
	}

	#endregion

	#region 位移修正
	/// <summary>
	/// 检测并修正水平方向的位移
	/// </summary>
	bool FixHorizon;
	void CheckHorizontalMove()
	{
		if (FixHorizon)
			return;
		HorizontalBox = playDir == PlayDir.Right ? RightBox : LeftBox;
		if (HorizontalBox.Length == 1)
		{
			var pointDis = HorizontalBox[0].point.y - Position.y;
			if (pointDis > 0.34f)
			{
				var offsetPos = Mathf.Ceil(Position.y);
				transform.position = new Vector3(transform.position.x, offsetPos - 0.22f, 0);
			}
			else if (pointDis < -0.42f)
			{
				var offsetPos = Mathf.Ceil(transform.position.y);
				transform.position = new Vector3(transform.position.x, offsetPos + 0.035f, 0);
			}
			FixHorizon = true;
		}
	}

	/// <summary>
	/// 检测并修正垂直方向的位移
	/// </summary>
	bool CheckUpMove()
	{
		if (UpBox.Length == 1)
		{
			var pointDis = UpBox[0].point.x - transform.position.x;
			if (pointDis > 0.34f)
			{
				var offsetPos = Mathf.Floor(transform.position.x);
				transform.position = new Vector3(offsetPos + 0.48f, transform.position.y, 0);
				return true;
			}
			else if (pointDis < -0.34f)
			{
				var offsetPos = Mathf.Floor(transform.position.x);
				transform.position = new Vector3(offsetPos + 0.52f, transform.position.y, 0);
				return true;
			}
			else
			{
				Velocity.y = 0;
				playState = PlayState.Fall;
				return false;
			}
		}
		return true;
	}
	#endregion

	#region 角色属性

	//角色面朝方向
	PlayDir lastDir;
    void CheckDir()
    {
        if (playState == PlayState.Climb || playState == PlayState.Dash)
            return;
        lastDir = playDir;
        if (input.MoveDir > 0)
        {
            playDir = PlayDir.Right;
        }
        else if (input.MoveDir < 0)
        {
            playDir = PlayDir.Left;
        }
        if(lastDir != playDir)
        {
            transform.localScale = new Vector3(GetDirInt, transform.localScale.y, transform.localScale.z);
        }
    }

    //玩家朝向的int值（1为right， -1为left）
    int GetDirInt
    {
        get{
            return playDir == PlayDir.Right ? 1 : -1;
        }
    }

	//正确情况的蹬墙跳应该是墙壁相对于玩家的反方向，爬墙的时候对玩家朝向进行了修正，所以玩家的反方向就是跳跃方向，
	//但是在fall状态下没有对玩家方向进行修改，所以只能通过墙的位置进行判断
	int GetClimpDirInt
	{
		get
		{
			return HorizontalBox == RightBox ? 1 : -1;
		}
	}

    //玩家中心点
    Vector2 Position
    {
        get
        {
            return transform.position - CenterPos;
        }
    }

    //地面检测
    bool isGround { get { return DownBox.collider != null ? true : false; } }
    #endregion
}
