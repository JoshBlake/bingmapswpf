using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.MapPoint.Rendering3D.Utility;
using Microsoft.MapPoint.Rendering3D;
using Microsoft.MapPoint.Rendering3D.Cameras;
using Microsoft.MapPoint.Rendering3D.Control;
using Microsoft.MapPoint.Graphics3D;
using Microsoft.MapPoint.Geometry.VectorMath;

namespace InfoStrat.VE
{
    class JumpCameraController : CameraController<PredictiveCamera>
    {
        #region Class Members

        Host host;
        LatLonAlt position;
        RollPitchYaw orientation;

        #endregion

        #region Constructors

        public JumpCameraController(Host host, CameraController<PredictiveCamera> nextCamera)
            : base()
        {
            this.host = host;
            this.Next = nextCamera;

            if (host != null &&
                host.Navigation != null &&
                host.Navigation.CameraSnapshot != null &&
                host.Navigation.CameraSnapshot.Position != null &&
                host.Navigation.CameraSnapshot.Orientation != null)
            {
                this.SetNext(host.Navigation.CameraSnapshot.Position.Location, host.Navigation.CameraSnapshot.Orientation.RollPitchYaw);
            }
        }

        public JumpCameraController(Host host, CameraController<PredictiveCamera> nextCamera, LatLonAlt nextPosition, RollPitchYaw nextOrientation)
            : base()
        {
            this.host = host;
            this.Next = nextCamera;

            this.SetNext(nextPosition, nextOrientation);
        }

        #endregion

        #region Public Methods

        public void SetNext(LatLonAlt nextPosition, RollPitchYaw nextOrientation)
        {
            this.position = nextPosition;
            this.orientation = nextOrientation;
        }

        public override void MoveCamera(Microsoft.MapPoint.Rendering3D.Scene.SceneState sceneState)
        {
            this.Camera.Viewpoint.Position.Location = position;
            this.Camera.Viewpoint.LocalOrientation.RollPitchYaw = orientation;
            HasArrived = true;
        }

        #endregion

    }
}
