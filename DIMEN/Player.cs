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

        public Player(string playerMaterialPath, string playerModelPath, Vector3 playerDimensions, float groundLevel)
        {
            Material = new PBRMaterial (playerMaterialPath);
            Model = LoadModel(playerModelPath);

            InitModels(Model, Material);

            Dimensions = playerDimensions;
            Position = new Vector3 (-10, groundLevel,0);
            Box = new BoundingBox(Position + Dimensions/2, Position + Dimensions/2);
            GroundLevel = groundLevel;
            Velocity = Vector3.Zero;
            Friction = 0.15f;
}
        public void Update(Vector3 moveDirection, Vector3 strafeDirection, float deltaTime)
        {
            float gravity = -9.81f;
            // Touches de déplacement
            if (IsKeyDown(KeyboardKey.W) || IsKeyDown(KeyboardKey.Up)) MovePlayerUp(moveDirection);
            if (IsKeyDown(KeyboardKey.S) || IsKeyDown(KeyboardKey.Down)) MovePlayerDown(moveDirection);
            if (IsKeyDown(KeyboardKey.D) || IsKeyDown(KeyboardKey.Left)) MovePlayerLeft(strafeDirection);
            if (IsKeyDown(KeyboardKey.A) || IsKeyDown(KeyboardKey.Right)) MovePlayerRight(strafeDirection);

            // Freiner le joueur s'il relâche les touches de déplacement
            if (IsKeyUp(KeyboardKey.W) && IsKeyUp(KeyboardKey.S) && IsKeyUp(KeyboardKey.A) && IsKeyUp(KeyboardKey.D)) Brake();

            // Appliquer les mouvements horizontaux (X, Z) indépendamment de la gravité
            Position.X += Velocity.X;
            Position.Z += Velocity.Z;

            if (Velocity.Y > gravity)
            {
                Velocity.Y = gravity;
            }
            // Appliquer la gravité uniquement si le joueur n'est pas sur le sol
            if (Position.Y > GroundLevel + 0.5f)
            {
                Velocity.Y += gravity * deltaTime;  // Applique la gravité uniquement en l'air
            }
            else
            {
                Velocity.Y = 0f;  // Réinitialise la vitesse verticale quand il touche le sol
                Position.Y = GroundLevel + 0.5f;  // Maintien le joueur sur le sol
            }


            Position += Raymath.Vector3Normalize(Velocity) / 10;
            Box = new BoundingBox(
                    new Vector3(Position.X - 0.5f, Position.Y - 0.5f, Position.Z - 0.5f),
                    new Vector3(Position.X + 0.5f, Position.Y + 0.5f, Position.Z + 0.5f)
            );
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
        public Vector3 LimitSpeed()
        {
            float speed = 0.01f;
            if (Velocity.Length() > speed)
            {
                return Velocity = Vector3.Normalize(Velocity) * speed;
            }
            return Velocity;
        }
        public Vector3 Brake()
        {
            float deceleration = 0.65f;
            Velocity *= deceleration; // Ralentissement progressif
            if (Velocity.Length() < 0.01f)
            {
                return Velocity = Vector3.Zero; // Éviter une vitesse résiduelle
            }
            return Velocity;
        }
        public Vector3 MovePlayerUp(Vector3 moveDirection)
        {
            return Position += moveDirection * Friction;
        }
        public Vector3 MovePlayerDown(Vector3 moveDirection)
        {
            return Position -= moveDirection * Friction;
        }
        public Vector3 MovePlayerLeft(Vector3 strafeDirection)
        {
            return Position -= strafeDirection * Friction;
        }
        public Vector3 MovePlayerRight(Vector3 strafeDirection)
        {
            return Position += strafeDirection * Friction;
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
