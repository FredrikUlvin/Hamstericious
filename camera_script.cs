using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEditor;

public class camera_script : MonoBehaviour {
    public GameObject playerClass;
    public float x_offset = 10;
	public float y_offset = 0;
	public float player_time_to_screen_edge = 1; // How long it should take for player to cross screen, if camera position is kept fixed.

    private Vector2 offset;
    private Vector3 vel = Vector3.zero;
    private Camera main_cam;
	public float camera_zoom_speed = 0.1f;

	private LineRenderer ground_track;

	void Start () {
        offset = new Vector2(5, 5);
        main_cam = Component.FindObjectOfType<Camera>();
		ground_track = GameObject.Find("LevelGenerator").GetComponent<LineRenderer> ();
	}

    void LateUpdate () {
		RaycastHit2D hitUp = Physics2D.Raycast(playerClass.transform.position,Vector2.up, Mathf.Infinity, LayerMask.GetMask("Track"));
		RaycastHit2D hitDown = Physics2D.Raycast(playerClass.transform.position, Vector2.down, Mathf.Infinity, LayerMask.GetMask("Track"));
		RaycastHit2D track_hit = Physics2D.Raycast (playerClass.transform.position, Mathf.Pow (-1,playerClass.GetComponent<player>().over) * Vector2.up, Mathf.Infinity, LayerMask.GetMask ("Track"));

		Vector3 camPos = transform.position;

		// Calculating the which of screen
		Vector2 player_velocity = playerClass.GetComponent<Rigidbody2D>().velocity;
		int camera_width = (int) (player_velocity.x * player_time_to_screen_edge / 2);

		x_offset = (Mathf.Abs(player_velocity.x) > 3) ? .95f * Camera.main.orthographicSize * Camera.main.aspect * (2*(1 / (1 + Mathf.Exp (-player_velocity.x/10)))-1):0;
		float[] track_y_lim = FindTrackExtremeties (camera_width);
		y_offset = 0.5f * (Mathf.Max (track_y_lim [1], playerClass.transform.position.y) + Mathf.Min (track_y_lim [0], playerClass.transform.position.y) - 2 * track_hit.point.y);

		float camera_height =	 Mathf.Min(50, Mathf.Max (Mathf.Max (0, 1.05f * Mathf.Abs(Mathf.Max (track_y_lim [1], playerClass.transform.position.y) - Mathf.Min (track_y_lim [0], playerClass.transform.position.y))), Mathf.Max(25,camera_width / Camera.main.aspect)));

		// Setting orthographic size
		Camera.main.orthographicSize =  (1 - camera_zoom_speed) * Camera.main.orthographicSize + camera_zoom_speed * camera_height;
		// This have to be more compelex: Take min max pos from track into account, so make the size max of [size calculated from forward speed, largest difference tween player position to track minmax]


		// Time to offset the camera. Target should be the track just below. the offset should be based on velocity and min max of visible line.
		//		Something like: x: dist_to_screen_edge * (1 / (1 + e^( K * vel.x) ))
		//						y: 0.5*(max(ymax, playerpos)-min(ymin, playerpos))


		transform.position = Vector3.SmoothDamp (camPos, new Vector3 (track_hit.point.x + x_offset, track_hit.point.y + y_offset, -1), ref vel, 0.3f);
		if (hitUp)
        {
            //transform.position = Vector3.SmoothDamp(camPos, new Vector3(playerClass.transform.position.x + x_offset, hitUp.point.y-Mathf.Min(250,Mathf.Max(5, offset.y+hitUp.distance/2)), -10), ref vel, 0.3f);
            //Camera.main.orthographicSize = Mathf.Min(250, Mathf.Max(25, offset.y + hitUp.distance / 2));
        }
        else if (hitDown)
        {
            //transform.position = Vector3.SmoothDamp(camPos, new Vector3(playerClass.transform.position.x + x_offset, hitDown.point.y+Mathf.Min(250,Mathf.Max(5,offset.y+hitDown.distance/2)), -10), ref vel, 0.3f);
            //Camera.main.orthographicSize = Mathf.Min(250, Mathf.Max(25,offset.y + hitDown.distance/2));
        }

		//	Showing the aproptiate background
        if (playerClass.GetComponent<player>().over == 1)
        {
            main_cam.cullingMask |= 1 << LayerMask.NameToLayer("Upper");
            main_cam.cullingMask &= ~(1 << LayerMask.NameToLayer("Lower"));
        }
        else
        {
            main_cam.cullingMask |= 1 << LayerMask.NameToLayer("Lower");
            main_cam.cullingMask &= ~( 1 << LayerMask.NameToLayer("Upper") );
        }
    }

	float[] FindTrackExtremeties(float screen_width){
		int track_index = 0;

		float max_y = -9999f;
		float min_y =  9999f;

		while (ground_track.GetPosition(track_index).x < playerClass.transform.position.x && track_index < ground_track.positionCount) {
			track_index += 10;
		}

		// Increment track_to until it's at end of visible screen
		while (ground_track.GetPosition(track_index).x < playerClass.transform.position.x + screen_width && track_index < ground_track.positionCount){

			// Testing if we have new min max
			max_y = Mathf.Max(max_y, ground_track.GetPosition (track_index).y);
			min_y = Mathf.Min(min_y, ground_track.GetPosition (track_index).y);

			// We don't need to test every position.
			track_index += 5;
		}

		// Return the min max from line segment.
		return new float[]{ min_y, max_y };

	}
}
