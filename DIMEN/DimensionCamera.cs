using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DIMEN
{
    internal class DimensionCamera
    {
        public Camera3D Camera { get; set; }
        public Vector3 PlayerPosition { get; set; }
        public float Radians { get; set; }
        public Vector3 MoveDirection { get; set; }


        public DimensionCamera ()
        {
            Camera = DefaultPosition();
        }
        public DimensionCamera (Vector3 playerPosition, float radians, Vector3 moveDirection)
        {
            Camera = DefaultPosition();
            PlayerPosition = playerPosition;
            Radians = radians;
            MoveDirection = moveDirection;
        }
        public Camera3D DefaultPosition()
        {
            Vector3 cameraOffset = new Vector3(
                    -MathF.Sin(Radians) * 10.0f,
                    1.0f,
                    -MathF.Cos(Radians) * 10.0f
            );
            Camera = new Camera3D
            {
                FovY = 45.0f,
                Position = PlayerPosition + cameraOffset,
                Target = PlayerPosition,
                Up = new Vector3(0.0f, 1.0f, 0.0f),
                Projection = CameraProjection.Perspective
            };
            return Camera;
        }
        public Camera3D TopViewPosition()
        {
            MoveDirection = new Vector3(MathF.Sin(Radians), 0, MathF.Cos(Radians));
            Camera = new Camera3D
            {
                FovY = 20.0f,
                Position = new Vector3(PlayerPosition.X, 200.0f, PlayerPosition.Z),
                Target = new Vector3(PlayerPosition.X + MoveDirection.X, 0.0f, PlayerPosition.Z + MoveDirection.Z),
                Up = new Vector3(0.0f, 1.0f, 0.0f),
                Projection = CameraProjection.Orthographic
            };
            return Camera;
        }
    }

}
