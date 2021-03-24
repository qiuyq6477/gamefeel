﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using DG.Tweening;

public class Movement : MonoBehaviour
{
    public bool facingRight = true;  // For determining which way the player is currently facing.

    private Collision coll;
    private Animator animator;
    [HideInInspector]
    public Rigidbody2D rb;
    //private AnimationScript anim;

    [Space]
    [Header("Stats")]
    public float speed = 10;
    [HideInInspector]
    public float jumpForce = 50;
    public float wallJumpForce = 1.5f;
    public float slideSpeed = 5;
    public float wallJumpLerp = 10;
    public float dashSpeed = 20;

    [Space]
    [Header("Booleans")]
    public bool canMove;
    public bool wallGrab;
    public bool wallJumped;
    public bool wallSlide;
    public bool isDashing;
    public bool doubleJumped;

    [Space]

    private bool groundTouch;
    private bool hasDashed;

    private float jumpTimer;

    public int side = 1;

    //[Space]
    //[Header("Polish")]
    //public ParticleSystem dashParticle;
    //public ParticleSystem jumpParticle;
    //public ParticleSystem wallJumpParticle;
    //public ParticleSystem slideParticle;
    // Start is called before the first frame update


    [Space]
    [Header("AblitiesSwitch")]
    public bool canGrab;
    public bool canDoubleJump;
    public bool canDash;

    private IMyInput input;
    private Weapon _weapon;
    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        coll = GetComponent<Collision>();
        rb = GetComponent<Rigidbody2D>();
        input = GetComponent<IMyInput>();
        _weapon = GetComponent<Weapon>();
        // anim = GetComponentInChildren<AnimationScript>();
    }
    
    // Update is called once per frame
    void Update()
    {
        //读取方向输入
        float x = input.X();
        float y = input.Y();
        //读取即时方向输入
        float xRaw = input.XRaw();
        float yRaw = input.YRaw();
        Vector2 dir = new Vector2(x, y);

        Walk(dir);
       // anim.SetHorizontalMovement(x, y, rb.velocity.y);
       if (_weapon != null && _weapon.onfire)
       {
           var velocity = rb.velocity;
           velocity = new Vector2(velocity.x + _weapon.backforce * -side, velocity.y);
           rb.velocity = velocity;
       }
       
        //如果靠着墙, 还按了爬墙按钮, 并且没有被锁定移动
        //取消滑墙动作, 改成抓墙
        if (coll.onWall && input.GetButton("Climb") && canMove)
        {
            //if(side != coll.wallSide)
            //    anim.Flip(side*-1);
            wallGrab = true;
            wallSlide = false;


        }

        //如果松开了爬墙按键, 或者离开了墙, 或者被锁定了移动
        //滑墙和抓墙全部取消
        if (input.GetButtonUp("Climb") || !coll.onWall || !canMove)
        {
            wallGrab = false;
            wallSlide = false;
        }

        if (jumpTimer > 0)
        {
            jumpTimer -= Time.deltaTime;
        }
        //如果和地面接触并且没有在冲刺, 打开跳墙后移动开关. 角色下落脚本打开
        if (coll.onGround && !isDashing)
        {
            wallJumped = false;
            GetComponent<BetterJumping>().enabled = true;

                //animator.SetBool("Jumping", false);
        }


        if (wallGrab && !isDashing)
        {
            rb.gravityScale = 0;
            if(x > .2f || x < -.2f)
                rb.velocity = new Vector2(rb.velocity.x, 0);

            float speedModifier = y > 0 ? .5f : 1;

            rb.velocity = new Vector2(rb.velocity.x, y * (speed * speedModifier));
        }
        else
        {
            rb.gravityScale = 3;
        }

        if(coll.onWall && !coll.onGround)
        {
            if (x != 0 && !wallGrab)
            {
                wallSlide = true;
                WallSlide();
            }
        }

        if (!coll.onWall || coll.onGround)
            wallSlide = false;

        if (input.GetButtonDown("Jump"))
        {
            //anim.SetTrigger("jump");
            AudioManager.instance.PlaySound("jump");
            if (coll.onGround)
                Jump(Vector2.up, false);
            if (coll.onWall && !coll.onGround)
                WallJump();

            if (!coll.onGround &&!doubleJumped &&canDoubleJump)
            {
                doubleJumped = true;
                Jump(Vector2.up, false);
            }
        }
        
        if (input.GetButtonDown("Dash") && !hasDashed)
        {
            if(xRaw != 0 || yRaw != 0)
                Dash(xRaw, yRaw);
        }

        if (coll.onGround && !groundTouch)
        {
            GroundTouch();
            groundTouch = true;
        }

        if(!coll.onGround && groundTouch)
        {
            groundTouch = false;
        }

        //WallParticle(y);

        if (wallGrab || wallSlide || !canMove)
            return;

        if (_weapon != null && _weapon.onfire)
        {
            
        }
        else
        {
            //Flip Check
            if (x > 0 && !facingRight)
            {
                // ... flip the player.
                Flip();
            }
            // Otherwise if the input is moving the player left and the player is facing right...
            else if (x < 0 && facingRight)
            {
                // ... flip the player.
                Flip();
            }
        }
    }

    void GroundTouch()
    {
        hasDashed = false;
        isDashing = false;
        doubleJumped = false;

        //side = anim.sr.flipX ? -1 : 1;

        //jumpParticle.Play();
    }

    //冲刺的方法
    private void Dash(float x, float y)
    {
        //Camera.main.transform.DOComplete();
        //Camera.main.transform.DOShakePosition(.2f, .5f, 14, 90, false, true);
        //FindObjectOfType<RippleEffect>().Emit(Camera.main.WorldToViewportPoint(transform.position));

        if (!canDash)
            return;
        hasDashed = true;

        //anim.SetTrigger("dash");

        rb.velocity = Vector2.zero;
        Vector2 dir = new Vector2(x, 0);

        rb.velocity += dir.normalized * dashSpeed;
        StartCoroutine(DashWait());
    }

    //冲刺协程, 冲刺后关闭重力, 跳跃模拟等. 0.3秒后恢复.
    IEnumerator DashWait()
    {
        //冲刺残影相关
        //FindObjectOfType<GhostTrail>().ShowGhost();

        //调用地面冲刺判断的协程.
        StartCoroutine(GroundDash());

        //粒子系统相关
        //DOVirtual.Float(14, 0, .8f, RigidbodyDrag
        //dashParticle.Play();


        rb.gravityScale = 0;
        GetComponent<BetterJumping>().enabled = false;
        wallJumped = true;
        isDashing = true;

        //这句话打开之后, 冲刺后将无法二段跳
        //doubleJumped = true;

        yield return new WaitForSeconds(.3f);
        //粒子系统相关
        //dashParticle.Stop();
        rb.gravityScale = 3;
        GetComponent<BetterJumping>().enabled = true;
        wallJumped = false;
        isDashing = false;
        //似乎没什么用
        //doubleJumped = false;
    }

    //地面冲刺协程. 如果在地面上冲刺的话, 会很快恢复
    IEnumerator GroundDash()
    {
        yield return new WaitForSeconds(.15f);
        if (coll.onGround)
            hasDashed = false;
    }

    //蹬墙跳
    private void WallJump()
    {
        if (!canGrab)
            return;
        //if ((side == 1 && coll.onRightWall) || side == -1 && !coll.onRightWall)
        //{
        //    side *= -1;
        //    //anim.Flip(side);
        //}

        StopCoroutine(DisableMovement(0));
        StartCoroutine(DisableMovement(.1f));

        Vector2 wallDir = coll.onRightWall ? Vector2.left : Vector2.right;

        Jump((Vector2.up / wallJumpForce + wallDir / wallJumpForce), true);

        wallJumped = true;
        // animator.SetBool("WallSlide", false);
    }

    //贴墙滑行的方法
    private void WallSlide()
    {
        if (!canGrab)
            return;
        //if(coll.wallSide != side)
        // anim.Flip(side * -1);

        if (!canMove)
            return;

        bool pushingWall = false;
        if((rb.velocity.x > 0 && coll.onRightWall) || (rb.velocity.x < 0 && coll.onLeftWall))
        {
            pushingWall = true;
        }
        float push = pushingWall ? 0 : rb.velocity.x;
        // animator.SetBool("WallSlide", true);
        rb.velocity = new Vector2(push, -slideSpeed);
    }

    //正常移动的方法
    private void Walk(Vector2 dir)
    {
        if (!canMove)
            return;

        if (wallGrab)
            return;

        if (!wallJumped)
        {
            rb.velocity = new Vector2(dir.x * speed, rb.velocity.y);
            animator.SetFloat("speedx", dir.x);
            animator.SetFloat("speedy", rb.velocity.y);
        }
        else
        {
            rb.velocity = Vector2.Lerp(rb.velocity, (new Vector2(dir.x * speed, rb.velocity.y)), wallJumpLerp * Time.deltaTime);
            animator.SetFloat("speedy", rb.velocity.y);
        }

        // animator.SetBool("WallSlide", false);

    }

    //跳跃的方法
    public void Jump(Vector2 dir, bool wall)
    {
        //slideParticle.transform.parent.localScale = new Vector3(ParticleSide(), 1, 1);
        //ParticleSystem particle = wall ? wallJumpParticle : jumpParticle;

        //二段跳
        if (doubleJumped)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0);
            rb.velocity += dir * jumpForce/1.2f;
            jumpTimer = 0.2f;
        }
        else
        {
            rb.velocity = new Vector2(rb.velocity.x, 0);
            rb.velocity += dir * jumpForce;
            jumpTimer = 0.2f;
        }
        //particle.Play();
    }

    //蹬墙跳的时候关闭可控移动的协程
    IEnumerator DisableMovement(float time)
    {
        canMove = false;
        yield return new WaitForSeconds(time);
        canMove = true;
    }

    void RigidbodyDrag(float x)
    {
        rb.drag = x;
    }

    private void Flip()
    {
        // Switch the way the player is labelled as facing.
        facingRight = !facingRight;
        side = facingRight ? 1 : -1;
        // Multiply the player's x local scale by -1.
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    //void WallParticle(float vertical)
    //{
    //    var main = slideParticle.main;

    //    if (wallSlide || (wallGrab && vertical < 0))
    //    {
    //        slideParticle.transform.parent.localScale = new Vector3(ParticleSide(), 1, 1);
    //        main.startColor = Color.white;
    //    }
    //    else
    //    {
    //        main.startColor = Color.clear;
    //    }
    //}

    //int ParticleSide()
    //{
    //    int particleSide = coll.onRightWall ? 1 : -1;
    //    return particleSide;
    //}
}
