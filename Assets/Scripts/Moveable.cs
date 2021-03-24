// using UnityEngine;
//
// [RequireComponent(typeof(BoxCollider2D))]
// public class Moveable : MonoBehaviour
// {
//     [SerializeField, Tooltip("Max speed, in units per second, that the character moves.")]
//     float speed = 9;
//
//     [SerializeField, Tooltip("Acceleration while grounded.")]
//     float walkAcceleration = 75;
//
//     [SerializeField, Tooltip("Acceleration while in the air.")]
//     float airAcceleration = 30;
//
//     [SerializeField, Tooltip("Deceleration applied when character is grounded and not attempting to move.")]
//     float groundDeceleration = 70;
//
//     [SerializeField, Tooltip("Max height the character will jump regardless of gravity")]
//     float jumpHeight = 4;
//
//     private BoxCollider2D boxCollider;
//
//     private Vector2 velocity;
//
//     /// <summary>
//     /// Set to true when the character intersects a collider beneath
//     /// them in the previous frame.
//     /// </summary>
//     private bool grounded;
//
//     private void Awake()
//     {      
//         boxCollider = GetComponent<BoxCollider2D>();
//     }
//
//     private void Update()
//     {
//         // Use GetAxisRaw to ensure our input is either 0, 1 or -1.
//         float moveInput = Input.GetAxisRaw("Horizontal");
//
//         if (grounded)
//         {
//             velocity.y = 0;
//
//             if (Input.GetButtonDown("Jump"))
//             {
//                 // Calculate the velocity required to achieve the target jump height.
//                 velocity.y = Mathf.Sqrt(2 * jumpHeight * Mathf.Abs(Physics2D.gravity.y));
//             }
//         }
//
//         float acceleration = grounded ? walkAcceleration : airAcceleration;
//         float deceleration = grounded ? groundDeceleration : 0;
//
//         if (moveInput != 0)
//         {
//             velocity.x = Mathf.MoveTowards(velocity.x, speed * moveInput, acceleration * Time.deltaTime);
//         }
//         else
//         {
//             velocity.x = Mathf.MoveTowards(velocity.x, 0, deceleration * Time.deltaTime);
//         }
//
//         velocity.y += Physics2D.gravity.y * Time.deltaTime;
//
//         transform.Translate(velocity * Time.deltaTime);
//
//         grounded = false;
//
//         // Retrieve all colliders we have intersected after velocity has been applied.
//         Collider2D[] hits = Physics2D.OverlapBoxAll(transform.position, boxCollider.size, 0);
//
//         foreach (Collider2D hit in hits)
//         {
//             // Ignore our own collider.
//             if (hit == boxCollider)
//                 continue;
//
//             ColliderDistance2D colliderDistance = hit.Distance(boxCollider);
//
//             // Ensure that we are still overlapping this collider.
//             // The overlap may no longer exist due to another intersected collider
//             // pushing us out of this one.
//             if (colliderDistance.isOverlapped)
//             {
//                 transform.Translate(colliderDistance.pointA - colliderDistance.pointB);
//                 Debug.DrawLine(colliderDistance.pointA , colliderDistance.pointB, Color.red);
//                 // If we intersect an object beneath us, set grounded to true. 
//                 if (Vector2.Angle(colliderDistance.normal, Vector2.up) < 30 && velocity.y < 0)
//                 {
//                     grounded = true;
//                 }
//             }
//         }
//     }
// }



using UnityEngine;
using UnityEngine.Events;

public class Moveable : MonoBehaviour
{
	[SerializeField] private float m_JumpForce = 400f;							// Amount of force added when the player jumps.
	[Range(0, 1)] [SerializeField] private float m_CrouchSpeed = .36f;			// Amount of maxSpeed applied to crouching movement. 1 = 100%
	[Range(0, .3f)] [SerializeField] private float m_MovementSmoothing = .05f;	// How much to smooth out the movement
	[SerializeField] private bool m_AirControl = false;							// Whether or not a player can steer while jumping;
	[SerializeField] private LayerMask m_WhatIsGround;							// A mask determining what is ground to the character
	[SerializeField] private Transform m_GroundCheck;							// A position marking where to check if the player is grounded.
	[SerializeField] private Transform m_CeilingCheck;							// A position marking where to check for ceilings
	[SerializeField] private Collider2D m_CrouchDisableCollider;				// A collider that will be disabled when crouching

	const float k_GroundedRadius = .2f; // Radius of the overlap circle to determine if grounded
	private bool m_Grounded;            // Whether or not the player is grounded.
	const float k_CeilingRadius = .2f; // Radius of the overlap circle to determine if the player can stand up
	private Rigidbody2D m_Rigidbody2D;
	private bool m_FacingRight = true;  // For determining which way the player is currently facing.
	private Vector3 m_Velocity = Vector3.zero;

	[Header("Events")]
	[Space]

	public UnityEvent OnLandEvent;

	[System.Serializable]
	public class BoolEvent : UnityEvent<bool> { }

	public BoolEvent OnCrouchEvent;
	private bool m_wasCrouching = false;

	private void Awake()
	{
		m_Rigidbody2D = GetComponent<Rigidbody2D>();

		if (OnLandEvent == null)
			OnLandEvent = new UnityEvent();

		if (OnCrouchEvent == null)
			OnCrouchEvent = new BoolEvent();
	}

	private void FixedUpdate()
	{
		bool wasGrounded = m_Grounded;
		m_Grounded = false;

		// The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
		// This can be done using layers instead but Sample Assets will not overwrite your project settings.
		Collider2D[] colliders = Physics2D.OverlapCircleAll(m_GroundCheck.position, k_GroundedRadius, m_WhatIsGround);
		for (int i = 0; i < colliders.Length; i++)
		{
			if (colliders[i].gameObject != gameObject)
			{
				m_Grounded = true;
				if (!wasGrounded)
					OnLandEvent.Invoke();
			}
		}
	}


	public void Move(float move, bool crouch, bool jump)
	{
		// If crouching, check to see if the character can stand up
		if (!crouch)
		{
			// If the character has a ceiling preventing them from standing up, keep them crouching
			if (Physics2D.OverlapCircle(m_CeilingCheck.position, k_CeilingRadius, m_WhatIsGround))
			{
				crouch = true;
			}
		}

		//only control the player if grounded or airControl is turned on
		if (m_Grounded || m_AirControl)
		{

			// If crouching
			if (crouch)
			{
				if (!m_wasCrouching)
				{
					m_wasCrouching = true;
					OnCrouchEvent.Invoke(true);
				}

				// Reduce the speed by the crouchSpeed multiplier
				move *= m_CrouchSpeed;

				// Disable one of the colliders when crouching
				if (m_CrouchDisableCollider != null)
					m_CrouchDisableCollider.enabled = false;
			} else
			{
				// Enable the collider when not crouching
				if (m_CrouchDisableCollider != null)
					m_CrouchDisableCollider.enabled = true;

				if (m_wasCrouching)
				{
					m_wasCrouching = false;
					OnCrouchEvent.Invoke(false);
				}
			}

			// Move the character by finding the target velocity
			Vector3 targetVelocity = new Vector2(move * 10f, m_Rigidbody2D.velocity.y);
			// And then smoothing it out and applying it to the character
			m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, targetVelocity, ref m_Velocity, m_MovementSmoothing);

			// If the input is moving the player right and the player is facing left...
			if (move > 0 && !m_FacingRight)
			{
				// ... flip the player.
				Flip();
			}
			// Otherwise if the input is moving the player left and the player is facing right...
			else if (move < 0 && m_FacingRight)
			{
				// ... flip the player.
				Flip();
			}
		}
		// If the player should jump...
		if (m_Grounded && jump)
		{
			// Add a vertical force to the player.
			m_Grounded = false;
			m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce));
		}
	}


	private void Flip()
	{
		// Switch the way the player is labelled as facing.
		m_FacingRight = !m_FacingRight;

		// Multiply the player's x local scale by -1.
		Vector3 theScale = transform.localScale;
		theScale.x *= -1;
		transform.localScale = theScale;
	}
}

