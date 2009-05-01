using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.MapPoint.Rendering3D.Scene;
using Microsoft.MapPoint.Rendering3D.State;
using Microsoft.MapPoint.Geometry.VectorMath;
using System.Drawing;
using Microsoft.MapPoint.Rendering3D;

namespace InfoStrat.VE
{
    internal class PositionStep : Step
    {
        bool isReady = false;
        Matrix4x4D transform;
        Size view;

        public PositionStep(StepManager stepManager) 
            : base (stepManager)
        {
            transform = new Matrix4x4D();
            view = new Size();
        }

        public override void OnExecute(SceneState state) // runs every frame
        {
            CameraData camera;
            if (state.TryGetData<CameraData>(out camera))
            {
                transform = camera.Snapshot.TransformMatrix;
                view = camera.View.Size;

                isReady = true;
            }
        }

        public Point? LatLongToScreenPosition(LatLonAlt ll)
        {
            return VectorToScreenPosition(ll.GetVector());
        }

        public Point? VectorToScreenPosition(Vector3D vector)
        {
            if (!isReady)
            {
                return null;
            }

            Vector4D v = new Vector4D(vector, 1.0);

            v = Vector4D.TransformCoordinate(ref v, ref transform);
            if (v.W > 0)
            {
                v.MultiplyBy(1.0 / v.W);
                
                Point position = new Point(
                        (int)(view.Width * (v.X + 1.0) * 0.5),
                        (int)(view.Height * (1.0 - v.Y) * 0.5));

                // check if the projected position is outside the bounds of the view

                if (position.X < 0 ||
                    position.X > view.Width ||
                    position.Y < 0 ||
                    position.Y > view.Height)
                {
                    return null;
                }

                return position;
            }
            else
            {
                // position is behind camera
                return null;
            }
        }

    }
}
