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
        static int screenHeight = 1050;

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
        }
        static void MenuScreen()
        {
            BeginDrawing();
            ClearBackground(Color.Gray);

            // Définition des textes
            string title = "DIMEN";
            string option1 = "Jouer";
            string option2 = "Assignations des touches";
            string option3 = "Quitter";

            int titleSize = 40;
            int optionSize = 30;

            int titleX = (screenWidth - MeasureText(title, titleSize)) / 2;
            int option1X = (screenWidth - MeasureText(option1, optionSize)) / 2;
            int option2X = (screenWidth - MeasureText(option2, optionSize)) / 2;
            int option3X = (screenWidth - MeasureText(option3, optionSize)) / 2;

            int option1Y = 450;
            int option2Y = 500;
            int option3Y = 550;

            // Position de la souris
            Vector2 mousePos = GetMousePosition();

            // Vérification du survol des options
            bool hover1 = mousePos.X >= option1X && mousePos.X <= option1X + MeasureText(option1, optionSize) &&
                          mousePos.Y >= option1Y && mousePos.Y <= option1Y + optionSize;

            bool hover2 = mousePos.X >= option2X && mousePos.X <= option2X + MeasureText(option2, optionSize) &&
                          mousePos.Y >= option2Y && mousePos.Y <= option2Y + optionSize;

            bool hover3 = mousePos.X >= option3X && mousePos.X <= option3X + MeasureText(option3, optionSize) &&
                          mousePos.Y >= option3Y && mousePos.Y <= option3Y + optionSize;

            // Affichage du menu avec effet de surbrillance
            DrawText(title, titleX, 300, titleSize, Color.White);
            DrawText(option1, option1X, option1Y, optionSize, hover1 ? Color.White : Color.LightGray);
            DrawText(option2, option2X, option2Y, optionSize, hover2 ? Color.White : Color.LightGray);
            DrawText(option3, option3X, option3Y, optionSize, hover3 ? Color.White : Color.LightGray);

            // Dessin des contours des zones cliquables en rouge
            DrawRectangleLines(option1X, option1Y, MeasureText(option1, optionSize), optionSize, Color.Red);
            DrawRectangleLines(option2X, option2Y, MeasureText(option2, optionSize), optionSize, Color.Red);
            DrawRectangleLines(option3X, option3Y, MeasureText(option3, optionSize), optionSize, Color.Red);

            // Détection des clics sur les options
            if (IsMouseButtonPressed(MouseButton.Left))
            {
                if (hover1) currentState = GameState.Playing;
                if (hover2) currentState = GameState.Settings;
                if (hover3) currentState = GameState.Exit;
            }

            EndDrawing();
        }

        static void SettingsScreen()
        {
            BeginDrawing();
            ClearBackground(Color.Gray);

            string text1 = "Touches de déplacement :\n\nW : Haut\n\nA : Gauche\n\nS : Bas\n\nD : Droite\n\n\nTouches dimensionnelles :\n\nE : Rotation caméra\n\nQ : Mode plan";
            string text2 = "<- Retour";

            int text2Size = 20;
            int text2X = 50;
            int text2Y = 50;
            int text2Width = MeasureText(text2, text2Size);

            Vector2 mousePos = GetMousePosition();
            bool hoverBack = mousePos.X >= text2X && mousePos.X <= text2X + text2Width &&
                             mousePos.Y >= text2Y && mousePos.Y <= text2Y + text2Size;

            DrawText(text1, 100, 150, 20, Color.White);
            DrawText(text2, text2X, text2Y, text2Size, hoverBack ? Color.Yellow : Color.White);

            // Dessin des contours de la zone cliquable en rouge
            DrawRectangleLines(text2X, text2Y, text2Width, text2Size, Color.Red);

            // Détection du clic sur "Retour"
            if (hoverBack && IsMouseButtonPressed(MouseButton.Left))
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

           // Obstacle obstacle1 = new(
           //    "assets/textures/default/",
           //    "assets/objects/Obstacle.obj",
           //    new Vector3(16),
           //    new Vector3(0),
           //    groundLevel
           //);

            Obstacle obstacle2 = new(
               "assets/textures/default/",
               "assets/objects/Obstacle.obj",
               new Vector3(2),
               new Vector3(12, 0, 12),
               groundLevel
           );
            Obstacle obstacle3 = new(
               "assets/textures/default/",
               "assets/objects/Obstacle.obj",
               new Vector3(3),
               new Vector3(-10, 0, -10),
               groundLevel
           );
            Obstacle obstacle4 = new(
               "assets/textures/default/",
               "assets/objects/Obstacle.obj",
               new Vector3(3),
               new Vector3(-20, 0, -20),
               groundLevel
           );

            List<Obstacle> obstacles = [obstacle1, obstacle2, obstacle3, obstacle4];

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

                if (IsKeyPressed(KeyboardKey.E) && !cooldown.IsTopView)
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

                player1.HandleCollision(obstacles, cooldown.IsTopView);

                // Dans la boucle principale
                if (IsKeyPressed(KeyboardKey.Q))
                {
                    cooldown.ToggleView();
                }

                cooldown.Update(deltaTime);

                // Appliquer la position de la caméra en fonction de l'état
                if (cooldown.IsTopView)
                    dimensionCamera.TopViewPosition();
                else
                    dimensionCamera.DefaultPosition();

                // Dessin de la scène
                BeginDrawing();
                ClearBackground(Color.RayWhite);
                BeginMode3D(dimensionCamera.Camera);
                Shaders.DrawSkybox(skyBox);

                Matrix4x4 rotationMatrix = Raymath.MatrixRotateY(keyRotationAngle * DEG2RAD);
                keyModel.Transform = rotationMatrix;

                DrawPlane(planePosition, planeSize, Color.DarkGray);
                DrawModel(player1.Model, player1.Position, 1, Color.White);


                if (player1.HasKey)
                {
                    // Vérification de la collision uniquement si la porte n'est pas déjà ouverte
                    if (!obstacle1.IsDoorOpen && CheckCollisionBoxes(player1.Box, obstacle1.DoorBox))
                    {
                        obstacle1.IsDoorOpen = true;
                    }
                    if (obstacle1.IsDoorOpen && !cooldown.IsTopView)
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

                DrawText(cooldown.IsTopView.ToString(), 120, 120, 20, Color.Black);

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
