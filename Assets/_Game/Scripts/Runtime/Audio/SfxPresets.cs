using System.Collections.Generic;
using UnityEngine;

namespace Game.Audio
{
    /// <summary>
    /// Every sound's design. SfxPlayer synthesises clips from it at startup; in Play mode, edit a preset, then use the
    /// SfxPlayer's "Rebuild Clips" context menu to hear the change.
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Sfx Presets", fileName = "SfxPresets")]
    public sealed class SfxPresets : ScriptableObject
    {
        [SerializeField] private SfxPreset[] presets = SfxPresetLibrary.Defaults();

        public IReadOnlyList<SfxPreset> Presets => presets;
    }
}
