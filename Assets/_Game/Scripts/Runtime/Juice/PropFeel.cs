using System;
using Game.Audio;
using Game.Levels;
using UnityEngine;

namespace Game.Juice
{
    /// <summary>How one prop kind sounds and looks when hit or broken (spec §5 material personalities).</summary>
    [Serializable]
    public struct PropFeel
    {
        public PropKind kind;
        public SoundSet hit;
        public SoundSet breakSound;
        public ParticleSystem breakVfx;
        [Tooltip("Impacts slower than this (m/s) stay silent.")]
        public float hitMinSpeed;
    }
}
