using Raylib_cs;
using System;
using System.Numerics;

namespace DIMEN
{
    internal class DimensionCamera
    {
        public Camera3D Camera;
        public Vector3 PlayerPosition { get; set; }
        public float Radians { get; set; }
        public Vector3 MoveDirection { get; set; }

        private Camera3D targetCamera;
        private float transitionTime;
        private float transitionDuration;
        private bool isTopView; // Suivre l'état actuel de la caméra (vue par défaut ou vue du dessus)

        public DimensionCamera()
        {
            Camera = DefaultPosition();
            targetCamera = Camera; // Initialement, la caméra cible est la même que la caméra actuelle
            isTopView = false; // La vue initiale est par défaut
            transitionTime = 0.0f;
            transitionDuration = 0.5f; // Durée de la transition
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
            targetCamera = new Camera3D
            {
                FovY = 20.0f,
                Position = new Vector3(PlayerPosition.X, 200.0f, PlayerPosition.Z),
                Target = new Vector3(PlayerPosition.X + MoveDirection.X, 0.0f, PlayerPosition.Z + MoveDirection.Z),
                Up = new Vector3(0.0f, 1.0f, 0.0f),
                Projection = CameraProjection.Orthographic
            };

            // Commence la transition en ajustant le temps
            transitionTime = 0.0f;
            return targetCamera;
        }

        // Méthode pour alterner entre les vues
        public void ToggleCameraView()
        {
            isTopView = !isTopView; // Alterne l'état de la caméra
            if (isTopView)
            {
                targetCamera = TopViewPosition(); // Passer à la vue top
            }
            else
            {
                targetCamera = DefaultPosition(); // Revenir à la vue par défaut
            }

            // Réinitialiser le temps de transition
            transitionTime = 0.0f;
        }

        public void UpdateCamera(float deltaTime)
        {
            // Interpoler entre les positions si une transition est en cours
            if (transitionTime < transitionDuration)
            {
                transitionTime += deltaTime;

                float transitionProgress = MathF.Min(transitionTime / transitionDuration, 1.0f);

                // Interpoler la position de la caméra
                Camera.Position = Vector3.Lerp(Camera.Position, targetCamera.Position, transitionProgress);
                Camera.Target = Vector3.Lerp(Camera.Target, targetCamera.Target, transitionProgress);
                Camera.Up = Vector3.Lerp(Camera.Up, targetCamera.Up, transitionProgress);
                Camera.FovY = Raymath.Lerp(Camera.FovY, targetCamera.FovY, transitionProgress);
                Camera.Projection = targetCamera.Projection; // Laisser la projection changer instantanément
            }
        }
    }
}
