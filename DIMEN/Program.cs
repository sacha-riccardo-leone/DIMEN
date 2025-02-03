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

            // Coefficient de frottement
            float frictionCoefficient = 0.65f;

            // Variables pour la gestion du mouvement et de la rotation
            float targetRotationAngle = 0f;
            float currentRotationAngle = 0f;
            float rotationSpeed = 5f;

            // Variables pour la gravité et la physique
            Vector3 velocity = Vector3.Zero;
            float gravity = -9.81f;
            float gridLevel = 1.0f;

            // Positions des objets
            Vector3 playerPosition = new Vector3(0, 10, 0);
            Vector3 cubePosition = new Vector3(3, 4.25f, 3);

            // Hitboxes pour la détection de collisions
            BoundingBox playerBox = new BoundingBox(playerPosition - new Vector3(0.5f), playerPosition + new Vector3(0.5f));
            BoundingBox cubeBox = new BoundingBox(cubePosition - new Vector3(1.5f, 4.5f, 1.5f), cubePosition + new Vector3(1.5f, 4.5f, 1.5f));

            // Variable pour savoir si on est en vue de dessus
            bool isTopView = false;

            // Boucle principale
            while (!WindowShouldClose())
            {
                // Appliquer la gravité
                velocity.Y += gravity * GetFrameTime();

                // Mise à jour de la position du cube
                playerPosition += velocity * GetFrameTime();

                // Vérification du contact avec le sol
                if (playerPosition.Y <= gridLevel)
                {
                    playerPosition.Y = gridLevel;
                    velocity.Y = 0;
                }

                // Calcul de la direction de la caméra
                Vector3 cameraDirection = Vector3.Normalize(camera.Target - camera.Position);
                float deltaTime = GetFrameTime();

                // Changer de mode de vue (vue de dessus ou perspective)
                if (IsKeyPressed(KeyboardKey.Q)) isTopView = !isTopView;
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

                // Si le joueur se déplace, applique le frottement
                if (velocity.X != 0 || velocity.Z != 0)
                {
                    velocity.X *= frictionCoefficient;
                    velocity.Z *= frictionCoefficient;
                }
                else
                {
                    // Si aucune touche n'est pressée, réduire lentement la vitesse
                    if (Math.Abs(velocity.X) < 0.01f) velocity.X = 0;
                    if (Math.Abs(velocity.Z) < 0.01f) velocity.Z = 0;
                    else
                    {
                        velocity.X *= frictionCoefficient;
                        velocity.Z *= frictionCoefficient;
                    }
                }
                Vector3 newPosition = playerPosition + velocity;
                // Détection de collision avec la grille (niveau Y de la grille)
                if (newPosition.Y <= gridLevel)
                {
                    newPosition.Y = gridLevel; // Empêcher de passer sous la grille
                    velocity.Y = 0; // Réinitialiser la vitesse verticale
                }
                BoundingBox newPlayerBox = new BoundingBox(newPosition - new Vector3(0.5f), newPosition + new Vector3(0.5f));

                // Détection de collision et glissade contre les parois
                if (CheckCollisionBoxes(newPlayerBox, cubeBox))
                {
                    if (velocity.X != 0 && !CheckCollisionBoxes(new BoundingBox(playerPosition + new Vector3(velocity.X, 0, 0) - new Vector3(0.5f, 0.5f, 0.5f), playerPosition + new Vector3(velocity.X, 0, 0) + new Vector3(0.5f, 0.5f, 0.5f)), cubeBox))
                    {
                        playerPosition.X += velocity.X;
                    }
                    if (velocity.Z != 0 && !CheckCollisionBoxes(new BoundingBox(playerPosition + new Vector3(0, 0, velocity.Z) - new Vector3(0.5f, 0.5f, 0.5f), playerPosition + new Vector3(0, 0, velocity.Z) + new Vector3(0.5f, 0.5f, 0.5f)), cubeBox))
                    {
                        playerPosition.Z += velocity.Z;
                    }
                }
                else
                {
                    playerPosition = newPosition;
                }
                playerBox = new BoundingBox(playerPosition - new Vector3(0.5f, 0.5f, 0.5f), playerPosition + new Vector3(0.5f, 0.5f, 0.5f));

                // Activer/Désactiver le mode de vue de dessus
                if (isTopView)
                {
                    camera.Position = new Vector3(playerPosition.X, 200.0f, playerPosition.Z);
                    camera.Target = new Vector3(playerPosition.X + moveDirection.X, 0.0f, playerPosition.Z + moveDirection.Z);
                    camera.Up = new Vector3(moveDirection.X, 0.0f, moveDirection.Z);
                    camera.Projection = CameraProjection.Orthographic;
                }
                else
                {
                    Vector3 cameraOffset = new Vector3(
                        -MathF.Sin(radians) * 10.0f,
                        1.0f,
                        -MathF.Cos(radians) * 10.0f
                    );
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
                DrawCube(cubePosition, 3.0f, 9.0f, 3.0f, Color.DarkGray);
                DrawCubeWires(cubePosition, 3.0f, 9.0f, 3.0f, Color.Red);
                DrawModel(cubeModel, playerPosition, 1, Color.White);
                DrawCubeWires(playerPosition, 1.0f, 1.0f, 1.0f, Color.Red);
                EndMode3D();
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
