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
            InitWindow(1920, 1080, "Raylib 3D in C#");
            SetTargetFPS(60);
            // Désactiver le curseur
            DisableCursor();

            // Charger le modèle et initialiser le matériau
            Mesh cubeMesh = GenMeshCube(1f, 1f, 1f);
            Model cubeModel = LoadModelFromMesh(cubeMesh); // Charger le modèle
            Material cubeMaterial = cubeModel.Materials[0]; // Récupérer le matériau par défaut

            // Position initiale du cube (le joueur)
            Vector3 playerPosition = new Vector3(0, 1, 0); // Position du joueur
            Vector3 cubePosition = new Vector3(3, 4, 3);   // Position d'un autre cube

            // Hitbox
            BoundingBox playerBox = new BoundingBox(playerPosition - new Vector3(0.5f), playerPosition + new Vector3(0.5f));
            BoundingBox cubeBox = new BoundingBox(cubePosition - new Vector3(1.5f, 4.5f, 1.5f), cubePosition + new Vector3(1.5f, 4.5f, 1.5f));

            // Charger les textures
            Texture2D baseColor = LoadTexture("assets/textures/metal/2K/Poliigon_MetalSteelBrushed_7174_BaseColor.jpg");
            Texture2D normalMap = LoadTexture("assets/textures/metal/2K/Poliigon_MetalSteelBrushed_7174_Normal.png");
            Texture2D metallicMap = LoadTexture("assets/textures/metal/2K/Poliigon_MetalSteelBrushed_7174_Metallic.jpg");
            Texture2D roughnessMap = LoadTexture("assets/textures/metal/2K/Poliigon_MetalSteelBrushed_7174_Roughness.jpg");

            // Appliquer les textures au matériau
            SetMaterialTexture(ref cubeMaterial, MaterialMapIndex.Albedo, baseColor); // Couleur de base
            SetMaterialTexture(ref cubeMaterial, MaterialMapIndex.Normal, normalMap); // Carte de normal
            SetMaterialTexture(ref cubeMaterial, MaterialMapIndex.Metalness, metallicMap); // Métallique
            SetMaterialTexture(ref cubeMaterial, MaterialMapIndex.Roughness, roughnessMap); // Rugosité
            SetMaterialTexture(ref cubeModel, 0, MaterialMapIndex.Albedo, ref baseColor);

            // Charger le shader pour la lumière
            Shader lightingShader = LoadShader("assets/shaders/lighting.vs", "assets/shaders/lighting.fs");
            SetMaterialShader(ref cubeModel, 0, ref lightingShader);
            // Configurer la lumière
            int lightLoc = GetShaderLocation(lightingShader, "lightPosition");
            Vector3 lightPosition = new Vector3(5, 5, 5);
            SetShaderValue(lightingShader, lightLoc, lightPosition, ShaderUniformDataType.Vec3);

            // Variable pour suivre si on est en mode vue de dessus
            bool isTopView = false;

            // Configuration de la caméra
            Camera3D camera = new Camera3D
            {
                Position = new Vector3(10, 10, 10),
                Target = new Vector3(0.0f, 0.0f, 0.0f),
                Up = new Vector3(0.0f, 1.0f, 0.0f),
                FovY = 45.0f,
                Projection = CameraProjection.Perspective
            };

            // Variables pour la rotation fluide
            float targetRotationAngle = 0f; // Angle cible de rotation
            float currentRotationAngle = 0f; // Angle actuel de la caméra
            float rotationSpeed = 5f;       // Vitesse de rotation (degrés par seconde)

            while (!WindowShouldClose())
            {
                // Calculer le temps écoulé depuis le dernier frame
                float deltaTime = GetFrameTime();
                if (IsKeyPressed(KeyboardKey.Q))
                {
                    isTopView = !isTopView; // Alterner entre vue normale et vue de dessus
                }
                if (IsKeyPressed(KeyboardKey.E) && !isTopView)
                {
                    targetRotationAngle += 90f; // Tourner de +90 degrés

                }

                // Interpolation fluide entre l'angle actuel et l'angle cible
                currentRotationAngle = Raymath.Lerp(currentRotationAngle, targetRotationAngle, rotationSpeed * deltaTime);
                // Calculer l'angle de rotation sur le joueur (playerPosition)
                float radians = MathF.PI * currentRotationAngle / 180f; // Conversion de l'angle en radians
                // Déterminer la direction de mouvement en fonction de l'angle de rotation
                Vector3 moveDirection = new Vector3(MathF.Sin(radians), 0, MathF.Cos(radians));

                // Calculer la direction de mouvement à gauche et à droite
                Vector3 moveDirectionRight = new Vector3(MathF.Cos(radians), 0, -MathF.Sin(radians));  // Droite
                Vector3 moveDirectionLeft = new Vector3(-MathF.Cos(radians), 0, MathF.Sin(radians));  // Gauche


                Vector3 velocity = Vector3.Zero;
                playerPosition += Raymath.Vector3Normalize(velocity) / 6;

                // DÉPLACEMENTS DU JOUEUR
                if (IsKeyDown(KeyboardKey.W) || IsKeyDown(KeyboardKey.Up)) velocity += moveDirection * 0.1f; // Déplacement en avant
                if (IsKeyDown(KeyboardKey.S) || IsKeyDown(KeyboardKey.Down)) velocity -= moveDirection * 0.1f; // Déplacement en arrière
                if (IsKeyDown(KeyboardKey.A) || IsKeyDown(KeyboardKey.Left)) velocity += moveDirectionRight * 0.1f; // Déplacement à droite
                if (IsKeyDown(KeyboardKey.D) || IsKeyDown(KeyboardKey.Right)) velocity += moveDirectionLeft * 0.1f; // Déplacement à gauche

                

                Vector3 newPosition = playerPosition + velocity;
                BoundingBox newPlayerBox = new BoundingBox(newPosition - new Vector3(0.5f), newPosition + new Vector3(0.5f));


                // COLLISION + GLISSADE CONTRE LA PAROI
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


                // ACTIVER/DESACTIVER MODE PLAN
                if (isTopView)
                {
                    // Vue depuis le dessus
                    camera.Position = new Vector3(playerPosition.X, 10.0f, playerPosition.Z);
                    camera.Target = new Vector3(playerPosition.X, 0.0f, playerPosition.Z);
                    camera.Up = new Vector3(0.0f, 0.0f, 1.0f);
                    camera.Projection = CameraProjection.Orthographic; // CHANGEMENT DE LA PROJECTION
                }
                else
                {
                    isTopView = false;
                    Vector3 cameraOffset = new Vector3(
                        -MathF.Sin(radians) * 10.0f,
                        1.0f,
                        -MathF.Cos(radians) * 10.0f
                    );
                    camera.Position = playerPosition + cameraOffset;
                    camera.Target = playerPosition;
                    camera.Up = new Vector3(0.0f, 1.0f, 0.0f);
                    camera.Projection = CameraProjection.Perspective; // CHANGEMENT DE LA PROJECTION
                }


                // Début du dessin
                BeginDrawing();
                ClearBackground(Color.RayWhite);
                BeginMode3D(camera);
                DrawGrid(30, 1.0f); // Grille de repère

                // Obstacle
                DrawCube(cubePosition, 3.0f, 9.0f, 3.0f, Color.DarkGray);
                DrawCubeWires(cubePosition, 3.0f, 9.0f, 3.0f, Color.Red);
                // Dessin du cube texturé
                DrawModel(cubeModel, playerPosition, 1, Color.White);
                DrawCubeWires(playerPosition, 1.0f, 1.0f, 1.0f, Color.Red);

                // Fin du dessin
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
