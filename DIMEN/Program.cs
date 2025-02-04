using Raylib_cs;
using static Raylib_cs.Raylib;
using System.Numerics;
using System;

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

            // Charger les ressources (modèles, textures, shader)
            Mesh cubeMesh = GenMeshCube(1f, 1f, 1f);
            Model cubeModel = LoadModelFromMesh(cubeMesh); // Charger le modèle
            Material cubeMaterial = cubeModel.Materials[0]; // Récupérer le matériau par défaut

            // Charger les textures
            Texture2D baseColor = LoadTexture("assets/textures/metal/2K/Poliigon_MetalSteelBrushed_7174_BaseColor.jpg");
            Texture2D normalMap = LoadTexture("assets/textures/metal/2K/Poliigon_MetalSteelBrushed_7174_Normal.png");
            Texture2D metallicMap = LoadTexture("assets/textures/metal/2K/Poliigon_MetalSteelBrushed_7174_Metallic.jpg");
            Texture2D roughnessMap = LoadTexture("assets/textures/metal/2K/Poliigon_MetalSteelBrushed_7174_Roughness.jpg");

            // Appliquer les textures au matériau
            SetMaterialTexture(ref cubeMaterial, MaterialMapIndex.Albedo, baseColor);
            SetMaterialTexture(ref cubeMaterial, MaterialMapIndex.Normal, normalMap);
            SetMaterialTexture(ref cubeMaterial, MaterialMapIndex.Metalness, metallicMap);
            SetMaterialTexture(ref cubeMaterial, MaterialMapIndex.Roughness, roughnessMap);
            SetMaterialTexture(ref cubeModel, 0, MaterialMapIndex.Albedo, ref baseColor);

            // Charger et configurer le shader
            Shader lightingShader = LoadShader("assets/shaders/lighting.vs", "assets/shaders/lighting.fs");
            SetMaterialShader(ref cubeModel, 0, ref lightingShader);

            // Configurer la lumière
            int lightLoc = GetShaderLocation(lightingShader, "lightPosition");
            Vector3 lightPosition = new Vector3(5, 5, 5);
            SetShaderValue(lightingShader, lightLoc, lightPosition, ShaderUniformDataType.Vec3);

            // Configuration de la caméra
            Camera3D camera = new Camera3D
            {
                Position = new Vector3(10, 10, 10),
                Target = new Vector3(0.0f, 0.0f, 0.0f),
                Up = new Vector3(0.0f, 1.0f, 0.0f),
                FovY = 45.0f,
                Projection = CameraProjection.Perspective
            };

            // Variables pour la gestion du mouvement et de la rotation
            float targetRotationAngle = 0f;
            float currentRotationAngle = 0f;
            float rotationSpeed = 5f;

            // Variables pour la gravité et la physique
            Vector3 velocity = Vector3.Zero;
            float gravity = -9.81f;
            float gridLevel = 0.5f;
            // Variables de gestion de la vitesse
            float maxSpeed = 0.005f; // Vitesse maximale du joueur

            // Positions des objets
            Vector3 playerPosition = new Vector3(0, 10, 0);
            Vector3 cubePosition = new Vector3(3, 4.25f, 3);

            // Variables du mode plan
            bool isTopView = false;
            float topViewDuration = 5.0f;  // Temps maximum en vue de dessus
            float topViewTimer = 0.0f;     // Timer pour la durée de la vue de dessus
            float topViewCooldown = 5.0f; // Temps de recharge avant de pouvoir recliquer Q
            double lastTopViewTime = -topViewCooldown; // Stocke le temps du dernier passage en vue de dessus

            // Boucle principale
            while (!WindowShouldClose())
            {
                float deltaTime = GetFrameTime();
                double currentTime = GetTime();
                DrawBar(currentTime, lastTopViewTime, topViewCooldown, isTopView, topViewDuration, ref topViewTimer);  // Passer topViewTimer par référence
                // Appliquer la gravité
                velocity.Y += gravity;

                // Vérification du contact avec le sol
                if (playerPosition.Y <= gridLevel)
                {
                    playerPosition.Y = gridLevel;
                    velocity.Y = 0;
                }

                // Calcul de la direction de la caméra
                Vector3 cameraDirection = Vector3.Normalize(camera.Target - camera.Position);
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

                // Mise à jour de la position du joueur
                playerPosition += Raymath.Vector3Normalize(velocity) / 6;

                // Déplacements du joueur (avant/arrière et latéraux)
                if (IsKeyDown(KeyboardKey.W) || IsKeyDown(KeyboardKey.Up)) velocity += moveDirection * 0.1f;
                if (IsKeyDown(KeyboardKey.S) || IsKeyDown(KeyboardKey.Down)) velocity -= moveDirection * 0.1f;
                if (IsKeyDown(KeyboardKey.D) || IsKeyDown(KeyboardKey.Left)) velocity -= strafeDirection * 0.1f;
                if (IsKeyDown(KeyboardKey.A) || IsKeyDown(KeyboardKey.Right)) velocity += strafeDirection * 0.1f;
                if (!IsKeyDown(KeyboardKey.W) && !IsKeyDown(KeyboardKey.S) && !IsKeyDown(KeyboardKey.A) && !IsKeyDown(KeyboardKey.D))
                {
                    velocity = Vector3.Zero; // Arrêter le joueur
                }
                // Limiter la vitesse
                if (velocity.Length() > maxSpeed)  // Si la vitesse dépasse la vitesse maximale
                {
                    velocity = Vector3.Normalize(velocity) * maxSpeed; // Normaliser et appliquer la vitesse maximale
                }
                // Appliquer la vitesse au joueur
                playerPosition += velocity;

                // Détection de collision avec la grille (niveau Y de la grille)
                if (playerPosition.Y <= gridLevel)
                {
                    playerPosition.Y = gridLevel; // Réinitialiser la position au niveau du sol
                    velocity.Y = 0; // Réinitialiser la vitesse verticale
                }
                Vector3 newPosition = playerPosition + velocity;



                // Changer de mode de vue (vue de dessus ou perspective)
                if (IsKeyPressed(KeyboardKey.Q) && (currentTime - lastTopViewTime >= topViewCooldown))
                {
                    isTopView = !isTopView;
                    lastTopViewTime = currentTime; // Stocke le temps du dernier passage en vue de dessus
                    topViewTimer = 0.0f; // Réinitialise le timer si on active la vue de dessus
                }
                if (isTopView)
                {
                    topViewTimer += deltaTime;
                    if (topViewTimer >= topViewDuration)
                    {
                        isTopView = false; // Désactiver la vue de dessus après 5 secondes
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
                    //DrawText($"Temps avant prochaine vue de dessus: {MathF.Max(0, topViewCooldown - (float)(currentTime - lastTopViewTime)):F1}s", 10, 110, 20, Color.Red)
                }

                // Dessin de la scène
                BeginDrawing();
                ClearBackground(Color.RayWhite);
                BeginMode3D(camera);
                DrawGrid(30, 1.0f);
                DrawCube(cubePosition, 3.0f, 9.0f, 3.0f, Color.DarkGray);
                DrawCubeWires(cubePosition, 3.0f, 9.0f, 3.0f, Color.Red);
                DrawModel(cubeModel, playerPosition, 1, Color.White);
                DrawCubeWires(playerPosition, 1.0f, 1.0f, 1.0f, Color.Red);
                EndMode3D();
                // Debug
                DrawText($"Position: X:{playerPosition.X:F2}, Y:{playerPosition.Y:F2}, Z:{playerPosition.Z:F2}", 10, 10, 20, Color.Black);
                DrawText($"FPS: {GetFPS()}", 10, 30, 20, Color.Black);
                DrawText($"Player Speed: {velocity}", 10, 50, 20, Color.Black);
                EndDrawing();
            }
            // Déchargement des ressources
            UnloadMaterial(cubeMaterial);
            UnloadTexture(baseColor);
            UnloadTexture(normalMap);
            UnloadTexture(metallicMap);
            UnloadTexture(roughnessMap);
            CloseWindow();
        }
        // Calculer la barre de cooldown et la mise à jour du temps
        static void DrawBar(double currentTime, double lastTopViewTime, float topViewCooldown, bool isTopView, float topViewDuration, ref float topViewTimer)
        {
            // Dimensions et position de la barre de cooldown
            int barWidth = 200;
            int barHeight = 20;
            int barX = 10;
            int barY = 130;

            // Calcul du pourcentage de remplissage
            float cooldownProgress = 0.0f;

            if (isTopView)
            {
                // Si on est en mode vue de dessus, la barre se vide
                topViewTimer += (float)(currentTime - lastTopViewTime); // Incrémentation du timer uniquement pendant la vue de dessus
                cooldownProgress = 1.0f - (topViewTimer / topViewDuration); // La barre se vide pendant la durée de la vue de dessus
            }
            else
            {
                // Si on n'est pas en mode vue de dessus, la barre se remplit
                // La barre doit commencer pleine lorsque le cooldown commence
                float timeSinceLastTopView = (float)(currentTime - lastTopViewTime);
                if (timeSinceLastTopView >= topViewDuration) // Si le mode vue de dessus est terminé
                {
                    cooldownProgress = (timeSinceLastTopView - topViewDuration) / topViewCooldown;
                }
            }

            // Clamp entre 0 et 1 pour éviter que la barre dépasse ses limites
            cooldownProgress = MathF.Min(MathF.Max(cooldownProgress, 0.0f), 1.0f);
            int fillWidth = (int)(cooldownProgress * barWidth);

            // Dessin de la barre de cooldown
            DrawRectangle(barX, barY, barWidth, barHeight, Color.DarkGray); // Fond
            DrawRectangle(barX, barY, fillWidth, barHeight, Color.Red); // Remplissage en fonction du cooldown
            DrawRectangleLines(barX, barY, barWidth, barHeight, Color.Black); // Contour de la barre 
        }


    }
}
