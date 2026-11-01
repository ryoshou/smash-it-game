namespace Game
{
    /// <summary>
    /// Lets the level editor's Preview start Play mode on a chosen level. Editor only; players always start at the
    /// fallback.
    /// </summary>
    public static class EditorStartLevel
    {
        private const string Key = "Smash.EditorStartLevel";

        public static void Set(int index)
        {
#if UNITY_EDITOR
            UnityEditor.SessionState.SetInt(Key, index);
#endif
        }

        /// <returns>The requested level once (then it is cleared), else the fallback.</returns>
        public static int Consume(int fallback)
        {
#if UNITY_EDITOR
            int index = UnityEditor.SessionState.GetInt(Key, -1);
            UnityEditor.SessionState.EraseInt(Key);
            return index >= 0 ? index : fallback;
#else
            return fallback;
#endif
        }
    }
}
