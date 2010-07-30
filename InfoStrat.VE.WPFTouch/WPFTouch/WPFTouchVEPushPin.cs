using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Media.Animation;
using InfoStrat.VE;
using System.Windows.Input;

namespace InfoStrat.VE.WPFTouch
{
    public class WPFTouchVEPushPin : VEPushPin
    {
        //TODO: implement if you want different behaviors for a push-pin other than the default
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            Button = this.Template.FindName("PART_button", this) as Button;

            if (Button != null)
            {
                Button.IsManipulationEnabled = true;
                Button.ManipulationDelta += new EventHandler<System.Windows.Input.ManipulationDeltaEventArgs>(Button_ManipulationDelta);
                Button.ManipulationCompleted += new EventHandler<System.Windows.Input.ManipulationCompletedEventArgs>(Button_ManipulationCompleted);
            }
        }

        void Button_ManipulationDelta(object sender, System.Windows.Input.ManipulationDeltaEventArgs e)
        {
            e.Handled = true;


            bool isInBounds = true;

            Rect bounds = new Rect(new Point(0, 0), Button.RenderSize);
            foreach (IManipulator manipulator in e.Manipulators)
            {
                Point p = manipulator.GetPosition(Button);
                if (!bounds.Contains(p))
                {
                    isInBounds = false;
                    break;
                }
            }


            if (!isInBounds)
            {
                e.Cancel();
            }
        }

        void Button_ManipulationCompleted(object sender, System.Windows.Input.ManipulationCompletedEventArgs e)
        {
            e.Handled = true;
            
            Button_Click(this, e);
            
        }

    }
}