using Raylib_cs;
using static Raylib_cs.Raylib;
using System.Numerics;
using System;


namespace DIMEN
{
    public class Program
    {
        enum GameState { Menu, Playing, Settings, Exit }
        static GameState currentState = GameState.Menu;
        static int screenWidth = 1800;
        static int screenHeight = 900;

        // Déclaration des polices en tant que variables statiques
        static Font hexagonFont;
        static Font upheavttFont;
        static Sound pickUpKeySound;
        static Sound keyAppearedSound;
        static Sound openDoorSound;
        static Sound pressureClick;
        static Sound pressureUnclick;
        static Music backgroundMusic;
        static void Main()
        {
            // Initialisation de la fenêtre
            InitWindow(screenWidth, screenHeight, "DIMEN - Menu");
            InitAudioDevice();
            SetTargetFPS(60);

            // Chargement des polices une seule fois
            hexagonFont = LoadFontEx("assets/ui_ux/fonts/HEXAGON_.TTF", 80, null, 0);
            upheavttFont = LoadFontEx("assets/ui_ux/fonts/upheavtt.ttf", 30, null, 0);
            pickUpKeySound = LoadSound("assets/sfx/keys/pickupkeys.wav");
            keyAppearedSound = LoadSound("assets/sfx/keys/keyappeared.wav");
            openDoorSound = LoadSound("assets/sfx/door/dooropen.wav");
            backgroundMusic = LoadMusicStream("assets/sfx/music/backgroundMusic.wav");
            pressureClick = LoadSound("assets/sfx/pressureplate/click.wav");
            pressureUnclick = LoadSound("assets/sfx/pressureplate/unclick.wav");

            // Boucle principale
            while (!WindowShouldClose() && currentState != GameState.Exit)
            {
                switch (currentState)
                {
                    case GameState.Menu:
                        MenuScreen();
                        break;
                    case GameState.Playing:
                        GameScreen();
                        break;
                    case GameState.Settings:
                        SettingsScreen();
                        break;
                    case GameState.Exit:
                        break;
                }
            }

        }

        static void MenuScreen()
        {
            BeginDrawing();
            ClearBackground(Color.Gray);

            // Définition des textes
            string title = "dimen";
            string option1 = "Jouer";
            string option2 = "Assignations des touches";
            string option3 = "Quitter";

            int optionSize = 30;

            int option1X = (screenWidth - MeasureText(option1, optionSize)) / 2 + 5;
            int option2X = (screenWidth - MeasureText(option2, optionSize)) / 2 - 30;
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
            DrawTextEx(hexagonFont, title, new Vector2((screenWidth - MeasureText(title, 80)) / 2, 300), 80, 2, Color.Black);
            DrawTextEx(upheavttFont, option1, new Vector2(option1X, option1Y), optionSize, 2, hover1 ? Color.White : Color.Black);
            DrawTextEx(upheavttFont, option2, new Vector2(option2X, option2Y), optionSize, 2, hover2 ? Color.White : Color.Black);
            DrawTextEx(upheavttFont, option3, new Vector2(option3X, option3Y), optionSize, 2, hover3 ? Color.White : Color.Black);

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

            string text1 = "Touches de deplacement :\n\nW : Haut\n\nA : Gauche\n\nS : Bas\n\nD : Droite\n\n\nTouches dimensionnelles :\n\nE : Rotation camera\n\nQ : Mode plan";
            string text2 = "<- Retour";

            int text1Size = 20;
            int text2Size = 20;

            int text1X = (screenWidth - MeasureText(text1, text1Size)) / 2;
            int text2X = (screenWidth - MeasureText(text2, text2Size)) / 2;

            int text1Y = 150;
            int text2Y = 50;

            Vector2 mousePos = GetMousePosition();
            bool hoverBack = mousePos.X >= text2X && mousePos.X <= text2X + MeasureText(text2, text2Size) &&
                             mousePos.Y >= text2Y && mousePos.Y <= text2Y + text2Size;

            // Affichage du texte d'instruction
            DrawTextEx(upheavttFont, text1, new Vector2(text1X, text1Y), text1Size, 2, Color.Black);

            // Affichage du deuxième texte avec la police upheavttFont et survol
            DrawTextEx(upheavttFont, text2, new Vector2(text2X, text2Y), text2Size, 2, hoverBack ? Color.White : Color.Black);

            // Détection du clic sur "Retour"
            if (hoverBack && IsMouseButtonPressed(MouseButton.Left))
            {
                currentState = GameState.Menu;
            }

            EndDrawing();

            if (IsKeyPressed(KeyboardKey.Escape))
            {
                currentState = GameState.Menu;
            }
        }
        public static unsafe void GameScreen()
        {

            // Initialisation
            InitWindow(screenWidth, screenHeight, "DIMEN");

            SetTargetFPS(60);
            DisableCursor();

            Shaders.Init();

            PBRMaterial KeyMaterial = new PBRMaterial("assets/textures/key/");
            Model keyModel = LoadModel("assets/objects/key/Old_Key.obj");
            Material skyBox = Shaders.LoadSkybox("assets/skybox/skybox.hdr");
            InitModels(keyModel, KeyMaterial);

            SetMasterVolume(100);

            Vector2 planeSize = new Vector2(1000, 1000);

            // Variables pour la gestion du mouvement et de la rotation
            float targetRotationAngle = 0f;
            float currentRotationAngle = 0f;
            float rotationSpeed = 5f;
            float keyRotationAngle = 0.0f;
            bool keyCanAppear = false;
            bool keySoundPlayed = false;

            const float GRIDLEVEL = -3f;
            float groundLevel = GRIDLEVEL;

            Vector3 planePosition = new Vector3(0, GRIDLEVEL, 0);

            Cooldown cooldown = new Cooldown(5.0f);

            DimensionCamera dimensionCamera = new DimensionCamera();
            Vector3 doorDimensions = new Vector3(1.4f, 2.5f, 0.5f);
            Vector3 colliderObstacleDimension = new Vector3(16) / 2;

            Obstacle obstacle1 = new(
                "assets/textures/default/grey/",
                "assets/textures/metal/yellow/",
                "assets/objects/obstacles/Obstacle_hole.obj",
                new Vector3(11), // Position centrale
                new Vector3(0),
                doorDimensions,
                "assets/objects/door/opendoor.obj",
                "assets/objects/door/closeddoor.obj",
                groundLevel
            );

            Obstacle obstacle2 = new(
                "assets/textures/default/grey/",
                "assets/objects/obstacles/Obstacle.obj",
                new Vector3(2, 30, 2), // Taille de l'obstacle
                new Vector3(-10, 0, 0), // Position à gauche de obstacle1
                groundLevel
            );

            Obstacle obstacle3 = new(
                "assets/textures/default/grey/",
                "assets/objects/obstacles/Obstacle.obj",
                new Vector3(2, 30, 2), // Taille de l'obstacle
                new Vector3(10, 0, 0), // Position à droite de obstacle1
                groundLevel
            );
            Obstacle obstacle5 = new(
                "assets/textures/default/grey/",
                "assets/objects/obstacles/Obstacle.obj",
                new Vector3(2, 30, 2), // Taille de l'obstacle
                new Vector3(0, 0, 10), // Position à droite de obstacle1
                groundLevel
            );

            Player player1 = new(
                "assets/textures/metal/grey/",
                "assets/objects/Cube.obj",
                new Vector3(1),
                groundLevel
            );

            MovableCube movableCube1 = new(
                "assets/textures/default/red/",
                "assets/objects/Cube.obj",
                new Vector3(1),
                groundLevel
            );
            MovableCube movableCube2 = new(
                "assets/textures/default/blue/",
                "assets/objects/Cube.obj",
                new Vector3(1),
                groundLevel
            );

            PressurePlate plate1 = new(
                "assets/objects/pressure_plate/pressed.obj",
                "assets/objects/pressure_plate/unpressed.obj",
                "assets/textures/default/red/",
                new Vector3(3, 0.3f, 3),
                new Vector3(10, groundLevel, 10),
                movableCube1
            );
            PressurePlate plate2 = new(
                "assets/objects/pressure_plate/pressed.obj",
                "assets/objects/pressure_plate/unpressed.obj",
                "assets/textures/default/blue/",
                new Vector3(3, 0.3f, 3),
                new Vector3(obstacle1.Position.X, obstacle1.Position.Y + obstacle1.Dimensions.Y / 2, obstacle1.Position.Z),
                movableCube2
            );

            bool bothPlatesPressed = false;

            Vector3 keyDimensions = new Vector3(0.5f, 1f, 0.5f);
            Vector3 keyPosition = new Vector3();

            BoundingBox finishLevelBox = new BoundingBox();
            BoundingBox hallway = new BoundingBox();
            BoundingBox wall1 = new BoundingBox();
            BoundingBox wall2 = new BoundingBox();


            List<MovableCube> movableCubes = new List<MovableCube> { movableCube1, movableCube2 };
            List<Obstacle> obstacles = new List<Obstacle> { obstacle1, obstacle2, obstacle3, obstacle5 };
            List<PressurePlate> plates = new List<PressurePlate> { plate1, plate2 };

            player1.Position = new Vector3(0, groundLevel, 20);
            movableCube1.Position = new Vector3(obstacle1.Position.X, 100, obstacle1.Position.Z);
            movableCube2.Position = new Vector3(obstacle3.Position.X, 200, obstacle3.Position.Z);

            PlayMusicStream(backgroundMusic);
            SetMusicVolume(backgroundMusic, 0.1f);

            while (currentState == GameState.Playing)
            {
                Shaders.UpdatePBRLighting(dimensionCamera.Camera.Position);
                float deltaTime = GetFrameTime();

                keyRotationAngle++;

                float obstacle2Top = obstacle2.Position.Y + obstacle2.Dimensions.Y / 2 + keyDimensions.Y / 2;

                keyPosition.Y = obstacle2Top;
                keyPosition.X = obstacle2.Position.X;
                keyPosition.Z = obstacle2.Position.Z;

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

                // Suivi de la caméra sur le joueur
                dimensionCamera.PlayerPosition = player1.Position;
                dimensionCamera.Radians = radians;
                dimensionCamera.MoveDirection = moveDirection;

                // Dans la boucle principale
                if (IsKeyPressed(KeyboardKey.Q))
                {
                    cooldown.ToggleView();
                }

                UpdateMusicStream(backgroundMusic);
                dimensionCamera.UpdateCamera(deltaTime);
                player1.Update(moveDirection, strafeDirection, deltaTime);

                foreach(MovableCube mvCube in movableCubes)
                {
                    mvCube.Update(deltaTime, player1, movableCubes);
                    mvCube.HandleCollision(obstacles, cooldown.IsTopView);
                    mvCube.HandlePlate(plates);
                }
                
                cooldown.Update(deltaTime);

                if (cooldown.IsTopView)
                {
                    dimensionCamera.TopViewPosition(); // Passer à la vue top
                }
                else
                {
                    dimensionCamera.DefaultPosition(); // Revenir à la vue par défaut
                }

                if (!cooldown.IsTopView && obstacle1.IsDoorOpen && !obstacle1.DoorExtended)
                {
                    float extension = 7f;
                    float reduction = 0.1f; // Ajuste cette valeur selon le besoin

                    hallway = new BoundingBox(
                        new Vector3(obstacle1.DoorBox.Min.X + reduction, obstacle1.DoorBox.Min.Y, obstacle1.DoorBox.Min.Z),
                        new Vector3(obstacle1.DoorBox.Max.X - reduction, obstacle1.DoorBox.Max.Y, obstacle1.DoorBox.Max.Z + extension / 2 - reduction)
                    );

                    finishLevelBox = new BoundingBox(
                        new Vector3(hallway.Min.X, hallway.Min.Y, hallway.Max.Z),
                        new Vector3(hallway.Max.X, hallway.Max.Y, hallway.Max.Z + extension / 2)
                    );

                    wall1 = new BoundingBox(
                        new Vector3(obstacle1.DoorBox.Min.X, obstacle1.DoorBox.Min.Y, obstacle1.DoorBox.Min.Z + reduction * 3),
                        new Vector3(obstacle1.DoorBox.Min.X, obstacle1.DoorBox.Max.Y, obstacle1.DoorBox.Max.Z + extension)
                    );
                    wall2 = new BoundingBox(
                        new Vector3(obstacle1.DoorBox.Max.X, obstacle1.DoorBox.Min.Y, obstacle1.DoorBox.Min.Z + reduction * 3),
                        new Vector3(obstacle1.DoorBox.Max.X, obstacle1.DoorBox.Max.Y, obstacle1.DoorBox.Max.Z + extension)
                    );

                    // Marquer que l'extension a été appliquée
                    obstacle1.DoorExtended = true;
                }

                bothPlatesPressed = PressurePlate(movableCubes, plates);
                if (bothPlatesPressed)
                {
                    keyCanAppear = true;
                }
                if (keyCanAppear && !keySoundPlayed) // Vérifie si le son a déjà été joué
                {
                    PlaySound(keyAppearedSound);
                    keySoundPlayed = true; // Empêche la relecture du son
                }
                if (keyCanAppear)
                {
                    if (!player1.HasKey)
                    {
                        Matrix4x4 rotationMatrix = Raymath.MatrixRotateY(keyRotationAngle * DEG2RAD);
                        keyModel.Transform = rotationMatrix;

                        if (CheckCollisionBoxes(player1.Box, keyBox) && !cooldown.IsTopView)
                        {
                            player1.HasKey = true;
                            PlaySound(pickUpKeySound);
                        }
                    }
                }
                if (CheckCollisionBoxes(player1.Box, hallway))
                {
                    List<BoundingBox> walls = new List<BoundingBox> { wall1, wall2 };
                    if (!cooldown.IsTopView)
                    {
                        player1.HandleHallway(walls);
                    }
                    else
                    {
                        player1.HandleCollision(obstacles, cooldown.IsTopView);
                        player1.HandlePlate(plates);
                    }
                }
                else
                {
                    player1.HandleCollision(obstacles, cooldown.IsTopView);
                    player1.HandlePlate(plates); 
                }
                // Réinitialiser doorExtended lorsque la porte se ferme pour réappliquer l'extension si nécessaire
                if (!obstacle1.IsDoorOpen)
                {
                    obstacle1.DoorExtended = false;
                }

                if (CheckCollisionBoxes(player1.Box, finishLevelBox))
                {
                    currentState = GameState.Menu;
                }

                // Dessin de la scène
                BeginDrawing();
                ClearBackground(Color.RayWhite);
                BeginMode3D(dimensionCamera.Camera);
                Shaders.DrawSkybox(skyBox);

                DrawPlane(planePosition, planeSize, Color.DarkGray);
                DrawModel(player1.Model, player1.Position, 1, Color.White);

                foreach (Obstacle obstacle in obstacles)
                {
                    // Utiliser DrawModelEx pour appliquer l'échelle, la position, et potentiellement la rotation
                    DrawModelEx(obstacle.Model, obstacle.Position, Vector3.One, 0f, obstacle.Scale, Color.White);
                }

                foreach (MovableCube mvCube in movableCubes)
                {
                    // Utiliser DrawModelEx pour appliquer l'échelle, la position, et potentiellement la rotation
                    DrawModel(mvCube.Model, mvCube.Position, 1, Color.White);
                }

                foreach (PressurePlate plate in plates)
                {
                    if (plate.IsPressed)
                    {
                        DrawModelEx(plate.PressedModel, plate.Position, Vector3.One, 0f, plate.Scale, Color.White);
                    }
                    else
                    {
                        DrawModelEx(plate.Model, plate.Position, Vector3.One, 0f, plate.Scale, Color.White);
                    }
                }
                if (player1.HasKey && !player1.HasUsedKey)
                {
                    // Vérification de la collision uniquement si la porte n'est pas déjà ouverte
                    if (!obstacle1.IsDoorOpen && CheckCollisionBoxes(player1.Box, obstacle1.DoorBox))
                    {
                        obstacle1.IsDoorOpen = true;
                        PlaySound(openDoorSound);

                        // Marquer la clé comme utilisée pour qu'elle disparaisse
                        player1.HasUsedKey = true;
                    }

                    // Dessiner la clé au-dessus du joueur seulement si elle n'a pas été utilisée
                    if (!player1.HasUsedKey)
                    {
                        DrawModel(keyModel, new Vector3(player1.Position.X, player1.Position.Y + keyDimensions.Y * 1.5f, player1.Position.Z), 1, Color.White);
                    }
                }

                // Dessiner la porte (ouverte ou fermée)
                if (obstacle1.IsDoorOpen)
                {
                    DrawModelEx(obstacle1.OpenDoorModel, obstacle1.DoorPosition, Vector3.Zero, 0, new Vector3(1), Color.White);
                }
                else
                {
                    DrawModelEx(obstacle1.ClosedDoorModel, obstacle1.DoorPosition, Vector3.Zero, 0, new Vector3(1), Color.White);
                }

                if (keyCanAppear)
                {
                    // Dessiner la clé au sol si elle n'a pas été ramassée
                    if (!player1.HasKey)
                    {
                        DrawModel(keyModel, keyPosition, 1, Color.White);
                    }
                }

                //DrawSphere(Shaders.Light1.Position, 1.0f, Color.Orange);

                EndMode3D();

                if(keyCanAppear)
                {
                    DrawText("Une clé est apparue", 30, 30, 30, Color.White);
                }

                // Calculer la position de la barre
                int barWidth = 400; // Largeur de la barre
                int barHeight = 5; // Hauteur de la barre
                int barX = (screenWidth - barWidth) / 2; // Centrer la barre horizontalement
                int barY = screenHeight - barHeight - 60; // Positionner la barre en bas

                // Dessiner la barre de cooldown (la partie remplie)
                DrawRectangle(barX, barY, (int)(barWidth * cooldown.Progress), barHeight, Color.White);
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
            static unsafe bool PressurePlate(List<MovableCube> mvCubes, List<PressurePlate> plaques)
            {
                bool allPressed = true;

                foreach (PressurePlate plate in plaques)
                {
                    bool previousState = plate.IsPressed;
                    plate.IsPressed = false; // Reset avant de vérifier les collisions

                    // Vérifier si le cube assigné peut appuyer sur cette plaque
                    if (plate.PressableBy != null && CheckCollisionBoxes(plate.PressableBy.Box, plate.PressBox))
                    {
                        plate.IsPressed = true;
                    }

                    // Jouer le son uniquement si l'état change
                    if (plate.IsPressed && !previousState)
                    {
                        PlaySound(pressureClick);
                    }
                    else if (!plate.IsPressed && previousState)
                    {
                        PlaySound(pressureUnclick);
                    }

                    // Si une seule plate n'est pas pressée, on retourne false
                    if (!plate.IsPressed)
                    {
                        allPressed = false;
                    }
                }

                return allPressed;
            }


            // Déchargement des ressources
            CloseWindow();
        }
    }
}
