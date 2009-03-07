using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace InfoStrat.VE
{
    public class VEPushPinClickedEventArgs : RoutedEventArgs
    {
        private Point pushPinScreenPosition;

        public Point PushPinScreenPosition
        {
            get
            {
                return pushPinScreenPosition;
            }
            set
            {
                pushPinScreenPosition = value;
            }
        }

        private EventArgs eventArgs;
        
        public EventArgs EventArgs
        {
            get
            {
                return eventArgs;
            }
            set
            {
                eventArgs = value;
            }

        }

        public VEPushPinClickedEventArgs()
        {
            this.eventArgs = null;
        }

        public VEPushPinClickedEventArgs(EventArgs eventArgs)
        {
            this.eventArgs = eventArgs;
        }
    }
}
