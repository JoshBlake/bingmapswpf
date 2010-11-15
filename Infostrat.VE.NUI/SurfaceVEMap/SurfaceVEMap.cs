using System;
using System.IO;
using System.Xml;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.ComponentModel;
using Microsoft.Surface.Presentation;
using Microsoft.Surface.Presentation.Controls;
using Microsoft.Surface.Presentation.Manipulations;
using System.Reflection;
using System.Collections.Generic;
using System.Windows.Interop;
using InfoStrat.VE;
using System.Diagnostics;

[assembly: CLSCompliant(true)]
namespace InfoStrat.VE.NUI
{
    public class SurfaceVEMap : VEMap
    {
        #region UIElements

        Viewbox viewbox;
        Grid hostGrid;
        Image targetImage;
        D3DImage d3dImage;
        Canvas canvasPushPin;

        #endregion

        #region Class Members

        List<Contact> downContacts;
        double lastExpansionDelta;

        Affine2DManipulationProcessor manipulationProcessor;
        Affine2DInertiaProcessor inertiaProcessor;

        #endregion

        #region Properties

        #region ZoomSensitivity DP

        public double ZoomSensitivity
        {
            get { return (double)GetValue(ZoomSensitivityProperty); }
            set { SetValue(ZoomSensitivityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ZoomSensitivity.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ZoomSensitivityProperty =
            DependencyProperty.Register("ZoomSensitivity", typeof(double), typeof(SurfaceVEMap), new UIPropertyMetadata(6.0));

        #endregion

        #region PanSensitivity DP

        public double PanSensitivity
        {
            get { return (double)GetValue(PanSensitivityProperty); }
            set { SetValue(PanSensitivityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PanSensitivity.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PanSensitivityProperty =
            DependencyProperty.Register("PanSensitivity", typeof(double), typeof(SurfaceVEMap), new UIPropertyMetadata(1.0));

        #endregion

        #region TiltSensitivity DP

        public double TiltSensitivity
        {
            get { return (double)GetValue(TiltSensitivityProperty); }
            set { SetValue(TiltSensitivityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PanSensitivity.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TiltSensitivityProperty =
            DependencyProperty.Register("TiltSensitivity", typeof(double), typeof(SurfaceVEMap), new UIPropertyMetadata(0.5));

        #endregion

        #endregion

        #region Constructors

        static SurfaceVEMap()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SurfaceVEMap), new FrameworkPropertyMetadata(typeof(SurfaceVEMap)));
        }

        public SurfaceVEMap()
            : base()
        {
            lastExpansionDelta = 0;

            downContacts = new List<Contact>();

            this.manipulationProcessor = new Affine2DManipulationProcessor(
                Affine2DManipulations.TranslateX |
                Affine2DManipulations.TranslateY |
                Affine2DManipulations.Scale |
                Affine2DManipulations.Rotate,
                this, false);

            this.manipulationProcessor.Affine2DManipulationCompleted += new EventHandler<Affine2DOperationCompletedEventArgs>(manipulationProcessor_Affine2DManipulationCompleted);
            this.manipulationProcessor.Affine2DManipulationDelta += new EventHandler<Affine2DOperationDeltaEventArgs>(manipulationProcessor_Affine2DManipulationDelta);
            this.manipulationProcessor.Affine2DManipulationStarted += new EventHandler<Affine2DOperationStartedEventArgs>(manipulationProcessor_Affine2DManipulationStarted);

            this.inertiaProcessor = new Affine2DInertiaProcessor();

            this.inertiaProcessor.Affine2DInertiaDelta += new EventHandler<Affine2DOperationDeltaEventArgs>(inertiaProcessor_Affine2DInertiaDelta);
            this.inertiaProcessor.Affine2DInertiaCompleted += new EventHandler<Affine2DOperationCompletedEventArgs>(inertiaProcessor_Affine2DInertiaCompleted);

        }

        #endregion

        #region Overridden Methods

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            try
            {
                viewbox = (Viewbox)this.Template.FindName("PART_viewbox", this);
                hostGrid = (Grid)this.Template.FindName("PART_hostGrid", this);
                targetImage = (Image)this.Template.FindName("PART_targetImage", this);
                d3dImage = (D3DImage)this.Template.FindName("PART_d3dImage", this);
                canvasPushPin = (Canvas)this.Template.FindName("PART_canvasPushPin", this);

                targetImage.AddHandler(Contacts.PreviewContactDownEvent, new ContactEventHandler(targetImage_PreviewContactDown));
                targetImage.AddHandler(Contacts.PreviewContactChangedEvent, new ContactEventHandler(targetImage_PreviewContactChanged));
                targetImage.AddHandler(Contacts.PreviewContactUpEvent, new ContactEventHandler(targetImage_PreviewContactUp));

            }
            catch (Exception)
            {
            }

            //Are we in Visual Studio Designer?
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {
                //then set a sample image and do anything else
                TextBlock tb = new TextBlock();
                tb.Text = "VE";
                tb.FontSize = 24;
                tb.Background = new SolidColorBrush(Colors.Green);

                this.hostGrid.Background = new VisualBrush(tb);
                return;
            }
        }

        #endregion

        #region Inertia Processor Event Handlers

        void inertiaProcessorZoom_Affine2DInertiaCompleted(object sender, Affine2DOperationCompletedEventArgs e)
        {
        }

        void inertiaProcessorZoom_Affine2DInertiaDelta(object sender, Affine2DOperationDeltaEventArgs e)
        {
        }

        void inertiaProcessor_Affine2DInertiaCompleted(object sender, Affine2DOperationCompletedEventArgs e)
        {
        }

        void inertiaProcessor_Affine2DInertiaDelta(object sender, Affine2DOperationDeltaEventArgs e)
        {
            ProcessManipulationInertiaDelta(e);
        }

        #endregion

        #region Manipulation Processor Event Handlers

        void manipulationProcessor_Affine2DManipulationStarted(object sender, Affine2DOperationStartedEventArgs e)
        {
            inertiaProcessor.End();
        }

        void manipulationProcessor_Affine2DManipulationDelta(object sender, Affine2DOperationDeltaEventArgs e)
        {
            ProcessManipulationInertiaDelta(e);
        }

        private void ProcessManipulationInertiaDelta(Affine2DOperationDeltaEventArgs e)
        {
            if (MapManipulationMode == MapManipulationMode.PanZoomPivot)
            {
                if (IsPanEnabled)
                {
                    DoMapMove(e.Delta.X * PanSensitivity, e.Delta.Y * PanSensitivity);
                }
            }
            else
            {
                if (IsSpinEnabled)
                {
                    double deltaAngle = InfoStrat.VE.Utilities.MathHelper.MapValue(e.Delta.X, -20, 20, -1, 1);
                    DoMapYaw(deltaAngle);

                }
                if (IsTiltEnabled)
                {
                    double desiredPitch = ClampPitch(this.Pitch + e.Delta.Y * TiltSensitivity, this.Altitude);
                    DoMapTilt(desiredPitch);
                }
            }
            
            if (IsZoomEnabled)
            {
                DoMapZoom(e.ExpansionDelta * ZoomSensitivity, e.ManipulationOrigin);
            }

            if (IsPivotEnabled)
            {

                DoMapPivot(e.RotationDelta, e.ManipulationOrigin);
            }

        }

        private double ClampPitch(double pitch, double altitude)
        {
            double maxAngle = InfoStrat.VE.Utilities.MathHelper.MapValue(altitude, 3000, 7500000, -5, -80);
            maxAngle = InfoStrat.VE.Utilities.MathHelper.Clamp(maxAngle, -80, -5);
            double minAngle = -90;

            return Math.Max(minAngle, Math.Min(maxAngle, pitch));
        }

        void manipulationProcessor_Affine2DManipulationCompleted(object sender, Affine2DOperationCompletedEventArgs e)
        {
            inertiaProcessor.InitialOrigin = e.ManipulationOrigin;

            inertiaProcessor.InitialVelocity = e.Velocity;
            inertiaProcessor.InitialExpansionVelocity = e.ExpansionVelocity;
            inertiaProcessor.InitialAngularVelocity = e.AngularVelocity;

            inertiaProcessor.DesiredDeceleration = 0.002;
            inertiaProcessor.DesiredAngularDeceleration = 0.0005f;
            inertiaProcessor.DesiredExpansionDeceleration = 0.0002f;

            inertiaProcessor.Begin();

        }

        #endregion

        #region UI Event Handlers

        private void targetImage_PreviewContactDown(object sender, ContactEventArgs e)
        {
            if (!e.Contact.IsFingerRecognized)
                return;

            e.Contact.Capture(targetImage, System.Windows.Input.CaptureMode.SubTree);
            manipulationProcessor.BeginTrack(e.Contact);
            e.Handled = true;

            downContacts.Add(e.Contact);
        }

        private void targetImage_PreviewContactChanged(object sender, ContactEventArgs e)
        {
            if (!e.Contact.IsFingerRecognized)
                return;

            Point position = e.Contact.GetPosition(targetImage);

            if (position.X < 0 ||
                position.Y < 0 ||
                position.X > targetImage.Width ||
                position.Y > targetImage.Height)
            {
                e.Contact.Capture(targetImage, System.Windows.Input.CaptureMode.None);

                if (downContacts.Contains(e.Contact))
                    downContacts.Remove(e.Contact);
            }
            else
            {
                e.Contact.Capture(targetImage, System.Windows.Input.CaptureMode.SubTree);
                manipulationProcessor.BeginTrack(e.Contact);
                e.Handled = true;

                if (!downContacts.Contains(e.Contact))
                    downContacts.Add(e.Contact);
            }
        }

        private void targetImage_PreviewContactUp(object sender, ContactEventArgs e)
        {
            if (!e.Contact.IsFingerRecognized)
                return;

            e.Contact.Capture(targetImage, System.Windows.Input.CaptureMode.None);

            downContacts.Remove(e.Contact);
        }


        #endregion

    }
}
