using Raylib_cs;
using static Raylib_cs.Raylib;
using System.Numerics;
using static System.Formats.Asn1.AsnWriter;

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
            // Qui peut presser la plaque
            PressableBy = pressableBy;

            // Charger les modèles
            Model = LoadModel(unpressedModelPath);
            PressedModel = LoadModel(pressedModelPath);

            // Charger le matériau et l'appliquer aux modèles
            Material = new PBRMaterial(materialPath);
            InitModels(Model, Material);
            InitModels(PressedModel, Material);

            // Définir les dimensions et la position initiale
            Dimensions = dimensions;
            Position = new Vector3(position.X, position.Y, position.Z);

            HalfSize = Dimensions / 2;

            // Calcul de la bounding box du modèle avant l'échelle
            BoundingBox rawModelBox = GetModelBoundingBox(Model);
            Vector3 rawModelSize = rawModelBox.Max - rawModelBox.Min;

            // Calcul du facteur d'échelle pour adapter le modèle à la bounding box
            Vector3 boxSize = Dimensions; // Dimensions déjà définies comme taille cible
            Scale = boxSize / rawModelSize;

            // Appliquer la mise à l'échelle au modèle
            SetModelScale(Model, Scale);
            SetModelScale(PressedModel, Scale);

            // Recalculer la BoundingBox après mise à l'échelle
            Vector3 scaledMin = rawModelBox.Min * Scale;
            Vector3 scaledMax = rawModelBox.Max * Scale;
            Box = new BoundingBox(Position + scaledMin, Position + scaledMax);

            OriginalMaxY = Box.Max.Y;

            // Ajustement de la bounding box de pression (plus petite)
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
