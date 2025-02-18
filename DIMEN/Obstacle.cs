using Raylib_cs;
using static Raylib_cs.Raylib;
using System.Numerics;


namespace DIMEN
{
    internal class Obstacle
    {
        public PBRMaterial ObstacleMaterial { get; private set; }
        public PBRMaterial DoorMaterial { get; private set; }
        public Model ObstacleModel { get; private set; }
        public Vector3 ObstacleDimensions { get; private set; }
        public Vector3 ObstaclePosition { get; private set; }
        public BoundingBox ObstacleBox { get; private set; }
        public Vector3 Center { get; private set; }
        public Vector3 HalfSize { get; private set; }
        public Vector3 DoorPosition { get; private set; }
        public Vector3 DoorDimensions { get; private set; }
        public Model OpenDoorModel { get; private set; }
        public Model ClosedDoorModel { get; private set; }
        public BoundingBox DoorBox { get; private set; }
        public float GroundLevel { get; private set; }
        public bool IsDoorOpen { get; private set; }

        public Obstacle(string obstacleMaterialPath, string doorMaterialPath, string obstacleModelPath, Vector3 dimensions, Vector3 position, Vector3 doorDimensions, string openDoorModelPath, string closedDoorModelPath, float groundLevel)
        {
            ObstacleMaterial = new PBRMaterial (obstacleMaterialPath);
            DoorMaterial = new PBRMaterial (doorMaterialPath);
            ObstacleModel = LoadModel(obstacleModelPath);
            OpenDoorModel = LoadModel(openDoorModelPath);
            ClosedDoorModel = LoadModel(closedDoorModelPath);

            InitModels(ObstacleModel, ObstacleMaterial);
            InitModels(OpenDoorModel, DoorMaterial);
            InitModels(ClosedDoorModel, DoorMaterial);

            ObstacleDimensions = dimensions;
            ObstaclePosition = position;
            DoorDimensions = doorDimensions;
            GroundLevel = groundLevel;
            IsDoorOpen = false;

            Center = ObstaclePosition;
            HalfSize = ObstacleDimensions / 2;

            ObstacleBox = new BoundingBox(ObstaclePosition - HalfSize, ObstaclePosition + HalfSize);
            DoorPosition = Center + new Vector3(0, GroundLevel + DoorDimensions.Y / 2, HalfSize.Z);
            DoorBox = new BoundingBox(DoorPosition - DoorDimensions / 2, DoorPosition + DoorDimensions / 2);
        }


        public Obstacle(PBRMaterial material, Model model, Vector3 dimensions, Vector3 position, float groundLevel)
        {
            ObstacleMaterial = material;
            ObstacleModel = model;
            ObstacleDimensions = dimensions;
            ObstaclePosition = position;
            GroundLevel = groundLevel;

            Center = ObstaclePosition;
            HalfSize = ObstacleDimensions / 2;

            ObstacleBox = new BoundingBox(
                ObstaclePosition - HalfSize,
                ObstaclePosition + HalfSize
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
