using System;
using System.Windows.Input;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.ComponentModel;
using System.Collections.Generic;
using System.Windows.Interop;
using InfoStrat.VE;
using InfoStrat.VE.Utilities;
using System.Diagnostics;

[assembly: CLSCompliant(true)]

namespace InfoStrat.VE.WPFTouch
{
    public class WPFTouchVEMap : VEMap
    {
        #region Fields

        #region UI Elements

        Viewbox _viewbox;
        Grid _hostGrid;
        Image _targetImage;
        D3DImage _d3DImage;
        Canvas _canvasPushPin;

        #endregion
       
        #endregion
        
        #region Constructors

        public WPFTouchVEMap()
        {
            //Are we in Visual Studio Designer?
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }
            IsManipulationEnabled = true;
            Loaded += WPFTouchVEMap_Loaded;
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
            DependencyProperty.Register("ZoomSensitivity", typeof(double), typeof(WPFTouchVEMap), new UIPropertyMetadata(6.0));

        #endregion

        #region PanSensitivity DP

        public double PanSensitivity
        {
            get { return (double)GetValue(PanSensitivityProperty); }
            set { SetValue(PanSensitivityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PanSensitivity.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PanSensitivityProperty =
            DependencyProperty.Register("PanSensitivity", typeof(double), typeof(WPFTouchVEMap), new UIPropertyMetadata(1.0));

        #endregion

        #region TiltSensitivity DP

        public double TiltSensitivity
        {
            get { return (double)GetValue(TiltSensitivityProperty); }
            set { SetValue(TiltSensitivityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PanSensitivity.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TiltSensitivityProperty =
            DependencyProperty.Register("TiltSensitivity", typeof(double), typeof(WPFTouchVEMap), new UIPropertyMetadata(0.5));

        #endregion
        
        #endregion

        #region Methods

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

                _targetImage.AddHandler(PreviewTouchDownEvent, new EventHandler<TouchEventArgs>(TargetImage_PreviewTouchDown));
                _targetImage.AddHandler(PreviewTouchMoveEvent, new EventHandler<TouchEventArgs>(TargetImage_PreviewTouchMove));
                _targetImage.AddHandler(PreviewTouchUpEvent, new EventHandler<TouchEventArgs>(TargetImage_PreviewTouchUp));
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
        
        #region Touch Events

        void WPFTouchVEMap_Loaded(object sender, RoutedEventArgs e)
        {
            ManipulationInertiaStarting += WPFTouchVEMap_ManipulationInertiaStarting;
            ManipulationDelta += WPFTouchVEMap_ManipulationDelta;
            ManipulationStarting += WPFTouchVEMap_ManipulationStarting;
            ManipulationCompleted += WPFTouchVEMap_ManipulationCompleted;
        }

        void WPFTouchVEMap_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            e.Handled = true;
        }

        void WPFTouchVEMap_ManipulationStarting(object sender, ManipulationStartingEventArgs e)
        {
            e.Handled = true;
        }

        void WPFTouchVEMap_ManipulationInertiaStarting(object sender, ManipulationInertiaStartingEventArgs e)
        {
            //panning
            e.TranslationBehavior = new InertiaTranslationBehavior()
            {
                InitialVelocity = e.InitialVelocities.LinearVelocity,
                DesiredDeceleration = 0.002 //10.0 * 96.0 / (1000.0 * 1000.0)
            };

            //Expansion (zoom)
            e.ExpansionBehavior = new InertiaExpansionBehavior()
            {
                InitialVelocity = e.InitialVelocities.ExpansionVelocity,
                DesiredDeceleration = 0.0005
            };

            //rotation
            e.RotationBehavior = new InertiaRotationBehavior()
            {
                InitialVelocity = e.InitialVelocities.AngularVelocity,
                DesiredDeceleration = 0.0002
            };

            e.Handled = true;

        }

        void WPFTouchVEMap_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            if (MapManipulationMode == MapManipulationMode.PanZoomPivot)
            {
                if (IsPanEnabled)
                {
                    DoMapMove(e.DeltaManipulation.Translation.X * PanSensitivity, e.DeltaManipulation.Translation.Y * PanSensitivity);
                    //DoMapMove(e.DeltaManipulation.Translation.X * PanSensitivity, e.DeltaManipulation.Translation.Y * PanSensitivity, e.ManipulationOrigin);
                }
            }
            else
            {
                if (IsSpinEnabled)
                {
                    double deltaAngle = MathHelper.MapValue(e.DeltaManipulation.Translation.X, -20, 20, -1, 1);
                    DoMapYaw(deltaAngle);
                }
                if (IsTiltEnabled)
                {
                    double desiredPitch = ClampPitch(Pitch + e.DeltaManipulation.Translation.Y * TiltSensitivity, this.Altitude);
                    DoMapTilt(desiredPitch);
                }
            }

            if (IsZoomEnabled)
            {
                DoMapZoom(e.DeltaManipulation.Expansion.X * ZoomSensitivity, e.ManipulationOrigin);
            }

            if (IsPivotEnabled)
            {
                DoMapPivot(e.DeltaManipulation.Rotation, e.ManipulationOrigin);
            }
            e.Handled = true;
        }

        private void TargetImage_PreviewTouchDown(object sender, TouchEventArgs e)
        {
            e.TouchDevice.Capture(_targetImage, CaptureMode.SubTree);
            //e.Handled = true;
        }

        private void TargetImage_PreviewTouchMove(object sender, TouchEventArgs e)
        {
            Point position = e.GetTouchPoint(_targetImage).Position;
           // e.Handled = true;
            if (position.X < 0 ||
                position.Y < 0 ||
                position.X > _targetImage.Width ||
                position.Y > _targetImage.Height)
            {
                e.TouchDevice.Capture(_targetImage, CaptureMode.None);

            }
            else
            {
                e.TouchDevice.Capture(_targetImage, CaptureMode.SubTree);
            }
        }

        private void TargetImage_PreviewTouchUp(object sender, TouchEventArgs e)
        {
          //  e.Handled = true;
            e.TouchDevice.Capture(_targetImage, CaptureMode.None);

        }

        #endregion

        private double ClampPitch(double pitch, double altitude)
        {
            double maxAngle = MathHelper.MapValue(altitude, 3000, 7500000, -5, -80);
            maxAngle = MathHelper.Clamp(maxAngle, -80, -5);
            double minAngle = -90;

            return Math.Max(minAngle, Math.Min(maxAngle, pitch));
        }

        #endregion
    }
}