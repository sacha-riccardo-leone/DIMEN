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
            // Désactiver le curseur
            DisableCursor();

            // Charger le modèle et initialiser le matériau
            Mesh cubeMesh = GenMeshCube(1f, 1f, 1f);
            Model cubeModel = LoadModelFromMesh(cubeMesh); // Charger le modèle
            Material cubeMaterial = cubeModel.Materials[0]; // Récupérer le matériau par défaut

            // Position initiale du cube (le joueur)
            Vector3 playerPosition = new Vector3(0, 1, 0); // Position du joueur
            Vector3 cubePosition = new Vector3(3, 1, 3);   // Position d'un autre cube

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
                Position = new Vector3(10, 10, 10), // Position de la caméra
                Target = new Vector3(0.0f, 0.0f, 0.0f), // Point regardé
                Up = new Vector3(0.0f, 1.0f, 0.0f),    // Orientation "haut"
                FovY = 45.0f,                           // Champ de vision
                Projection = CameraProjection.Perspective  // Perspective
            };

            // Variables pour la rotation fluide
            float targetRotationAngle = 0f; // Angle cible de rotation
            float currentRotationAngle = 0f; // Angle actuel de la caméra
            float rotationSpeed = 5f;       // Vitesse de rotation (degrés par seconde)

            while (!WindowShouldClose())
            {
                // Calculer le temps écoulé depuis le dernier frame
                float deltaTime = GetFrameTime();
                // Basculer la vue lorsque la touche Q est pressée
                if (IsKeyPressed(KeyboardKey.Q))
                {
                    isTopView = !isTopView; // Alterner entre vue normale et vue de dessus
                }
                // Vérifier si la touche espace est pressée pour faire tourner le joueur
                if (IsKeyPressed(KeyboardKey.Space) && !isTopView)
                {
                    targetRotationAngle += 90f; // Tourner de +90 degrés
                    // Ne pas réinitialiser l'angle à 0 à chaque fois, laisser l'angle augmenter
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

                // Déplacement du joueur à gauche et à droite
                if (IsKeyDown(KeyboardKey.A) || IsKeyDown(KeyboardKey.Left)) playerPosition += moveDirectionRight * 0.1f; // Déplacement à droite
                if (IsKeyDown(KeyboardKey.D) || IsKeyDown(KeyboardKey.Right)) playerPosition += moveDirectionLeft * 0.1f; // Déplacement à gauche

                if (isTopView)
                {
                    // Vue depuis le dessus
                    camera.Position = new Vector3(playerPosition.X, 10.0f, playerPosition.Z); // Position en hauteur au-dessus du joueur
                    camera.Target = new Vector3(playerPosition.X, 0.0f, playerPosition.Z);  // La caméra regarde le sol
                    camera.Up = new Vector3(0.0f, 0.0f, 1.0f); // Orientation "haut" de la caméra (la caméra regarde vers le bas)
                    if (IsKeyDown(KeyboardKey.W)) playerPosition += moveDirection * 0.1f; // Déplacement en avant
                    if (IsKeyDown(KeyboardKey.S)) playerPosition -= moveDirection * 0.1f; // Déplacement en arrière

                }
                else
                {
                    isTopView = false;
                    // Vue normale derrière le joueur
                    Vector3 cameraOffset = new Vector3(
                        -MathF.Sin(radians) * 10.0f,
                        1.0f,
                        -MathF.Cos(radians) * 10.0f
                    );
                    camera.Position = playerPosition + cameraOffset; // Position de la caméra
                    camera.Target = playerPosition; // La caméra regarde toujours le joueur
                    camera.Up = new Vector3(0.0f, 1.0f, 0.0f);    // Orientation "haut"
                }


                // Dessin
                BeginDrawing();
                ClearBackground(Color.RayWhite);
                BeginMode3D(camera);
                DrawGrid(30, 1.0f); // Grille de repère
                DrawCube(cubePosition, 1.0f, 1.0f, 1.0f, Color.Red);
                // Dessin du cube texturé
                DrawModel(cubeModel, playerPosition, 1, Color.White);

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
