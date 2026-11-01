using System.Collections.Generic;
using UnityEngine;

namespace Game.Editor
{
    /// <summary>What the art validator measures on one imported model: all its meshes summed.</summary>
    public readonly struct ArtMeshStats
    {
        public readonly int triangles;
        public readonly int materialCount;
        public readonly Vector2[] uvs;
        public readonly int[] triangleIndices;
        public readonly Vector3 boundsSize;

        public ArtMeshStats(int triangles, int materialCount, Vector2[] uvs, int[] triangleIndices, Vector3 boundsSize)
        {
            this.triangles = triangles;
            this.materialCount = materialCount;
            this.uvs = uvs;
            this.triangleIndices = triangleIndices;
            this.boundsSize = boundsSize;
        }

        /// <summary>Sums every MeshFilter and SkinnedMeshRenderer under the model and counts distinct materials.</summary>
        public static ArtMeshStats FromModel(GameObject modelRoot)
        {
            var uvs = new List<Vector2>();
            var indices = new List<int>();
            var materials = new HashSet<Material>();
            int triangles = 0;

            foreach (var renderer in modelRoot.GetComponentsInChildren<Renderer>(true))
            {
                var mesh = MeshOf(renderer);
                if (mesh == null) continue;
                foreach (var material in renderer.sharedMaterials)
                    if (material != null) materials.Add(material);
                int offset = uvs.Count;
                uvs.AddRange(mesh.uv);
                foreach (int index in mesh.triangles) indices.Add(index + offset);
                triangles += mesh.triangles.Length / 3;
            }

            // Imported models carry no materials (materialImportMode None): count material slots instead.
            int materialCount = materials.Count > 0 ? materials.Count : MaxSubMeshes(modelRoot);
            return new ArtMeshStats(triangles, materialCount, uvs.ToArray(), indices.ToArray(), ModelBounds(modelRoot).size);
        }

        // Mesh bounds in the model's own space: renderer.bounds is empty for a model asset that is not in a scene.
        private static IEnumerable<Vector3> Corners(Bounds b)
        {
            for (int i = 0; i < 8; i++)
                yield return b.center + Vector3.Scale(b.extents,
                    new Vector3((i & 1) == 0 ? -1f : 1f, (i & 2) == 0 ? -1f : 1f, (i & 4) == 0 ? -1f : 1f));
        }

        /// <summary>Bounds of every mesh under the model, in the model root's own space.</summary>
        public static Bounds ModelBounds(GameObject modelRoot)
        {
            var bounds = new Bounds();
            bool hasBounds = false;
            foreach (var renderer in modelRoot.GetComponentsInChildren<Renderer>(true))
            {
                var mesh = MeshOf(renderer);
                if (mesh == null) continue;
                var toRoot = modelRoot.transform.worldToLocalMatrix * renderer.transform.localToWorldMatrix;
                foreach (var corner in Corners(mesh.bounds))
                {
                    var point = toRoot.MultiplyPoint3x4(corner);
                    if (hasBounds) bounds.Encapsulate(point);
                    else bounds = new Bounds(point, Vector3.zero);
                    hasBounds = true;
                }
            }
            return bounds;
        }

        private static Mesh MeshOf(Renderer renderer)
        {
            if (renderer is SkinnedMeshRenderer skinned) return skinned.sharedMesh;
            return renderer.TryGetComponent(out MeshFilter filter) ? filter.sharedMesh : null;
        }

        private static int MaxSubMeshes(GameObject modelRoot)
        {
            int max = 0;
            foreach (var renderer in modelRoot.GetComponentsInChildren<Renderer>(true))
            {
                var mesh = MeshOf(renderer);
                if (mesh != null) max = Mathf.Max(max, mesh.subMeshCount);
            }
            return max;
        }
    }
}
