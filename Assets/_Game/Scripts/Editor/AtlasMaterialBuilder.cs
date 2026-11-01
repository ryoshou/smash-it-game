using System.IO;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
    /// <summary>One shared URP Lit material per pack atlas (spec §12.2: metallic 0, roughness 0.5, one material per prop).</summary>
    public static class AtlasMaterialBuilder
    {
        public const string PacksRoot = "Assets/_Game/Art/Packs";
        public const string MaterialsRoot = "Assets/_Game/Art/Materials";
        private const float Smoothness = 0.5f;

        [MenuItem("Smash/Build Atlas Materials")]
        public static void BuildAll()
        {
            Directory.CreateDirectory(MaterialsRoot);
            foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { PacksRoot }))
            {
                string texturePath = AssetDatabase.GUIDToAssetPath(guid);
                string pack = Path.GetFileName(Path.GetDirectoryName(texturePath));
                Build(pack, AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath));
            }
            AssetDatabase.SaveAssets();
        }

        /// <summary>The shared material for a pack folder name, e.g. "TinyTreats".</summary>
        public static Material Load(string pack) =>
            AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsRoot}/Atlas_{pack}.mat");

        private static void Build(string pack, Texture2D atlas)
        {
            string path = $"{MaterialsRoot}/Atlas_{pack}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(material, path);
            }
            material.SetTexture("_BaseMap", atlas);
            material.SetColor("_BaseColor", Color.white);
            material.SetFloat("_Metallic", 0f);
            material.SetFloat("_Smoothness", Smoothness);
            EditorUtility.SetDirty(material);
        }
    }
}
