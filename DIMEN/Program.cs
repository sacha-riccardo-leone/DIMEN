using Raylib_cs;
using static Raylib_cs.Raylib;
using System.Numerics;
using System;

namespace DIMEN
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Initialisation
            InitWindow(1600, 900, "Raylib 3D in C#");
            SetTargetFPS(60);
            // Désactiver le curseur
            DisableCursor();

            // Configuration de la caméra
            Camera3D camera = new Camera3D
            {
                Position = new Vector3(10, 10, 10.0f), // Position de la caméra
                Target = new Vector3(0.0f, 0.0f, 0.0f), // Point regardé
                Up = new Vector3(0.0f, 1.0f, 0.0f),    // Orientation "haut"
                FovY = 45.0f,                           // Champ de vision
                Projection = CameraProjection.Perspective  // Perspective
            };

            // Position initiale du cube (le joueur)
            Vector3 playerPosition = new Vector3(0, 1, 0); // Position du cube
            Vector3 cubePosition = new Vector3(3, 1, 3);   // Position du cube
            float rotationAngle = 0f; // Angle de rotation du cube
            float targetRotationAngle = 0f; // Angle cible
            float rotationProgress = 0f; // Progression de la transition
            float rotationDuration = 0.5f; // Durée de la transition en secondes
            float elapsedTime = 0f; // Temps écoulé

            while (!WindowShouldClose())
            {
                // Calculer le temps écoulé depuis le dernier frame
                float deltaTime = GetFrameTime();
                elapsedTime += deltaTime;

                // Vérifier si la touche espace est pressée pour faire tourner le joueur
                if (IsKeyPressed(KeyboardKey.Space))
                {
                    targetRotationAngle += 90f; // Tourner de +90 degrés
                    if (targetRotationAngle > 360f) targetRotationAngle -= 360f; // Réinitialiser l'angle à 0 après 360°
                    rotationProgress = 0f; // Réinitialiser la progression
                    elapsedTime = 0f; // Réinitialiser le temps écoulé
                }

                // Interpolation entre l'angle actuel et l'angle cible
                if (elapsedTime < rotationDuration)
                {
                    rotationProgress = elapsedTime / rotationDuration; // Progression de la transition
                }
                else
                {
                    rotationProgress = 1f; // Lorsque la durée est terminée, la transition est complète
                }

                // Calculer l'angle de rotation interpolé
                rotationAngle = Raymath.Lerp(rotationAngle, targetRotationAngle, rotationProgress);

                // Calculer la rotation sur le joueur (playerPosition)
                float radians = MathF.PI * rotationAngle / 180f; // Conversion de l'angle en radians

                // Déterminer la direction de mouvement en fonction de l'angle de rotation
                Vector3 moveDirection = new Vector3(MathF.Sin(radians), 0, MathF.Cos(radians));

                // Appliquer le mouvement du joueur
                if (IsKeyDown(KeyboardKey.W)) playerPosition += moveDirection * 0.1f; // Déplacement en avant
                if (IsKeyDown(KeyboardKey.S)) playerPosition -= moveDirection * 0.1f; // Déplacement en arrière

                // Mises à jour de la caméra pour suivre le joueur
                Vector3 cameraOffset = new Vector3(10.0f, 5.0f, 10.0f); // Décalage de la caméra par rapport au joueur
                camera.Position = playerPosition + cameraOffset;
                camera.Target = playerPosition; // La caméra regarde toujours le joueur

                // Mises à jour de la caméra
                UpdateCamera(ref camera, CameraMode.Orbital);

                // Dessin
                BeginDrawing();
                ClearBackground(Color.RayWhite);

                BeginMode3D(camera);
                DrawGrid(30, 1.0f); // Grille de repère

                // Dessin du cube rouge sur le plan
                DrawCube(cubePosition, 1.0f, 1.0f, 1.0f, Color.Red); // Cube rouge représentant le joueur

                // Dessiner le cube bleu qui représente la "face" du joueur
                // La position du cube bleu est ajustée pour le placer devant le joueur
                Vector3 rotatedPosition = playerPosition + new Vector3(MathF.Sin(radians), 0, MathF.Cos(radians)) * 1.0f;
                DrawCube(rotatedPosition, 1.0f, 1.0f, 1.0f, Color.Blue); // Cube bleu représentant la face du joueur
                EndMode3D();

                EndDrawing();
            }

            // Fermeture
            CloseWindow();
        }
    }
}
