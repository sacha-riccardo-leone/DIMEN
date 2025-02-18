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
        public bool HasKey { get; private set; }
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
        public void Update()
        {
            Position += Raymath.Vector3Normalize(Velocity) / 10;
            Box = new BoundingBox(
                    new Vector3(Position.X - 0.5f, Position.Y - 0.5f, Position.Z - 0.5f),
                    new Vector3(Position.X + 0.5f, Position.Y + 0.5f, Position.Z + 0.5f)
            );
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
