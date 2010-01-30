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
        #region Fields

        double _lat;
        double _lon;

        #endregion

        #region Constructors

        public VELatLong()
            : this(0, 0, 0, VEAltMode.FromGround)
        {
        }

        public VELatLong(double lat, double lon)
            : this(lat, lon, 0, VEAltMode.FromGround)
        {
        }

        public VELatLong(double lat, double lon, double alt)
            : this(lat, lon, alt, VEAltMode.FromGround)
        {
        }

        public VELatLong(double lat, double lon, double alt, VEAltMode altMode)
        {
            this._lat = lat;
            this._lon = lon;
            this.Altitude = alt;
            this.AltMode = altMode;
        }

        internal VELatLong(LatLonAlt lla)
            : this(lla.Latitude, lla.Longitude, lla.Latitude, VEAltMode.FromGround)
        {
        }

        #endregion

        #region Properties

        public double Latitude
        {
            get { return this._lat; }
            set { this._lat = value; }
        }

        public double Longitude
        {
            get { return this._lon; }
            set { this._lon = value; }
        }

        public double Altitude { get; set; }

        public VEAltMode AltMode { get; set; }

        #endregion

        #region Methods

        public override string ToString()
        {
            return this._lat + ", " + this._lon;
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

        #endregion
    }
}
