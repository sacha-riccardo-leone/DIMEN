using Raylib_cs;
using static Raylib_cs.Raylib;
using System.Numerics;
using System;
using System.Diagnostics;
using System.Reflection;

namespace DIMEN
{
    public class Program
    {

        public static unsafe void InitModels(Model model, PBRMaterial material)
        {
            for (int i = 0; i < model.MeshCount; i++)
            {
                GenMeshTangents(ref model.Meshes[i]);
            }
            if (model.MaterialCount > 0)
            {
                model.Materials[0] = material.Material;
            }
        }
        public static unsafe void Main(string[] args)
        {
            // Initialisation
            InitWindow(1600, 900, "Raylib 3D in C#");
            SetTargetFPS(60);
            DisableCursor(); // Désactiver le curseur

            Shaders.Init();
            
            PBRMaterial defaultMaterial = new PBRMaterial("assets/textures/default/");
            PBRMaterial playerMaterial = new PBRMaterial("assets/textures/metal/player/");
            PBRMaterial doorMaterial = new PBRMaterial("assets/textures/metal/door/");
            PBRMaterial obstacleMaterial = new PBRMaterial("assets/textures/metal/obstacle/");
            PBRMaterial KeyMaterial = new PBRMaterial("assets/textures/key/");
            Material skyBox = Shaders.LoadSkybox("assets/skybox/skybox.hdr");

            Model playerModel = LoadModel("assets/objects/Cube.obj");
            Model keyModel = LoadModel("assets/objects/Old_Key.obj");
            Model obstacleModel = LoadModel("assets/objects/Obstacle.obj");
            Model openDoorModel = LoadModel("assets/objects/opendoor.obj");
            Model closedDoorModel = LoadModel("assets/objects/closeddoor.obj");

            InitModels(playerModel, playerMaterial);
            InitModels(obstacleModel, defaultMaterial);
            InitModels(keyModel, KeyMaterial);
            InitModels(openDoorModel, doorMaterial);
            InitModels(closedDoorModel, doorMaterial);

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
            Vector3 obstacleDimensions = new Vector3(16);
            Vector3 keyDimensions = new Vector3(0.5f, 1f, 0.5f);
            Vector2 planeSize = new Vector2(300, 300);
            Vector3 doorDimensions = new Vector3(1.4f, 2.5f, 0.5f);

            Vector3 colliderObstacleDimension = obstacleDimensions / 2;

            // Variables pour la gestion du mouvement et de la rotation
            float targetRotationAngle = 0f;
            float currentRotationAngle = 0f;
            float rotationSpeed = 5f;
            float keyRotationAngle = 0.0f;

            // Variables pour la gravité et la physique
            Vector3 playerVelocity = Vector3.Zero;
            float gravity = -0.2f;
            const float GRIDLEVEL = -3f;
            float groundLevel = GRIDLEVEL;
            float friction = 0.15f;
            float deceleration = 0.65f; // Facteur de ralentissement
            float speed = 0.01f; // Vitesse maximale du joueur

            // Positions des objets
            Vector3 playerPosition = new Vector3(-10,groundLevel,0);
            Vector3 obstaclePosition = new Vector3(0, 0, 0);
            Vector3 planePosition = new Vector3(0, -3, 0);

            Vector3 keyPosition = new Vector3(0);
          
            Vector3 center = obstaclePosition;
            Vector3 halfSize = obstacleDimensions / 2;
            Vector3 doorPosition = center + new Vector3(0, groundLevel + doorDimensions.Y / 2, halfSize.Z);


            BoundingBox doorBox = new BoundingBox(
                doorPosition - doorDimensions / 2,  // Coin inférieur de la BoundingBox
                doorPosition + doorDimensions / 2   // Coin supérieur de la BoundingBox
            );


            BoundingBox obstacleBox = new BoundingBox(
                obstaclePosition - (obstacleDimensions / 2),
                obstaclePosition + (obstacleDimensions / 2)
            );

            BoundingBox playerBox = new BoundingBox(
                playerPosition - (playerDimensions / 2),
                playerPosition + (playerDimensions / 2)
            );

            BoundingBox keyBox = new BoundingBox();

            // Variables pour la vue plan et le cooldown
            bool isTopView = false;
            float topViewDuration = 5.0f;
            float topViewTimer = 0.0f;
            float topViewCooldown = 10.0f;
            float progress = 1.0f;
            bool filling = false; 

            // Variables pour la porte
            bool isDoorOpen = false;
            bool playerHasKey = false;

            while (!WindowShouldClose())
            {
                Shaders.UpdatePBRLighting(camera.Position);
                float deltaTime = GetFrameTime();
                keyRotationAngle++;
                keyPosition.Y = obstaclePosition.Y + colliderObstacleDimension.Y + 1f;

                if (CheckCollisionBoxes(keyBox, playerBox))
                {
                    playerHasKey = true;
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
 
                // Déplacements du joueur (avant/arrière et latéraux)
                if (IsKeyDown(KeyboardKey.W) || IsKeyDown(KeyboardKey.Up)) playerPosition += moveDirection * friction;
                if (IsKeyDown(KeyboardKey.S) || IsKeyDown(KeyboardKey.Down)) playerPosition -= moveDirection * friction;
                if (IsKeyDown(KeyboardKey.D) || IsKeyDown(KeyboardKey.Left)) playerPosition -= strafeDirection * friction;
                if (IsKeyDown(KeyboardKey.A) || IsKeyDown(KeyboardKey.Right)) playerPosition += strafeDirection * friction;

                // Mise à jour de la position du joueur
                playerPosition += Raymath.Vector3Normalize(playerVelocity) / 10;

                playerBox = new BoundingBox(
                    new Vector3(playerPosition.X - 0.5f, playerPosition.Y - 0.5f, playerPosition.Z - 0.5f),
                    new Vector3(playerPosition.X + 0.5f, playerPosition.Y + 0.5f, playerPosition.Z + 0.5f)
                );
                keyBox = new BoundingBox(
                    keyPosition - (keyDimensions / 2),
                    keyPosition + (keyDimensions / 2)
                );
                

                // Appliquer un ralentissement progressif si aucune touche n'est pressée
                if (IsKeyUp(KeyboardKey.W) && IsKeyUp(KeyboardKey.S) && IsKeyUp(KeyboardKey.A) && IsKeyUp(KeyboardKey.D))
                {
                    playerVelocity *= deceleration; // Ralentissement progressif
                    if (playerVelocity.Length() < 0.01f) playerVelocity = Vector3.Zero; // Éviter une vitesse résiduelle
                }

                if (CheckCollisionBoxes(playerBox, obstacleBox) && !isTopView)
                {
                    // Déterminer les distances sur chaque axe
                    float deltaX = playerPosition.X - obstaclePosition.X;
                    float deltaZ = playerPosition.Z - obstaclePosition.Z;
                    float deltaY = playerPosition.Y - obstaclePosition.Y;  // Nouvelle distance sur l'axe Y

                    // Calculer la profondeur de la collision sur chaque côté
                    float overlapX = playerDimensions.X/2 + obstacleDimensions.X/2 - Math.Abs(deltaX);
                    float overlapZ = playerDimensions.Z/2 + obstacleDimensions.Z/2 - Math.Abs(deltaZ);
                    float overlapY = playerDimensions.Y/2+ obstacleDimensions.Y/2 - Math.Abs(deltaY);  // Profondeur sur Y

                    // Détecter les collisions horizontales
                    if (overlapX < overlapZ && overlapX < overlapY)
                    {
                        // Collision sur les côtés (axe X)
                        if (deltaX > 0)
                        {
                            // Glissement vers la droite
                            playerPosition.X += overlapX;
                        }
                        else
                        {
                            // Glissement vers la gauche
                            playerPosition.X -= overlapX;
                        }
                    }
                    // Détecter les collisions devant/arrière
                    else if (overlapZ < overlapX && overlapZ < overlapY)
                    {
                        // Collision sur le devant ou l'arrière (axe Z)
                        if (deltaZ > 0)
                        {
                            // Glissement vers l'avant
                            playerPosition.Z += overlapZ;
                        }
                        else
                        {
                            // Glissement vers l'arrière
                            playerPosition.Z -= overlapZ;
                        }
                    }
                    // Détecter les collisions sur le dessus (axe Y)
                    else
                    {
                        // Collision sur le dessus (axe Y)
                        if (deltaY > 0)
                        {
                            // Le joueur est en dessous de l'obstacle, glisse vers le bas
                            playerPosition.Y += overlapY - 0.01f;
                        }
                        else
                        {
                            // Le joueur est au-dessus de l'obstacle, glisse vers le haut
                            playerPosition.Y -= overlapY - 0.01f;
                        }
                    }
                }
                if (isTopView)
                {
                    if (CheckCollisionBoxes(playerBox, obstacleBox))
                    {
                        playerPosition.Y = obstaclePosition.Y + colliderObstacleDimension.Y;
                    }
                    else
                    {
                        playerPosition.Y = groundLevel;
                    }
                }
                // Appliquer les mouvements horizontaux (X, Z) indépendamment de la gravité
                playerPosition.X += playerVelocity.X;
                playerPosition.Z += playerVelocity.Z;

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
                if (playerPosition.Y > groundLevel + 0.5f)
                {
                    playerVelocity.Y += gravity * deltaTime;  // Applique la gravité uniquement en l'air
                }
                else
                {
                    playerVelocity.Y = 0f;  // Réinitialise la vitesse verticale quand il touche le sol
                    playerPosition.Y = groundLevel + 0.5f;  // Maintien le joueur sur le sol
                }

                // Changer de mode de vue (vue de dessus ou perspective)
                if (IsKeyPressed(KeyboardKey.Q))
                {
                    if (isTopView)
                    {
                        // Si la vue est déjà activée, désactive-la immédiatement
                        isTopView = false;
                        progress = 0.0f;
                        filling = true; // Recommence le remplissage
                    }
                    else if (progress >= 1.0f) // Ne peut réactiver la vue de dessus que si le cooldown est terminé
                    {
                        // Sinon, active la vue de dessus
                        isTopView = true;
                        topViewTimer = 0.0f;
                        filling = false;
                    }
                }

                // Gestion de la barre de progression (remplissage ou vidage)
                if (isTopView)
                {
                    progress -= deltaTime / topViewDuration;
                    if (progress <= 0.0f)
                    {
                        progress = 0.0f;
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

                // Si en vue de dessus
                if (isTopView)
                {
                    topViewTimer += deltaTime;
                    if (topViewTimer >= topViewDuration)
                    {
                        isTopView = false; // Désactiver la vue de dessus après topViewDuration
                        filling = true; // Démarrer le remplissage après la désactivation
                    }
                    camera.FovY = 20.0f;
                    camera.Position = new Vector3(playerPosition.X, 200.0f, playerPosition.Z);
                    camera.Target = new Vector3(playerPosition.X + moveDirection.X, 0.0f, playerPosition.Z + moveDirection.Z);
                    camera.Up = new Vector3(0.0f, 1.0f, 0.0f);
                    camera.Projection = CameraProjection.Orthographic;
                }
                else
                {
                    // Si la vue de dessus est désactivée, on continue avec la vue perspective
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
                Shaders.DrawSkybox(skyBox);
                Matrix4x4 rotationMatrix = Raymath.MatrixRotateY(keyRotationAngle * DEG2RAD);
                keyModel.Transform = rotationMatrix;

                DrawPlane(planePosition, planeSize, Color.DarkGray);
                DrawModel(playerModel, playerPosition, 1, Color.White);
                DrawModel(obstacleModel, obstaclePosition, 1, Color.White);

                if (playerHasKey)
                {
                    // Vérification de la collision uniquement si la porte n'est pas déjà ouverte
                    if (!isDoorOpen && CheckCollisionBoxes(playerBox, doorBox))
                    {
                        isDoorOpen = true;
                    }
                    if (isDoorOpen && !isTopView)
                    {
                        DrawModel(openDoorModel, doorPosition, 1, Color.White);
                    }
                    else
                    {
                        DrawModel(closedDoorModel, doorPosition, 1, Color.White); 
                    }
                }
                else
                {
                    DrawModel(keyModel, keyPosition, 1, Color.White);
                    //DrawBoundingBox(keyBox, Color.Yellow);
                    DrawModel(closedDoorModel, doorPosition, 1, Color.White);  // Affiche la porte fermée si le joueur n'a pas la clé
                }

                DrawSphere(Shaders.Light1.Position, 1.0f, Color.White);
                //DrawSphere(Shaders.Light2.Position, 1.0f, Color.White);
                //DrawSphere(Shaders.Light3.Position, 1.0f, Color.White);
                //DrawSphere(Shaders.Light4.Position, 1.0f, Color.White);

                //DrawBoundingBox(playerBox, Color.Blue);
                //DrawBoundingBox(obstacleBox, Color.Red);
                //DrawBoundingBox(doorBox, Color.Green);

                EndMode3D();
                // Dessin de la barre de progression
                DrawRectangle(10, 10, 200, 25, Color.LightGray);
                DrawRectangle(10, 10, (int)(200 * progress), 25, Color.Green);
                DrawRectangleLines(10, 10, 200, 25, Color.Black);

                // Debug
                //DrawText($"Position: X:{playerPosition.X:F2}, Y:{playerPosition.Y:F2}, Z:{playerPosition.Z:F2}", 10, 10, 20, Color.Black);
                //DrawText($"FPS: {GetFPS()}", 10, 30, 20, Color.Black);
                //DrawText($"Player Speed: {playerVelocity}", 10, 50, 20, Color.Black);
                //DrawText($"Is player falling: {isFalling}", 10, 70, 20, Color.Black);
                //DrawText($"Player box: {playerBox.Min}, {playerBox.Max}", 10, 90, 20, Color.Black);
                //DrawText($"Obstacle box: {obstacleBox.Min}, {obstacleBox.Max}", 10, 110, 20, Color.Black);
                //DrawText($"Is player touching obstacle: {CheckCollisionBoxes(obstacleBox, playerBox)}", 10, 130, 20, Color.Black);
                DrawText($"isTopView :{isTopView}", 10, 40, 20, Color.Black);

                EndDrawing();
            }
            // Déchargement des ressources
            CloseWindow();
        }
    }
}
