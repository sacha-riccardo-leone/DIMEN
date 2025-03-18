using Raylib_cs;
using static Raylib_cs.Raylib;
using System.Numerics;
using static System.Formats.Asn1.AsnWriter;

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
        public Vector3 Center;
        public Vector3 HalfSize;
        public Vector3 DoorPosition;
        public Vector3 DoorDimensions;
        public Model OpenDoorModel;
        public Model ClosedDoorModel;
        public BoundingBox DoorBox;
        public float GroundLevel;
        public bool IsDoorOpen;
        public bool DoorExtended;
        public readonly Vector3 Scale;

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
            DoorExtended = false;

            // Ajustement de la hauteur pour que l'obstacle repose bien au sol
            Position = new Vector3(position.X, GroundLevel + Dimensions.Y / 2, position.Z);

            Center = Position;
            HalfSize = Dimensions / 2;

            Box = new BoundingBox(Position - HalfSize, Position + HalfSize);

            // Ajustement de la hauteur de la porte pour qu'elle soit au sol
            DoorPosition = new Vector3(
                Center.X,  // Aligné avec l'obstacle
                GroundLevel + DoorDimensions.Y / 2, // La base de la porte touche le sol
                Center.Z - HalfSize.Z // Placée sur la face arrière de l'obstacle
            );


            DoorBox = new BoundingBox(DoorPosition - DoorDimensions / 2, DoorPosition + DoorDimensions / 2);

            // Calcul de la taille du modèle
            BoundingBox tempBox = GetModelBoundingBox(Model);
            Vector3 modelsize = tempBox.Max - tempBox.Min;

            // Calcul du facteur d'échelle en fonction de la bounding box
            Vector3 boxSize = Box.Max - Box.Min;
            Vector3 scale = boxSize / modelsize;

            // Appliquer l'échelle au modèle
            Scale = scale;

            // Appliquer la mise à l'échelle sur le modèle
            SetModelScale(Model, scale);
            SetModelScale(OpenDoorModel, scale);
            SetModelScale(ClosedDoorModel, scale);
        }

        public Obstacle(string obstacleMaterialPath,string obstacleModelPath, Vector3 dimensions, Vector3 position,   float groundLevel)
        {
            Material = new PBRMaterial(obstacleMaterialPath);
            Model = LoadModel(obstacleModelPath);


            InitModels(Model, Material);
            InitModels(OpenDoorModel, DoorMaterial);
            InitModels(ClosedDoorModel, DoorMaterial);

            Dimensions = dimensions;
            GroundLevel = groundLevel;
            IsDoorOpen = false;

            // Ajustement de la hauteur pour que l'obstacle repose bien au sol
            Position = new Vector3(position.X, GroundLevel + Dimensions.Y / 2, position.Z);

            Center = Position;
            HalfSize = Dimensions / 2;

            Box = new BoundingBox(Position - HalfSize, Position + HalfSize);


            // Calcul de la taille du modèle
            BoundingBox tempBox = GetModelBoundingBox(Model);
            Vector3 modelsize = tempBox.Max - tempBox.Min;

            // Calcul du facteur d'échelle en fonction de la bounding box
            Vector3 boxSize = Box.Max - Box.Min;
            Vector3 scale = boxSize / modelsize;

            // Appliquer l'échelle au modèle
            Scale = scale;

            // Appliquer la mise à l'échelle sur le modèle
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

        // Nouvelle fonction pour appliquer l'échelle au modèle
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
