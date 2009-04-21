using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.MapPoint.Rendering3D.Utility.Types;
using Microsoft.MapPoint.Rendering3D.Utility;
using Microsoft.MapPoint.Rendering3D;
using System.Drawing;
using Microsoft.MapPoint.Geometry.VectorMath;

namespace InfoStrat.VE
{
    internal class RegisteredPosition : ILocationListener
    {
        public Vector3D Vector;
        public LatLonAlt Position;
        public Point Screen;
        public bool Valid = false;
        public bool InView = false;

        public LatLon Location
        {
            get { return Position.LatLon; }
        }

        public TileLevelOfDetail MaxLod
        {
            get { return TileLevelOfDetail.MaxLod; }
        }

        public TileLevelOfDetail MinLod
        {
            get { return TileLevelOfDetail.MinLod; }
        }

        public void LocationChanged(double groundElevation, double objectElevation)
        {
            LatLonAlt newLLA = Position;
            newLLA.Altitude += groundElevation;
            Vector = newLLA.GetVector();
            Valid = true;
        }

    }
}
