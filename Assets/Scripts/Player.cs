using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class Player : MonoBehaviour
{
   private Movement _movement;
   private float move;
   private bool jump;
   private Animator animator;
   private Rigidbody2D _rigidbody2D;
   private MyInput _myInput;
   public bool death;
   private Collision _collision;
   private void Start()
   {
      _movement = GetComponent<Movement>();
      _rigidbody2D = GetComponent<Rigidbody2D>();
      animator = GetComponentInChildren<Animator>();
      _myInput = GetComponent<MyInput>();
      _collision = GetComponent<Collision>();
      death = false;
   }

   private void OnCollisionEnter2D(Collision2D other)
   {
      if (other.gameObject.CompareTag("Enemy") && !death)
      {
         gameObject.layer = 10;
         foreach (Transform child in transform)
         {
            child.gameObject.layer = 10;
         }
         death = true;
         _myInput.enabled = false;
         Time.timeScale = 0.1f;
         animator.SetTrigger("death");
         transform.Find("sprMachinegun_0").gameObject.SetActive(false);
         _movement.enabled = false;
         AudioManager.instance.PlaySound("deadPlayer");
         GetComponentInChildren<ShellGenerator>().enabled = false;
         _rigidbody2D.velocity = new Vector2(8*(Random.Range(-1, 1)>0?1:-1), _movement.jumpForce);
      }
   }

   private bool onground = false;
   private void Update()
   {
      if (death)
      {
         if (_collision.onGround && _movement.rb.velocity.y < 0)
         {
            animator.SetTrigger("sleep");
            _rigidbody2D.bodyType = RigidbodyType2D.Static;
            onground = true;
         }

         if (onground && Time.timeScale < 1)
         {
            Time.timeScale = Mathf.Lerp(Time.timeScale, 1, Time.unscaledDeltaTime);
         }
      }
   }

   private void FixedUpdate()
   {
      
   }
}
