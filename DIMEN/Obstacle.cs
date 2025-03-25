using Raylib_cs;
using static Raylib_cs.Raylib;
using System.Numerics;

namespace DIMEN
{
    internal class Obstacle
    {
        public PBRMaterial Material;
        public PBRMaterial DoorMaterial;
        public Model Model;
        public Vector3 Dimensions;
        public Vector3 Position;
        public BoundingBox Box;
        public BoundingBox OriginalBox;
        public Vector3 Center;
        public Vector3 HalfSize;
        public Vector3 DoorPosition;
        public Vector3 DoorDimensions;
        public Model OpenDoorModel;
        public Model ClosedDoorModel;
        public BoundingBox DoorBox;
        public float GroundLevel;
        public bool IsFloating;
        public bool IsDoorOpen;
        public bool DoorExtended;
        public readonly Vector3 Scale;

        // Obstacle avec porte
        public Obstacle(string materialPath, string doorMaterialPath, string modelPath, Vector3 dimensions, Vector3 position, Vector3 doorDimensions, string openDoorModelPath, string closedDoorModelPath, float groundLevel)
        {
            Material = new PBRMaterial(materialPath);
            DoorMaterial = new PBRMaterial(doorMaterialPath);
            Model = LoadModel(modelPath);
            OpenDoorModel = LoadModel(openDoorModelPath);
            ClosedDoorModel = LoadModel(closedDoorModelPath);

            InitModels(Model, Material);
            InitModels(OpenDoorModel, DoorMaterial);
            InitModels(ClosedDoorModel, DoorMaterial);

            Dimensions = dimensions;
            DoorDimensions = doorDimensions;
            GroundLevel = groundLevel;
            IsDoorOpen = false;
            IsFloating = false;
            DoorExtended = false;

            // Ajustement de la hauteur de l'obstacle
            Position = new Vector3(position.X, GroundLevel + Dimensions.Y / 2, position.Z);
            Center = Position;
            HalfSize = Dimensions / 2;
            OriginalBox = new BoundingBox(Position - HalfSize, Position + HalfSize);
            Box = OriginalBox;

            // Ajustement de la hauteur de la porte
            DoorPosition = new Vector3(
                Center.X,
                GroundLevel + DoorDimensions.Y / 2,
                Center.Z - HalfSize.Z
            );
            DoorBox = new BoundingBox(DoorPosition - DoorDimensions / 2, DoorPosition + DoorDimensions / 2);

            // Calcul de la taille du modèle
            BoundingBox tempBox = GetModelBoundingBox(Model);
            Vector3 modelsize = tempBox.Max - tempBox.Min;
            // Calcul du facteur d'échelle
            Vector3 boxSize = Box.Max - Box.Min;
            Vector3 scale = boxSize / modelsize;

            Scale = scale;
            SetModelScale(Model, scale);
            SetModelScale(OpenDoorModel, scale);
            SetModelScale(ClosedDoorModel, scale);
        }
        // Obstacle sans porte
        public Obstacle(string obstacleMaterialPath, string obstacleModelPath, Vector3 dimensions, Vector3 position, float groundLevel, bool isFloating)
        {
            Material = new PBRMaterial(obstacleMaterialPath);
            Model = LoadModel(obstacleModelPath);

            InitModels(Model, Material);
            InitModels(OpenDoorModel, DoorMaterial);
            InitModels(ClosedDoorModel, DoorMaterial);

            Dimensions = dimensions;
            GroundLevel = groundLevel;
            IsDoorOpen = false;
            IsFloating = isFloating;

            if (isFloating)
            {
                // Ajustement de la hauteur de l'obstacle
                Position = position;
            }
            else
            {
                // Ajustement de la hauteur de l'obstacle
                Position = new Vector3(position.X, GroundLevel + Dimensions.Y / 2, position.Z);
            }
            Center = Position;
            HalfSize = Dimensions / 2;
            OriginalBox = new BoundingBox(Position - HalfSize, Position + HalfSize);
            Box = OriginalBox;

            // Calcul de la taille du modèle
            BoundingBox tempBox = GetModelBoundingBox(Model);
            Vector3 modelsize = tempBox.Max - tempBox.Min;
            // Calcul du facteur d'échelle
            Vector3 boxSize = Box.Max - Box.Min;
            Vector3 scale = boxSize / modelsize;

            Scale = scale;
            SetModelScale(Model, scale);
        }

        public void ToggleDoor()
        {
            IsDoorOpen = !IsDoorOpen;
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
