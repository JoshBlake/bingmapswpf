using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.Surface;
using Microsoft.Surface.Presentation;
using Microsoft.Surface.Presentation.Controls;
using InfoStrat.VE;
using InfoStrat.VE.NUI;

namespace SurfaceVEMapSample
{
    /// <summary>
    /// Interaction logic for SurfaceWindow1.xaml
    /// </summary>
    public partial class SurfaceVEMapSampleWindow : SurfaceWindow
    {
        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SurfaceVEMapSampleWindow()
        {
            InitializeComponent();
            // Add handlers for Application activation events
            AddActivationHandlers();
            map.CameraChanged += new EventHandler<VECameraChangedEventArgs>(map_CameraChanged);
        }

        void map_CameraChanged(object sender, VECameraChangedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("camera changed: A:" + e.IsAltitudeChanged + " La:" + e.IsLatitudeChanged + " Lo:" + e.IsLongitudeChanged + " P:" + e.IsPitchChanged + " R:" + e.IsRollChanged + " Y:" + e.IsYawChanged);
        }

        #endregion

        #region Activation Methods

        /// <summary>
        /// Occurs when the window is about to close. 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            // Remove handlers for Application activation events
            RemoveActivationHandlers();
        }

        /// <summary>
        /// Adds handlers for Application activation events.
        /// </summary>
        private void AddActivationHandlers()
        {
            // Subscribe to surface application activation events
            ApplicationLauncher.ApplicationActivated += OnApplicationActivated;
            ApplicationLauncher.ApplicationPreviewed += OnApplicationPreviewed;
            ApplicationLauncher.ApplicationDeactivated += OnApplicationDeactivated;
        }

        /// <summary>
        /// Removes handlers for Application activation events.
        /// </summary>
        private void RemoveActivationHandlers()
        {
            // Unsubscribe from surface application activation events
            ApplicationLauncher.ApplicationActivated -= OnApplicationActivated;
            ApplicationLauncher.ApplicationPreviewed -= OnApplicationPreviewed;
            ApplicationLauncher.ApplicationDeactivated -= OnApplicationDeactivated;
        }

        #endregion

        #region Activation Handler Events

        /// <summary>
        /// This is called when application has been activated.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnApplicationActivated(object sender, EventArgs e)
        {
            //TODO: enable audio, animations here
        }

        /// <summary>
        /// This is called when application is in preview mode.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnApplicationPreviewed(object sender, EventArgs e)
        {
            //TODO: Disable audio here if it is enabled

            //TODO: optionally enable animations here
        }

        /// <summary>
        ///  This is called when application has been deactivated.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnApplicationDeactivated(object sender, EventArgs e)
        {
            //TODO: disable audio, animations here
        }

        #endregion

        #region UI Event Handlers

        private void map_MapLoaded(object sender, EventArgs e)
        {
        }

        private void btnStyleRoad_Click(object sender, RoutedEventArgs e)
        {
            map.MapStyle = InfoStrat.VE.VEMapStyle.Road;
        }

        private void btnStyleHybrid_Click(object sender, RoutedEventArgs e)
        {
            map.MapStyle = InfoStrat.VE.VEMapStyle.Hybrid;
        }

        private void btnStyleAerial_Click(object sender, RoutedEventArgs e)
        {
            map.MapStyle = InfoStrat.VE.VEMapStyle.Aerial;
        }

        private void btnUp_Click(object sender, RoutedEventArgs e)
        {
            map.DoMapMove(0, 50);
        }

        private void btnRight_Click(object sender, RoutedEventArgs e)
        {
            map.DoMapMove(-50, 0);
        }

        private void btnDown_Click(object sender, RoutedEventArgs e)
        {
            map.DoMapMove(0, -50);
        }

        private void btnLeft_Click(object sender, RoutedEventArgs e)
        {
            map.DoMapMove(50, 0);
        }

        private void btnZoomIn_Click(object sender, RoutedEventArgs e)
        {
            map.DoMapZoom(50);
        }

        private void btnZoomOut_Click(object sender, RoutedEventArgs e)
        {
            map.DoMapZoom(-50);
        }
        
        private void SurfacePushPin_Click(object sender, SurfaceVEPushPinClickedEventArgs e)
        {
            VEPushPin pin = sender as VEPushPin;
            
            map.FlyTo(pin.LatLong, -90, 0, 300, null);
        }

        private void btnTiltMode_Click(object sender, RoutedEventArgs e)
        {
            if (btnTiltMode.IsChecked.HasValue)
            {
                if (btnTiltMode.IsChecked.Value)
                {
                    this.map.MapManipulationMode = MapManipulationMode.TiltSpinZoomPivot;
                }
                else
                {
                    this.map.MapManipulationMode = MapManipulationMode.PanZoomPivot;
                }
            }
        }
        #endregion
    }
}