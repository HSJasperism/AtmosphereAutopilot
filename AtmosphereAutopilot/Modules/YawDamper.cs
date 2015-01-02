﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace AtmosphereAutopilot
{
    /// <summary>
    /// Simple yaw damper on PID
    /// </summary>
    class YawDamper: PIDAngularVelDampener
    {
        public YawDamper(Vessel cur_vessel)
            : base(cur_vessel, "Yaw dampener", 752348) 
        {
            if (loadFromPreset("YawDamper"))
                return;
            pid.KP = 5.0;
            pid.KI = 0.0;
            pid.AccumulatorClamp = 1.0;
            pid.AccumulDerivClamp = 0.25;
            pid.KD = 0.4;
            pid.IntegralClamp = 0.1;
        }

        public override void serialize()
        {
            saveToFile("YawDamper");
        }

        double time = 0.0;

        protected override void OnFixedUpdate(FlightCtrlState cntrl)
        {
            angular_velocity = -currentVessel.angularVelocity.z;
            time = time + TimeWarp.fixedDeltaTime;
            
            // check if user is inputing control
            if (cntrl.killRot)                          // when sas works just back off
                return;
            if (cntrl.yaw == cntrl.yawTrim)             // when user doesn't use control, yaw is on the same level as trim
            {
                output = pid.Control(angular_velocity, 0.0, time);          // get output from controller
                cntrl.yaw = (float)Common.Clamp(output, 1.0);
            }
            else
            {
                pid.clear();
                output = 0.0;
            }
        }
    }
}
