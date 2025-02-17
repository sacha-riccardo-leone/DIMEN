using Raylib_cs;
using static Raylib_cs.Raylib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DIMEN
{
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>Represents an instance of a PBR material.</summary>
    public unsafe struct PBRMaterial
    {
        public Material Material;
        public Dictionary<MaterialMapIndex, Texture2D> Maps;

        /// <summary>Sets one of the material's maps.</summary>
        /// <param name="map">Map type to use.</param>
        /// <param name="texture">Texture to use.</param>
        public void SetMap(MaterialMapIndex map, Texture2D texture)
        {
            SetMaterialTexture(ref Material, map, texture);
            UnloadTexture(Maps[map]); // Unload unused map
            Maps[map] = texture; // Set new texture
        }

        /// <summary>Creates an instance of PBR material.</summary>
        /// <param name="mapFolder"></param>
        public PBRMaterial(string mapFolder)
        {
            Material = LoadMaterialDefault();
            Material.Shader = Shaders.PBRLightingShader;
            Maps = new Dictionary<MaterialMapIndex, Texture2D>();
            string[] mapsPaths = Directory.GetFiles(mapFolder);
            for (int i = 0; i < mapsPaths.Length; i++)
            {
                string mapType = mapsPaths[i].Split('_').Last().Split('.')[0];
                if (Enum.IsDefined(typeof(MaterialMapIndex), mapType))
                {
                    MaterialMapIndex _type = (MaterialMapIndex)Enum.Parse(typeof(MaterialMapIndex), mapType);
                    Maps.Add(_type, LoadTexture(mapsPaths[i]));
                    SetMaterialTexture(ref Material, _type, Maps[_type]);
                }
            }

            Material.Maps[(int)MaterialMapIndex.Albedo].Color = Color.White;
            Material.Maps[(int)MaterialMapIndex.Metalness].Value = 0;
            Material.Maps[(int)MaterialMapIndex.Roughness].Value = 0;
            Material.Maps[(int)MaterialMapIndex.Occlusion].Value = 1;
            Material.Maps[(int)MaterialMapIndex.Emission].Color = Color.White;


            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"ORION: PBR Material loaded successfully");
            Console.ForegroundColor = ConsoleColor.White;
        }
    }

    /// <summary>Defines the type of a light source.</summary>
    public enum PbrLightType
    {
        Directorional,
        Point,
        Spot
    }

    /// <summary>Represents an instance of light point in 3D world.</summary>
    public struct PbrLight
    {
        public PbrLightType Type;
        public bool Enabled;
        public Vector3 Position;
        public Vector3 Target;
        public Vector4 Color;
        public float Intensity;

        // Shader light parameters locations
        public int TypeLoc;
        public int EnabledLoc;
        public int PositionLoc;
        public int TargetLoc;
        public int ColorLoc;
        public int IntensityLoc;
    }

    /// <summary>Represents an instance of the static lights management class.</summary>
    public class PbrLights
    {
        public static PbrLight CreateLight(
            int lightsCount,
            PbrLightType type,
            Vector3 pos,
            Vector3 target,
            Color color,
            float intensity,
            Shader shader
        )
        {
            PbrLight light = new();

            light.Enabled = true;
            light.Type = type;
            light.Position = pos;
            light.Target = target;
            light.Color = new Vector4(
                color.R / 255.0f,
                color.G / 255.0f,
                color.B / 255.0f,
                color.A / 255.0f
            );
            light.Intensity = intensity;

            string enabledName = "lights[" + lightsCount + "].enabled";
            string typeName = "lights[" + lightsCount + "].type";
            string posName = "lights[" + lightsCount + "].position";
            string targetName = "lights[" + lightsCount + "].target";
            string colorName = "lights[" + lightsCount + "].color";
            string intensityName = "lights[" + lightsCount + "].intensity";

            light.EnabledLoc = GetShaderLocation(shader, enabledName);
            light.TypeLoc = GetShaderLocation(shader, typeName);
            light.PositionLoc = GetShaderLocation(shader, posName);
            light.TargetLoc = GetShaderLocation(shader, targetName);
            light.ColorLoc = GetShaderLocation(shader, colorName);
            light.IntensityLoc = GetShaderLocation(shader, intensityName);

            UpdateLightValues(shader, light);

            return light;
        }

        public static void UpdateLightValues(Shader shader, PbrLight light)
        {
            // Send to shader light enabled state and type
            SetShaderValue(
                shader,
                light.EnabledLoc,
                light.Enabled ? 1 : 0,
                ShaderUniformDataType.Int
            );
            SetShaderValue(shader, light.TypeLoc, (int)light.Type, ShaderUniformDataType.Int);

            // Send to shader light target position values
            SetShaderValue(shader, light.PositionLoc, light.Position, ShaderUniformDataType.Vec3);

            // Send to shader light target position values
            SetShaderValue(shader, light.TargetLoc, light.Target, ShaderUniformDataType.Vec3);

            // Send to shader light color values
            SetShaderValue(shader, light.ColorLoc, light.Color, ShaderUniformDataType.Vec4);

            // Send to shader light intensity values
            SetShaderValue(shader, light.IntensityLoc, light.Intensity, ShaderUniformDataType.Float);
        }
    }
    public static class Shaders
    {
        public static Shader PBRLightingShader;
        public static PbrLight Light1;
        public static PbrLight Light2;
        public static PbrLight Light3;
        public static PbrLight Light4;
        public static int EmissivePowerLoc;
        public static int EmissiveColorLoc;
        public static int TextureTilingLoc;
        public static unsafe void Init()
        {
            // PBR lighting shader
            PBRLightingShader = LoadShader("assets/shaders/lighting.vs", "assets/shaders/lighting.fs");

            // Modify PBR shader uniform locations
            PBRLightingShader.Locs[(int)ShaderLocationIndex.MapAlbedo] = GetShaderLocation(PBRLightingShader, "albedoMap");
            PBRLightingShader.Locs[(int)ShaderLocationIndex.MapMetalness] = GetShaderLocation(PBRLightingShader, "mraMap");
            PBRLightingShader.Locs[(int)ShaderLocationIndex.MapNormal] = GetShaderLocation(PBRLightingShader, "normalMap");
            PBRLightingShader.Locs[(int)ShaderLocationIndex.MapEmission] = GetShaderLocation(PBRLightingShader, "emissiveMap");
            PBRLightingShader.Locs[(int)ShaderLocationIndex.ColorDiffuse] = GetShaderLocation(PBRLightingShader, "albedoColor");

            // Set PBR shader uniform locations
            PBRLightingShader.Locs[(int)ShaderLocationIndex.VectorView] = GetShaderLocation(PBRLightingShader, "viewPos");
            int lightCountLoc = GetShaderLocation(PBRLightingShader, "numOfLights");
            int maxLightCount = 4;
            SetShaderValue(PBRLightingShader, lightCountLoc, &maxLightCount, ShaderUniformDataType.Int);

            // Setup ambient color and intensity parameters
            float ambientIntensity = 0.01f;
            Color ambientColor = new Color(255,255,255, 100);
            //Color ambientColor = new Color(75, 75, 75, 255);
            Vector3 ambientColorNormalized = new Vector3(ambientColor.R / 255f, ambientColor.G / 255f, ambientColor.B / 255f);
            SetShaderValue(PBRLightingShader, GetShaderLocation(PBRLightingShader, "ambientColor"), &ambientColorNormalized, ShaderUniformDataType.Vec3);
            SetShaderValue(PBRLightingShader, GetShaderLocation(PBRLightingShader, "ambient"), &ambientIntensity, ShaderUniformDataType.Float);

            // Get real-time modification elligible uniforms
            EmissivePowerLoc = GetShaderLocation(PBRLightingShader, "emissivePower");
            EmissiveColorLoc = GetShaderLocation(PBRLightingShader, "emissiveColor");
            TextureTilingLoc = GetShaderLocation(PBRLightingShader, "tiling");
            // Create main light source
            Light1 = PbrLights.CreateLight(0, PbrLightType.Point, new Vector3(30, 10, 0) , Vector3.Zero, new Color(255, 255, 255, 255), 250, PBRLightingShader);
            Light2 = PbrLights.CreateLight(1, PbrLightType.Point, new Vector3(-30, 10, 0) , Vector3.Zero, new Color(255, 255, 255, 255), 250, PBRLightingShader);
            Light3 = PbrLights.CreateLight(2, PbrLightType.Point, new Vector3(0, 10, 30), Vector3.Zero, new Color(255, 255, 255, 255), 250, PBRLightingShader);
            Light4 = PbrLights.CreateLight(3, PbrLightType.Point, new Vector3(0, 10, -30), Vector3.Zero, new Color(255, 255, 255, 255), 250, PBRLightingShader);

            // Set PBR shader used maps
            int usage = 1;
            SetShaderValue(PBRLightingShader, GetShaderLocation(PBRLightingShader, "useTexAlbedo"), &usage, ShaderUniformDataType.Int);
            SetShaderValue(PBRLightingShader, GetShaderLocation(PBRLightingShader, "useTexNormal"), &usage, ShaderUniformDataType.Int);
            SetShaderValue(PBRLightingShader, GetShaderLocation(PBRLightingShader, "useTexMRA"), &usage, ShaderUniformDataType.Int);
            SetShaderValue(PBRLightingShader, GetShaderLocation(PBRLightingShader, "useTexEmissive"), &usage, ShaderUniformDataType.Int);

        }
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>Updates the environement's PBR lighting.</summary>
        /// <param name="viewPos">Camera position of the currently rendered scene.</param>
        public static unsafe void UpdatePBRLighting(Vector3 viewPos)
        {
            SetShaderValue(PBRLightingShader, PBRLightingShader.Locs[(int)ShaderLocationIndex.VectorView], viewPos, ShaderUniformDataType.Vec3);
            //UpdateLight(PBRLightingShader, GlobalLight);
            // Update tiling values
            SetShaderValue(PBRLightingShader, TextureTilingLoc, Vector2.One / 2, ShaderUniformDataType.Vec2);
            // Update intensity
            SetShaderValue(PBRLightingShader, EmissivePowerLoc, 0.5f, ShaderUniformDataType.Float);
            SetShaderValue(PBRLightingShader, EmissiveColorLoc, Vector4.One, ShaderUniformDataType.Vec4);
        }

        /// <summary>Updates a single PBR light source.</summary>
        /// <param name="shader">PBR shader to update to.</param>
        /// <param name="light">PBR light to use.</param>
        private static void UpdateLight(Shader shader, PbrLight light)
        {
            SetShaderValue(shader, light.EnabledLoc, light.Enabled, ShaderUniformDataType.Int);
            SetShaderValue(shader, light.TypeLoc, light.Type, ShaderUniformDataType.Int);

            // Send to shader light position values
            SetShaderValue(shader, light.PositionLoc, light.Position, ShaderUniformDataType.Vec3);

            // Send to shader light target position values
            SetShaderValue(shader, light.TargetLoc, light.Target, ShaderUniformDataType.Vec3);
            SetShaderValue(shader, light.ColorLoc, light.Color, ShaderUniformDataType.Vec4);
            SetShaderValue(shader, light.IntensityLoc, light.Intensity, ShaderUniformDataType.Float);
        }

    }
}
