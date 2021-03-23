using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
   private Moveable _moveable;
   private float move;
   private bool jump;
   private void Start()
   {
      _moveable = GetComponent<Moveable>();
      
   }

   private void Update()
   {
      move = Input.GetAxis("Horizontal");
      jump = Input.GetButtonDown("Jump");
      _moveable.Move(move, false, jump);
   }

   private void FixedUpdate()
   {
      
   }
}
