using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.MapPoint.Rendering3D.Cameras;
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
        
        private CameraData oldData;
        public event EventHandler<EventArgs> CameraChanged;

        public PositionStep(StepManager stepManager)
            : base(stepManager)
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

                if ((oldData == null) || (!AreSnapshotsEqual(oldData.Snapshot, camera.Snapshot)))
                {
                    CameraChanged(this,new EventArgs());                       
                }
       
                oldData = camera;
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
        private bool AreSnapshotsEqual(GeodeticCameraSnapshot c1, GeodeticCameraSnapshot c2)
        {
            if (c1.AspectRatio != c2.AspectRatio) return false;
            if (c1.FarClipPlane != c2.FarClipPlane) return false;
            if (c1.FieldOfViewY != c2.FieldOfViewY) return false;
            if (c1.NearClipPlane != c2.NearClipPlane) return false;
            if (!c1.InverseTransformMatrix.Equals(c2.InverseTransformMatrix)) return false;
            if (!c1.LocalOrientation.Equals(c2.LocalOrientation)) return false;
            if (!c1.Orientation.Equals(c2.Orientation)) return false;
            if (!c1.Position.Equals(c2.Position)) return false;
            if (!c1.ProjectionMatrix.Equals(c2.ProjectionMatrix)) return false;
            if (!c1.TransformMatrix.Equals(c2.TransformMatrix)) return false;
            if (!c1.ViewMatrix.Equals(c2.ViewMatrix)) return false;
            return true;
        }

    }

}