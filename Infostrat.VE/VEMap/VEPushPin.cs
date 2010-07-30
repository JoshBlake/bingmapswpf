using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using InfoStrat.VE.Utilities;
using System.Windows.Media.Animation;
using Microsoft.MapPoint.Geometry.VectorMath;
using Microsoft.MapPoint.Rendering3D.Utility.Types;

namespace InfoStrat.VE
{
    public enum VEPushPinState
    {
        Visible,
        Hidden,
        FadingIn,
        FadingOut
    };

    public class VEPushPin : VEShape
    {
        #region Fields

        protected Button Button;
        protected VEPushPin parentPushPin;

        protected VEPushPinState currentState;

        double previousCameraAltitude;

        protected enum VEPushPinAltitudeEvent
        {
            None,
            TransitionAboveUpperRange,
            TransitionBelowLowerRange,
            TransitionIntoUpperRange,
            TransitionIntoLowerRange
        };

        bool isNewPin = true;

        #endregion

        #region Events

        public event EventHandler OnShowPin;
        public event EventHandler OnHidePin;

        #endregion

        #region DPs

        #region Latitude DP

        public double Latitude
        {
            get { return (double)GetValue(LatitudeProperty); }
            set
            {
                SetValue(LatitudeProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for Latitude.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LatitudeProperty =
            DependencyProperty.Register("Latitude", typeof(double), typeof(VEPushPin), new UIPropertyMetadata(0.0, new PropertyChangedCallback(OnLatitudePropertyChanged)));

        static void OnLatitudePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            VEPushPin pin = d as VEPushPin;

            if (pin == null)
                return;

            if (e.NewValue == null)
            {
                return;
            }

            AnimateUtility.StopAnimation(pin, VEPushPin.DisplayLatitudeProperty);
            if (e.OldValue != null)
                pin.DisplayLatitude = (double)e.OldValue;

            if (pin.isNewPin)
            {
                pin.DisplayLatitude = (double)e.NewValue;
            }
            else
            {
                AnimateUtility.AnimateElementDouble(pin,
                                                    VEPushPin.DisplayLatitudeProperty,
                                                    (double)e.NewValue,
                                                    0, 2);
            }
        }

        #endregion

        #region Longitude DP

        public double Longitude
        {
            get { return (double)GetValue(LongitudeProperty); }
            set
            {
                SetValue(LongitudeProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for Longitude.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LongitudeProperty =
            DependencyProperty.Register("Longitude", typeof(double), typeof(VEPushPin), new UIPropertyMetadata(0.0, new PropertyChangedCallback(OnLongitudePropertyChanged)));

        static void OnLongitudePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            VEPushPin pin = d as VEPushPin;

            if (pin == null)
                return;

            if (e.NewValue == null)
            {
                return;
            }

            AnimateUtility.StopAnimation(pin, VEPushPin.DisplayLongitudeProperty);
            if (e.OldValue != null)
                pin.DisplayLongitude = (double)e.OldValue;

            if (pin.isNewPin)
            {
                pin.DisplayLongitude = (double)e.NewValue;
            }
            else
            {
                AnimateUtility.AnimateElementDouble(pin,
                                                    VEPushPin.DisplayLongitudeProperty,
                                                    (double)e.NewValue,
                                                    0, 2);
            }
        }

        #endregion

        #region DisplayLatitude DP

        public double DisplayLatitude
        {
            get { return (double)GetValue(DisplayLatitudeProperty); }
            set { SetValue(DisplayLatitudeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DisplayLatitude.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DisplayLatitudeProperty =
            DependencyProperty.Register("DisplayLatitude", typeof(double), typeof(VEPushPin), new UIPropertyMetadata(0.0));

        #endregion

        #region DisplayLongitude DP

        public double DisplayLongitude
        {
            get { return (double)GetValue(DisplayLongitudeProperty); }
            set { SetValue(DisplayLongitudeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DisplayLongitude.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DisplayLongitudeProperty =
            DependencyProperty.Register("DisplayLongitude", typeof(double), typeof(VEPushPin), new UIPropertyMetadata(0.0));

        #endregion

        #region Altitude DP

        /// <summary>
        /// Altitude above or below Earth, in meters
        /// </summary>
        public double Altitude
        {
            get { return (double)GetValue(AltitudeProperty); }
            set { SetValue(AltitudeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Altitude.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AltitudeProperty =
            DependencyProperty.Register("Altitude", typeof(double), typeof(VEPushPin), new UIPropertyMetadata(0.0));

        #endregion

        #region AltMode DP

        public VEAltMode AltMode
        {
            get { return (VEAltMode)GetValue(AltModeProperty); }
            set { SetValue(AltModeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AltMode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AltModeProperty =
            DependencyProperty.Register("AltMode", typeof(VEAltMode), typeof(VEPushPin), new UIPropertyMetadata(VEAltMode.FromGround));

        #endregion

        #region MinAltitude DP

        /// <summary>
        /// Minimum altitude, in meters, where the pushpin is visible
        /// </summary>
        public double MinAltitude
        {
            get { return (double)GetValue(MinAltitudeProperty); }
            set { SetValue(MinAltitudeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MinAltitude.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinAltitudeProperty =
            DependencyProperty.Register("MinAltitude", typeof(double), typeof(VEPushPin), new UIPropertyMetadata(double.MinValue));

        #endregion

        #region MaxAltitude DP

        /// <summary>
        /// Maximum altitude, in meters, where the pushpin is visible
        /// </summary>
        public double MaxAltitude
        {
            get { return (double)GetValue(MaxAltitudeProperty); }
            set { SetValue(MaxAltitudeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MaxAltitude.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaxAltitudeProperty =
            DependencyProperty.Register("MaxAltitude", typeof(double), typeof(VEPushPin), new UIPropertyMetadata(double.MaxValue));

        #endregion

        #region Visibility DP

        public new Visibility Visibility
        {
            get { return (Visibility)GetValue(VisibilityProperty); }
            set
            {
                SetValue(VisibilityProperty, value);
                AnimateUtility.StopAnimation(this, VEPushPin.OpacityProperty);

                if (value == Visibility.Visible)
                    this.Opacity = 1;
                else
                    this.Opacity = 0;
            }
        }

        #endregion

        #region PushPinBackground DP

        public Brush PushPinBackground
        {
            get { return (Brush)GetValue(PushPinBackgroundProperty); }
            set { SetValue(PushPinBackgroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PushPinBackground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PushPinBackgroundProperty =
            DependencyProperty.Register("PushPinBackground", typeof(Brush), typeof(VEPushPin), new UIPropertyMetadata(new SolidColorBrush(Color.FromRgb(245, 159, 101))));

        #endregion

        #region PinVisible DP

        public bool PinVisible
        {
            get { return (bool)GetValue(PinVisibleProperty); }
            set { SetValue(PinVisibleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PinVisible.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PinVisibleProperty =
            DependencyProperty.Register("PinVisible", typeof(bool), typeof(VEPushPin), new UIPropertyMetadata(true));

        #endregion

        #endregion

        #region Public Properties

        public VELatLong LatLong
        {
            get
            {
                return new VELatLong(this.Latitude, this.Longitude, this.Altitude, this.AltMode);
            }
        }

        public VELatLong DisplayLatLong
        {
            get
            {
                return new VELatLong(this.DisplayLatitude, this.DisplayLongitude, this.Altitude, this.AltMode);
            }
        }

        public VEPushPin ParentPushPin
        {
            get
            {
                return parentPushPin;
            }
            set
            {
                parentPushPin = value;
            }
        }

        public VEPushPinState CurrentState
        {
            get
            {
                return currentState;
            }
        }

        #endregion

        #region Public Events

        public event EventHandler<VEPushPinClickedEventArgs> Click;

        #endregion

        #region Constructors

        static VEPushPin()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VEPushPin), new FrameworkPropertyMetadata(typeof(VEPushPin)));
        }

        public VEPushPin()
        {
            Initialize();
        }

        public VEPushPin(VELatLong latLong)
        {
            if (latLong != null)
            {
                this.Latitude = latLong.Latitude;
                this.Longitude = latLong.Longitude;

                this.DisplayLatitude = this.Latitude;
                this.DisplayLongitude = this.Longitude;

                this.Altitude = latLong.Altitude;
                this.AltMode = latLong.AltMode;
            }

            Initialize();
        }

        public VEPushPin(VELatLong latLong, double minAltitude, double maxAltitude)
        {

            this.Latitude = latLong.Latitude;
            this.Longitude = latLong.Longitude;
            this.Altitude = latLong.Altitude;
            this.AltMode = latLong.AltMode;

            this.DisplayLatitude = this.Latitude;
            this.DisplayLongitude = this.Longitude;

            this.MinAltitude = minAltitude;
            this.MaxAltitude = maxAltitude;

            Initialize();
        }

        #endregion

        #region Destructors

        ~VEPushPin()
        {
            // This throws an exception in some cases
            //if (this.Map != null)
            //{
            //      this.Map.RemoveRegisteredPosition(this);
            //}
        }

        #endregion

        #region Overridden Methods

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            Button = this.Template.FindName("PART_button", this) as Button;

            if (Button != null)
            {
                Button.Click += new RoutedEventHandler(Button_Click);
            }
        }

        public override Point? UpdatePosition(VEMap map)
        {
            if (this.Map != map)
            {
                map.AddRegisteredPosition(this, DisplayLatLong);
                isNewPin = false;
            }

            base.UpdatePosition(map);

            VEPushPinAltitudeEvent altEvent;
            bool isVisible;
            Point? anchorPosition = GetPosition(DisplayLatLong, out altEvent, out isVisible);

            if (isVisible)
            {

                this.SetValue(VEPushPin.VisibilityProperty, Visibility.Visible);

                //If transitioning from invisible
                if (currentState != VEPushPinState.Visible &&
                    currentState != VEPushPinState.FadingIn)
                {
                    OnShow();

                    AnimationClock showClock = AnimateUtility.AnimateElementDouble(this,
                                                        VEPushPin.OpacityProperty,
                                                        1,
                                                        0,
                                                        1);

                    showClock.Completed += new EventHandler(showClock_Completed);
                    currentState = VEPushPinState.FadingIn;

                    //If transitioning and has a parent
                    if (this.parentPushPin != null &&
                        (altEvent == VEPushPinAltitudeEvent.TransitionIntoUpperRange))
                    {
                        this.DisplayLatitude = this.parentPushPin.Latitude;
                        this.DisplayLongitude = this.parentPushPin.Longitude;
                    }

                    if (this.DisplayLatitude != this.Latitude ||
                        this.DisplayLongitude != this.Longitude)
                    {
                        AnimateUtility.AnimateElementDouble(this,
                                                            VEPushPin.DisplayLatitudeProperty,
                                                            this.Latitude,
                                                            0,
                                                            1);
                        AnimateUtility.AnimateElementDouble(this,
                                                            VEPushPin.DisplayLongitudeProperty,
                                                            this.Longitude,
                                                            0,
                                                            1);

                        anchorPosition = GetPosition(DisplayLatLong);
                    }
                }


            }
            else
            {
                //If transitioning from Visible
                if (currentState != VEPushPinState.Hidden &&
                    currentState != VEPushPinState.FadingOut)
                {
                    OnHide();

                    AnimationClock hideClock = AnimateUtility.AnimateElementDouble(this,
                                                            VEPushPin.OpacityProperty,
                                                            0,
                                                            0,
                                                            1);

                    hideClock.Completed += new EventHandler(hideClock_Completed);
                    currentState = VEPushPinState.FadingOut;

                    //If transitioning and has a parent
                    if (this.parentPushPin != null &&
                        (altEvent == VEPushPinAltitudeEvent.TransitionAboveUpperRange))
                    {
                        AnimateUtility.AnimateElementDouble(this,
                                                            VEPushPin.DisplayLatitudeProperty,
                                                            this.parentPushPin.Latitude,
                                                            0,
                                                            1);
                        AnimateUtility.AnimateElementDouble(this,
                                                            VEPushPin.DisplayLongitudeProperty,
                                                            this.parentPushPin.Longitude,
                                                            0,
                                                            1);

                        anchorPosition = GetPosition(DisplayLatLong);
                    }
                    else
                    {
                        //Not animating, just hide immediately
                        //this.Visibility = Visibility.Collapsed;
                    }
                }
            }

            //Update the calculated position
            if (anchorPosition == null || !map.IsMapLoaded)
            {
                this.Visibility = Visibility.Collapsed;
                this.Opacity = 0;
                this.currentState = VEPushPinState.Hidden;

                return null;
            }

            if (!this.IsMeasureValid)
            {
                this.Opacity = 0;
                this.currentState = VEPushPinState.Hidden;
                return null;
            }
            
            Point anchorOffset = GetAnchorOffset();

            double displayLeft = anchorPosition.Value.X - anchorOffset.X;
            double displayTop = anchorPosition.Value.Y - anchorOffset.Y;

            return new Point(displayLeft, displayTop);

        }

        void showClock_Completed(object sender, EventArgs e)
        {
            currentState = VEPushPinState.Visible;
            this.Opacity = 1.0;
        }

        void hideClock_Completed(object sender, EventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            currentState = VEPushPinState.Hidden;
            this.Opacity = 0.0;
        }

        #endregion

        #region Virtual Methods

        protected virtual void Initialize()
        {
            this.ShapeType = VEShapeType.Pushpin;
            this.Button = null;
            this.parentPushPin = null;
            this.previousCameraAltitude = 0;

            this.Visibility = Visibility.Collapsed;
            this.Opacity = 0;
            this.currentState = VEPushPinState.Hidden;
        }

        protected virtual void OnClick(object sender, VEPushPinClickedEventArgs e)
        {
            if (Click != null)
            {
                Click(this, e);
            }
        }

        protected virtual void OnShow()
        {
            if (OnShowPin != null)
                OnShowPin(this, EventArgs.Empty);
        }

        protected virtual void OnHide()
        {
            if (OnHidePin != null)
                OnHidePin(this, EventArgs.Empty);
        }

        /// <summary>
        /// Gets the lat/long anchor offset relative to the top-left of this control
        /// </summary>
        /// <returns>A point relative to the control that represents the location of the current latitude and longitude</returns>
        protected virtual Point GetAnchorOffset()
        {
            return new Point(this.ActualWidth / 2, this.ActualHeight);
        }

        #endregion

        #region Private Methods

        protected void Button_Click(object sender, RoutedEventArgs e)
        {
            if (Map != null)
            {
                Map.SendToFront(this);
            }
            if (Click != null)
            {
                VEPushPinClickedEventArgs args = new VEPushPinClickedEventArgs(e);

                Point? position = GetPosition(LatLong);
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

        protected Point? GetPosition(VELatLong latLong)
        {
            VEPushPinAltitudeEvent alt;
            bool isVisible;
            return GetPosition(latLong, out alt, out isVisible);
        }

        protected Point? GetPosition(VELatLong latLong, out VEPushPinAltitudeEvent altEvent, out bool isVisible)
        {
            Point? position = null;

            isVisible = true;
            altEvent = VEPushPinAltitudeEvent.None;

            if (this.Map != null)
            {
                position = this.Map.LatLongToPoint(latLong, this);

                //Not visible if no position (off screen)
                if (position == null)
                    isVisible = false;

                //Not visible if behind planet
                if (this.Map.IsBehindPlanet(latLong))
                {
                    isVisible = false;
                }

                //Not visible if out of altitude range
                double newCameraAltitude = Map.Altitude;

                if (newCameraAltitude > this.MaxAltitude ||
                    newCameraAltitude < this.MinAltitude)
                {
                    isVisible = false;
                }

                if (newCameraAltitude <= this.MaxAltitude &&
                    newCameraAltitude >= this.MinAltitude &&
                    previousCameraAltitude > this.MaxAltitude)
                {
                    altEvent = VEPushPinAltitudeEvent.TransitionIntoUpperRange;
                }
                else if (newCameraAltitude <= this.MaxAltitude &&
                         newCameraAltitude >= this.MinAltitude &&
                         previousCameraAltitude < this.MinAltitude)
                {
                    altEvent = VEPushPinAltitudeEvent.TransitionIntoLowerRange;
                }
                else if (newCameraAltitude > this.MaxAltitude &&
                         previousCameraAltitude <= this.MaxAltitude &&
                         previousCameraAltitude >= this.MinAltitude)
                {
                    altEvent = VEPushPinAltitudeEvent.TransitionAboveUpperRange;
                }
                else if (newCameraAltitude < this.MinAltitude &&
                         previousCameraAltitude <= this.MaxAltitude &&
                         previousCameraAltitude >= this.MinAltitude)
                {
                    altEvent = VEPushPinAltitudeEvent.TransitionBelowLowerRange;
                }

                previousCameraAltitude = newCameraAltitude;

                //Not visible if manual override
                if (PinVisible == false)
                {
                    isVisible = false;
                }

            }
            return position;
        }

        #endregion

    }
}
