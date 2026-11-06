using Game.Audio;
using Game.Core;
using Game.Levels;
using UnityEngine;

namespace Game.Juice
{
    /// <summary>
    /// Each prop kind's own hit and break sounds, its break effect, and a light haptic when something shatters or
    /// splashes. A splash effect carries a child <see cref="SplatName"/> that lies flat where the food landed.
    /// </summary>
    public sealed class PropFeedback : MonoBehaviour
    {
        public const string SplatName = "Splat";
        private const int KeysPerKind = 2;

        private readonly RateGate hitGate = new RateGate();
        private readonly RateGate breakGate = new RateGate();
        private GameEvents events;
        private JuiceConfig config;
        private PropFeelLibrary library;
        private SfxPlayer sfx;
        private VfxPool vfx;

        public void Bind(GameEvents events, JuiceConfig config, PropFeelLibrary library, SfxPlayer sfx, VfxPool vfx)
        {
            Unbind();
            this.events = events;
            this.config = config;
            this.library = library;
            this.sfx = sfx;
            this.vfx = vfx;
            foreach (var feel in library.feels) vfx.Prewarm(feel.breakVfx, Copies(feel.breakVfx));
            events.PropImpact += OnPropImpact;
            events.Broke += OnBroke;
        }

        private void OnDestroy() => Unbind();

        private void Unbind()
        {
            if (events == null) return;
            events.PropImpact -= OnPropImpact;
            events.Broke -= OnBroke;
            events = null;
        }

        private void OnPropImpact(PropKind kind, Vector3 point, float speed)
        {
            if (!library.TryGet(kind, out var feel) || feel.hit == null || speed < feel.hitMinSpeed) return;
            if (!hitGate.TryPass(Time.unscaledTime, config.impactMinInterval)) return;
            sfx.PlayClip(feel.hit.Next(), VoiceKey(kind, 0), feel.hit.Volume(speed), feel.hit.Pitch(speed), config.maxHitCopies);
        }

        private void OnBroke(PropKind kind, Vector3 position, BreakMode mode)
        {
            if (!library.TryGet(kind, out var feel)) return;
            if (mode == BreakMode.Splash) position += Vector3.up * config.splatLift;
            vfx.Play(feel.breakVfx, position);
            if (BreakRule.HasPieces(mode)) HapticsBridge.Play(HapticKind.Light, config.ShatterHaptic);
            if (feel.breakSound == null || !breakGate.TryPass(Time.unscaledTime, config.impactMinInterval)) return;
            float loudest = feel.breakSound.hardSpeed;
            sfx.PlayClip(feel.breakSound.Next(), VoiceKey(kind, 1), feel.breakSound.Volume(loudest),
                feel.breakSound.Pitch(loudest), config.maxHitCopies);
        }

        private int Copies(ParticleSystem effect) =>
            effect != null && effect.transform.Find(SplatName) != null ? config.splashVfxPerKind : config.vfxPerKind;

        private static int VoiceKey(PropKind kind, int slot) => SfxBank.Count + (int)kind * KeysPerKind + slot;
    }
}
