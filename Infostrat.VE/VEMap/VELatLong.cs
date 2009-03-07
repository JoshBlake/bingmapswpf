using System;
using Microsoft.MapPoint.Rendering3D;
using Microsoft.MapPoint.Geometry.VectorMath;

namespace InfoStrat.VE
{
    public enum VEAltMode
    {
        FromGround = 0,
        FromDatum = 1,
    };

    public class VELatLong
    {
        double lat;
        double lon;
        double alt;
        VEAltMode altMode;

        public VELatLong()
        {
            this.lat = 0;
            this.lon = 0;
            this.alt = 0;
            this.altMode = VEAltMode.FromGround;
        }

        public VELatLong(double lat, double lon)
        {
            this.lat = lat;
            this.lon = lon;
            this.alt = 0;
            this.altMode = VEAltMode.FromGround;
        }

        public VELatLong(double lat, double lon, double alt)
        {
            this.lat = lat;
            this.lon = lon;
            this.alt = alt;
            this.altMode = VEAltMode.FromGround;
        }

        public VELatLong(double lat, double lon, double alt, VEAltMode altMode)
        {
            this.lat = lat;
            this.lon = lon;
            this.alt = alt;
            this.altMode = altMode;
        }

        internal VELatLong(LatLonAlt lla)
        {
            this.lat = lla.Latitude;
            this.lon = lla.Longitude;
            this.alt = lla.Altitude;
            this.altMode = VEAltMode.FromGround;
        }

        public double Latitude
        {
            get { return this.lat; }
            set { value = this.lat; }
        }

        public double Longitude
        {
            get { return this.lon; }
            set { value = this.lon; }
        }

        public double Altitude
        {
            get { return this.alt; }
            set { value = this.alt; }
        }

        public VEAltMode AltMode
        {
            get { return this.altMode; }
            set { value = this.altMode; }
        }

        public override string ToString()
        {
            return this.lat + ", " + this.lon;
        }

        internal LatLonAlt ToLatLonAlt()
        {
            return new LatLonAlt(Latitude * Math.PI / 180.0, Longitude * Math.PI / 180.0, Altitude);
        }

        public Vector3D ToVector()
        {            
            return ToLatLonAlt().GetVector();
        }

        public static double GreatCircleDistance(VELatLong a, VELatLong b)
        {
            return LatLonAlt.GreatCircleDistance(a.ToLatLonAlt(), b.ToLatLonAlt());
        }
    }
}
