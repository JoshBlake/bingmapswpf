using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InfoStrat.VE;
using System.Windows;
using System.Windows.Media;

namespace DataBindingSample
{
    public class CustomPushPin : VEPushPin
    {
        #region Constructors

        static CustomPushPin()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomPushPin), new FrameworkPropertyMetadata(typeof(CustomPushPin)));
        }

        public CustomPushPin()
        {
        }

        #endregion

        protected override Point GetAnchorOffset()
        {
            return new Point(this.ActualWidth / 2, 0);
        }
    }
}
