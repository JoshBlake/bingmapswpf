using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.MapPoint.Geometry.VectorMath;

namespace InfoStrat.VE
{
    public class VERollPitchYaw
    {
        double roll;
        double pitch;
        double yaw;

        public double Roll
        {
            get { return roll; }
            set { roll = value; }
        }

        public double Pitch
        {
            get { return pitch; }
            set { pitch = value; }
        }

        public double Yaw
        {
            get { return yaw; }
            set { yaw = value; }
        }

        public VERollPitchYaw()
        {
            roll = 0;
            pitch = 0;
            yaw = 0;
        }

        public VERollPitchYaw(double roll, double pitch, double yaw)
        {
            this.roll = roll;
            this.pitch = pitch;
            this.yaw = yaw;
        }

        internal VERollPitchYaw(RollPitchYaw rpy)
        {
            this.roll = rpy.Roll * 180.0 / Math.PI;
            this.pitch = rpy.Pitch * 180.0 / Math.PI;
            this.yaw = rpy.Yaw * 180.0 / Math.PI;
        }

        public override string ToString()
        {
            return this.roll + ", " + this.pitch + ", " + this.yaw;
        }

        internal RollPitchYaw ToRollPitchYaw()
        {
            return new RollPitchYaw(this.roll * Math.PI / 180.0,
                                    this.pitch * Math.PI / 180.0,
                                    this.yaw * Math.PI / 180.0);
        }
    }
}
