using Raylib_cs;
using static Raylib_cs.Raylib;
using System.Numerics;


namespace DIMEN
{
    internal class Obstacle
    {
        public PBRMaterial Material { get; private set; }
        public PBRMaterial DoorMaterial { get; private set; }
        public Model Model { get; private set; }
        public Vector3 Dimensions { get; private set; }
        public Vector3 Position { get; private set; }
        public BoundingBox Box { get; private set; }
        public Vector3 Center { get; private set; }
        public Vector3 HalfSize { get; private set; }
        public Vector3 DoorPosition { get; private set; }
        public Vector3 DoorDimensions { get; private set; }
        public Model OpenDoorModel { get; private set; }
        public Model ClosedDoorModel { get; private set; }
        public BoundingBox DoorBox { get; private set; }
        public float GroundLevel { get; private set; }
        public bool IsDoorOpen;

        public Obstacle(string obstacleMaterialPath, string doorMaterialPath, string obstacleModelPath, Vector3 dimensions, Vector3 position, Vector3 doorDimensions, string openDoorModelPath, string closedDoorModelPath, float groundLevel)
        {
            Material = new PBRMaterial (obstacleMaterialPath);
            DoorMaterial = new PBRMaterial (doorMaterialPath);
            Model = LoadModel(obstacleModelPath);
            OpenDoorModel = LoadModel(openDoorModelPath);
            ClosedDoorModel = LoadModel(closedDoorModelPath);

            InitModels(Model, Material);
            InitModels(OpenDoorModel, DoorMaterial);
            InitModels(ClosedDoorModel, DoorMaterial);

            Dimensions = dimensions;
            Position = position;
            DoorDimensions = doorDimensions;
            GroundLevel = groundLevel;
            IsDoorOpen = false;

            Center = Position;
            HalfSize = Dimensions / 2;

            Box = new BoundingBox(Position - HalfSize, Position + HalfSize);
            DoorPosition = Center + new Vector3(0, GroundLevel + DoorDimensions.Y / 2, HalfSize.Z);
            DoorBox = new BoundingBox(DoorPosition - DoorDimensions / 2, DoorPosition + DoorDimensions / 2);
        }


        public Obstacle(string obstacleMaterialPath, string obstacleModelPath, Vector3 dimensions, Vector3 position, float groundLevel)
        {
            Material = new PBRMaterial(obstacleMaterialPath);
            Model = LoadModel(obstacleModelPath);
            Dimensions = dimensions;
            Position = position;
            GroundLevel = groundLevel;

            Center = Position;
            HalfSize = Dimensions / 2;

            Box = new BoundingBox(
                Position - HalfSize,
                Position + HalfSize
            );
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
    }
}
