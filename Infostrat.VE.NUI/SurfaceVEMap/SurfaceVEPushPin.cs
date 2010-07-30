using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Surface.Presentation;
using Microsoft.Surface.Presentation.Controls;
using InfoStrat.VE.NUI.Utilities;
using System.Windows.Media.Animation;
using InfoStrat.VE;

namespace InfoStrat.VE.NUI
{
    public class SurfaceVEPushPin : VEPushPin
    {
        
        #region Constructors

        static SurfaceVEPushPin()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SurfaceVEPushPin), new FrameworkPropertyMetadata(typeof(SurfaceVEPushPin)));
        }
        
        #endregion
                
    }
}
