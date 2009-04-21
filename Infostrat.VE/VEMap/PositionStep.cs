using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.MapPoint.Rendering3D.Scene;
using Microsoft.MapPoint.Rendering3D.State;
using Microsoft.MapPoint.Geometry.VectorMath;
using System.Drawing;

namespace InfoStrat.VE
{
    internal class PositionStep : Step
    {
        List<RegisteredPosition> positions;

        public PositionStep(StepManager stepManager) 
            : base (stepManager)
        {
            positions = new List<RegisteredPosition>();
        }

        public override void OnExecute(SceneState state) // runs every frame
        {
            CameraData camera;
            if (state.TryGetData<CameraData>(out camera))
            {
                Matrix4x4D transform = camera.Snapshot.TransformMatrix;
                Size view = camera.View.Size;
                Vector3D cameraPosition = camera.Snapshot.Position.Vector;
                Vector3D cameraAt = camera.Snapshot.Orientation.LookAt;

                foreach (RegisteredPosition position in positions)
                {
                    Vector4D v = new Vector4D(position.Vector, 1.0);

                    v = Vector4D.TransformCoordinate(ref v, ref transform);
                    if (v.W > 0)
                    {
                        v.MultiplyBy(1.0 / v.W);

                        position.Screen = new Point(
                                (int)(view.Width * (v.X + 1.0) * 0.5),
                                (int)(view.Height * (1.0 - v.Y) * 0.5));

                        // TODO: could also check if the projected position is outside the bounds of the view,
                        // depending on the size of the object in screen space

                        position.InView = true;
                    }
                    else
                    {
                        // position is behind camera
                        position.InView = false;
                    }
                }
            }
        }

    }
}
