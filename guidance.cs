using System;
using UnityEngine;


namespace Guidance {

    public static class Pure_pursuit
    {
        public static float[] Vec2_orientation(float x_pos, float y_pos,
                                                float x_tar, float y_tar,
                                                float kappa)
        {
            /* Simple PP vel. calculation */
            float mag = (float)System.Math.Sqrt(System.Math.Pow(y_tar - y_pos, 2) + System.Math.Pow(x_tar - x_pos, 2)) +0.00001f;
            return new float[] { (x_tar - x_pos) / mag, (y_tar - y_pos) / mag };
        }

        public static float[] Vec3_orientation(float[] pos, float[] target, float kappa)
        {
            float mag = (float)System.Math.Sqrt(System.Math.Pow(target[0] - pos[0], 2) + System.Math.Pow(target[1] - pos[1], 2) + Math.Pow(target[2] - pos[2], 2));
            return new float[] { kappa * (target[0] - pos[0]) / mag, kappa * (target[1] - pos[1]) / mag, kappa * (target[2] - pos[2]) / mag };
        }
    }

	public static class Constant_bearing{

		public static Vector2 Vec2_vel(float[] pos, float[] target, float[] target_vel, float flight_time, Vector2 grav){
			/* This is a modified CB
		 	 * 	It returns an instantanious calculation of desired velocity (x, and y), 
		 	 *   	which cause interception after [flight_time] seconds
		 	 *	
		 	 *	Input:
		 	 *		float[] pos - the position of the object wanting CB
		 	 *		float[] target = position of the target
		 	 *		float[] target_vel - the target's velocity
		 	 *		float 	flight_time - the time to impact (from "now")
		 	 *
		 	 * 	Output:
		 	 * 		float[] {vel_d_x, vel_d_y} - The velocity neccessary to intercept the target after flight_time seconds
		 	 */

			// Get the distance from obj to target
			Vector2 to_player = new Vector2 (target [0] - pos [0], target [1] - pos [1]);

			// Calculate the point of intection
			Vector2 dist_intersect = to_player + new Vector2 (target_vel [0] * flight_time, target_vel [1] * flight_time) + grav*(float)System.Math.Pow (flight_time,2);

			// The velocity needed would be distance over flight_time
			return new Vector2(dist_intersect[0]/flight_time, dist_intersect[1]/flight_time);
			}
	}
}
