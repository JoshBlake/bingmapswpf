using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace InfoStrat.VE
{
    public class VECameraChangedEventArgs : RoutedEventArgs
    {
        public VELatLong NewLatLong;
        public VELatLong OldLatLong;

        public VERollPitchYaw NewRollPitchYaw;
        public VERollPitchYaw OldRollPitchYaw;

        public bool IsLatitudeChanged
        {
            get
            {
                return OldLatLong.Latitude != NewLatLong.Latitude;
            }
        }
        public bool IsLongitudeChanged
        {
            get
            {
                return OldLatLong.Longitude != NewLatLong.Longitude;
            }
        }

        public bool IsAltitudeChanged
        {
            get
            {
                return OldLatLong.Altitude != NewLatLong.Altitude;
            }
        }

        
        public bool IsRollChanged
        {
            get
            {
                return OldRollPitchYaw.Roll != NewRollPitchYaw.Roll;
            }
        }

        public bool IsPitchChanged
        {
            get
            {
                return OldRollPitchYaw.Pitch != NewRollPitchYaw.Pitch;
            }
        }

        public bool IsYawChanged
        {
            get
            {
                return OldRollPitchYaw.Yaw != NewRollPitchYaw.Yaw;
            }
        }

        
        public bool IsChanged
        {
            get
            {
                return IsLatitudeChanged ||
                       IsLongitudeChanged ||
                       IsAltitudeChanged ||
                       IsRollChanged ||
                       IsPitchChanged ||
                       IsYawChanged;
            }
        }


        public VECameraChangedEventArgs(VELatLong oldLatLong, VELatLong newLatLong, VERollPitchYaw oldRPY, VERollPitchYaw newRPY)
        {
            this.OldLatLong = oldLatLong;
            this.NewLatLong = newLatLong;
            this.OldRollPitchYaw = oldRPY;
            this.NewRollPitchYaw = newRPY;

            if (oldLatLong == null)
                throw new ArgumentNullException("oldLatLong");

            if (newLatLong == null)
                throw new ArgumentNullException("newLatLong");

            if (oldRPY == null)
                throw new ArgumentNullException("oldRPY");

            if (newRPY == null)
                throw new ArgumentNullException("newRPY");
        }
    }
}
