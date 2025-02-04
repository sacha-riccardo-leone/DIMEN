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

            // Dimensions
            Vector3 cubeDimensions = new Vector3(10f, 10f, 10f);
            Vector3 colliderCubeDimension = cubeDimensions / 2;


            // Variables pour la gestion du mouvement et de la rotation
            float targetRotationAngle = 0f;
            float currentRotationAngle = 0f;
            float rotationSpeed = 5f;

            // Variables pour la gravité et la physique
            Vector3 velocity = Vector3.Zero;
            float gravity = -0.2f;
            float gridLevel = 0.5f;
            // Hauteur au sommet du cube
            float topViewHeight = cubeDimensions.Y;
            // Variables de gestion de la vitesse
            float maxSpeed = 0.2f; // Vitesse maximale du joueur

            // Positions des objets
            Vector3 playerPosition = new Vector3(-3, 12, 0);
            Vector3 cubePosition = new Vector3(3, 4.25f, 3);
            BoundingBox playerBox = new BoundingBox(
                playerPosition - new Vector3(0.5f, 0.5f, 0.5f),
                playerPosition + new Vector3(0.5f, 0.5f, 0.5f)
            );


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

                // Appliquer la gravité
                velocity.Y = gravity;
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
                playerPosition += Raymath.Vector3Normalize(velocity) / 5;
                // Déplacements du joueur (avant/arrière et latéraux)
                if (IsKeyDown(KeyboardKey.W) || IsKeyDown(KeyboardKey.Up)) velocity += moveDirection * 0.1f;
                if (IsKeyDown(KeyboardKey.S) || IsKeyDown(KeyboardKey.Down)) velocity -= moveDirection * 0.1f;
                if (IsKeyDown(KeyboardKey.D) || IsKeyDown(KeyboardKey.Left)) velocity -= strafeDirection * 0.1f;
                if (IsKeyDown(KeyboardKey.A) || IsKeyDown(KeyboardKey.Right)) velocity += strafeDirection * 0.1f;
                if (!IsKeyDown(KeyboardKey.W) && !IsKeyDown(KeyboardKey.S) && !IsKeyDown(KeyboardKey.A) && !IsKeyDown(KeyboardKey.D))
                {
                    velocity = Vector3.Zero; // Arrêter le joueur
                }
                if (velocity.Length() > maxSpeed)
                {
                    velocity = Vector3.Normalize(velocity) * maxSpeed;
                }
                if (velocity.Y > gravity)
                {
                    velocity.Y = gravity;
                }
                BoundingBox cubeBox = new BoundingBox(
                    cubePosition - colliderCubeDimension, // Min
                    cubePosition + colliderCubeDimension  // Max
                );
                Vector3 tempPosX = playerPosition + new Vector3(velocity.X, 0, 0);
                BoundingBox testBoxX = new BoundingBox(tempPosX - new Vector3(0.5f, 0.5f, 0.5f), tempPosX + new Vector3(0.5f, 0.5f, 0.5f));

                Vector3 tempPosZ = playerPosition + new Vector3(0, 0, velocity.Z);
                BoundingBox testBoxZ = new BoundingBox(tempPosZ - new Vector3(0.5f, 0.5f, 0.5f), tempPosZ + new Vector3(0.5f, 0.5f, 0.5f));

                if (isTopView)
                {
                    //DrawText($"Collides? : {CheckCollisionBoxes(playerBox, cubeBox)}", 10, 120, 20, Color.Black);
                    if (playerPosition.X >= cubePosition.X - cubeDimensions.X / 2 &&
                        playerPosition.X <= cubePosition.X + cubeDimensions.X / 2 &&
                        playerPosition.Y >= cubePosition.Y - cubeDimensions.Y / 2 &&
                        playerPosition.Y <= cubePosition.Y + cubeDimensions.Y / 2 &&
                        playerPosition.Z >= cubePosition.Z - cubeDimensions.Z / 2 &&
                        playerPosition.Z <= cubePosition.Z + cubeDimensions.Z / 2)
                    {
                        playerPosition.Y = cubePosition.Y + cubeDimensions.Y / 2 + 0.5f;
                    }
                }
                else
                {
                    // Gérer les collisions normales (X et Z) en mode perspective
                    // Vérification de la collision sur X uniquement
                    if (CheckCollisionBoxes(testBoxX, cubeBox))
                    {
                        velocity.X = 0f; // Bloquer le mouvement en X
                    }
                    // Vérification de la collision sur Z uniquement
                    if (CheckCollisionBoxes(testBoxZ, cubeBox))
                    {
                        velocity.Z = 0f; // Bloquer le mouvement en Z
                    }
                }
                // Appliquer la gravité uniquement si le joueur n'est pas sur le sol
                if (playerPosition.Y > gridLevel)
                {
                    velocity.Y += gravity * deltaTime;  // Applique la gravité uniquement en l'air
                }
                else
                {
                    velocity.Y = 0f;  // Réinitialise la vitesse verticale quand il touche le sol
                    playerPosition.Y = gridLevel;  // Maintien le joueur sur le sol
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
                DrawGrid(30, 1.0f);
                DrawCube(cubePosition, cubeDimensions.X, cubeDimensions.Y, cubeDimensions.Z, Color.DarkGray);
                DrawCubeWires(cubePosition, cubeDimensions.X, cubeDimensions.Y, cubeDimensions.Z, Color.Red);


                DrawModel(cubeModel, playerPosition, 1, Color.White);
                DrawCubeWires(playerPosition, 1.0f, 1.0f, 1.0f, Color.Red);
                EndMode3D();
                // Dessin de la barre de progression
                DrawRectangle(10, 10, 200, 25, Color.LightGray);
                DrawRectangle(10, 10, (int)(200 * progress), 25, Color.Green);
                DrawRectangleLines(10, 10, 200, 25, Color.Black);
                // Debug
                //DrawText($"Position: X:{playerPosition.X:F2}, Y:{playerPosition.Y:F2}, Z:{playerPosition.Z:F2}", 10, 10, 20, Color.Black);
                //DrawText($"FPS: {GetFPS()}", 10, 30, 20, Color.Black);
                DrawText($"Player Speed: {velocity}", 10, 50, 20, Color.Black);
                DrawText($"Collides? : {CheckCollisionBoxes(testBoxX, cubeBox) || CheckCollisionBoxes(testBoxZ, cubeBox)}", 10, 70, 20, Color.Black);
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


    }
}
