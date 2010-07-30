using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Surface.Presentation;
using System.Windows;
using InfoStrat.VE;

namespace InfoStrat.VE.NUI
{
    [Obsolete("SurfaveVEPushPinClickedEventArgs is now obsolete. Use regular VEPushPinClickedEventArgs.")]
    public class SurfaceVEPushPinClickedEventArgs : VEPushPinClickedEventArgs
    {
        private ContactEventArgs contactEventArgs;

        public new ContactEventArgs EventArgs
        {
            get
            {
                return contactEventArgs;
            }
            set
            {
                contactEventArgs = value;
            }
        }

        public SurfaceVEPushPinClickedEventArgs()
        {
            this.contactEventArgs = null;
        }

        public SurfaceVEPushPinClickedEventArgs(ContactEventArgs contactEventArgs)
        {
            this.contactEventArgs = contactEventArgs;
            this.RoutedEvent = contactEventArgs.RoutedEvent;
        }
    }
}
