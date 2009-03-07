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
        Affine2DInertiaProcessor inertiaProcessorMove;
        Affine2DInertiaProcessor inertiaProcessorZoom;

        #endregion
        
        #region ZoomSensitivity DP
        
        public double ZoomSensitivity
        {
            get { return (double)GetValue(ZoomSensitivityProperty); }
            set { SetValue(ZoomSensitivityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ZoomSensitivity.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ZoomSensitivityProperty =
            DependencyProperty.Register("ZoomSensitivity", typeof(double), typeof(SurfaceVEMap), new UIPropertyMetadata(15.0));
        
        #endregion

        #region PanSensitivity DP
        
        public double PanSensitivity
        {
            get { return (double)GetValue(PanSensitivityProperty); }
            set { SetValue(PanSensitivityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PanSensitivity.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PanSensitivityProperty =
            DependencyProperty.Register("PanSensitivity", typeof(double), typeof(SurfaceVEMap), new UIPropertyMetadata(3.5));
        
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
                Affine2DManipulations.Scale,
                this, false);

            this.manipulationProcessor.Affine2DManipulationCompleted += new EventHandler<Affine2DOperationCompletedEventArgs>(manipulationProcessor_Affine2DManipulationCompleted);
            this.manipulationProcessor.Affine2DManipulationDelta += new EventHandler<Affine2DOperationDeltaEventArgs>(manipulationProcessor_Affine2DManipulationDelta);
            this.manipulationProcessor.Affine2DManipulationStarted += new EventHandler<Affine2DOperationStartedEventArgs>(manipulationProcessor_Affine2DManipulationStarted);

            this.inertiaProcessorMove = new Affine2DInertiaProcessor();

            this.inertiaProcessorMove.Affine2DInertiaDelta += new EventHandler<Affine2DOperationDeltaEventArgs>(inertiaProcessorMove_Affine2DInertiaDelta);
            this.inertiaProcessorMove.Affine2DInertiaCompleted += new EventHandler<Affine2DOperationCompletedEventArgs>(inertiaProcessorMove_Affine2DInertiaCompleted);

            this.inertiaProcessorZoom = new Affine2DInertiaProcessor();

            this.inertiaProcessorZoom.Affine2DInertiaDelta += new EventHandler<Affine2DOperationDeltaEventArgs>(inertiaProcessorZoom_Affine2DInertiaDelta);
            this.inertiaProcessorZoom.Affine2DInertiaCompleted += new EventHandler<Affine2DOperationCompletedEventArgs>(inertiaProcessorZoom_Affine2DInertiaCompleted);
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
            DoMapZoom(0, true);
        }

        void inertiaProcessorZoom_Affine2DInertiaDelta(object sender, Affine2DOperationDeltaEventArgs e)
        {
            DoMapZoom(e.Velocity.X, true);
        }

        void inertiaProcessorMove_Affine2DInertiaCompleted(object sender, Affine2DOperationCompletedEventArgs e)
        {
            DoMapMove(0, 0, true);
        }

        void inertiaProcessorMove_Affine2DInertiaDelta(object sender, Affine2DOperationDeltaEventArgs e)
        {
            DoMapMove(e.Velocity.X,
                      e.Velocity.Y, 
                      true);
        }

        #endregion

        #region Manipulation Processor Event Handlers

        void manipulationProcessor_Affine2DManipulationStarted(object sender, Affine2DOperationStartedEventArgs e)
        {
            inertiaProcessorMove.End();

            inertiaProcessorZoom.End();

            DoMapZoom(0, false);
            DoMapMove(0, 0, false);
        }

        void manipulationProcessor_Affine2DManipulationDelta(object sender, Affine2DOperationDeltaEventArgs e)
        {
            DoMapMove(e.Velocity.X * this.PanSensitivity, e.Velocity.Y * this.PanSensitivity, true);

            if (downContacts.Count >= 2)
                DoMapZoom(e.ExpansionVelocity * this.ZoomSensitivity, true);
            else
                DoMapZoom(lastExpansionDelta, true);
        }

        void manipulationProcessor_Affine2DManipulationCompleted(object sender, Affine2DOperationCompletedEventArgs e)
        {
            inertiaProcessorMove.InitialOrigin = new Point(0, 0);
            inertiaProcessorMove.InitialVelocity = e.Velocity * this.PanSensitivity;
            inertiaProcessorMove.DesiredDeceleration = 0.002;

            inertiaProcessorMove.Begin();

            inertiaProcessorZoom.InitialOrigin = new Point(0, 0);
            inertiaProcessorZoom.InitialVelocity = new Vector(lastExpansionDelta, 0);
            inertiaProcessorZoom.DesiredDeceleration = 0.0005;

            inertiaProcessorZoom.Begin();
        }

        #endregion

        #region Public Map Movement Helpers

        public override void  DoMapZoom(double zoom, bool isContinuous)
        {
            lastExpansionDelta = zoom;
 	        base.DoMapZoom(zoom, isContinuous);
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
