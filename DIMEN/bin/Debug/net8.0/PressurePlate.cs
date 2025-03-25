using Raylib_cs;
using static Raylib_cs.Raylib;
using System.Numerics;

namespace DIMEN
{
    internal class PressurePlate
    {
        public PBRMaterial Material;
        public Model Model;
        public Model PressedModel;
        public Vector3 Dimensions;
        public Vector3 Position;
        public BoundingBox Box;
        public BoundingBox PressBox;
        public Vector3 HalfSize;
        public bool IsPressed;
        public float OriginalMaxY;
        public readonly Vector3 Scale;
        public MovableCube PressableBy;
        public PressurePlate(string pressedModelPath, string unpressedModelPath, string materialPath, Vector3 dimensions, Vector3 position, MovableCube pressableBy)
        {
            PressableBy = pressableBy;

            Model = LoadModel(unpressedModelPath);
            PressedModel = LoadModel(pressedModelPath);
            Material = new PBRMaterial(materialPath);
            InitModels(Model, Material);
            InitModels(PressedModel, Material);

            Dimensions = dimensions;
            Position = new Vector3(position.X, position.Y, position.Z);
            HalfSize = Dimensions / 2;

            BoundingBox rawModelBox = GetModelBoundingBox(Model);
            Vector3 rawModelSize = rawModelBox.Max - rawModelBox.Min;
            Vector3 boxSize = Dimensions;

            Scale = boxSize / rawModelSize;
            SetModelScale(Model, Scale);
            SetModelScale(PressedModel, Scale);

            Vector3 scaledMin = rawModelBox.Min * Scale;
            Vector3 scaledMax = rawModelBox.Max * Scale;

            Box = new BoundingBox(Position + scaledMin, Position + scaledMax);
            OriginalMaxY = Box.Max.Y;
            Vector3 pressOffset = new Vector3(0.2f, 0.0025f, 0.2f);
            PressBox = new BoundingBox(Box.Min + pressOffset, Box.Max - pressOffset);
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
