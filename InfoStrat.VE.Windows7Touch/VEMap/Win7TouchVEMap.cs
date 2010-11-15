using System;
using System.Windows.Input;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.ComponentModel;
using System.Collections.Generic;
using System.Windows.Interop;
using Windows7.Multitouch;
using Windows7.Multitouch.Manipulation;
using Windows7.Multitouch.WPF;
using InfoStrat.VE;
using InfoStrat.VE.Utilities;
using System.Diagnostics;

[assembly: CLSCompliant(true)]

namespace InfoStrat.VE.Windows7Touch
{
    public class Win7TouchVEMap : VEMap
    {
        #region Fields

        private bool _disposed;

        #endregion
        
        #region Touch

        private ManipulationInertiaProcessor _manipulationProcessor;

        #endregion

        #region UI Elements

        Viewbox _viewbox;
        Grid _hostGrid;
        Image _targetImage;
        D3DImage _d3DImage;
        Canvas _canvasPushPin;

        #endregion

        #region Constructors

        public Win7TouchVEMap()
        {
            //Are we in Visual Studio Designer?
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            } 
            
            Loaded += Win7TouchVeMapLoaded;
        }

        void Win7TouchVeMapLoaded(object sender, RoutedEventArgs e)
        {
            if (!Handler.DigitizerCapabilities.IsMultiTouchReady) return;

            //the current window is not available during construction, so the current window will haved to be obtained in another event, such as loaded
            Window parentWindow = Window.GetWindow(this);
            Factory.EnableStylusEvents(parentWindow);

            Stylus.SetIsPressAndHoldEnabled(this, true);
            Stylus.SetIsFlicksEnabled(this, false);
            Stylus.SetIsTapFeedbackEnabled(this, true);
            Stylus.SetIsTouchFeedbackEnabled(this, true);

            _manipulationProcessor = new ManipulationInertiaProcessor(ProcessorManipulations.ALL, Factory.CreateTimer());
            _manipulationProcessor.BeforeInertia += ManipulationProcessorBeforeInertia;
            _manipulationProcessor.ManipulationDelta += ManipulationProcessorManipulationDelta;
        }

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
            DependencyProperty.Register("ZoomSensitivity", typeof(double), typeof(Win7TouchVEMap), new UIPropertyMetadata(6.0));

        #endregion

        #region PanSensitivity DP

        public double PanSensitivity
        {
            get { return (double)GetValue(PanSensitivityProperty); }
            set { SetValue(PanSensitivityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PanSensitivity.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PanSensitivityProperty =
            DependencyProperty.Register("PanSensitivity", typeof(double), typeof(Win7TouchVEMap), new UIPropertyMetadata(1.0));

        #endregion

        #region TiltSensitivity DP

        public double TiltSensitivity
        {
            get { return (double)GetValue(TiltSensitivityProperty); }
            set { SetValue(TiltSensitivityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PanSensitivity.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TiltSensitivityProperty =
            DependencyProperty.Register("TiltSensitivity", typeof(double), typeof(Win7TouchVEMap), new UIPropertyMetadata(0.5));

        #endregion
        
        #endregion

        #region Methods

        #region Overridden Methods

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            try
            {
                _viewbox = (Viewbox)Template.FindName("PART_viewbox", this);
                _hostGrid = (Grid)Template.FindName("PART_hostGrid", this);
                _targetImage = (Image)Template.FindName("PART_targetImage", this);
                _d3DImage = (D3DImage)Template.FindName("PART_d3dImage", this);
                _canvasPushPin = (Canvas)Template.FindName("PART_canvasPushPin", this);

                _targetImage.AddHandler(PreviewStylusDownEvent, new StylusDownEventHandler(TargetImagePreviewContactDown));
                _targetImage.AddHandler(PreviewStylusMoveEvent, new StylusEventHandler(TargetImagePreviewContactChanged));
                _targetImage.AddHandler(PreviewStylusUpEvent, new StylusEventHandler(TargetImagePreviewContactUp));
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to apply map control template", ex);
            }

            //Are we in Visual Studio Designer?
            if (!DesignerProperties.GetIsInDesignMode(this)) return;
            //then set a sample image and do anything else
            TextBlock tb = new TextBlock
            {
                Text = "VE",
                FontSize = 24,
                Background = new SolidColorBrush(Colors.Green)
            };

            _hostGrid.Background = new VisualBrush(tb);
            return;
        }

        #endregion
        #region Mouse Input Overrides

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (e.StylusDevice != null) return;
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            if (e.StylusDevice != null) return;
            base.OnMouseUp(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.StylusDevice != null) return;
            base.OnMouseMove(e);
        }

        #endregion

        #region Stylus Events

        private void TargetImagePreviewContactDown(object sender, StylusDownEventArgs e)
        {
            e.StylusDevice.Capture(_targetImage, CaptureMode.SubTree);
            _manipulationProcessor.ProcessDown((uint)e.StylusDevice.Id, e.GetPosition(_targetImage).ToDrawingPointF());
            e.Handled = true;
        }

        private void TargetImagePreviewContactChanged(object sender, StylusEventArgs e)
        {
            Point position = e.GetPosition(_targetImage);
            e.Handled = true;

            if (position.X < 0 ||
                position.Y < 0 ||
                position.X > _targetImage.Width ||
                position.Y > _targetImage.Height)
            {
                e.StylusDevice.Capture(_targetImage, CaptureMode.None);
                _manipulationProcessor.ProcessUp((uint)e.StylusDevice.Id, e.GetPosition(_targetImage).ToDrawingPointF());
            }
            else
            {
                e.StylusDevice.Capture(_targetImage, CaptureMode.SubTree);
                _manipulationProcessor.ProcessMove((uint)e.StylusDevice.Id, e.GetPosition(_targetImage).ToDrawingPointF());
                
            }
        }

        private void TargetImagePreviewContactUp(object sender, StylusEventArgs e)
        {
            e.Handled = true;

            e.StylusDevice.Capture(_targetImage, CaptureMode.None);
            _manipulationProcessor.ProcessUp((uint)e.StylusDevice.Id, e.GetPosition(_targetImage).ToDrawingPointF());
        }

        #endregion

        #region Manipulation Processor Events

        void ManipulationProcessorBeforeInertia(object sender, BeforeInertiaEventArgs e)
        {
            //change the inertia steps and/or use a timespan with the inertiaprocessor for different behaviors/sensitivity
            _manipulationProcessor.InertiaProcessor.MaxInertiaSteps = 500;
            
            //panning
            _manipulationProcessor.InertiaProcessor.DesiredDeceleration = 0.002f;
            _manipulationProcessor.InertiaProcessor.InitialVelocity = _manipulationProcessor.Velocity;
            
            //Expansion (zoom)
            _manipulationProcessor.InertiaProcessor.InitialExpansionVelocity = _manipulationProcessor.ExpansionVelocity;
            _manipulationProcessor.InertiaProcessor.DesiredExpansionDeceleration = 0.0005f;

            //rotation
            _manipulationProcessor.InertiaProcessor.InitialAngularVelocity = _manipulationProcessor.AngularVelocity;
            _manipulationProcessor.InertiaProcessor.DesiredAngularDeceleration = 0.0002f;

        }

        void ManipulationProcessorManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            if (MapManipulationMode == MapManipulationMode.PanZoomPivot)
            {
                if (IsPanEnabled)
                {
                    DoMapMove(e.TranslationDelta.Width * PanSensitivity, e.TranslationDelta.Height * PanSensitivity, e.Location.ToDrawingPointF());
                }
            }
            else
            {
                if (IsSpinEnabled)
                {
                    //This rotation mapping is different than the other version
                    double deltaAngle = Utilities.MathHelper.MapValue(e.TranslationDelta.Width, -20, 20, -1, 1);
                    DoMapYaw(deltaAngle);
                }
                if (IsTiltEnabled)
                {
                    double desiredPitch = ClampPitch(this.Pitch + e.TranslationDelta.Height * TiltSensitivity, this.Altitude);
                    DoMapTilt(desiredPitch);
                }
            }

            if (IsZoomEnabled)
            {
                DoMapZoom(e.ExpansionDelta * ZoomSensitivity, e.Location.ToDrawingPointF());
            }

            if (IsPivotEnabled)
            {
                DoMapPivot(e.RotationDelta * 180.0 / Math.PI, e.Location.ToDrawingPointF());
            }

        }
        
        private double ClampPitch(double pitch, double altitude)
        {
            double maxAngle = Utilities.MathHelper.MapValue(altitude, 3000, 7500000, -5, -80);
            maxAngle = Utilities.MathHelper.Clamp(maxAngle, -80, -5);
            double minAngle = -90;

            return Math.Max(minAngle, Math.Min(maxAngle, pitch));
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                if (_manipulationProcessor != null)
                {
                    _manipulationProcessor.Dispose();
                }
            }
            _disposed = true;
        }

        #endregion
    }
}