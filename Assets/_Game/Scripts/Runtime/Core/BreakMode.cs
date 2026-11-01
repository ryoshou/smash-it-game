namespace Game.Core
{
    /// <summary>What a prop does when it hits the floor. Assets store this as a number: append, never reorder.</summary>
    public enum BreakMode
    {
        /// <summary>Stays whole (fixed or decorative props).</summary>
        None,

        /// <summary>A puff, then the prop itself shrinks away (keycaps, books).</summary>
        Pop,

        /// <summary>Swaps to its pre-fractured pieces, which fade away (mugs, pots).</summary>
        Shatter,
    }
}
