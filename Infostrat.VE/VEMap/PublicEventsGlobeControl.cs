﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.MapPoint.Rendering3D;
using System.Windows.Forms;
using Microsoft.MapPoint.Rendering3D.Steps;

namespace InfoStrat.VE
{
    [CLSCompliant(false)]
    public class PublicEventsGlobeControl : GlobeControl
    {
        PositionStep positionStep = null;
        public event EventHandler<EventArgs> CameraChanged;

        internal PositionStep PositionStep
        {
            get
            {
                return positionStep;
            }
        }

        public PublicEventsGlobeControl()
            : base()
        {
        }

        public PublicEventsGlobeControl(GlobeControlInitializationOptions options)
            : base(options)
        {
        }

        protected override void Dispose(bool disposing)
        {
            // wpf calls this twice, and VE doesn't handle it correctly, 
            // bug :(

            if (this.IsDisposed)
                return;

            if (Host != null &&
                Host.RenderEngine != null)
                Host.RenderEngine.ManuallyUninitializeRender();

            base.Dispose(disposing);
        }
        
        protected override void  OnUIThreadYield(int timeoutInMilliseconds)
        {
            // do nothing, we use this to prevent UI thread starvation 
            // but it is not applicable, kind of a bug too :(
        }

        public void InitRenderEngine()
        {
            this.ManualInitialize();
            if (!this.Host.Ready)
            {
                this.Host.RenderEngine.ManuallyInitializeRender();
                this.Host.RenderEngine.ManuallyRenderNextFrame();
                this.Host.RenderEngine.ManuallyRenderNextFrame(); //make sure it is ready
            }
            positionStep = new PositionStep(this.Host.RenderEngine.StepManager);
            positionStep.CameraChanged += positionStep_CameraChanged;
            this.Host.RenderEngine.StepManager.InsertBefore(typeof(RenderStep), positionStep);

        }

        private void positionStep_CameraChanged(object sender, EventArgs args)
        {
            CameraChanged(sender, args);
        }
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