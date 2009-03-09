using System;
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

[assembly: CLSCompliant(true)]
namespace InfoStrat.VE
{
    public class VEMap : ItemsControl, INotifyPropertyChanged, IDisposable
    {
        #region UIElements

        Viewbox viewbox;
        Grid hostGrid;
        Image targetImage;
        D3DImage d3dImage;
        Canvas canvasPushPin;

        #endregion

        #region Class Members

        VEWindow winVE;

        IntPtr veSurface;
        IntPtr veSurfaceSrc;
        Size veSurfaceSize;

        bool isTemplateLoaded;
        bool isControlLoaded;
        bool isDependencyPropertiesDirty;
        
        private delegate void DelegateGlobeRedraw();
        private delegate void DelegateGlobeRendered();
        
        Microsoft.MapPoint.Graphics3D.Types.Surface surfCpy;

        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

        int numFrames = 0;

        Dictionary<int, EventHandler> onFlyToEndCallbacks;

        private PublicEventsGlobeControl globeControl;

        private VEMapStyle currentMapStyle;
        
        #endregion

        #region Public Events

        public event EventHandler MapLoaded;

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
                //Are we in Visual Studio Designer?  Is the control loaded yet?
                if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this) && isControlLoaded)
                {
                    switch (value)
                    {
                        case VEMapStyle.Road:
                            this.globeControl.Host.DataSources.Remove("Texture", "Texture");
                            this.globeControl.Host.DataSources.Add(new DataSourceLayerData("Texture", "Texture", @"http://maps.live.com//Manifests/RT.xml", DataSourceUsage.TextureMap));
                            break;
                        case VEMapStyle.Aerial:
                            this.globeControl.Host.DataSources.Remove("Texture", "Texture");
                            this.globeControl.Host.DataSources.Add(new DataSourceLayerData("Texture", "Texture", @"http://maps.live.com//Manifests/AT.xml", DataSourceUsage.TextureMap));
                            break;
                        case VEMapStyle.Hybrid:
                            this.globeControl.Host.DataSources.Remove("Texture", "Texture");
                            this.globeControl.Host.DataSources.Add(new DataSourceLayerData("Texture", "Texture", @"http://maps.live.com//Manifests/HT.xml", DataSourceUsage.TextureMap));
                            break;
                        default:
                            break;
                    }
                }

                this.currentMapStyle = value;

                NotifyPropertyChanged("MapStyle");
            }
        }

        public bool Show3DCursor
        {
            get
            {
                if (this.globeControl != null &&
                    this.globeControl.Host != null &&
                    this.globeControl.Host.WorldEngine != null)
                {
                    return this.globeControl.Host.WorldEngine.Display3DCursor;
                }
                else
                {
                    return false;
                }
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
                if (this.globeControl != null &&
                    this.globeControl.Host != null &&
                    this.globeControl.Host.WorldEngine != null)
                {
                    return this.globeControl.Host.WorldEngine.ShowBuildings;
                }
                else
                {
                    return false;
                }
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


            map.isDependencyPropertiesDirty = true;
        }

        #endregion

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
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
            if (disposing)
            {
                //Free managed resources
                if (globeControl != null)
                {
                    globeControl.Dispose();
                    globeControl = null;
                }
            }
        }

        #endregion

        #region Constructors

        static VEMap()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VEMap), new FrameworkPropertyMetadata(typeof(VEMap)));
        }

        public VEMap()
        {
            //InitializeComponent();
            viewbox = null;
            hostGrid = null;
            targetImage = null;
            d3dImage = null;
            canvasPushPin = null;

            isTemplateLoaded = false;
            isControlLoaded = false;
            
            isDependencyPropertiesDirty = false;

            //Are we in Visual Studio Designer?
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }

            this.winVE = new VEWindow();
            
            surfCpy = null;

            onFlyToEndCallbacks = new Dictionary<int, EventHandler>();

            veSurface = IntPtr.Zero;
            veSurfaceSrc = IntPtr.Zero;

            this.globeControl = new PublicEventsGlobeControl();
            
            winVE.winFormsHost.Child = this.globeControl;

            //Must show window briefly to init VE drawing contexts
            winVE.Show();
            winVE.Hide();

            this.Loaded += new RoutedEventHandler(VEMap_Loaded);
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

            StartVERendering();

        }

        #endregion
        
        #region Public Map Movement Helpers

        public virtual void DoMapMove(double dx, double dy, bool isContinuous)
        {
            Microsoft.MapPoint.Geometry.VectorMath.Vector2D delta = new Microsoft.MapPoint.Geometry.VectorMath.Vector2D();
            delta.X = -dx;
            delta.Y = -dy;
            globeControl.Host.BindingsManager.NavControl.NavMove(delta);

            if (!isContinuous)
            {
                delta.X = 0;
                delta.Y = 0;
                globeControl.Host.BindingsManager.NavControl.NavMove(delta);
            }
        }

        public virtual void DoMapZoom(double zoom, bool isContinuous)
        {
            globeControl.Host.BindingsManager.NavControl.NavZoom(zoom);

            if (!isContinuous)
            {
                globeControl.Host.BindingsManager.NavControl.NavZoom(0);
            }
        }

        #endregion

        #region Control Input Events

        protected virtual void DoControlInputDown(Point p)
        {
            if (this.globeControl != null)
            {
                int x = (int)MapValue(p.X, 0, this.ActualWidth, 0, globeControl.Width);
                int y = (int)MapValue(p.Y, 0, this.ActualHeight, 0, globeControl.Height);

                MouseEventArgs eventArgs = new MouseEventArgs(MouseButtons.Left, 1, x, y, 0);
                globeControl.DoMouseDown(eventArgs);
            }
        }

        protected virtual void DoControlInputUp(Point p)
        {
            if (this.globeControl != null)
            {
                int x = (int)MapValue(p.X, 0, this.ActualWidth, 0, globeControl.Width);
                int y = (int)MapValue(p.Y, 0, this.ActualHeight, 0, globeControl.Height);

                MouseEventArgs eventArgs = new MouseEventArgs(MouseButtons.Left, 1, x, y, 0);
                globeControl.DoMouseUp(eventArgs);
            }
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

            if (this.globeControl != null)
            {
                int x = (int)MapValue(p.X, 0, this.ActualWidth, 0, globeControl.Width);
                int y = (int)MapValue(p.Y, 0, this.ActualHeight, 0, globeControl.Height);
                MouseEventArgs eventArgs;

                if (isHover)
                    eventArgs = new MouseEventArgs(MouseButtons.None, 0, x, y, 0);
                else
                    eventArgs = new MouseEventArgs(MouseButtons.Left, 1, x, y, 0);

                globeControl.DoMouseMove(eventArgs);
            }
        }

        protected virtual void DoControlInputDoubleClick(Point p)
        {
            if (this.globeControl != null)
            {
                int x = (int)MapValue(p.X, 0, this.ActualWidth, 0, globeControl.Width);
                int y = (int)MapValue(p.Y, 0, this.ActualHeight, 0, globeControl.Height);

                MouseEventArgs eventArgs = new MouseEventArgs(MouseButtons.Left, 2, x, y, 0);
                globeControl.DoMouseDoubleClick(eventArgs);
            }
        }

        protected virtual void DoControlInputWheelZoom(Point p, int delta)
        {
            if (this.globeControl != null)
            {
                int x = (int)MapValue(p.X, 0, this.ActualWidth, 0, globeControl.Width);
                int y = (int)MapValue(p.Y, 0, this.ActualHeight, 0, globeControl.Height);

                MouseEventArgs eventArgs = new MouseEventArgs(MouseButtons.None, 0, x, y, delta);
                globeControl.DoMouseWheel(eventArgs);
            }
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
            isControlLoaded = true;

            //Are we in Visual Studio Designer?
            if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {

                this.globeControl.Host.DataSources.Add(new DataSourceLayerData("Elevation", "Elevation", @"http://maps.live.com//Manifests/HD.xml", DataSourceUsage.ElevationMap));
                
                //this.globeControl.Host.DataSources.Add(new DataSourceLayerData("Texture", "Texture", @"http://maps.live.com//Manifests/RT.xml", DataSourceUsage.TextureMap));
                //MapStyle setter sets Texture layer
                this.MapStyle = this.currentMapStyle;

                this.globeControl.Host.DataSources.Add(new DataSourceLayerData("Models", "Models", @"http://maps.live.com//Manifests/MO.xml", DataSourceUsage.Model));
                

                this.globeControl.Host.RenderEngine.FirstFrameRendered += new EventHandler(RenderEngine_FirstFrameRendered);

                Show3DCursor = false;
                ShowBuildings = true;

            }
        }

        void VEMap_Unloaded(object sender, RoutedEventArgs e)
        {
            isControlLoaded = false;

            this.winVE.Close();
        }

        #endregion

        #region Map Loaded Event

        void RenderEngine_FirstFrameRendered(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new DelegateGlobeRendered(GlobeRendered));
        }

        void GlobeRendered()
        {
            this.OnMapLoaded(new EventArgs());
        }

        protected virtual void OnMapLoaded(EventArgs e)
        {
            UpdateDPFromMap();
            
            if (MapLoaded != null)
            {
                MapLoaded(this, e);
            }
        }

        #endregion

        #region Public Shape Management Methods
        
        public void AddShape(VEShape shape, string layerId)
        {
            if (shape.ShapeType == VEShapeType.Polyline)
            {
                List<LatLonAlt> lla = new List<LatLonAlt>();

                foreach (VELatLong item in shape.Points)
                {
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
            if (globeControl.Host.WorldEngine.IntersectsGround(ray, vector.Length(), out groundPosition))
            {
                return true;
            }

            return false;
        }

        public Point? LatLongToPoint(VELatLong latLong)
        {
            if (this.globeControl == null ||
                this.globeControl.Host == null ||
                this.globeControl.Host.Navigation == null)
            {
                return null;
            }

            LatLon ll = LatLon.CreateUsingDegrees(latLong.Latitude, latLong.Longitude);


            System.Drawing.Point? position = this.globeControl.Host.Navigation.ScreenPositionFromLatLon(ll);

            if (position != null)
            {
                double x = (int)MapValue(position.Value.X,
                                         0,
                                         globeControl.Width,
                                         0,
                                         targetImage.Width); //ActualWidth
                double y = (int)MapValue(position.Value.Y,
                                         0,
                                         globeControl.Height,
                                         0,
                                         targetImage.Height); //ActualHeight
                return new System.Windows.Point(x, y);
            }
            else
            {
                return null;
            }
        }

        public VELatLong PointToLatLong(Point? point)
        {
            if (point == null)
                return null;

            if (this.globeControl == null ||
                this.globeControl.Host == null ||
                this.globeControl.Host.Navigation == null)
            {
                return null;
            }

            double x = (int)MapValue(point.Value.X,
                                         0,
                                         targetImage.ActualWidth,
                                         0,
                                         globeControl.Width);
            double y = (int)MapValue(point.Value.Y,
                                     0,
                                     targetImage.ActualHeight,
                                     0,
                                     globeControl.Height);

            LatLonAlt? lla = this.globeControl.Host.Navigation.LatLonAltFromScreenPosition(new System.Drawing.Point((int)x, (int)y));

            if (lla != null)
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

            Microsoft.MapPoint.Rendering3D.Control.CameraParameters cam = new Microsoft.MapPoint.Rendering3D.Control.CameraParameters();
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
                            if (kvp.Key == eventID)
                            {
                                if (kvp.Value != null)
                                {
                                    kvp.Value(this, EventArgs.Empty);
                                }
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
            
            Microsoft.MapPoint.Geometry.VectorMath.Vector3D vec = this.globeControl.Host.Navigation.CameraPosition.Vector;

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
            JumpTo(this.VELatLong, this.Roll, this.Pitch, this.Yaw, this.Altitude);
        }

        private void UpdateDPFromMap()
        {
            VELatLong ll = GetCameraPosition();
            if (ll == null)
                return;

            VERollPitchYaw rpy = GetCameraOrientation();
            if (rpy == null)
                return;

            this.LatLong = new Point(ll.Latitude, ll.Longitude);
            this.Altitude = ll.Altitude;
            this.Roll = rpy.Roll;
            this.Pitch = rpy.Pitch;
            this.Yaw = rpy.Yaw;
        }

        protected static double MapValue(double value, double fromMin, double fromMax, double toMin, double toMax)
        {
            //Normalize
            double ret = value / (fromMax - fromMin);
            //Resize and translate
            return ret * (toMax - toMin) + toMin;
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
                GraphicsEngine3D graphicsEngine = GetGraphicsEngine();

                if (graphicsEngine != null)
                {
                    graphicsEngine.PostRender += new EventHandler(graphicsEngine_PostRender);

                    CreateVESurface();

                    veSurface = GetSourceSurfacePtr();
                    InvalidateVESurface();
                }

            }
        }

        private void StopVERendering()
        {
            // This method is called when WPF loses its D3D device.
            // In such a circumstance, it is very likely that we have lost 
            // our custom D3D device also, so we should just release the scene.
            // We will create a new scene when a D3D device becomes 
            // available again.

            GraphicsEngine3D graphicsEngine = GetGraphicsEngine();

            graphicsEngine.PostRender -= graphicsEngine_PostRender;

            veSurface = IntPtr.Zero;

            if (surfCpy != null)
            {
                surfCpy.Dispose();
                surfCpy = null;
            }

        }

        private void graphicsEngine_PostRender(object sender, EventArgs e)
        {
            sw.Start();
            //Get the direct3d9 pointer
            veSurface = GetSourceSurfacePtr();

            if (sw.IsRunning)
            {
                numFrames++;

                //sw.Stop();
                //System.Diagnostics.Debug.WriteLine("redraw internal: " + (numFrames / sw.Elapsed.TotalSeconds));
                //sw.Reset();
            }


            if (globeControl.InvokeRequired)
            {
                globeControl.BeginInvoke(new DelegateGlobeRedraw(InvalidateVESurface));
            }
            else
            {
                InvalidateVESurface();
            }
        }

        private void InvalidateVESurface()
        {


            if (d3dImage.IsFrontBufferAvailable && veSurface != IntPtr.Zero)
            {

                //Set the d3dImage to the slimDX pointer and invalidate the surface
                d3dImage.Lock();
                d3dImage.SetBackBuffer(D3DResourceType.IDirect3DSurface9, veSurface);
                d3dImage.AddDirtyRect(new Int32Rect(0, 0, (int)veSurfaceSize.Width, (int)veSurfaceSize.Height));
                d3dImage.Unlock();
            }

            //TODO: find proper method to see if view changed.  Zooming can be continuous...
            //if (pushpinDirty)
            {
                RaiseViewChanged();
            }

        }

        private void RaiseViewChanged()
        {
            foreach (object o in this.Items)
            {
                if (o is VEPushPin)
                {

                    VEPushPin pin = o as VEPushPin;
                    pin.UpdatePosition(this);
                }
            }
            if (isControlLoaded)
            {
                if (isDependencyPropertiesDirty)
                {
                    UpdateMapFromDP();
                }
                else
                {
                    UpdateDPFromMap();
                }

                isDependencyPropertiesDirty = false;
            }
        }

        #endregion

        #region UI Event Handlers

        private void viewbox_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            NotifyPropertyChanged("GlobeWidth");
            NotifyPropertyChanged("GlobeHeight");
        }

        #endregion

    }
}
