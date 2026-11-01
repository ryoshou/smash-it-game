using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
    /// <summary>
    /// Fixed import settings for every model under Art/, then the art validator. Materials are not imported:
    /// prefab builders assign the shared atlas material, so a prop always has exactly one.
    /// </summary>
    public sealed class ArtPostprocessor : AssetPostprocessor
    {
        public const string ArtRoot = "Assets/_Game/Art/";

        private void OnPreprocessModel()
        {
            if (!assetPath.StartsWith(ArtRoot)) return;
            var importer = (ModelImporter)assetImporter;
            importer.materialImportMode = ModelImporterMaterialImportMode.None;
            importer.globalScale = 1f;
            importer.useFileScale = true;
            importer.isReadable = false;
            bool isCat = assetPath.Contains("/Generated/Cat");
            importer.importAnimation = isCat;
            importer.animationType = isCat ? ModelImporterAnimationType.Generic : ModelImporterAnimationType.None;
        }

        private void OnPostprocessModel(GameObject root)
        {
            if (!assetPath.StartsWith(ArtRoot)) return;
            var errors = new List<string>();
            if (ArtCheck.Check(ArtMeshStats.FromModel(root), AssetBudget.For(assetPath), errors)) return;
            foreach (string error in errors) Debug.LogError($"Art check failed for {assetPath}: {error}");
        }
    }
}
