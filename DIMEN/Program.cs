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

        public static unsafe void Main(string[] args)
        {
            // Initialisation
            InitWindow(1600, 900, "Raylib 3D in C#");
            SetTargetFPS(60);
            DisableCursor();

            Shaders.Init();
            
            PBRMaterial defaultMaterial = new PBRMaterial("assets/textures/default/");
            PBRMaterial KeyMaterial = new PBRMaterial("assets/textures/key/");
            Model keyModel = LoadModel("assets/objects/Old_Key.obj");
            Material skyBox = Shaders.LoadSkybox("assets/skybox/skybox.hdr"); 
            InitModels(keyModel, KeyMaterial);

            Vector3 keyDimensions = new Vector3(0.5f, 1f, 0.5f);
            Vector2 planeSize = new Vector2(300, 300);

            // Variables pour la gestion du mouvement et de la rotation
            float targetRotationAngle = 0f;
            float currentRotationAngle = 0f;
            float rotationSpeed = 5f;
            float keyRotationAngle = 0.0f;

            // Variables pour la gravité et la physique
            float gravity = -0.2f;
            const float GRIDLEVEL = -3f;
            float groundLevel = GRIDLEVEL;

            // Positions des objets
            Vector3 planePosition = new Vector3(0, -3, 0);
            Vector3 keyPosition = new Vector3(0);
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


            DimensionCamera dimensionCamera = new DimensionCamera();

            Vector3 obstacleDimensions = new Vector3(16);
            Vector3 obstaclePosition = new Vector3(0, 0, 0);
            Vector3 doorDimensions = new Vector3(1.4f, 2.5f, 0.5f);
            Vector3 colliderObstacleDimension = obstacleDimensions / 2;
            
            Obstacle obstacle1 = new(
                "assets/textures/default/",
                "assets/textures/metal/door/",
                "assets/objects/Obstacle.obj",
                obstacleDimensions,
                obstaclePosition,
                doorDimensions,
                "assets/objects/opendoor.obj",
                "assets/objects/closeddoor.obj",
                groundLevel
            );
 
            Vector3 playerDimensions = new Vector3(1);
            Player player1 = new(
                "assets/textures/metal/player/",
                "assets/objects/Cube.obj",
                playerDimensions,
                groundLevel
            );


            while (!WindowShouldClose())
            {
                Shaders.UpdatePBRLighting(dimensionCamera.Camera.Position);
                float deltaTime = GetFrameTime();
                keyRotationAngle++;
                keyPosition.Y = obstaclePosition.Y + colliderObstacleDimension.Y + 1f;

                if (CheckCollisionBoxes(keyBox, player1.Box))
                {
                    playerHasKey = true;
                }
                if (IsKeyPressed(KeyboardKey.E) && !isTopView)
                {
                    targetRotationAngle += 90f;
                }
                // Interpolation fluide de la rotation
                currentRotationAngle = Raymath.Lerp(currentRotationAngle, targetRotationAngle, rotationSpeed * deltaTime);
                // Calcul de l'angle et direction du mouvement
                float radians = MathF.PI * currentRotationAngle / 180f;
                Vector3 moveDirection = new Vector3(MathF.Sin(radians), 0, MathF.Cos(radians));
                Vector3 strafeDirection = new Vector3(MathF.Cos(radians), 0, -MathF.Sin(radians));

                // Déplacements du joueur (avant/arrière et latéraux)
                if (IsKeyDown(KeyboardKey.W) || IsKeyDown(KeyboardKey.Up)) player1.MovePlayerUp(moveDirection);
                if (IsKeyDown(KeyboardKey.S) || IsKeyDown(KeyboardKey.Down)) player1.MovePlayerDown(moveDirection);
                if (IsKeyDown(KeyboardKey.D) || IsKeyDown(KeyboardKey.Left)) player1.MovePlayerLeft(strafeDirection);
                if (IsKeyDown(KeyboardKey.A) || IsKeyDown(KeyboardKey.Right)) player1.MovePlayerRight(strafeDirection);

                dimensionCamera.PlayerPosition = player1.Position;
                dimensionCamera.Radians = radians;
                dimensionCamera.MoveDirection = moveDirection;

                player1.Update();

                keyBox = new BoundingBox(
                    keyPosition - (keyDimensions / 2),
                    keyPosition + (keyDimensions / 2)
                );
                
                // Appliquer un ralentissement progressif si aucune touche n'est pressée
                if (IsKeyUp(KeyboardKey.W) && IsKeyUp(KeyboardKey.S) && IsKeyUp(KeyboardKey.A) && IsKeyUp(KeyboardKey.D))
                {
                    player1.Brake();
                }

                if (CheckCollisionBoxes(player1.Box, obstacle1.ObstacleBox) && !isTopView)
                {
                    // Déterminer les distances sur chaque axe
                    float deltaX = player1.Position.X - obstaclePosition.X;
                    float deltaZ = player1.Position.Z - obstaclePosition.Z;
                    float deltaY = player1.Position.Y - obstaclePosition.Y;  // Nouvelle distance sur l'axe Y

                    // Calculer la profondeur de la collision sur chaque côté
                    float overlapX = player1.Dimensions.X/2 + obstacle1.ObstacleDimensions.X/2 - Math.Abs(deltaX);
                    float overlapZ = player1.Dimensions.Z/2 + obstacle1.ObstacleDimensions.Z / 2 - Math.Abs(deltaZ);
                    float overlapY = player1.Dimensions.Y/2+ obstacle1.ObstacleDimensions.Y/2 - Math.Abs(deltaY);  // Profondeur sur Y

                    // Détecter les collisions horizontales
                    if (overlapX < overlapZ && overlapX < overlapY)
                    {
                        // Collision sur les côtés (axe X)
                        if (deltaX > 0)
                        {
                            // Glissement vers la droite
                            player1.Position.X += overlapX;
                        }
                        else
                        {
                            // Glissement vers la gauche
                            player1.Position.X -= overlapX;
                        }
                    }
                    // Détecter les collisions devant/arrière
                    else if (overlapZ < overlapX && overlapZ < overlapY)
                    {
                        // Collision sur le devant ou l'arrière (axe Z)
                        if (deltaZ > 0)
                        {
                            // Glissement vers l'avant
                            player1.Position.Z += overlapZ;
                        }
                        else
                        {
                            // Glissement vers l'arrière
                            player1.Position.Z -= overlapZ;
                        }
                    }
                    // Détecter les collisions sur le dessus (axe Y)
                    else
                    {
                        // Collision sur le dessus (axe Y)
                        if (deltaY > 0)
                        {
                            // Le joueur est en dessous de l'obstacle, glisse vers le bas
                            player1.Position.Y += overlapY - 0.01f;
                        }
                        else
                        {
                            // Le joueur est au-dessus de l'obstacle, glisse vers le haut
                            player1.Position.Y -= overlapY - 0.01f;
                        }
                    }
                }

                if (isTopView)
                {
                    if (CheckCollisionBoxes(player1.Box, obstacle1.ObstacleBox))
                    {
                        player1.Position.Y = obstaclePosition.Y + colliderObstacleDimension.Y;
                    }
                    else
                    {
                        player1.Position.Y = groundLevel;
                    }
                }
                // Appliquer les mouvements horizontaux (X, Z) indépendamment de la gravité
                player1.Position.X += player1.Velocity.X;
                player1.Position.Z += player1.Velocity.Z;

                // Limiter la vitesse maximale
                player1.LimitSpeed();
                

                if (player1.Velocity.Y > gravity)
                {
                    player1.Velocity.Y = gravity;
                }

                // Appliquer la gravité uniquement si le joueur n'est pas sur le sol
                if (player1.Position.Y > groundLevel + 0.5f)
                {
                    player1.Velocity.Y += gravity * deltaTime;  // Applique la gravité uniquement en l'air
                }
                else
                {
                    player1.Velocity.Y = 0f;  // Réinitialise la vitesse verticale quand il touche le sol
                    player1.Position.Y = groundLevel + 0.5f;  // Maintien le joueur sur le sol
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
                    dimensionCamera.TopViewPosition();
                }
                else
                {              
                    dimensionCamera.DefaultPosition();
                }

                // Dessin de la scène
                BeginDrawing();
                ClearBackground(Color.RayWhite);
                BeginMode3D(dimensionCamera.Camera);
                Shaders.DrawSkybox(skyBox);
                Matrix4x4 rotationMatrix = Raymath.MatrixRotateY(keyRotationAngle * DEG2RAD);
                keyModel.Transform = rotationMatrix;

                DrawPlane(planePosition, planeSize, Color.DarkGray);

                DrawModel(player1.Model, player1.Position, 1, Color.White);
                DrawModel(obstacle1.ObstacleModel, obstaclePosition, 1, Color.White);

                if (playerHasKey)
                {
                    // Vérification de la collision uniquement si la porte n'est pas déjà ouverte
                    if (!obstacle1.IsDoorOpen && CheckCollisionBoxes(player1.Box, obstacle1.DoorBox))
                    {
                        isDoorOpen = true;
                    }
                    if (isDoorOpen && !isTopView)
                    {
                        DrawModel(obstacle1.OpenDoorModel, obstacle1.DoorPosition, 1, Color.White);
                    }
                    else
                    {
                        DrawModel(obstacle1.ClosedDoorModel, obstacle1.DoorPosition, 1, Color.White); 
                    }
                }
                else
                {
                    DrawModel(keyModel, keyPosition, 1, Color.White);
                    //DrawBoundingBox(keyBox, Color.Yellow);
                    DrawModel(obstacle1.ClosedDoorModel, obstacle1.DoorPosition, 1, Color.White);  // Affiche la porte fermée si le joueur n'a pas la clé
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


                EndDrawing();
            }
            static unsafe void InitModels(Model model, PBRMaterial material)
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
            // Déchargement des ressources
            CloseWindow();
        }
    }
}
