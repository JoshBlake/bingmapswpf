using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.Animation;
using Microsoft.Surface;
using Microsoft.Surface.Presentation;
using Microsoft.Surface.Presentation.Controls;
using Microsoft.Surface.Presentation.Manipulations;
using System.Windows.Threading;
using InfoStrat.VE.Utilities;

namespace InfoStrat.VE.NUI.Utilities
{
    public class SurfaceAnimateUtility : AnimateUtility
    {
        #region Public Static Methods

        public static void ThrowSVI(ScatterViewItem svi, Point targetPoint, double targetOrientation, double fromTime, double toTime)
        {

            SurfaceAnimateUtility.AnimateElementPoint(svi, ScatterViewItem.CenterProperty,
                                                targetPoint, fromTime, toTime);
            SurfaceAnimateUtility.AnimateElementDouble(svi, ScatterViewItem.OrientationProperty,
                                                targetOrientation, fromTime, toTime);
        }

        #endregion

        protected SurfaceAnimateUtility()
        {
        }
    }
}
