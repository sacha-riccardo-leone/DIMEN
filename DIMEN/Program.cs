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

        // Déclaration des polices et sons en tant que variables statiques
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
            InitWindow(screenWidth, screenHeight, "DIMEN - Menu");
            InitAudioDevice();
            SetTargetFPS(60);

            // Chargement des polices et sons une seule fois
            hexagonFont = LoadFontEx("assets/ui_ux/fonts/HEXAGON_.TTF", 80, null, 0);
            upheavttFont = LoadFontEx("assets/ui_ux/fonts/upheavtt.ttf", 30, null, 0);
            pickUpKeySound = LoadSound("assets/sfx/keys/pickupkeys.wav");
            keyAppearedSound = LoadSound("assets/sfx/keys/keyappeared.wav");
            openDoorSound = LoadSound("assets/sfx/door/dooropen.wav");
            backgroundMusic = LoadMusicStream("assets/sfx/music/backgroundMusic.wav");
            pressureClick = LoadSound("assets/sfx/pressureplate/click.wav");
            pressureUnclick = LoadSound("assets/sfx/pressureplate/unclick.wav");

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

            Vector2 mousePos = GetMousePosition();

            bool hover1 = mousePos.X >= option1X && mousePos.X <= option1X + MeasureText(option1, optionSize) &&
                          mousePos.Y >= option1Y && mousePos.Y <= option1Y + optionSize;

            bool hover2 = mousePos.X >= option2X && mousePos.X <= option2X + MeasureText(option2, optionSize) &&
                          mousePos.Y >= option2Y && mousePos.Y <= option2Y + optionSize;

            bool hover3 = mousePos.X >= option3X && mousePos.X <= option3X + MeasureText(option3, optionSize) &&
                          mousePos.Y >= option3Y && mousePos.Y <= option3Y + optionSize;

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

            DrawTextEx(upheavttFont, text1, new Vector2(text1X, text1Y), text1Size, 2, Color.Black);

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
            InitWindow(screenWidth, screenHeight, "DIMEN");

            SetTargetFPS(60);
            DisableCursor();

            Shaders.Init();

            // Ciel
            Material skyBox = Shaders.LoadSkybox("assets/skybox/skybox.hdr");
            

            SetMasterVolume(100);

            // Chemin des "Materials"
            string defaultGrey = "assets/textures/default/grey/";
            string defaultBlue = "assets/textures/default/blue/";
            string defaultred = "assets/textures/default/red/";
            string metalGrey = "assets/textures/metal/grey/";
            string metalYellow = "assets/textures/metal/yellow/";

            // Chemin des objets 3D
            string obstacleObject = "assets/objects/obstacles/Obstacle.obj";
            string cubeObject = "assets/objects/Cube.obj";
            string pressedPlateObject = "assets/objects/pressure_plate/pressed.obj";
            string unpressedPlateObject = "assets/objects/pressure_plate/unpressed.obj";

            // Niveau du sol
            const float GRIDLEVEL = -3f;
            float groundLevel = GRIDLEVEL;

            // Taille et position du terrain
            Vector2 planeSize = new Vector2(1000, 1000);
            Vector3 planePosition = new Vector3(0, GRIDLEVEL, 0);

            // Variables pour la gestion du mouvement et de la rotation
            float targetRotationAngle = 0f;
            float currentRotationAngle = 0f;
            float rotationSpeed = 5f;

            // Clé
            PBRMaterial KeyMaterial = new PBRMaterial("assets/textures/key/");
            Model keyModel = LoadModel("assets/objects/key/Old_Key.obj");
            float keyRotationAngle = 0.0f;
            bool keyCanAppear = false;
            bool keySoundPlayed = false;
            Vector3 keyDimensions = new Vector3(0.5f, 1f, 0.5f);
            Vector3 keyPosition = new Vector3();
            InitModels(keyModel, KeyMaterial);
            

            // Temps de recharge de 5 secondes
            Cooldown cooldown = new Cooldown(5.0f);

            // Caméra dimensionnelle 
            DimensionCamera dimensionCamera = new DimensionCamera();

            // Dimensions de la porte
            Vector3 doorDimensions = new Vector3(1.4f, 2.5f, 0.5f);

            // Création des obstacles
            Obstacle obstacle1 = new(
                defaultGrey,
                metalYellow,
                "assets/objects/obstacles/Obstacle_hole.obj",
                new Vector3(11),
                new Vector3(0),
                doorDimensions,
                "assets/objects/door/opendoor.obj",
                "assets/objects/door/closeddoor.obj",
                groundLevel
            );
            Obstacle obstacle2 = new(
                defaultGrey,
                obstacleObject,
                new Vector3(2, 30, 2),
                new Vector3(-10, 0, 0),
                groundLevel
            );
            Obstacle obstacle3 = new(
                defaultGrey,
                obstacleObject,
                new Vector3(2, 30, 2),
                new Vector3(10, 0, 0),
                groundLevel
            );
            Obstacle obstacle4 = new(
                defaultGrey,
                obstacleObject,
                new Vector3(2, 30, 2),
                new Vector3(0, 0, 10),
                groundLevel
            );
            Obstacle obstacle5 = new(
                defaultGrey,
                obstacleObject,
                new Vector3(3), 
                new Vector3(15, 0, 0),
                groundLevel
            );
            Obstacle obstacle6 = new(
                defaultGrey,
                obstacleObject,
                new Vector3(3), 
                new Vector3(-20, 0, 3),
                groundLevel
            ); 
            Obstacle obstacle7 = new(
                defaultGrey,
                obstacleObject,
                new Vector3(3), 
                new Vector3(-13, 0, -8),
                groundLevel
            );

            // Création des cubes déplaçables
            MovableCube movableCube1 = new(
               defaultred,
               cubeObject,
               new Vector3(1),
               groundLevel
           );
            MovableCube movableCube2 = new(
                defaultBlue,
                cubeObject,
                new Vector3(1),
                groundLevel
            );

            // Création des plaques de pression
            PressurePlate plate1 = new(
                pressedPlateObject,
                unpressedPlateObject,
                defaultred,
                new Vector3(3, 0.3f, 3),
                new Vector3(10, groundLevel, 10),
                movableCube1
            );
            PressurePlate plate2 = new(
                pressedPlateObject,
                unpressedPlateObject,
                defaultBlue,
                new Vector3(3, 0.3f, 3),
                new Vector3(obstacle1.Position.X, obstacle1.Position.Y + obstacle1.Dimensions.Y / 2, obstacle1.Position.Z),
                movableCube2
            );

            // Création du joueur
            Player player1 = new(
                metalGrey,
                cubeObject,
                new Vector3(1),
                groundLevel
            );

            bool bothPlatesPressed = false;

            // Couloir de fin de partie
            BoundingBox finishLevelBox = new BoundingBox();
            BoundingBox hallway = new BoundingBox();
            BoundingBox wall1 = new BoundingBox();
            BoundingBox wall2 = new BoundingBox();

            // Listes d'objets
            List<MovableCube> movableCubes = new List<MovableCube> { movableCube1, movableCube2 };
            List<Obstacle> obstacles = new List<Obstacle> { obstacle1, obstacle2, obstacle3, obstacle4, obstacle5, obstacle6, obstacle7 };
            List<PressurePlate> plates = new List<PressurePlate> { plate1, plate2 };

            // Positionnement du joueur et des cubes déplaçables
            player1.Position = new Vector3(0, groundLevel, 20);
            movableCube1.Position = new Vector3(obstacle1.Position.X, 100, obstacle1.Position.Z);
            movableCube2.Position = new Vector3(obstacle3.Position.X, 200, obstacle3.Position.Z);

            // Position de clé
            float obstacle2Top = obstacle2.Position.Y + obstacle2.Dimensions.Y / 2 + keyDimensions.Y / 2;
            keyPosition.Y = obstacle2Top;
            keyPosition.X = obstacle2.Position.X;
            keyPosition.Z = obstacle2.Position.Z;

            // Musique
            PlayMusicStream(backgroundMusic);
            SetMusicVolume(backgroundMusic, 0.1f);

            // Boucle principale
            while (currentState == GameState.Playing)
            {
                // Quitter
                if(IsKeyPressed(KeyboardKey.Escape))
                {
                    currentState = GameState.Menu;
                }
                // Jouer la musique en boucle
                UpdateMusicStream(backgroundMusic);

                // Calcul des shaders/ombres
                Shaders.UpdatePBRLighting(dimensionCamera.Camera.Position);

                float deltaTime = GetFrameTime();

                // Augmentation de l'angle de rotation et hitbox de clé
                keyRotationAngle++;
                BoundingBox keyBox = new BoundingBox(keyPosition - (keyDimensions / 2), keyPosition + (keyDimensions / 2));

                // Rotation de caméra
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

                // Alignement de la caméra avec le joueur
                dimensionCamera.PlayerPosition = player1.Position;
                dimensionCamera.Radians = radians;
                dimensionCamera.MoveDirection = moveDirection;

                // Passer en vue orthographique
                if (IsKeyPressed(KeyboardKey.Q))
                {
                    cooldown.ToggleView();
                }
                
                // Mettre à jour la caméra et le joueur
                dimensionCamera.UpdateCamera(deltaTime);
                player1.Update(moveDirection, strafeDirection, deltaTime);

                // Mettre à jour les cubes déplaçables
                foreach(MovableCube mvCube in movableCubes)
                {
                    mvCube.Update(deltaTime, player1, movableCubes);
                    mvCube.HandleCollision(obstacles, cooldown.IsTopView);
                    mvCube.HandlePlate(plates);
                }

                // Mettre à jour le temps de recharge
                cooldown.Update(deltaTime);

                if (cooldown.IsTopView)
                {
                    dimensionCamera.TopViewPosition();
                }
                else
                {
                    dimensionCamera.DefaultPosition();
                }

                if (obstacle1.IsDoorOpen && !obstacle1.DoorExtended)
                {

                    // Création du couloir de fin
                    float extension = 7f;
                    float reduction = 0.1f;

                    // Couloir franchissable
                    hallway = new BoundingBox(
                        new Vector3(obstacle1.DoorBox.Min.X + reduction, obstacle1.DoorBox.Min.Y, obstacle1.DoorBox.Min.Z),
                        new Vector3(obstacle1.DoorBox.Max.X - reduction, obstacle1.DoorBox.Max.Y, obstacle1.DoorBox.Max.Z + extension / 2 - reduction)
                    );

                    // Zone de détection pour terminer le niveau
                    finishLevelBox = new BoundingBox(
                        new Vector3(hallway.Min.X, hallway.Min.Y, hallway.Max.Z),
                        new Vector3(hallway.Max.X, hallway.Max.Y, hallway.Max.Z + extension / 2)
                    );

                    // Murs du couloir
                    wall1 = new BoundingBox(
                        new Vector3(obstacle1.DoorBox.Min.X, obstacle1.DoorBox.Min.Y, obstacle1.DoorBox.Min.Z + reduction * 3),
                        new Vector3(obstacle1.DoorBox.Min.X, obstacle1.DoorBox.Max.Y, obstacle1.DoorBox.Max.Z + extension)
                    );
                    wall2 = new BoundingBox(
                        new Vector3(obstacle1.DoorBox.Max.X, obstacle1.DoorBox.Min.Y, obstacle1.DoorBox.Min.Z + reduction * 3),
                        new Vector3(obstacle1.DoorBox.Max.X, obstacle1.DoorBox.Max.Y, obstacle1.DoorBox.Max.Z + extension)
                    );

                    // Le couloir est créé
                    obstacle1.DoorExtended = true;
                }

                bothPlatesPressed = PressurePlate(movableCubes, plates);
                if (bothPlatesPressed)
                {
                    keyCanAppear = true;
                }
                if (keyCanAppear && !keySoundPlayed)
                {
                    PlaySound(keyAppearedSound);
                    keySoundPlayed = true;
                }
                if (keyCanAppear)
                {
                    if (!player1.HasKey)
                    {
                        // Rotation de la clé
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
                        // Collision avec les murs du couloir
                        player1.HandleHallway(walls);
                    }
                    else
                    {
                        // Collision normales
                        player1.HandleCollision(obstacles, cooldown.IsTopView);
                        player1.HandlePlate(plates);
                    }
                }
                else
                {
                    // Collision normales
                    player1.HandleCollision(obstacles, cooldown.IsTopView);
                    player1.HandlePlate(plates); 
                }
                if (!obstacle1.IsDoorOpen)
                {
                    obstacle1.DoorExtended = false;
                }

                if (CheckCollisionBoxes(player1.Box, finishLevelBox))
                {
                    // Fin du niveau
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
                    DrawModelEx(obstacle.Model, obstacle.Position, Vector3.One, 0f, obstacle.Scale, Color.White);
                }

                foreach (MovableCube mvCube in movableCubes)
                {
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
                    if (!obstacle1.IsDoorOpen && CheckCollisionBoxes(player1.Box, obstacle1.DoorBox))
                    {
                        obstacle1.IsDoorOpen = true;
                        PlaySound(openDoorSound);
                        player1.HasUsedKey = true;
                    }

                    if (!player1.HasUsedKey)
                    {
                        DrawModel(keyModel, new Vector3(player1.Position.X, player1.Position.Y + keyDimensions.Y * 1.5f, player1.Position.Z), 1, Color.White);
                    }
                }

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

                // Centre de la fenêtre
                int barWidth = 400; 
                int barHeight = 5;
                int barX = (screenWidth - barWidth) / 2;
                int barY = screenHeight - barHeight - 60;

                // Dessiner la barre de cooldown
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
                    plate.IsPressed = false;

                    if (plate.PressableBy != null && CheckCollisionBoxes(plate.PressableBy.Box, plate.PressBox))
                    {
                        plate.IsPressed = true;
                    }

                    if (plate.IsPressed && !previousState)
                    {
                        PlaySound(pressureClick);
                    }
                    else if (!plate.IsPressed && previousState)
                    {
                        PlaySound(pressureUnclick);
                    }

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
