using System.Collections.Generic;
using Game.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Game.Tests
{
    public sealed class ArtAssetsTests
    {
        [Test]
        public void AllModels_PassArtCheck()
        {
            var failures = new List<string>();
            foreach (string guid in AssetDatabase.FindAssets("t:Model", new[] { "Assets/_Game/Art" }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var errors = new List<string>();
                var model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (!ArtCheck.Check(ArtMeshStats.FromModel(model), AssetBudget.For(path), errors))
                    failures.Add(path + ": " + string.Join("; ", errors));
            }

            Assert.IsEmpty(failures, string.Join("\n", failures));
        }
    }
}
