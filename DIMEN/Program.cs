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
            const float GRIDLEVEL = -3f;
            float groundLevel = GRIDLEVEL;

            // Positions des objets
            Vector3 planePosition = new Vector3(0, GRIDLEVEL, 0);
            Vector3 keyPosition = new Vector3(0);

            // Variables pour la vue plan et le cooldown
            bool isTopView = false;

            Cooldown cooldown = new Cooldown(5.0f);

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
                "assets/textures/default/",
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
                BoundingBox keyBox = new BoundingBox(keyPosition - (keyDimensions / 2), keyPosition + (keyDimensions / 2));

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

                dimensionCamera.PlayerPosition = player1.Position;
                dimensionCamera.Radians = radians;
                dimensionCamera.MoveDirection = moveDirection;

                if (CheckCollisionBoxes(keyBox, player1.Box))
                {
                    player1.HasKey = true;
                }
                player1.Update(moveDirection, strafeDirection, deltaTime);
                player1.LimitSpeed();
                player1.HandleCollision(obstacle1, isTopView);
                               
                // Changer de mode de vue (vue de dessus ou perspective)
                if (IsKeyPressed(KeyboardKey.Q))
                {
                    if (isTopView)
                    {
                        // Si la vue est déjà activée, désactive-la immédiatement
                        isTopView = false;
                        cooldown.Progress = 0.0f;
                        cooldown.Filling = true; // Recommence le remplissage
                    }
                    else if (cooldown.Progress >= 1.0f) // Ne peut réactiver la vue de dessus que si le cooldown est terminé
                    {
                        // Sinon, active la vue de dessus
                        isTopView = true;
                        cooldown.TotalCooldown = 0.0f;
                        cooldown.Filling = false;
                    }
                }
                // Gestion de la barre de progression (remplissage ou vidage)
                if (isTopView)
                {
                    cooldown.Progress -= deltaTime / cooldown.Duration;
                    if (cooldown.Progress <= 0.0f)
                    {
                        cooldown.Progress = 0.0f;
                        cooldown.Filling = true;
                    }
                }
                else if (cooldown.Filling)
                {
                    cooldown.Progress += deltaTime / cooldown.Duration;
                    if (cooldown.Progress >= 1.0f)
                    {
                        cooldown.Progress = 1.0f;
                        cooldown.Filling = false;
                    }
                }
                // Si en vue de dessus
                if (isTopView)
                {
                    cooldown.Timer += deltaTime;
                    if (cooldown.Timer >= cooldown.Duration)
                    {
                        isTopView = false; // Désactiver la vue de dessus après topViewDuration
                        cooldown.Filling = true; // Démarrer le remplissage après la désactivation
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
                DrawModel(obstacle1.Model, obstaclePosition, 1, Color.White);

                if (player1.HasKey)
                {
                    // Vérification de la collision uniquement si la porte n'est pas déjà ouverte
                    if (!obstacle1.IsDoorOpen && CheckCollisionBoxes(player1.Box, obstacle1.DoorBox))
                    {
                        obstacle1.IsDoorOpen = true;
                    }
                    if (obstacle1.IsDoorOpen && !isTopView)
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
                DrawRectangle(10, 10, (int)(200 * cooldown.Progress), 25, Color.Green);
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
