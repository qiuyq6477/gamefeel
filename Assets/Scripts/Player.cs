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
   private float deathTime;
   private MyInput _myInput;
   public bool death;
   private void Start()
   {
      _movement = GetComponent<Movement>();
      _rigidbody2D = GetComponent<Rigidbody2D>();
      animator = GetComponentInChildren<Animator>();
      _myInput = GetComponent<MyInput>();
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
         deathTime = 5f;
         transform.Find("sprMachinegun_0").gameObject.SetActive(false);
         _rigidbody2D.velocity = new Vector2(100*Random.Range(-1, 1)>0?1:-1, _movement.jumpForce);
      }
   }

   private void Update()
   {
      if (deathTime > 0)
      {
         deathTime -= Time.unscaledDeltaTime;
         if (deathTime <= 0)
         {
            Time.timeScale = 1;
            SceneManager.LoadScene("main");
         }
      }
   }

   private void FixedUpdate()
   {
      
   }
}
