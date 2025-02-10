using Raylib_cs;
using static Raylib_cs.Raylib;
using System.Numerics;
using System;
using System.Diagnostics;

namespace DIMEN
{
    public class Program
    {
        public static unsafe void Main(string[] args)
        {
            // Initialisation
            InitWindow(1600, 900, "Raylib 3D in C#");
            SetTargetFPS(60);
            DisableCursor(); // Désactiver le curseur

            Shaders.Init();
            PBRMaterial pBRMaterial = new PBRMaterial("assets/textures/metal/2K/");
            Model cubeModel = LoadModel("assets/objects/Cube.obj"); // Charger le modèle
            Model obstacleModel = LoadModel("assets/objects/Obstacle.obj"); // Charger le modèle
            for (int i = 0; i < cubeModel.MeshCount; i++)
            {
                GenMeshTangents(ref cubeModel.Meshes[i]);
                GenMeshTangents(ref obstacleModel.Meshes[i]);
            }
            cubeModel.Materials[0] = pBRMaterial.Material;
            obstacleModel.Materials[0] = pBRMaterial.Material;


            // Configuration de la caméra
            Camera3D camera = new Camera3D
            {
                Position = new Vector3(10, 10, 10),
                Target = new Vector3(0.0f, 0.0f, 0.0f),
                Up = new Vector3(0.0f, 1.0f, 0.0f),
                FovY = 45.0f,
                Projection = CameraProjection.Perspective
            };

            // Dimensions
            Vector3 playerDimensions = new Vector3(1);
            Vector3 obstacelDimensions = new Vector3(8);
            Vector3 colliderObstacleDimension = obstacelDimensions / 2;
            Vector2 planeSize = new Vector2(300, 300);


            // Variables pour la gestion du mouvement et de la rotation
            float targetRotationAngle = 0f;
            float currentRotationAngle = 0f;
            float rotationSpeed = 5f;

            // Variables pour la gravité et la physique
            Vector3 playerVelocity = Vector3.Zero;
            float gravity = -0.2f;
            bool isFalling = false;
            const float GRIDLEVEL = 0.5f;
            float groundLevel = GRIDLEVEL;
            float friction = 0.9f;
            float deceleration = 0.65f; // Facteur de ralentissement
            float speed = 0.01f; // Vitesse maximale du joueur

            // Positions des objets
            Vector3 playerPosition = new Vector3(5,1,5);
            Vector3 obstaclePosition = new Vector3(0,0,0);
            Vector3 planePosition = new Vector3(0, 0, 0);


            BoundingBox obstacleBox = GetMeshBoundingBox(obstacleModel.Meshes[0]);

            BoundingBox playerBox = GetMeshBoundingBox(cubeModel.Meshes[0]);
            

            // Variables pour la vue plan et le cooldown
            bool isTopView = false;
            float topViewDuration = 5.0f;
            float topViewTimer = 0.0f;
            float topViewCooldown = 10.0f;
            float progress = 1.0f; // Commence rempli (prêt à être utilisé)
            bool filling = false; // Indique si la jauge est en train de se remplir

            double lastTopViewTime = -topViewCooldown;
            double currentTime;

        

            // Boucle principale
            while (!WindowShouldClose())
            {
                Shaders.UpdatePBRLighting(camera.Position);
               
                float deltaTime = GetFrameTime();
                currentTime = GetTime();
                
                // Gestion de la barre de progression (remplissage ou vidage)
                if (isTopView)
                {
                    
                    progress -= deltaTime / topViewDuration;
                    if (progress <= 0.0f)
                    {
                        progress = 0.0f;
                        isTopView = false;
                        lastTopViewTime = currentTime;
                        filling = true;
                    }
                }
                else if (filling)
                {
                    progress += deltaTime / topViewCooldown;
                    if (progress >= 1.0f)
                    {
                        progress = 1.0f;
                        filling = false;
                    }
                }
                // Interpolation fluide de la rotation
                currentRotationAngle = Raymath.Lerp(currentRotationAngle, targetRotationAngle, rotationSpeed * deltaTime);
                // Calcul de la direction de la caméra
                Vector3 cameraDirection = Vector3.Normalize(camera.Target - camera.Position);
                if (IsKeyPressed(KeyboardKey.E) && !isTopView)
                {
                    targetRotationAngle += 90f;
                }
                // Calcul de l'angle et direction du mouvement
                float radians = MathF.PI * currentRotationAngle / 180f;
                Vector3 moveDirection = new Vector3(MathF.Sin(radians), 0, MathF.Cos(radians));
                Vector3 strafeDirection = new Vector3(MathF.Cos(radians), 0, -MathF.Sin(radians));

                // Mise à jour de la position du joueur
                playerPosition += Raymath.Vector3Normalize(playerVelocity) / 10;
                // Déplacements du joueur (avant/arrière et latéraux)
                if (IsKeyDown(KeyboardKey.W) || IsKeyDown(KeyboardKey.Up)) playerVelocity += moveDirection * 0.1f * friction;
                if (IsKeyDown(KeyboardKey.S) || IsKeyDown(KeyboardKey.Down)) playerVelocity -= moveDirection * 0.1f * friction;
                if (IsKeyDown(KeyboardKey.D) || IsKeyDown(KeyboardKey.Left)) playerVelocity -= strafeDirection * 0.1f * friction;
                if (IsKeyDown(KeyboardKey.A) || IsKeyDown(KeyboardKey.Right)) playerVelocity += strafeDirection * 0.1f * friction;

                // Appliquer un ralentissement progressif si aucune touche n'est pressée
                if (IsKeyUp(KeyboardKey.W) && IsKeyUp(KeyboardKey.S) && IsKeyUp(KeyboardKey.A) && IsKeyUp(KeyboardKey.D))
                {
                    playerVelocity *= deceleration; // Ralentissement progressif
                    if (playerVelocity.Length() < 0.01f) playerVelocity = Vector3.Zero; // Éviter une vitesse résiduelle
                }

                if (CheckCollisionBoxes(playerBox, obstacleBox))
                {
                    Vector3 tempPos = playerPosition + new Vector3(playerVelocity.X, 0, 0);
                    BoundingBox tempBox = new BoundingBox(tempPos - new Vector3(0.3f), tempPos + new Vector3(0.3f));
                    if (!CheckCollisionBoxes(tempBox, obstacleBox))
                    {
                        playerPosition.X += playerVelocity.X;
                    }
                    // Tester le glissement sur Z
                    tempPos = playerPosition + new Vector3(0, 0, playerVelocity.Z);
                    tempBox = new BoundingBox(tempPos - new Vector3(0.3f), tempPos + new Vector3(0.3f));
                    if (!CheckCollisionBoxes(tempBox, obstacleBox))
                    {
                        playerPosition.Z += playerVelocity.Z;
                    }
                }
                else
                {
                    playerPosition += playerVelocity;
                }

                if (isTopView)
                {
                    if (CheckCollisionBoxes(playerBox, obstacleBox))
                    {
                        playerPosition.Y = obstacelDimensions.Y; // Place le joueur au sommet
                    }
                }

                // Limiter la vitesse maximale
                if (playerVelocity.Length() > speed)
                {
                    playerVelocity = Vector3.Normalize(playerVelocity) * speed;
                }

                if (playerVelocity.Y > gravity)
                {
                    playerVelocity.Y = gravity;
                }

                // Appliquer la gravité uniquement si le joueur n'est pas sur le sol
                if (playerPosition.Y > groundLevel && !CheckCollisionBoxes(obstacleBox, playerBox))
                {
                    playerVelocity.Y += gravity * deltaTime;  // Applique la gravité uniquement en l'air
                    isFalling = true;
                }
                else
                {
                    playerVelocity.Y = 0f;  // Réinitialise la vitesse verticale quand il touche le sol
                    playerPosition.Y = groundLevel;  // Maintien le joueur sur le sol
                    isFalling = false;
                }

                // Changer de mode de vue (vue de dessus ou perspective)
                if (IsKeyPressed(KeyboardKey.Q) && progress >= 1.0f)
                {
                    isTopView = true;
                    topViewTimer = 0.0f;
                    filling = false;
                }

                if (isTopView)
                {  
                    topViewTimer += deltaTime;
                    if (topViewTimer >= topViewDuration)
                    {
                        isTopView = false; // Désactiver la vue de dessus après 5 secondes
                        filling = true; // Démarrer le remplissage après la désactivation
                    }
                    camera.FovY = 20.0f;
                    camera.Position = new Vector3(playerPosition.X, 200.0f, playerPosition.Z);
                    camera.Target = new Vector3(playerPosition.X + moveDirection.X, 0.0f, playerPosition.Z + moveDirection.Z);
                    camera.Up = new Vector3(0.0f, 1.0f, 0.0f);
                    camera.Projection = CameraProjection.Orthographic;
                    float timeRemaining = MathF.Max(0, topViewDuration - topViewTimer);
                }
                else
                {
                    //playerPosition.Y = UpdateGroundLevel(groundLevel, playerPosition, playerVelocity);
                    Vector3 cameraOffset = new Vector3(
                        -MathF.Sin(radians) * 10.0f,
                        1.0f,
                        -MathF.Cos(radians) * 10.0f
                    );
                    camera.FovY = 45.0f;
                    camera.Position = playerPosition + cameraOffset;
                    camera.Target = playerPosition;
                    camera.Up = new Vector3(0.0f, 1.0f, 0.0f);
                    camera.Projection = CameraProjection.Perspective;
                }

                // Dessin de la scène
                BeginDrawing();
                ClearBackground(Color.RayWhite);
                BeginMode3D(camera);
                DrawPlane(planePosition, planeSize, Color.Gray);

                DrawModel(cubeModel, playerPosition, 1, Color.White);
                DrawModel(obstacleModel, obstaclePosition, 1, Color.White);

                DrawCubeWires(playerPosition, playerDimensions.X, playerDimensions.Y, playerDimensions.Z, Color.Red);
                DrawCubeWires(obstaclePosition, obstacelDimensions.X, obstacelDimensions.Y, obstacelDimensions.Z, Color.Red);

                DrawSphere(Shaders.Light1.Position, 1.0f, Color.Orange);
                DrawSphere(Shaders.Light2.Position, 1.0f, Color.Orange);
                EndMode3D();
                // Dessin de la barre de progression
                DrawRectangle(10, 10, 200, 25, Color.LightGray);
                DrawRectangle(10, 10, (int)(200 * progress), 25, Color.Green);
                DrawRectangleLines(10, 10, 200, 25, Color.Black);
                // Debug
                //DrawText($"Position: X:{playerPosition.X:F2}, Y:{playerPosition.Y:F2}, Z:{playerPosition.Z:F2}", 10, 10, 20, Color.Black);
                DrawText($"FPS: {GetFPS()}", 10, 30, 20, Color.Black);
                DrawText($"Player Speed: {playerVelocity}", 10, 50, 20, Color.Black);
                DrawText($"Is player falling: {isFalling}", 10, 70, 20, Color.Black); 
                DrawText($"Is player touching obstacle: {CheckCollisionBoxes(obstacleBox, playerBox)}", 10, 90, 20, Color.Black); 
                

                EndDrawing();
            }
            // Déchargement des ressources
            CloseWindow();
        }

    }
    
    

}
