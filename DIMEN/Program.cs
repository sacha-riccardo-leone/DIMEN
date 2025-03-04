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
        enum GameState { Menu, Playing, Settings, Exit }
        static GameState currentState = GameState.Menu;  // On commence dans le menu
        static int screenWidth = 1920;
        static int screenHeight = 1080;

        static void Main()
        {
            InitWindow(screenWidth, screenHeight, "Menu de démarrage - Raylib");
            SetTargetFPS(60);

            while (!WindowShouldClose() && currentState != GameState.Exit)
            {
                switch (currentState)
                {
                    case GameState.Menu:
                        MenuScreen();  // Affichage du menu
                        break;
                    case GameState.Playing:
                        GameScreen();  // Jeu en cours
                        break;
                    case GameState.Settings:
                        SettingsScreen();
                        break;
                }
            }
            CloseWindow();
        }
        static void MenuScreen()
        {
            BeginDrawing();
            ClearBackground(Color.Gray);

            // Texte centré pour le menu
            string title = "DIMEN";
            string option1 = "1. Jouer";
            string option2 = "2. Assignations des touches";
            string option3 = "3. Quitter";

            int titleSize = 40;
            int optionSize = 30;

            int titleX = (screenWidth - MeasureText(title, titleSize)) / 2;
            int option1X = (screenWidth - MeasureText(option1, optionSize)) / 2;
            int option2X = (screenWidth - MeasureText(option2, optionSize)) / 2;
            int option3X = (screenWidth - MeasureText(option3, optionSize)) / 2;

            // Affichage du menu
            DrawText(title, titleX, 300, titleSize, Color.White);
            DrawText(option1, option1X, 450, optionSize, Color.LightGray);
            DrawText(option2, option2X, 500, optionSize, Color.LightGray);
            DrawText(option3, option3X, 550, optionSize, Color.LightGray);

            // Vérification de l'entrée utilisateur
            if (IsKeyPressed(KeyboardKey.One))
            {
                currentState = GameState.Playing;  // Passage à l'écran de jeu
            }
            if (IsKeyPressed(KeyboardKey.Two))
            {
                currentState = GameState.Settings;  // Sortie de l'application
            }
            if (IsKeyPressed(KeyboardKey.Three))
            {
                currentState = GameState.Exit;  // Sortie de l'application
            }

            EndDrawing();
        }
        static void SettingsScreen()
        {
            BeginDrawing();
            ClearBackground(Color.Gray);

            string text1 = "Touches de déplacement :\n\nW : Haut\n\nA : Gauche\n\nS : Bas\n\nD : Droite\n\n\nTouches dimentionnelles :\n\nE : Rotation caméra\n\nQ : Mode plan";
            string text2 = "Retour : <-";
            
            DrawText(text1, 100, 150, 20, Color.White);
            DrawText(text2, 50, 50, 20, Color.White);

            // Vérification de l'entrée utilisateur
            if (IsKeyPressed(KeyboardKey.Left))
            {
                currentState = GameState.Menu;
            }

            EndDrawing();
        }
        public static unsafe void GameScreen()
        {

            // Initialisation
            InitWindow(screenWidth, screenHeight, "Raylib 3D in C#");
            SetTargetFPS(60);
            DisableCursor();

            Shaders.Init();

            PBRMaterial KeyMaterial = new PBRMaterial("assets/textures/key/");
            Model keyModel = LoadModel("assets/objects/Old_Key.obj");
            Material skyBox = Shaders.LoadSkybox("assets/skybox/skybox.hdr");
            InitModels(keyModel, KeyMaterial);

            Vector3 keyDimensions = new Vector3(0.5f, 1f, 0.5f);
            Vector2 planeSize = new Vector2(1000, 1000);

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
            Vector3 doorDimensions = new Vector3(1.4f, 2.5f, 0.5f);
            Vector3 colliderObstacleDimension = new Vector3(16) / 2;

            Obstacle obstacle1 = new(
               "assets/textures/default/",
               "assets/textures/metal/door/",
               "assets/objects/Obstacle.obj",
               new Vector3(16),
               new Vector3(0),
               doorDimensions,
               "assets/objects/opendoor.obj",
               "assets/objects/closeddoor.obj",
               groundLevel
           );

            Obstacle obstacle2 = new(
               "assets/textures/default/",
               "assets/objects/Obstacle.obj",
               new Vector3(8),
               new Vector3(20, 0, 20),
               groundLevel
           );

            List<Obstacle> obstacles = [obstacle1, obstacle2];

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
                keyPosition.Y = obstacle1.Position.Y + colliderObstacleDimension.Y + 1f;
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
                player1.HandleCollision(obstacles, isTopView);

                // Changer de mode de vue (vue de dessus ou perspective)
                if (IsKeyPressed(KeyboardKey.Q))
                {
                    if (isTopView)
                    {
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
                        isTopView = false;
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
                DrawModel(obstacle1.Model, obstacle1.Position, 1, Color.White);
                DrawModelEx(obstacle2.Model, obstacle2.Position, Vector3.Zero, 0, new Vector3(0f), Color.White);

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
                    DrawModel(obstacle1.ClosedDoorModel, obstacle1.DoorPosition, 1, Color.White);  // Affiche la porte fermée si le joueur n'a pas la clé
                }

                DrawSphere(Shaders.Light1.Position, 1.0f, Color.White);
                DrawSphere(Shaders.Light2.Position, 1.0f, Color.White);
                DrawSphere(Shaders.Light3.Position, 1.0f, Color.White);
                DrawSphere(Shaders.Light4.Position, 1.0f, Color.White);

                foreach (Obstacle obstacle in obstacles)
                {
                    DrawBoundingBox(obstacle.Box, Color.Red);
                }
                DrawBoundingBox(player1.Box, Color.Blue);

                EndMode3D();

                

                // Dessin de la barre de progression
                DrawRectangle(30, 30, 200, 25, Color.LightGray);
                DrawRectangle(30, 30, (int)(200 * cooldown.Progress), 25, Color.Green);
                DrawRectangleLines(30, 30, 200, 25, Color.Black);
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
