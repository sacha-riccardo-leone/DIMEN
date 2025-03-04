using Raylib_cs;
using static Raylib_cs.Raylib;
using System.Numerics;

namespace DIMEN
{
    internal class Player
    {
        public PBRMaterial Material { get; private set; }
        public Model Model { get; private set; }
        public Vector3 Dimensions { get; private set; }
        public Vector3 Position;
        public Vector3 Velocity;
        public BoundingBox Box { get; private set; }
        public float Friction { get; private set; }
        public bool HasKey;
        public float GroundLevel { get; private set; }
        public bool IsFalling;
        public float Speed;

        public Player(string playerMaterialPath, string playerModelPath, Vector3 playerDimensions, float groundLevel)
        {
            Material = new PBRMaterial (playerMaterialPath);
            Model = LoadModel(playerModelPath);
            InitModels(Model, Material);

            Dimensions = playerDimensions;
            Position = new Vector3 (-10, groundLevel,0);
            Box = new BoundingBox(Position + Dimensions/2, Position + Dimensions/2);
            GroundLevel = groundLevel + 0.5f;
            Velocity = Vector3.Zero;
            Friction = 0.15f;
            IsFalling = false;
            Speed = 5.0f;
}
        public void Update(Vector3 moveDirection, Vector3 strafeDirection, float deltaTime)
        {
            float gravity = -9.81f;

            if (Position.Y > GroundLevel)
            {
                Velocity.Y += gravity * deltaTime; // Accumulation progressive de la vitesse de chute
            }
            else
            {
                Velocity.Y = 0f; // Arrêter la chute
                Position.Y = GroundLevel; // Ajuster la position pour coller au sol
            }

            if (Velocity.Y < gravity)
            {
                Velocity.Y = gravity;
            }

            // Gestion des déplacements
            Vector3 movement = Vector3.Zero;

            if (IsKeyDown(KeyboardKey.W)) movement += moveDirection;
            if (IsKeyDown(KeyboardKey.S)) movement -= moveDirection;
            if (IsKeyDown(KeyboardKey.A)) movement += strafeDirection;
            if (IsKeyDown(KeyboardKey.D)) movement -= strafeDirection;

            // Normalisation pour éviter un déplacement plus rapide en diagonale
            if (movement != Vector3.Zero)
            {
                movement = Raymath.Vector3Normalize(movement) * Speed;
                Velocity.X = movement.X;
                Velocity.Z = movement.Z;
            }
            else
            {
                Brake(); // Applique un freinage progressif quand aucune touche n'est pressée
            }

            // Mise à jour de la position avec deltaTime pour une physique plus fluide
            Position += Velocity * deltaTime;

            // Mise à jour de la hitbox
            Box = new BoundingBox(
                new Vector3(Position.X - 0.5f, Position.Y - 0.5f, Position.Z - 0.5f),
                new Vector3(Position.X + 0.5f, Position.Y + 0.5f, Position.Z + 0.5f)
            );
        }

        private void Brake()
        {
            float deceleration = 0.9f; // Freinage plus naturel
            Velocity.X *= deceleration;
            Velocity.Z *= deceleration;

            // Éviter les valeurs proches de zéro qui pourraient causer un mouvement résiduel
            if (Math.Abs(Velocity.X) < 0.01f) Velocity.X = 0f;
            if (Math.Abs(Velocity.Z) < 0.01f) Velocity.Z = 0f;
            
        }

        public void HandleCollision(Obstacle obstacle, bool isTopView)
        {
            if (CheckCollisionBoxes(Box, obstacle.Box) && !isTopView)
            {
                float deltaX = Position.X - obstacle.Position.X;
                float deltaZ = Position.Z - obstacle.Position.Z;
                float deltaY = Position.Y - obstacle.Position.Y;

                float overlapX = Dimensions.X / 2 + obstacle.Dimensions.X / 2 - Math.Abs(deltaX);
                float overlapZ = Dimensions.Z / 2 + obstacle.Dimensions.Z / 2 - Math.Abs(deltaZ);
                float overlapY = Dimensions.Y / 2 + obstacle.Dimensions.Y / 2 - Math.Abs(deltaY);

                if (overlapX < overlapZ && overlapX < overlapY)
                {
                    Position.X += deltaX > 0 ? overlapX : -overlapX;
                }
                else if (overlapZ < overlapX && overlapZ < overlapY)
                {
                    Position.Z += deltaZ > 0 ? overlapZ : -overlapZ;
                }
                else
                {
                    Position.Y += deltaY > 0 ? overlapY - 0.01f : -overlapY + 0.01f;
                }
            }
            if (isTopView)
            {
                if (CheckCollisionBoxes(Box, obstacle.Box))
                {
                    Position.Y = obstacle.Position.Y + obstacle.Dimensions.Y/2;
                }
                else
                {
                    Position.Y = GroundLevel;
                }
            }
        }
        public unsafe void InitModels(Model model, PBRMaterial material)
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

    }
}
