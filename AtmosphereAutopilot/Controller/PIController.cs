﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace AtmosphereAutopilot
{

    /// <summary>
    /// PI-based input value controller
    /// </summary>
    public class PIController
    {
        /// <summary>
        /// Proportional gain coefficient
        /// </summary>
        public double KP { get { return kp; } set { kp = value; } }
        protected double kp = 1.0;

        /// <summary>
        /// Integral gain coefficient
        /// </summary>
        public double KI { get { return ki; } set { ki = value; } }
        protected double ki = 1.0;

        /// <summary>
        /// Maximum error, wich lets integral component to raise
        /// </summary>
        public double IntegralClamp { get { return iclamp; } set { iclamp = value; } }
        protected double iclamp = 1.0;

        /// <summary>
        /// Maximum accumulator derivative
        /// </summary>
        public double AccumulDerivClamp { get { return adclamp; } set { adclamp = value; } }
        protected double adclamp = 1.0;

        /// <summary>
        /// Maximum magnitude for integral component of controller reaction
        /// </summary>
        public double AccumulatorClamp { get { return aclamp; } set { aclamp = value; } }
        protected double aclamp = 0.1;

        /// <summary>
        /// Accumulator gain, is multiplied on error * dt to get accumulator change
        /// </summary>
        public double IntegralGain { get { return igain; } set { igain = value; } }
        protected double igain = 1.0;

		/// <summary>
		/// Current accumulator value
		/// </summary>
        public double Accumulator { get { return i_accumulator; } set { i_accumulator = value; } }

        // Main step variables
        protected double i_accumulator = 0.0;               // current integral accumulator state
        protected double last_dt = 1.0;                     // last delta time

		/// <summary>
		/// Last error value. Error = desire - input
		/// </summary>
		public double LastError { get { return last_error; } }
		protected double last_error = 0.0;

        public double Control(double input, double desire, double dt)
        {
            double error = desire - input;
            double new_dt = dt;

            // proportional component
            double proportional = error * kp;

            // diffirential component
            if (!dt_is_constant(new_dt))
            {
                // dt has changed
                new_dt = TimeWarp.fixedDeltaTime;
                last_error = error;
            }

            // integral component             
            if (ki != 0.0)
            {
                double d_integral = Math.Abs(error) > iclamp ? 0.0 : new_dt * 0.5 * (error + last_error);       // raw diffirential
                d_integral = Common.Clamp(igain * d_integral, adclamp * new_dt);                                // clamp it
                i_accumulator = Common.Clamp(i_accumulator + d_integral, aclamp);                               // accumulate
            }
            double integral = i_accumulator * ki;

            // update previous values
            last_dt = new_dt;
            last_error = error;

            return proportional + integral;
        }

		/// <summary>
		/// Clear accumulator
		/// </summary>
        public void clear()
        {
            i_accumulator = 0.0;
        }

        protected bool dt_is_constant(double new_dt)            // return true if new dt is roughly the same as old one
        {
            if (Math.Abs(new_dt / last_dt - 1.0) < 0.1)
                return true;
            else
                return false;
        }

    }
}