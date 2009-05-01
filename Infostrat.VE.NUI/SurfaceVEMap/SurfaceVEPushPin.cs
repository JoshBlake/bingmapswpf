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
        #region Members

        private SurfaceButton SurfButton;

        #endregion

        #region Events

        public new event EventHandler<SurfaceVEPushPinClickedEventArgs> Click;

        #endregion

        #region Constructors

        static SurfaceVEPushPin()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SurfaceVEPushPin), new FrameworkPropertyMetadata(typeof(SurfaceVEPushPin)));
        }

        public SurfaceVEPushPin()
            : base()
        {            
        }

        public SurfaceVEPushPin(VELatLong latLong)
            : base(latLong)
        {           
        }

        public SurfaceVEPushPin(VELatLong latLong, double minAltitude, double maxAltitude)
            : base(latLong, minAltitude, maxAltitude)
        {
        }

        #endregion

        #region Overridden Methods

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            SurfButton = (SurfaceButton)this.Template.FindName("PART_button", this);
            
            if (SurfButton != null)
            {
                SurfButton.ContactTapGesture += new ContactEventHandler(Button_ContactTapGesture);
            }
        }
        
        #endregion

        #region Virtual Methods

        protected override void Initialize()
        {
            base.Initialize();

            this.SurfButton = null;
        }

        private void OnClick(object sender, SurfaceVEPushPinClickedEventArgs e)
        {
            if (Click != null)
            {
                Click(this, e);
            }
        }

        #endregion

        #region Private Methods

        private void Button_ContactTapGesture(object sender, ContactEventArgs e)
        {
            if (Click != null)
            {
                SurfaceVEPushPinClickedEventArgs args = new SurfaceVEPushPinClickedEventArgs(e);

                Point? position = this.GetPosition(LatLong);
                if (position != null)
                {
                    args.PushPinScreenPosition = this.Map.PointToScreen(new Point(position.Value.X, position.Value.Y));
                }
                else
                {
                    args.PushPinScreenPosition = new Point(0, 0);
                }

                OnClick(this, args);
            }
        }
        

        #endregion

    }
}
