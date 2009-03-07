using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.MapPoint.Rendering3D;
using System.Windows.Forms;

namespace InfoStrat.VE
{
    [CLSCompliant(false)]
    public class PublicEventsGlobeControl : GlobeControl
    {
        public void DoKeyDown(KeyEventArgs e)
        {
            this.OnKeyDown(e);
        }

        public void DoKeyUp(KeyEventArgs e)
        {
            this.OnKeyUp(e);
        }

        public void DoMouseDown(MouseEventArgs e)
        {
            this.OnMouseDown(e);
        }

        public void DoMouseUp(MouseEventArgs e)
        {
            this.OnMouseUp(e);
        }

        public void DoMouseDoubleClick(MouseEventArgs e)
        {
            this.OnMouseDoubleClick(e);
        }

        public void DoMouseMove(MouseEventArgs e)
        {
            this.OnMouseMove(e);            
        }

        public void DoMouseWheel(MouseEventArgs e)
        {
            this.OnMouseWheel(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
        }
    }
}