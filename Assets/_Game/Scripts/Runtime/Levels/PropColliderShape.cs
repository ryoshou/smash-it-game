namespace Game.Levels
{
    /// <summary>The collider a prop prefab gets. Assets store this as a number: append, never reorder.</summary>
    public enum PropColliderShape
    {
        /// <summary>A box of the spec's size.</summary>
        Box,

        /// <summary>A convex hull of the visual mesh (round or handled props).</summary>
        ConvexMesh,
    }
}
