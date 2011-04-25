using System;
using System.Collections;
using System.IO;
using System.Xml;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.ComponentModel;
using Microsoft.MapPoint.Rendering3D.Utility;
using Microsoft.MapPoint.Rendering3D;
using Microsoft.MapPoint.Rendering3D.Control;
using Microsoft.MapPoint.Rendering3D.Cameras;
using Microsoft.MapPoint.Graphics3D;
using Microsoft.MapPoint.Geometry.VectorMath;
using System.Reflection;
using System.Collections.Generic;
using System.Windows.Interop;
using System.Windows.Forms;
using Microsoft.MapPoint.Rendering3D.Steps;
using Microsoft.MapPoint.Rendering3D.Scene;
using Microsoft.MapPoint.Rendering3D.State;
using InfoStrat.VE.Utilities;
using Microsoft.MapPoint.Binding;
using System.Diagnostics;
using System.Threading;
using System.Windows.Threading;

[assembly: CLSCompliant(true)]
namespace InfoStrat.VE
{
    public enum MapManipulationMode
    {
        PanZoomPivot,
        TiltSpinZoomPivot
    }

    public class VEMap : ItemsControl, INotifyPropertyChanged, IDisposable
    {
        #region UIElements

        Viewbox viewbox;
        Grid hostGrid;
        Image targetImage;
        D3DImage d3dImage;
        Canvas canvasPushPin;

        #endregion

        #region Fields

        VEWindow winVE;

        AppDomain appDomainHost;

        IntPtr veSurface;
        IntPtr veSurfaceSrc;
        Size veSurfaceSize;

        bool isTemplateLoaded;
        bool _isMapLoaded;
        volatile bool isMapPositionDirty = false;
        volatile bool isItemsPositionDirty = false;
        private bool disposed = false;

        private delegate void DelegateGlobeRedraw();
        private delegate void DelegateGlobeInitialized();

        Microsoft.MapPoint.Graphics3D.Types.Surface surfCpy;

        readonly Stopwatch stopwatch = new Stopwatch();

        Dictionary<int, EventHandler> onFlyToEndCallbacks;

        private PublicEventsGlobeControl globeControl;

        private VEMapStyle currentMapStyle;

        Dictionary<object, RegisteredPosition> registeredPositions;

        BackgroundWorker VEUpdateWorker;

        int frameCount = 0;
        Stopwatch fpsStopwatch = new Stopwatch();
        #endregion

        #region Public Events

        public event EventHandler MapLoaded;

        public event EventHandler<VECameraChangedEventArgs> CameraChanged;

        #endregion

        #region Public Properties

        [CLSCompliant(false)]
        public PublicEventsGlobeControl GlobeControl
        {
            get
            {
                return globeControl;
            }
        }

        public double GlobeWidth
        {
            get
            {
                if (isTemplateLoaded)
                {
                    if (viewbox.ActualWidth == 0)
                        return hostGrid.ActualWidth;
                    else
                        return viewbox.ActualWidth;
                }
                else
                {
                    return 1024;
                }
            }
        }

        public double GlobeHeight
        {
            get
            {
                if (isTemplateLoaded)
                {
                    if (viewbox.Height == 0)
                        return hostGrid.ActualHeight;
                    else
                        return viewbox.ActualHeight;
                }
                else
                {
                    return 768;
                }
            }
        }

        public VEMapStyle MapStyle
        {
            get
            {
                return this.currentMapStyle;
            }

            set
            {
                UpdateMapStyle(value);

                this.currentMapStyle = value;

                NotifyPropertyChanged("MapStyle");
            }
        }

        public bool Show3DCursor
        {
            get
            {
                return this.globeControl != null &&
                       this.globeControl.Host != null &&
                       this.globeControl.Host.WorldEngine != null &&
                       this.globeControl.Host.WorldEngine.Display3DCursor;
            }
            set
            {
                if (this.globeControl != null &&
                    this.globeControl.Host != null &&
                    this.globeControl.Host.WorldEngine != null)
                {
                    this.globeControl.Host.WorldEngine.Display3DCursor = value;
                }

                NotifyPropertyChanged("Display3DCursor");
            }
        }

        public bool ShowBuildings
        {
            get
            {
                return this.globeControl != null &&
                       this.globeControl.Host != null &&
                       this.globeControl.Host.Ready &&
                       this.globeControl.Host.WorldEngine != null &&
                       this.globeControl.Host.WorldEngine.ShowBuildings;
            }
            set
            {
                if (this.globeControl != null &&
                    this.globeControl.Host != null &&
                    this.globeControl.Host.WorldEngine != null)
                {
                    this.globeControl.Host.WorldEngine.ShowBuildings = value;
                }

                NotifyPropertyChanged("ShowBuildings");
            }
        }

        public bool ShowBuildingTextures
        {
            get
            {
                return this.globeControl != null &&
                       this.globeControl.Host != null &&
                       this.globeControl.Host.Ready &&
                       this.globeControl.Host.WorldEngine != null &&
                       this.globeControl.Host.WorldEngine.ShowBuildingTextures;
            }
            set
            {
                if (this.globeControl != null &&
                    this.globeControl.Host != null &&
                    this.globeControl.Host.WorldEngine != null)
                {
                    this.globeControl.Host.WorldEngine.ShowBuildingTextures = value;
                }

                NotifyPropertyChanged("ShowBuildingTextures");
            }
        }

        public bool IsMapLoaded
        {
            get
            {
                return _isMapLoaded;
            }
        }

        public bool IsItemsPositionDirty
        {
            get
            {
                return isItemsPositionDirty;
            }
            set
            {
                isItemsPositionDirty = value;
            }
        }


        private MapManipulationMode _mapManipulationMode = MapManipulationMode.PanZoomPivot;
        public MapManipulationMode MapManipulationMode
        {
            get
            {
                return _mapManipulationMode;
            }
            set
            {
                _mapManipulationMode = value;
                NotifyPropertyChanged("MapManipulationMode");
            }
        }

        private bool _isPanEnabled = true;
        public bool IsPanEnabled
        {
            get
            {
                return _isPanEnabled;
            }
            set
            {
                _isPanEnabled = value;
                NotifyPropertyChanged("IsPanEnabled");
            }
        }

        private bool _isZoomEnabled = true;
        public bool IsZoomEnabled
        {
            get
            {
                return _isZoomEnabled;
            }
            set
            {
                _isZoomEnabled = value;
                NotifyPropertyChanged("IsZoomEnabled");
            }
        }

        private bool _isPivotEnabled = true;
        public bool IsPivotEnabled
        {
            get
            {
                return _isPivotEnabled;
            }
            set
            {
                _isPivotEnabled = value;
                NotifyPropertyChanged("IsPivotEnabled");

            }
        }

        private bool _isTiltEnabled = true;
        public bool IsTiltEnabled
        {
            get
            {
                return _isTiltEnabled;
            }
            set
            {
                _isTiltEnabled = value;
                NotifyPropertyChanged("IsTiltEnabled");

            }
        }

        private bool _isSpinEnabled = true;
        public bool IsSpinEnabled
        {
            get
            {
                return _isSpinEnabled;
            }
            set
            {
                _isSpinEnabled = value;
                NotifyPropertyChanged("IsSpinEnabled");
            }
        }

        #region Camera Control Dependency Properties

        #region VELatLong Property

        public VELatLong VELatLong
        {
            get
            {
                return new VELatLong(this.LatLong.X, this.LatLong.Y, this.Altitude);
            }
        }

        #endregion

        #region LatLong DP

        public Point LatLong
        {
            get { return (Point)GetValue(LatLongProperty); }
            set { SetValue(LatLongProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LatLong.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LatLongProperty =
            DependencyProperty.Register("LatLong", typeof(Point), typeof(VEMap), new UIPropertyMetadata(new Point(0, 0), new PropertyChangedCallback(OnMapPositionPropertyChanged)));

        #endregion

        #region Altitude DP

        public double Altitude
        {
            get { return (double)GetValue(AltitudeProperty); }
            set { SetValue(AltitudeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Altitude.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AltitudeProperty =
            DependencyProperty.Register("Altitude", typeof(double), typeof(VEMap), new UIPropertyMetadata(10000000.0, new PropertyChangedCallback(OnMapPositionPropertyChanged)));

        #endregion

        #region Roll DP

        public double Roll
        {
            get { return (double)GetValue(RollProperty); }
            set { SetValue(RollProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Altitude.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RollProperty =
            DependencyProperty.Register("Roll", typeof(double), typeof(VEMap), new UIPropertyMetadata(0.0, new PropertyChangedCallback(OnMapPositionPropertyChanged)));

        #endregion

        #region Pitch DP

        public double Pitch
        {
            get { return (double)GetValue(PitchProperty); }
            set { SetValue(PitchProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Altitude.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PitchProperty =
            DependencyProperty.Register("Pitch", typeof(double), typeof(VEMap), new UIPropertyMetadata(-90.0, new PropertyChangedCallback(OnMapPositionPropertyChanged)));

        #endregion

        #region Yaw DP

        public double Yaw
        {
            get { return (double)GetValue(YawProperty); }
            set { SetValue(YawProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Altitude.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty YawProperty =
            DependencyProperty.Register("Yaw", typeof(double), typeof(VEMap), new UIPropertyMetadata(0.0, new PropertyChangedCallback(OnMapPositionPropertyChanged)));

        #endregion

        static void OnMapPositionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            VEMap map = d as VEMap;

            if (map == null)
                return;

            if (e.NewValue == null)
            {
                return;
            }


            map.isMapPositionDirty = true;
        }

        #endregion

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged == null)
                return;

            PropertyChanged(this, new PropertyChangedEventArgs(info));
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return;
            if (disposing)
            {
                //Free managed resources
                if (globeControl != null)
                {
                    globeControl.Dispose();
                    globeControl = null;
                }
            }
            disposed = true;
            _isMapLoaded = false;
        }

        #endregion

        #region Constructors

        static VEMap()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VEMap), new FrameworkPropertyMetadata(typeof(VEMap)));
        }

        public VEMap()
        {
            viewbox = null;
            hostGrid = null;
            targetImage = null;
            d3dImage = null;
            canvasPushPin = null;

            isTemplateLoaded = false;
            _isMapLoaded = false;

            appDomainHost = null;

            InitVEMap();
        }

        private void InitVEMap()
        {
            if (globeControl != null)
            {
                globeControl.Dispose();
            }

            if (this.winVE != null)
            {
                this.winVE.Close();
            }

            if (appDomainHost != null)
            {
                AppDomain.Unload(appDomainHost);
                appDomainHost = null;
            }

            this.Loaded -= VEMap_Loaded;
            this.Unloaded -= VEMap_Unloaded;

            //Are we in Visual Studio Designer?
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }

            this.winVE = new VEWindow();

            surfCpy = null;

            onFlyToEndCallbacks = new Dictionary<int, EventHandler>();

            veSurface = IntPtr.Zero;
            veSurfaceSrc = IntPtr.Zero;

            registeredPositions = new Dictionary<object, RegisteredPosition>();

            GlobeControl.GlobeControlInitializationOptions options = new GlobeControl.GlobeControlInitializationOptions();
            // prevents the globeControl from trying to create a render thread
            options.DelayRenderThreadCreation = true;

            this.globeControl = new PublicEventsGlobeControl(options);

            winVE.winFormsHost.Child = this.globeControl;

            //Must show window briefly to init VE drawing contexts
            winVE.Show();
            winVE.Hide();

            if (!this.IsLoaded)
            {
                this.Loaded += new RoutedEventHandler(VEMap_Loaded);
            }
            else
            {
                InitGlobeControl();
            }
            this.Unloaded += new RoutedEventHandler(VEMap_Unloaded);
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

                viewbox.SizeChanged += new SizeChangedEventHandler(viewbox_SizeChanged);

                d3dImage.IsFrontBufferAvailableChanged += new DependencyPropertyChangedEventHandler(d3dImage_IsFrontBufferAvailableChanged);

                isTemplateLoaded = true;
            }
            catch (Exception)
            {
                isTemplateLoaded = false;
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

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new VEMapItem();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return (item is VEMapItem);
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);

            VEMapItem mapitem = element as VEMapItem;
            if (mapitem == null)
                return;

            if (this.ItemTemplateSelector != null)
            {
                mapitem.ContentTemplate = this.ItemTemplateSelector.SelectTemplate(item, element);
            }
            /*
            mapitem.LayoutUpdated += new EventHandler((s, e) => { 
                if (_isMapLoaded) 
                    mapitem.UpdatePosition(this); });
            */
        }

        protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            base.OnItemsSourceChanged(oldValue, newValue);
            isItemsPositionDirty = true;
        }

        protected override void OnItemsChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);
            isItemsPositionDirty = true;
        }

        #endregion

        #region Public Map Movement Helpers

        [Obsolete("DoMapMove(double, double, bool) is obsolete. Use DoMapMove(double, double) instead", true)]
        public virtual void DoMapMove(double dx, double dy, bool isContinuous)
        {
            DoMapMove(dx, dy);
        }

        [Obsolete("DoMapZoom(double, bool) is obsolete. Use DoMapZoom(double) or DoMapZoom(double, Point) instead", true)]
        public virtual void DoMapZoom(double zoom, bool isContinuous)
        {
            DoMapZoom(zoom);
        }

        /// <summary>
        /// Pans the map using the center of the screen for reference.
        /// </summary>
        /// <param name="dx">X change in pointer position</param>
        /// <param name="dy">Y change in pointer position</param>
        public virtual void DoMapMove(double dx, double dy)
        {
            DoMapMove(dx, dy, new Point(this.ActualWidth / 2.0, this.ActualHeight / 2.0));
        }

        /// <summary>
        /// Pans the map using the specified Point for as reference.
        /// </summary>
        /// <param name="dx">X change in pointer position</param>
        /// <param name="dy">Y change in pointer position</param>
        /// <param name="center">Pointer position</param>
        public virtual void DoMapMove(double dx, double dy, Point center)
        {
            if (this.IsMapLoaded == false)
                return;
            CancelFlyTo();

            CameraStep cameraStep = null;
            foreach (Step s in this.globeControl.Host.RenderEngine.StepManager)
            {
                cameraStep = s as CameraStep;
                if (cameraStep != null) break;
            }

            if (cameraStep == null)
                return;

            int x = (int)MathHelper.MapValue(center.X - dx,
                                                0, this.ActualWidth,
                                                0, globeControl.Width);
            int y = (int)MathHelper.MapValue(center.Y - dy,
                                                0, this.ActualHeight,
                                                0, globeControl.Height);

            System.Drawing.Point newPosition = new System.Drawing.Point(x, y);

            int x2 = (int)MathHelper.MapValue(center.X,
                                                0, this.ActualWidth,
                                                0, globeControl.Width);
            int y2 = (int)MathHelper.MapValue(center.Y,
                                                0, this.ActualHeight,
                                                0, globeControl.Height);

            System.Drawing.Point oldPosition = new System.Drawing.Point(x2, y2);

            CameraData camera;
            if (!this.globeControl.Host.RenderEngine.PreviousSceneState.TryGetData<CameraData>(out camera))
            {
                return;
            }

            LatLonAlt? llaDelta = this.globeControl.Host.Navigation.LatLonAltFromScreenPosition(newPosition);

            if (!llaDelta.HasValue)
                return;

            GeodeticPosition deltaGeo = new GeodeticPosition(llaDelta.Value);

            LatLonAlt? llaCenter = this.globeControl.Host.Navigation.LatLonAltFromScreenPosition(oldPosition);

            if (!llaCenter.HasValue)
                return;

            GeodeticPosition centerGeo = new GeodeticPosition(llaCenter.Value);
            centerGeo.Altitude = deltaGeo.Altitude;

            Vector3D cameraPosDelta = deltaGeo - centerGeo.Vector;

            if (camera.CenterGroundPosition != null)
            {
                SweptSphereD sweptSphere = new SweptSphereD(
                    camera.CenterGroundPosition.Vector,
                    1.2,
                    cameraPosDelta);

                Vector3D collisionPosition;
                if (this.globeControl.Host.WorldEngine.GetValidResultOfSweptSphere(sweptSphere, out collisionPosition))
                {
                    cameraPosDelta = collisionPosition - centerGeo.Vector;
                }
            }

            this.globeControl.Host.WorldEngine.ExecuteOnRenderThread(() =>
            {
                double oldAltitude = cameraStep.Camera.Viewpoint.Position.Altitude;
                cameraStep.Camera.Viewpoint.Position.Vector += cameraPosDelta;

                cameraStep.Camera.Viewpoint.Position.Altitude = oldAltitude;
            });
        }

        /// <summary>
        /// Zooms the map towards the center of the screen
        /// </summary>
        /// <param name="deltaZoom">A zero-centered zoom-factor. Positive values zoom in, negative values zoom out.</param>
        public virtual void DoMapZoom(double deltaZoom)
        {
            DoMapZoom(deltaZoom, new Point(this.ActualWidth / 2, this.ActualHeight / 2));
        }

        /// <summary>
        /// Zooms the map toward the specified point.
        /// </summary>
        /// <param name="deltaZoom">A zero-centered zoom-factor. Positive values zoom in, negative values zoom out.</param>
        /// <param name="center">Center of the zoom</param>
        public virtual void DoMapZoom(double deltaZoom, Point center)
        {
            CancelFlyTo();

            CameraStep cameraStep = null;
            foreach (Step s in this.globeControl.Host.RenderEngine.StepManager)
            {
                cameraStep = s as CameraStep;
                if (cameraStep != null) break;
            }

            if (cameraStep == null)
                return;

            int x = (int)MathHelper.MapValue(center.X, 0, this.ActualWidth, 0, globeControl.Width);
            int y = (int)MathHelper.MapValue(center.Y, 0, this.ActualHeight, 0, globeControl.Height);

            System.Drawing.Point pointerPosition = new System.Drawing.Point(x, y);

            Ray3D movementRay;
            CameraData camera;
            if (this.globeControl.Host.RenderEngine.PreviousSceneState.TryGetData<CameraData>(out camera))
            {
                movementRay = camera.ScreenToGlobalRay(pointerPosition);
            }
            else
            {
                return;
            }

            double fromGround = camera.MetersAboveGround;

            double distance = fromGround * 0.002 * deltaZoom;

            this.globeControl.Host.WorldEngine.ExecuteOnRenderThread(() =>
            {
                Vector3D current = cameraStep.Camera.Viewpoint.Position.Vector;

                SweptSphereD sweptSphere = new SweptSphereD(
                    current,
                    1.2,
                    movementRay.Direction * distance);

                Vector3D collisionPosition;
                if (this.globeControl.Host.WorldEngine.GetValidResultOfSweptSphere(sweptSphere, out collisionPosition))
                {
                    distance = Vector3D.Distance(current, collisionPosition);
                }
                cameraStep.Camera.Viewpoint.Position.Vector += movementRay.Direction * distance;
            });

        }

        /// <summary>
        /// Rotates the map around the center of the screen
        /// </summary>
        /// <param name="deltaAngle">The angle to rotate, in degrees</param>
        public virtual void DoMapPivot(double deltaAngle)
        {
            DoMapPivot(deltaAngle, new Point(this.ActualWidth / 2, this.ActualHeight / 2));
        }

        /// <summary>
        /// Rotates the map around the specified point
        /// </summary>
        /// <param name="deltaAngle">The angle to rotate, in degrees</param>
        /// <param name="center">Center of the rotation</param>
        public virtual void DoMapPivot(double deltaAngle, Point center)
        {
            CancelFlyTo();

            //Convert degrees to radians
            deltaAngle = deltaAngle * Math.PI / 180.0;

            //Get the cameraStep
            CameraStep cameraStep = null;
            foreach (Step s in this.globeControl.Host.RenderEngine.StepManager)
            {
                cameraStep = s as CameraStep;
                if (cameraStep != null) break;
            }

            if (cameraStep == null)
                return;

            //Map the rotation value from control screen coordinates to viewbox/VE coordinates
            int x = (int)MathHelper.MapValue(center.X, 0, this.ActualWidth, 0, globeControl.Width);
            int y = (int)MathHelper.MapValue(center.Y, 0, this.ActualHeight, 0, globeControl.Height);


            System.Drawing.Point pointerPosition = new System.Drawing.Point(x, y);

            CameraData camera;
            if (!this.globeControl.Host.RenderEngine.PreviousSceneState.TryGetData<CameraData>(out camera))
            {
                return;
            }

            //Gets the LatLonAlt under the pointer position
            LatLonAlt? lla = this.globeControl.Host.Navigation.LatLonAltFromScreenPosition(pointerPosition);

            //If pointer is not a valid LatLonAlt, then return
            if (!lla.HasValue)
                return;

            //Convert pointer position to GeodeticPosition and set the altitude to the same as the camera
            GeodeticPosition anchor = new GeodeticPosition(lla.Value);

            Vector3D tempVector = anchor.Vector;
            anchor.Altitude += 10;
            Vector3D axis = Vector3D.Normalize(anchor.Vector - tempVector);
            QuaternionD rotationAxis = QuaternionD.RotationAxis(axis, -deltaAngle);

            this.globeControl.Host.WorldEngine.ExecuteOnRenderThread(() =>
            {
                Vector3D rotatedPosition = QuaternionD.RotateVectorByQuaternion(cameraStep.Camera.Viewpoint.Position.Vector - anchor.Vector, rotationAxis) + anchor.Vector;
                Vector3D rotatedLookAt = QuaternionD.RotateVectorByQuaternion(cameraStep.Camera.Viewpoint.Orientation.LookAt, rotationAxis);
                Vector3D rotatedLookUp = QuaternionD.RotateVectorByQuaternion(cameraStep.Camera.Viewpoint.Orientation.LookUp, rotationAxis);

                cameraStep.Camera.Viewpoint.Position.Vector = rotatedPosition;
                cameraStep.Camera.Viewpoint.Orientation.LookAt = rotatedLookAt;
                cameraStep.Camera.Viewpoint.Orientation.LookUp = rotatedLookUp;
            });
        }

        /// <summary>
        /// Tilts the map immediately to the desired pitch
        /// </summary>
        /// <param name="desiredPitch">The desired pitch angle in degrees</param>
        public virtual void DoMapTilt(double desiredPitch)
        {
            CancelFlyTo();

            //Convert degrees to radians
            desiredPitch = desiredPitch * Math.PI / 180.0;

            //Get the cameraStep
            CameraStep cameraStep = null;
            foreach (Step s in this.globeControl.Host.RenderEngine.StepManager)
            {
                cameraStep = s as CameraStep;
                if (cameraStep != null) break;
            }

            if (cameraStep == null)
                return;

            this.globeControl.Host.WorldEngine.ExecuteOnRenderThread(() =>
            {
                cameraStep.Camera.Viewpoint.LocalOrientation.Pitch = desiredPitch;
            });
        }

        /// <summary>
        /// Yaws the map by the specified angle
        /// </summary>
        /// <param name="desiredPitch">The delta yaw angle in degrees</param>
        public virtual void DoMapYaw(double deltaYaw)
        {
            CancelFlyTo();

            //Convert degrees to radians
            deltaYaw = deltaYaw * Math.PI / 180.0;

            //Get the cameraStep
            CameraStep cameraStep = null;
            foreach (Step s in this.globeControl.Host.RenderEngine.StepManager)
            {
                cameraStep = s as CameraStep;
                if (cameraStep != null) break;
            }

            if (cameraStep == null)
                return;

            this.globeControl.Host.WorldEngine.ExecuteOnRenderThread(() =>
            {
                cameraStep.Camera.Viewpoint.LocalOrientation.Yaw += deltaYaw;
            });
        }

        #endregion

        #region Control Input Events

        protected virtual void DoControlInputDown(Point p)
        {
            if (this.globeControl == null)
                return;

            int x = (int)MathHelper.MapValue(p.X, 0, this.ActualWidth, 0, globeControl.Width);
            int y = (int)MathHelper.MapValue(p.Y, 0, this.ActualHeight, 0, globeControl.Height);

            MouseEventArgs eventArgs = new MouseEventArgs(MouseButtons.Left, 1, x, y, 0);
            globeControl.DoMouseDown(eventArgs);
        }

        protected virtual void DoControlInputUp(Point p)
        {
            if (this.globeControl == null)
                return;

            int x = (int)MathHelper.MapValue(p.X, 0, this.ActualWidth, 0, globeControl.Width);
            int y = (int)MathHelper.MapValue(p.Y, 0, this.ActualHeight, 0, globeControl.Height);

            MouseEventArgs eventArgs = new MouseEventArgs(MouseButtons.Left, 1, x, y, 0);
            globeControl.DoMouseUp(eventArgs);
        }

        protected virtual void DoControlInputMove(Point p, bool isHover)
        {
            if (isHover)
            {
                DoControlInputUp(p);
            }
            else
            {
                DoControlInputDown(p);
            }

            if (this.globeControl == null)
                return;

            int x = (int)MathHelper.MapValue(p.X, 0, this.ActualWidth, 0, globeControl.Width);
            int y = (int)MathHelper.MapValue(p.Y, 0, this.ActualHeight, 0, globeControl.Height);
            MouseEventArgs eventArgs;

            if (isHover)
                eventArgs = new MouseEventArgs(MouseButtons.None, 0, x, y, 0);
            else
                eventArgs = new MouseEventArgs(MouseButtons.Left, 1, x, y, 0);

            globeControl.DoMouseMove(eventArgs);
        }

        protected virtual void DoControlInputDoubleClick(Point p)
        {
            if (this.globeControl == null)
                return;

            int x = (int)MathHelper.MapValue(p.X, 0, this.ActualWidth, 0, globeControl.Width);
            int y = (int)MathHelper.MapValue(p.Y, 0, this.ActualHeight, 0, globeControl.Height);

            MouseEventArgs eventArgs = new MouseEventArgs(MouseButtons.Left, 2, x, y, 0);
            globeControl.DoMouseDoubleClick(eventArgs);
        }

        protected virtual void DoControlInputWheelZoom(Point p, int delta)
        {
            if (this.globeControl == null)
                return;

            this.globeControl.Host.BindingsManager.Mouse.Wheel(delta);
        }

        #endregion

        #region Mouse Input Overrides

        protected override void OnMouseDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
                DoControlInputDown(e.GetPosition(this));
        }

        protected override void OnMouseUp(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);

            if (e.LeftButton == System.Windows.Input.MouseButtonState.Released)
                DoControlInputDown(e.GetPosition(this));
        }

        protected override void OnMouseMove(System.Windows.Input.MouseEventArgs e)
        {
            base.OnMouseMove(e);

            bool isHover = (e.LeftButton == System.Windows.Input.MouseButtonState.Released);

            DoControlInputMove(e.GetPosition(this), isHover);
        }

        protected override void OnMouseDoubleClick(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
                DoControlInputDoubleClick(e.GetPosition(this));
        }

        protected override void OnMouseWheel(System.Windows.Input.MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

            DoControlInputWheelZoom(e.GetPosition(this), e.Delta);
        }

        #endregion

        #region Control Loaded Unloaded Events

        void VEMap_Loaded(object sender, RoutedEventArgs e)
        {
            InitGlobeControl();
        }

        private void InitGlobeControl()
        {
            if (!_isMapLoaded)
            {
                _isMapLoaded = true;
                //Are we in Visual Studio Designer?
                if (!DesignerProperties.GetIsInDesignMode(this))
                {
                    if (this.globeControl.Host.RenderEngine == null)
                        return;
                    this.globeControl.Host.RenderEngine.Initialized += new EventHandler(RenderEngine_Initialized);

                    this.globeControl.InitRenderEngine();

                }
            }
            else
            {
                GlobeInitialized();
            }
        }

        void VEMap_Unloaded(object sender, RoutedEventArgs e)
        {
            _isMapLoaded = false;

            this.winVE.Close();
        }

        #endregion

        #region Map Loaded Event

        void RenderEngine_Initialized(object sender, EventArgs e)
        {
            //Invoke back to UI thread
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new DelegateGlobeInitialized(GlobeInitialized));
        }

        private void UpdateMapStyle(VEMapStyle value)
        {
            //Are we in Visual Studio Designer?  Is the control loaded yet?
            if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this) && _isMapLoaded)
            {
                switch (value)
                {
                    //JRB 4-13-09: Updated URLS to use forward links per VE3D team blog: http://blogs.msdn.com/virtualearth3d/archive/2009/04/06/data-format-revision.aspx
                    case VEMapStyle.Road:
                        this.globeControl.Host.DataSources.Remove("Texture", "Texture");
                        this.globeControl.Host.DataSources.Add(new DataSourceLayerData("Texture", "Texture", @"http://go.microsoft.com/fwlink/?LinkID=98770", DataSourceUsage.TextureMap));
                        break;
                    case VEMapStyle.Aerial:
                        this.globeControl.Host.DataSources.Remove("Texture", "Texture");
                        this.globeControl.Host.DataSources.Add(new DataSourceLayerData("Texture", "Texture", @"http://go.microsoft.com/fwlink/?LinkID=98771", DataSourceUsage.TextureMap));
                        break;
                    case VEMapStyle.Hybrid:
                        this.globeControl.Host.DataSources.Remove("Texture", "Texture");
                        this.globeControl.Host.DataSources.Add(new DataSourceLayerData("Texture", "Texture", @"http://go.microsoft.com/fwlink/?LinkID=98772", DataSourceUsage.TextureMap));
                        break;
                    default:
                        break;
                }
            }
        }

        void GlobeInitialized()
        {
            GraphicsEngine3D graphicsEngine = GetGraphicsEngine();
            graphicsEngine.CapFrameRate = false;
            graphicsEngine.Device.Settings.TargetFramesPerSecond = 0;

            //JRB 4-13-09: Updated URLS to use forward links per VE3D team blog: http://blogs.msdn.com/virtualearth3d/archive/2009/04/06/data-format-revision.aspx
            this.globeControl.Host.DataSources.Add(new DataSourceLayerData("Elevation", "Elevation", @"http://go.microsoft.com/fwlink/?LinkID=98774", DataSourceUsage.ElevationMap));

            //MapStyle setter sets Texture layer
            this.MapStyle = this.currentMapStyle;

            this.globeControl.Host.DataSources.Add(new DataSourceLayerData("Models", "Models", @"http://go.microsoft.com/fwlink/?LinkID=98775", DataSourceUsage.Model));

            Show3DCursor = false;
            ShowBuildings = true;
            ShowBuildingTextures = true;

            this.globeControl.Host.WorldEngine.ShowCursorLocationInformation = false;

            NotifyPropertyChanged("Display3DCursor");
            NotifyPropertyChanged("ShowBuildings");
            NotifyPropertyChanged("ShowBuildingTextures");

            globeControl.CameraChanged += globeControl_CameraChanged;

            StartVERendering();

            this.OnMapLoaded(EventArgs.Empty);
        }

        protected virtual void OnMapLoaded(EventArgs e)
        {
            UpdateDPFromMap();
            UpdateMapItems();

            if (MapLoaded != null)
            {
                MapLoaded(this, e);
            }
        }

        #endregion

        #region globeControl Camera Changed Event

        void globeControl_CameraChanged(object sender, EventArgs e)
        {
            isItemsPositionDirty = true;
        }

        #endregion

        #region Public Shape Management Methods

        public void SendToFront(VEShape shape)
        {
            int maxZIndex = int.MinValue;

            foreach (object item in Items)
            {
                VEMapItem container = ItemContainerGenerator.ContainerFromItem(item) as VEMapItem;

                if (container != null)
                {
                    VEShape shapeItem = VisualUtility.GetChildByType(container, typeof(VEShape)) as VEShape;
                    if (shapeItem != null)
                    {
                        if (shapeItem.ZIndex > maxZIndex)
                            maxZIndex = shapeItem.ZIndex;
                        shapeItem.ZIndex--;
                    }
                }
            }
            if (maxZIndex == int.MinValue)
                maxZIndex = 1;

            shape.ZIndex = maxZIndex + 1;
        }

        public void SendToBack(VEShape shape)
        {
            int minZIndex = int.MaxValue;

            foreach (object item in Items)
            {
                VEMapItem container = ItemContainerGenerator.ContainerFromItem(item) as VEMapItem;

                if (container != null)
                {
                    VEShape shapeItem = VisualUtility.GetChildByType(container, typeof(VEShape)) as VEShape;
                    if (shapeItem != null)
                    {
                        if (shapeItem.ZIndex < minZIndex)
                            minZIndex = shapeItem.ZIndex;
                        shapeItem.ZIndex++;
                    }
                }
            }

            if (minZIndex == int.MaxValue)
                minZIndex = -1;

            shape.ZIndex = minZIndex - 1;
        }

        public void SetItemsPositionDirty()
        {
            isItemsPositionDirty = true;
        }

        public void ForceUpdateItemPosition(VEShape item)
        {
            if (!_isMapLoaded)
                return;

            VEMapItem container = ItemContainerGenerator.ContainerFromItem(item) as VEMapItem;

            if (container == null)
            {
                container = ItemContainerGenerator.ContainerFromItem(item.DataContext) as VEMapItem;
            }

            if (container != null)
            {
                container.UpdatePosition(this);
            }
        }

        public void AddShape(VEShape shape, string layerId)
        {
            if (shape.ShapeType == VEShapeType.Polyline)
            {
                List<LatLonAlt> lla = new List<LatLonAlt>();

                for (int index = 0; index < shape.Points.Count; index++)
                {
                    VELatLong item = shape.Points[index];
                    lla.Add(item.ToLatLonAlt());
                }

                PolyInfo polylineStyle = PolyInfo.DefaultPolyline;
                polylineStyle.AltitudeMode = AltitudeMode.FromGround;

                this.globeControl.Host.Geometry.AddGeometry(
                    new PolylineGeometry(
                        layerId, Guid.NewGuid().ToString(),
                        lla.ToArray(),
                        polylineStyle));
            }
        }

        public void ClearGlobe()
        {
            this.globeControl.Host.Geometry.Clear();
        }

        #endregion

        #region Public Layer Management Methods

        public void ClearLayer(string layerId)
        {
            if (this.globeControl.Host.Geometry.ContainsLayer(layerId))
            {
                this.globeControl.Host.Geometry.ClearLayer(layerId);
            }
        }

        public void RemoveLayer(string layerId)
        {
            if (this.globeControl.Host.Geometry.ContainsLayer(layerId))
            {
                this.globeControl.Host.Geometry.RemoveLayer(layerId);
            }
        }

        /// <summary>
        /// Adds new layer if it doesn't exist. Otherwise clears it.
        /// </summary>
        /// <param name="layerId"></param>
        public void AddLayer(string layerId)
        {
            if (this.globeControl.Host.Geometry.ContainsLayer(layerId))
            {
                this.globeControl.Host.Geometry.ClearLayer(layerId);
            }
            else
            {
                this.globeControl.Host.Geometry.AddLayer(layerId);
            }
        }

        #endregion

        #region Public LatLong Transformation Methods

        public bool IsBehindPlanet(VELatLong latLong)
        {
            VELatLong cameraLatLong = GetCameraPosition();

            double distance = VELatLong.GreatCircleDistance(cameraLatLong, latLong);
            double angle = distance * 180.0 / (Math.PI * 6371007.0);

            Vector3D cameraPosition = cameraLatLong.ToVector();
            VELatLong highLatLong = new VELatLong(latLong.Latitude, latLong.Longitude, 10000);
            Vector3D pinPosition = highLatLong.ToVector();
            Vector3D vector = pinPosition - cameraPosition;

            Ray3D ray = new Ray3D(cameraPosition, vector);
            Vector3D groundPosition;

            return globeControl.Host.WorldEngine.IntersectsGround(ray, vector.Length(), out groundPosition);
        }

        public void RemoveRegisteredPosition(object key)
        {
            if (!registeredPositions.ContainsKey(key)) return;
            if (globeControl.Host == null) return;
            RegisteredPosition rp = registeredPositions[key];
            this.globeControl.Host.WorldEngine.RemoveLocationListener(rp);

            registeredPositions.Remove(key);
        }

        public void AddRegisteredPosition(object key, VELatLong latLong)
        {
            if (!registeredPositions.ContainsKey(key))
            {
                //Don't add surface elevation -- it is handled by the ILocationListener code in RegisteredPosition
                LatLonAlt lla = LatLonAlt.CreateUsingDegrees(latLong.Latitude, latLong.Longitude, latLong.Altitude);

                RegisteredPosition rp = new RegisteredPosition();

                rp.Position = lla;
                isItemsPositionDirty = true;


                //This call to update the surface elevation is slower than the ILocationListener way, but will only be needed
                //once in a while when the latLong is changed
                double surfaceElevation = this.globeControl.Host.WorldEngine.GetSurfaceElevation(lla.LatLon);

                LatLonAlt newLLA = lla;
                newLLA.Altitude += surfaceElevation;
                rp.Vector = newLLA.GetVector();

                this.globeControl.Host.WorldEngine.AddLocationListener(rp);

                registeredPositions.Add(key, rp);
            }
        }

        /// <summary>
        /// Use with LatLongToPoint if you are setting the Canvas.Left or Canvas.Top for placement within the map
        /// </summary>
        /// <param name="point">Control-relative point</param>
        /// <returns>Inner canvas-relative point</returns>
        public Point? OuterPointToInnerPoint(Point? point)
        {
            if (point == null ||
                !point.HasValue)
                return null;
            return this.TranslatePoint(point.Value, canvasPushPin);
        }

        /// <summary>
        /// Use with PointToLatLong if you need to convert the inner Canvas position to an control-relative point
        /// </summary>
        /// <param name="point">Inner canvas-relative point</param>
        /// <returns>Control-relative point</returns>
        public Point? InnerPointToOuterPoint(Point? point)
        {
            if (point == null ||
                !point.HasValue)
                return null;
            return canvasPushPin.TranslatePoint(point.Value, this);
        }

        public Point? LatLongToPoint(VELatLong latLong)
        {
            return LatLongToPoint(latLong, null);
        }

        public Point? LatLongToPoint(VELatLong latLong, object key)
        {
            return InnerPointToOuterPoint(LatLongToPointInternal(latLong, key));
        }

        internal Point? LatLongToPointInternal(VELatLong latLong, object key)
        {
            if (this.globeControl == null ||
                this.globeControl.PositionStep == null ||
                latLong == null)
            {
                return null;
            }

            System.Drawing.Point? position = null;

            if (key != null && registeredPositions.ContainsKey(key))
            {
                RegisteredPosition rp = registeredPositions[key];

                //If position changed, update the RegisteredPosition

                LatLonAlt lla = LatLonAlt.CreateUsingDegrees(latLong.Latitude, latLong.Longitude, latLong.Altitude);

                if (rp.Position.Latitude != lla.Latitude ||
                    rp.Position.Longitude != lla.Longitude ||
                    rp.Position.Altitude != lla.Altitude)
                {

                    //This call to update the surface elevation is slower than the ILocationListener way, but will only be needed
                    //once in a while when the latLong is changed
                    double surfaceElevation = this.globeControl.Host.WorldEngine.GetSurfaceElevation(lla.LatLon);

                    rp.Position = lla;
                    isItemsPositionDirty = true;
                    LatLonAlt newLLA = lla;
                    newLLA.Altitude += surfaceElevation;
                    rp.Vector = newLLA.GetVector();
                }
                if (!rp.Valid)
                    return null;
                position = this.globeControl.PositionStep.VectorToScreenPosition(rp.Vector);


            }
            else
            {
                //Add surface elevation
                double surfaceElevation = this.globeControl.Host.WorldEngine.GetSurfaceElevation(latLong.ToLatLonAlt().LatLon);
                LatLonAlt lla = LatLonAlt.CreateUsingDegrees(latLong.Latitude, latLong.Longitude, latLong.Altitude + surfaceElevation);

                position = this.globeControl.PositionStep.LatLongToScreenPosition(lla);
            }

            //System.Drawing.Point? position = this.globeControl.Host.Navigation.ScreenPositionFromLatLon(ll);

            if (position != null)
            {
                double x = (int)MathHelper.MapValue(position.Value.X,
                                         0,
                                         globeControl.Width,
                                         0,
                                         this.targetImage.ActualWidth);
                double y = (int)MathHelper.MapValue(position.Value.Y,
                                         0,
                                         globeControl.Height,
                                         0,
                                         this.targetImage.ActualHeight);
                return new Point(x, y);
            }

            return null;
        }

        public VELatLong PointToLatLong(Point? point)
        {
            if (point == null ||
                !point.HasValue)
                return null;

            if (this.globeControl == null ||
                this.globeControl.Host == null ||
                this.globeControl.Host.Navigation == null)
            {
                return null;
            }

            point = OuterPointToInnerPoint(point.Value);

            if (!point.HasValue)
                return null;

            LatLonAlt? lla = this.globeControl.Host.Navigation.LatLonAltFromScreenPosition(new System.Drawing.Point((int)point.Value.X, (int)point.Value.Y));

            if (lla.HasValue)
            {
                return new VELatLong(lla.Value.LatitudeDegrees, lla.Value.LongitudeDegrees);
            }
            else
            {
                return null;
            }
        }

        #endregion

        #region Public Globe Navigation Animation Methods

        public void FlyTo(VELatLong latLong, double pitch, double yaw)
        {
            VELatLong cameraPosition = GetCameraPosition();

            FlyTo(latLong, pitch, yaw, cameraPosition.Altitude, null);
        }

        public void FlyTo(VELatLong latLong, double pitch, double yaw, EventHandler callback)
        {
            VELatLong cameraPosition = GetCameraPosition();

            FlyTo(latLong, pitch, yaw, cameraPosition.Altitude, callback);
        }

        public void FlyTo(VELatLong latLong, double pitch, double yaw, double altitude)
        {
            FlyTo(latLong, pitch, yaw, altitude, null);
        }

        public void FlyTo(VELatLong latLong, double pitch, double yaw, double altitude, EventHandler callback)
        {

            CameraParameters cam = new CameraParameters();
            cam.Speed = 1;
            cam.AccelPeriod = .2;
            cam.DecelPeriod = .9;

            if (callback != null)
            {
                int eventID = 1000;

                foreach (KeyValuePair<int, EventHandler> kvp in onFlyToEndCallbacks)
                {
                    if (kvp.Key >= eventID)
                    {
                        eventID = kvp.Key + 1;
                    }
                }
                cam.Event = eventID;
                onFlyToEndCallbacks.Add(eventID, callback);
            }

            this.globeControl.Host.CommunicationManager.AttachToEvent(EngineEvents.Group,
                                                                      EngineEvents.OnFlyToEnd,
                                                                      "OnFlyToEnd",
                                                                      OnFlyToEnd);

            LatLonAlt lla = latLong.ToLatLonAlt();
            lla.Altitude = altitude;

            this.globeControl.Host.Navigation.FlyTo(lla, pitch, yaw, false, cam);
        }

        private void OnFlyToEnd(string functionName, CommunicationParameter param)
        {
            this.Dispatcher.BeginInvoke(new CommunicationManager.EngineEvent(OnFlyToEndWorker),
                                        new object[] { functionName, param });
        }

        private void OnFlyToEndWorker(string functionName, CommunicationParameter param)
        {
            if (functionName == EngineEvents.OnFlyToEnd)
            {
                CommunicationParameterSet set = param.Value as CommunicationParameterSet;
                if (set != null)
                {
                    OnFlyToEndStatus status = (OnFlyToEndStatus)set["status"].Value;
                    int eventID = (int)set["id"].Value;

                    if (status == OnFlyToEndStatus.AlreadyThere ||
                        status == OnFlyToEndStatus.Arrived)
                    {
                        foreach (KeyValuePair<int, EventHandler> kvp in onFlyToEndCallbacks)
                        {
                            if (kvp.Key == eventID && kvp.Value != null)
                            {
                                kvp.Value(this, EventArgs.Empty);
                            }
                        }
                    }

                    if (onFlyToEndCallbacks.ContainsKey(eventID))
                        onFlyToEndCallbacks.Remove(eventID);
                }
            }
        }

        public void JumpTo(VELatLong latLong, double roll, double pitch, double yaw, double altitude)
        {
            JumpCameraController jumpCamera = new JumpCameraController(globeControl.Host, this.globeControl.Host.CameraControllers.Default);

            LatLonAlt lla = latLong.ToLatLonAlt();
            lla.Altitude = altitude;

            RollPitchYaw rpy = new RollPitchYaw(roll * Math.PI / 180.0,
                                                pitch * Math.PI / 180.0,
                                                yaw * Math.PI / 180.0);

            jumpCamera.SetNext(lla, rpy);

            this.globeControl.Host.CameraControllers.Current = jumpCamera;
        }

        public void CancelFlyTo()
        {
            ActionCameraController cam2 = this.globeControl.Host.CameraControllers.Current as ActionCameraController;

            if (cam2 != null)
            {
                cam2.CancelChildAnimations();
            }
        }

        #endregion

        #region Public Camera Control Methods

        public VELatLong GetCameraPosition()
        {
            if (this.globeControl.Host.Navigation.CameraPosition == null)
                return null;

            LatLonAlt position = this.globeControl.Host.Navigation.CameraPosition.Location;

            //Convert from radians to degrees
            VELatLong ret = new VELatLong(position.Latitude * 180.0 / Math.PI, position.Longitude * 180.0 / Math.PI, position.Altitude, VEAltMode.FromDatum);

            return ret;
        }

        public VERollPitchYaw GetCameraOrientation()
        {
            ActionCameraController camera = this.globeControl.Host.CameraControllers.Current as ActionCameraController;
            if (camera != null && camera.LastReportedViewpoint != null && camera.LastReportedViewpoint.LocalOrientation != null)
            {

                return new VERollPitchYaw(camera.LastReportedViewpoint.LocalOrientation.RollPitchYaw);
            }
            return null;
        }

        public System.Windows.Media.Media3D.Vector3D GetCameraVector()
        {
            if (this.globeControl.Host.Navigation.CameraPosition == null)
                return new System.Windows.Media.Media3D.Vector3D(0, 0, 0);

            Vector3D vec = this.globeControl.Host.Navigation.CameraPosition.Vector;

            return new System.Windows.Media.Media3D.Vector3D(vec.X, vec.Y, vec.Z);
        }

        public CameraController<PredictiveCamera> GetCameraController()
        {
            return this.globeControl.Host.CameraControllers.Current;
        }

        public void SetCameraController(CameraController<PredictiveCamera> newCamera)
        {
            if (newCamera != null)
            {
                this.globeControl.Host.CameraControllers.Current = newCamera;
            }
        }

        public void ResetCameraController()
        {
            this.globeControl.Host.CameraControllers.Current = this.globeControl.Host.CameraControllers.Default;
        }

        #endregion

        #region Private Helper Functions

        private void UpdateMapFromDP()
        {
            if (_isMapLoaded)
                JumpTo(this.VELatLong, this.Roll, this.Pitch, this.Yaw, this.Altitude);
        }

        private void UpdateDPFromMap()
        {
            VELatLong oldLatLong = new VELatLong(this.LatLong.X, this.LatLong.Y, this.Altitude, VEAltMode.FromDatum);
            VERollPitchYaw oldRPY = new VERollPitchYaw(this.Roll, this.Pitch, this.Yaw);

            VELatLong newLatLong = GetCameraPosition();
            if (newLatLong == null)
                return;

            VERollPitchYaw newRPY = GetCameraOrientation();
            if (newRPY == null)
                return;

            this.LatLong = new Point(newLatLong.Latitude, newLatLong.Longitude);
            this.Altitude = newLatLong.Altitude;
            this.Roll = newRPY.Roll;
            this.Pitch = newRPY.Pitch;
            this.Yaw = newRPY.Yaw;

            if (this.CameraChanged != null)
            {
                VECameraChangedEventArgs eventArgs = new VECameraChangedEventArgs(oldLatLong, newLatLong, oldRPY, newRPY);

                if (eventArgs.IsChanged)
                {
                    CameraChanged(this, eventArgs);
                }
            }
        }

        #endregion

        #region Internal Reflection Helpers

        internal static E ReadPrivateVariable<C, E>(C myClass, string element)
        {
            try
            {
                Type t = myClass.GetType();
                FieldInfo fieldInfo = t.GetField(element,
                    BindingFlags.NonPublic | BindingFlags.Instance);

                return (E)fieldInfo.GetValue(myClass);
            }
            catch (Exception)
            {
                return default(E);
            }
        }

        internal static E InvokePrivateMethod<C, E>(C myClass, string methodName, Object[] paramList)
        {
            try
            {
                Type t = myClass.GetType();
                MethodInfo methodInfo = t.GetMethod(methodName,
                    BindingFlags.NonPublic | BindingFlags.Instance);

                return (E)methodInfo.Invoke(myClass, paramList);
            }
            catch (Exception)
            {
                return default(E);
            }
        }

        #endregion

        #region Private d3dImage Methods

        private GraphicsEngine3D GetGraphicsEngine()
        {
            Host h = globeControl.Host;

            GraphicsEngine3D graphicsEngine = null;

            graphicsEngine = ReadPrivateVariable<Host, GraphicsEngine3D>(h, "graphicsEngine");
            return graphicsEngine;
        }

        private void CreateVESurface()
        {
            GraphicsEngine3D graphicsEngine = GetGraphicsEngine();

            if (graphicsEngine == null)
                return;

            Microsoft.MapPoint.Graphics3D.Types.Surface surfSrc = null;

            IntPtr ret = IntPtr.Zero;

            try
            {
                surfSrc = graphicsEngine.Device.GetRenderTarget(0);
                if (surfSrc != null)
                {

                    surfCpy = graphicsEngine.Device.CreateRenderTarget(surfSrc.Description.Width,
                                                                                   surfSrc.Description.Height,
                                                                                   surfSrc.Description.Format,
                                                                                   surfSrc.Description.MultipleSampleType,
                                                                                   0, false);
                    if (surfCpy != null)
                    {
                        veSurfaceSize.Width = surfCpy.Description.Width;
                        veSurfaceSize.Height = surfCpy.Description.Height;
                    }

                }
            }
            finally
            {
                if (surfSrc != null)
                    surfSrc.Dispose();
            }

        }

        /// <summary>
        /// Gets pointer to the Virtual Earth D3D backbuffer
        /// </summary>
        /// <returns></returns>
        private IntPtr GetSourceSurfacePtr()
        {
            GraphicsEngine3D graphicsEngine = GetGraphicsEngine();

            if (graphicsEngine == null)
                return IntPtr.Zero;

            Microsoft.MapPoint.Graphics3D.Types.Surface surfSrc = null;

            IntPtr ret = IntPtr.Zero;

            try
            {

                surfSrc = graphicsEngine.Device.GetRenderTarget(0);

                if (surfCpy == null)
                {
                    CreateVESurface();
                }

                if (surfSrc != null && surfCpy != null)
                {
                    graphicsEngine.Device.StretchRectangle(surfSrc, new System.Drawing.Rectangle(0, 0, surfSrc.Description.Width, surfSrc.Description.Height),
                                                           surfCpy, new System.Drawing.Rectangle(0, 0, surfSrc.Description.Width, surfSrc.Description.Height));

                    Microsoft.MapPoint.GraphicsAPI.Graphics.Surface internalSurf = null;

                    internalSurf = ReadPrivateVariable<Microsoft.MapPoint.Graphics3D.Types.Surface,
                                            Microsoft.MapPoint.GraphicsAPI.Graphics.Surface>(surfCpy, "internalSurface");

                    if (internalSurf != null)
                    {

                        //NativePointer is hidden from intellisense
                        ret = internalSurf.NativePointer;
                    }
                }

            }
            finally
            {
                if (surfSrc != null)
                    surfSrc.Dispose();
            }

            return ret;
        }

        private void d3dImage_IsFrontBufferAvailableChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // if the front buffer is available, then WPF has just created a new
            // D3D device, so we need to start rendering our custom scene
            if (d3dImage.IsFrontBufferAvailable)
            {
                StartVERendering();
            }
            else
            {
                // If the front buffer is no longer available, then WPF has lost 
                // its D3D device so there is no reason to waste cycles rendering
                // our custom scene until a new device is created.
                StopVERendering();
            }

        }

        private void StartVERendering()
        {
            if (!isTemplateLoaded)
                return;

            if (d3dImage.IsFrontBufferAvailable)
            {
                CompositionTargetEx.FrameUpdating += CompositionTargetEx_FrameUpdating;

                GraphicsEngine3D graphicsEngine = GetGraphicsEngine();

                if (graphicsEngine != null)
                {
                    graphicsEngine.PostRender += new EventHandler(graphicsEngine_PostRender);

                    CreateVESurface();

                    veSurface = GetSourceSurfacePtr();
                    InvalidateVESurface();
                }

                if (VEUpdateWorker != null)
                {
                    VEUpdateWorker.CancelAsync();
                }
                VEUpdateWorker = new BackgroundWorker();
                VEUpdateWorker.DoWork += new DoWorkEventHandler(VEUpdateWorker_DoWork);
                VEUpdateWorker.WorkerSupportsCancellation = true;
                VEUpdateWorker.RunWorkerAsync();
            }
        }

        private void StopVERendering()
        {
            // This method is called when WPF loses its D3D device.
            // In such a circumstance, it is very likely that we have lost 
            // our custom D3D device also, so we should just release the scene.
            // We will create a new scene when a D3D device becomes 
            // available again.

            CompositionTargetEx.FrameUpdating -= CompositionTargetEx_FrameUpdating;
            stopwatch.Stop();

            GraphicsEngine3D graphicsEngine = GetGraphicsEngine();

            graphicsEngine.PostRender -= graphicsEngine_PostRender;

            VEUpdateWorker.CancelAsync();

            veSurface = IntPtr.Zero;

            if (surfCpy != null)
            {
                surfCpy.Dispose();
                surfCpy = null;
            }

        }

        void VEUpdateWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var worker = sender as BackgroundWorker;
            int numMilliseconds = 30;
            while (!worker.CancellationPending)
            {
                if (!stopwatch.IsRunning)
                    stopwatch.Start();
                if (stopwatch.ElapsedMilliseconds > numMilliseconds)
                {
                    stopwatch.Reset();

                    this.globeControl.Host.NeedUpdate();

                    this.globeControl.Host.RenderEngine.ManuallyRenderNextFrame();

                    Dispatcher.BeginInvoke((System.Action)delegate
                    {
                        UpdateMapItems();
                    }, DispatcherPriority.Render);
                }
                Thread.Sleep(TimeSpan.FromMilliseconds(Math.Max(1, numMilliseconds - stopwatch.ElapsedMilliseconds)));
            }
        }

        void CompositionTargetEx_FrameUpdating(object sender, RenderingEventArgs e)
        {
            if (!IsMapLoaded)
                return;

            if (isMapPositionDirty)
            {
                UpdateMapFromDP();
            }
            else
            {
                UpdateDPFromMap();
            }
            isMapPositionDirty = false;
        }

        private void graphicsEngine_PostRender(object sender, EventArgs e)
        {
            //Get the direct3d9 pointer
            veSurface = GetSourceSurfacePtr();

            if (this.globeControl.InvokeRequired)
            {
                this.Dispatcher.BeginInvoke(new DelegateGlobeRedraw(InvalidateVESurface), DispatcherPriority.Render);
            }
            else
            {
                InvalidateVESurface();
            }
        }

        private void CountFrames()
        {
            if (!fpsStopwatch.IsRunning)
                fpsStopwatch.Start();
            frameCount++;
            if (fpsStopwatch.ElapsedMilliseconds >= 1000)
            {
                double fps = frameCount / fpsStopwatch.Elapsed.TotalSeconds;
                Trace.WriteLine("VE FPS: " + fps);
                frameCount = 0;
                fpsStopwatch.Reset();
            }
        }

        private void InvalidateVESurface()
        {
            CountFrames();
            if (d3dImage.IsFrontBufferAvailable && veSurface != IntPtr.Zero)
            {
                try
                {
                    //Set the d3dImage to the slimDX pointer and invalidate the surface
                    d3dImage.Lock();
                    d3dImage.SetBackBuffer(D3DResourceType.IDirect3DSurface9, veSurface);
                    d3dImage.AddDirtyRect(new Int32Rect(0, 0, (int)veSurfaceSize.Width, (int)veSurfaceSize.Height));
                    d3dImage.Unlock();
                }
                catch (ArgumentException ex)
                {
                    _isMapLoaded = false;

                    //Backbuffer might be lost after sleeping
                    Debug.WriteLine("Error in InvalidateVESurface(): " + ex.ToString());
                    StopVERendering();

                    //Re-init everything
                    InitVEMap();
                }
            }
        }

        // Positions all WPF items in the Items collection in proper lla position over top of the 
        public void UpdateMapItems()
        {
            if (!_isMapLoaded)
                return;

            foreach (object item in Items)
            {
                VEMapItem container = ItemContainerGenerator.ContainerFromItem(item) as VEMapItem;

                if (container != null)
                {
                    container.UpdatePosition(this);
                }
            }
            isItemsPositionDirty = false;
        }

        #endregion

        #region UI Event Handlers

        private void viewbox_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            NotifyPropertyChanged("GlobeWidth");
            NotifyPropertyChanged("GlobeHeight");
            isItemsPositionDirty = true;
        }

        #endregion

    }
}
