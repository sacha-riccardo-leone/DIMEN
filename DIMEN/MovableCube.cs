using Raylib_cs;
using static Raylib_cs.Raylib;
using System.Numerics;

namespace DIMEN
{
    internal class MovableCube
    {
        public PBRMaterial Material { get; private set; }
        public Model Model { get; private set; }
        public Vector3 Dimensions { get; private set; }
        public Vector3 Position;
        public Vector3 Velocity;
        public BoundingBox Box { get; private set; }
        public float Friction { get; private set; }
        public float GroundLevel { get; private set; }
        public float Speed;
        public MovableCube(string materialPath, string modelPath, Vector3 dimensions, float groundLevel)
        {
            Material = new PBRMaterial(materialPath);
            Model = LoadModel(modelPath);
            InitModels(Model, Material);
            Dimensions = dimensions;
            Position = new Vector3(-10, groundLevel, 0);
            Box = new BoundingBox(Position + Dimensions / 2, Position + Dimensions / 2);
            GroundLevel = groundLevel + 0.5f;
            Velocity = Vector3.Zero;
            Friction = 0.15f;
            Speed = 5.0f;
        }
        public void Update(float deltaTime, Player player, List<MovableCube> movableCubes)
        {
            float gravity = -18;

            if (Position.Y > GroundLevel)
            {
                Velocity.Y += gravity * deltaTime;
            }
            else
            {
                Velocity.Y = 0f;
                Position.Y = GroundLevel;
            }

            if (Velocity.Y < gravity)
            {
                Velocity.Y = gravity;
            }

            HandlePush(player, movableCubes); // Appliquer la poussée du joueur

            Brake(); // Appliquer le freinage pour éviter les mouvements infinis

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

        public void HandleCollision(List<Obstacle> obstacles, bool isTopView)
        {
            foreach (Obstacle obstacle in obstacles)
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
                        Velocity.Y = 0;
                    }
                }
                if (isTopView)
                {
                    if (CheckCollisionBoxes(Box, obstacle.Box))
                    {
                        Position.Y = obstacle.Position.Y + obstacle.Dimensions.Y / 2;
                        break;
                    }
                    else
                    {
                        Position.Y = GroundLevel;
                    }
                }
            }

        }
        public void HandlePlate(List<PressurePlate> plaques)
        {
            foreach (PressurePlate plaque in plaques)
            {
                if (CheckCollisionBoxes(Box, plaque.Box))
                {
                    float compressionAmount = 0.1f;

                    if (plaque.IsPressed)
                    {
                        plaque.Box.Max.Y = plaque.OriginalMaxY - compressionAmount;
                    }
                    else
                    {
                        plaque.Box.Max.Y = plaque.OriginalMaxY;
                    }
                
                    if (CheckCollisionBoxes(Box, plaque.PressBox))
                    {
                        Position.Y = plaque.Box.Max.Y + Dimensions.Y / 2;
                    }

                    Position.Y = plaque.Box.Max.Y + Dimensions.Y / 2;
                    Velocity.Y = 0;
                    break;
                }
            }
        }
        public void HandlePush(Player player, List<MovableCube> movableCubes)
        {
            if (CheckCollisionBoxes(Box, player.Box))
            {
                Vector3 pushDirection = new Vector3(player.Velocity.X, 0, player.Velocity.Z);

                if (pushDirection != Vector3.Zero)
                {
                    pushDirection = Raymath.Vector3Normalize(pushDirection);
                    Velocity += pushDirection * 2.0f;
                }
            }
            foreach (MovableCube otherCube in movableCubes) 
            {
                if (otherCube != this && CheckCollisionBoxes(Box, otherCube.Box))
                {
                    // Calculer le centre des deux blocs en utilisant les coordonnées min et max du BoundingBox
                    Vector3 thisBlockCenter = new Vector3((Box.Min.X + Box.Max.X) / 2, (Box.Min.Y + Box.Max.Y) / 2, (Box.Min.Z + Box.Max.Z) / 2);
                    Vector3 otherBlockCenter = new Vector3((otherCube.Box.Min.X + otherCube.Box.Max.X) / 2,
                                                           (otherCube.Box.Min.Y + otherCube.Box.Max.Y) / 2,
                                                           (otherCube.Box.Min.Z + otherCube.Box.Max.Z) / 2);
                    Vector3 direction = thisBlockCenter - otherBlockCenter;

                    if (direction != Vector3.Zero)
                    {
                        direction = Raymath.Vector3Normalize(direction);

                        // Appliquer une poussée sur le bloc en fonction de la direction
                        Velocity += direction * 2.0f;
                        otherCube.Velocity -= direction * 2.0f;
                    }
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
        public unsafe void SetModelScale(Model model, Vector3 scale)
        {
            // Applique la mise à l'échelle sur chaque sommet du modèle
            for (int i = 0; i < model.MeshCount; i++)
            {
                Mesh mesh = model.Meshes[i];
                for (int j = 0; j < mesh.VertexCount; j++)
                {
                    // Calculer la nouvelle position de chaque sommet
                    float x = mesh.Vertices[j * 3] * scale.X;
                    float y = mesh.Vertices[j * 3 + 1] * scale.Y;
                    float z = mesh.Vertices[j * 3 + 2] * scale.Z;

                    // Appliquer la nouvelle position au sommet
                    mesh.Vertices[j * 3] = x;
                    mesh.Vertices[j * 3 + 1] = y;
                    mesh.Vertices[j * 3 + 2] = z;
                }
            }
        }

    }
}
