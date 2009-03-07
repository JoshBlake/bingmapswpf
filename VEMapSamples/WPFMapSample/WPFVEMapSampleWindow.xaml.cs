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
using System.Windows.Navigation;
using System.Windows.Shapes;
using InfoStrat.VE;

namespace WPFVEMapSample
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class WPFVEMapSampleWindow : Window
    {
        #region Constructors

        public WPFVEMapSampleWindow()
        {
            InitializeComponent();
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
            map.DoMapMove(0, 1000, false);
        }

        private void btnRight_Click(object sender, RoutedEventArgs e)
        {
            map.DoMapMove(-1000, 0, false);
        }

        private void btnDown_Click(object sender, RoutedEventArgs e)
        {
            map.DoMapMove(0, -1000, false);
        }

        private void btnLeft_Click(object sender, RoutedEventArgs e)
        {
            map.DoMapMove(1000, 0, false);
        }

        private void btnZoomIn_Click(object sender, RoutedEventArgs e)
        {
            map.DoMapZoom(1000, false);
        }

        private void btnZoomOut_Click(object sender, RoutedEventArgs e)
        {
            map.DoMapZoom(-1000, false);
        }

        private void VEPushPin_Click(object sender, VEPushPinClickedEventArgs e)
        {
            VEPushPin pin = sender as VEPushPin;

            map.FlyTo(pin.LatLong, -90, 0, 300, null);
        }

        #endregion

    }
}
