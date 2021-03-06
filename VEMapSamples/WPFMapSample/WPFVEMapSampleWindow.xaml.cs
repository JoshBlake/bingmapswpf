﻿/**************************************************
 * InfoStrat.VE
 * Copyright (c) 2009 
 * Information Strategies
 * www.infostrat.com
 * 
 * This project is covered by the LGPL license:
 * http://virtualearthwpf.codeplex.com/license
 * 
 * This library has NO WARRANTY and is UNSUPPORTED
 * but check in at http://virtualearthwpf.codeplex.com
 * if you need help.
 * 
 * This copyright notice must be preserved.
 * 
 **************************************************/

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

            //This binding won't update till later for some reason
            cbxCursor.IsChecked = false;
            
            map.CameraChanged += new EventHandler<VECameraChangedEventArgs>(map_CameraChanged);
        }

        void map_CameraChanged(object sender, VECameraChangedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("camera changed: A:" + e.IsAltitudeChanged + " La:" + e.IsLatitudeChanged + " Lo:" + e.IsLongitudeChanged + " P:" + e.IsPitchChanged + " R:" + e.IsRollChanged + " Y:" + e.IsYawChanged);
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

        private void VEPushPin_Click(object sender, VEPushPinClickedEventArgs e)
        {
            VEPushPin pin = sender as VEPushPin;

            map.FlyTo(pin.LatLong, -90, 0, 300, null);
        }

        #endregion

    }
}
