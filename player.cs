/*
 * PLAYER HANDELING
 * [UNDER HEAVY DEVELOPMENT]
 * That little demon hamster needs at least some sort of control.
 * Together with other functions and events to be acted upon the player.
 * 
 * Main things:
 * 		Player Control: move and shift dimentions
 * 		
 * 		Physics control:
 * 			Horizontal air resistance when high
 * 			Gravity decreasing with height, although not too much
 * 		
 * 		Enemy spawn:
 * 			Slap down when too much hangtime.
 * 
 * TODO:
 * 		Player control - Add:
 * 				- Slam into track
 * 				- Air jump
 * 
 * 		Physics control:
 * 				- loosen close_to_ground
 * 
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Security.Cryptography;

public class player : MonoBehaviour {
    
	// Player physics
    public int over = 1;
    private Vector2 gravity_magnitutde = new Vector2(0,9.81f);
    public Vector2 gravity_force;
    private int gravity_direction = 1;
    private float countdown = 0.00f;

	// Player display
    private SpriteRenderer hamster_sprite;
    private Hampster hamster_script;
    private Rigidbody2D hamster_wheel;
    
	// Player control
	public float sprint_force = 15000f;

	// Enemy spawn
	private float air_hang_time = 0f;
	public GameObject missile;


	// Player displacement mechanics
    private bool shift_left_latch  = false;
    private bool shift_right_latch = false;

	public float shift_vel = 0f;
	public float shift_vel_prev = 0;
	public float shift_ref = 0f;
	public float shift_pos = 0f;
	public float shift_step = 5f;
	public float shift_smooth_factor = 0.2f;

    void Start () {
        
		//Starting above the line
		over = 1;

		//Gravity starts down
        Physics2D.gravity = Mathf.Pow(-1,over)*gravity_magnitutde;
        gravity_direction = -1;

		// Finds other objects
        hamster_sprite = gameObject.transform.Find("Hampster").GetComponentInChildren<SpriteRenderer>();
        hamster_script = gameObject.transform.Find("Hampster").GetComponentInChildren<Hampster>();
        hamster_wheel = GameObject.Find("Player").GetComponent<Rigidbody2D>();
    }

	void OnCollisionEnter2D (Collision2D crash){
		// When we get hit or land, reset displacement and hangtime.
		shift_pos = 0f;
		shift_vel = 0f;
		shift_ref = 0f;

		air_hang_time = 0;
	}

    // Update is called once per frame
    void Update ()
    {
		// Need to know how far the player is from ground underneath
		RaycastHit2D distance_to_track = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y), Mathf.Pow(-1, over) * Vector2.up, Mathf.Infinity, LayerMask.GetMask("Track"));

		// Is the player in the vesinity of ground track? (distance_to_track don't work on slopes)
		Collider2D close_to_ground = Physics2D.OverlapCircle (new Vector2 (transform.position.x, transform.position.y), 7f, LayerMask.GetMask("Track"));

		// Decrease forward speed if too high up.
		if (distance_to_track.distance > 70 && hamster_wheel.velocity.x > 0) {
			//This isnt quite right, but we have to do some dirty maths unless we're building physics from the ground up
			float vel_decrease = 1 / (1 + Mathf.Exp (distance_to_track.distance / 4 - 25));
			hamster_wheel.velocity -= new Vector2(Time.deltaTime * vel_decrease * hamster_wheel.velocity.x, 0);
		}


		// Set gravity force depending on distance to track.
		gravity_force = (Mathf.Abs(hamster_wheel.velocity.x) > 2) ? gravity_direction * gravity_magnitutde * (.75f / (1 + Mathf.Exp ( (distance_to_track.distance - 20) / 3)) + .25f) : 1.25f * gravity_direction * gravity_magnitutde;
		Physics2D.gravity = gravity_force;

        if (countdown > 0) {
            countdown -= Time.deltaTime;
        }

		// Try and change dimentions
		if ( (Input.GetKeyDown("space") || Input.touchCount == 1 || (shift_right_latch && shift_left_latch)) && countdown <= 0 && close_to_ground)
        {
			RaycastHit2D hit = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y), Mathf.Pow(-1, over) * Vector2.up, Mathf.Infinity, LayerMask.GetMask("Track"));
            if (hit)
            {
				// Teleport to other side
                transform.position += new Vector3(0, 2 * (hit.point.y - transform.position.y), 0);
                
				// change side flag and gravity
				over = over ^ 1;
                gravity_direction *= -1;

				// Remembering to flip angular speed and sprite
				GetComponent<Rigidbody2D>().angularVelocity *= -1;
                hamster_sprite.flipY = !hamster_sprite.flipY;
                hamster_script.angVel *= -1;
                
                countdown = 0.15f;
            }
        }

		// Shift player to the right or add torque
		if (Input.GetKey (KeyCode.RightArrow) && !Input.GetKey (KeyCode.LeftArrow)) {

			if (close_to_ground) {

				// Add torque with clock direction (when over)
				hamster_wheel.AddTorque (gravity_direction * Time.deltaTime * sprint_force);
			
			} else {
				if (!shift_right_latch) {
					// If we haven't shifted before, shift now
					shift_ref = shift_step;
				}

				// Latces remain
				shift_right_latch = true;
				shift_left_latch = false;
			}
		} 

		// Shift player to left or add torque
		else if (Input.GetKey (KeyCode.LeftArrow) && !Input.GetKey (KeyCode.RightArrow)) {
			if (close_to_ground) {

				// Add torque against clock (when over)
				hamster_wheel.AddTorque (-gravity_direction * Time.deltaTime * sprint_force);

			} else {
				
				if (!shift_left_latch) {
					// If haven't shifted before
					shift_ref = -shift_step;
				}

				// Set latches
				shift_right_latch = false;
				shift_left_latch  = true;
			}

		} else {
			// Both or none of arrowkeys pushed. Resets the latches
			shift_ref = 0f;
			shift_left_latch  = false;
			shift_right_latch = false;

		}

		// The player is displaced with SmoothDamps velocity, based on shift_pos. This is in addition to physics engine. 
		//(Using position don't work well with the physics engine, and SmoothDamp gives a continuous velocity)
		// Its like having the player bound to a point with a rubber band, with force is can move somwhat, but if it lets go then it returns.

		// Store the previous velocity
		shift_vel_prev = shift_vel;

		// Calculate next displacement
		shift_pos = Mathf.SmoothDamp (shift_pos, shift_ref, ref shift_vel, shift_smooth_factor);

		// Store this velocity, setting to 0 if too low
		shift_vel = (Mathf.Abs(shift_vel) > 0.05f) ? shift_vel : 0;

		// Updating player velocity
		hamster_wheel.velocity += new Vector2(shift_vel - shift_vel_prev, 0);


		// The player has been in air for too long. A missile might get'em back.
		if (Time.deltaTime / (1 + Mathf.Exp (5 - air_hang_time)) > Random.value  && !close_to_ground) {
			GameObject new_missile = Instantiate (missile, transform.position + new Vector3 (10, -50*gravity_direction, 0), Quaternion.Euler (new Vector3 (0, 0, 90))) as GameObject;
			air_hang_time = -1;
		} else {
			air_hang_time += Time.deltaTime;
		}
    }
}