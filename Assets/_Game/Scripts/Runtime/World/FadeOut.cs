using System;
using Game.Core;
using UnityEngine;

namespace Game.World
{
    /// <summary>Holds its targets at full size, shrinks them to nothing, then deactivates this object.</summary>
    public sealed class FadeOut : MonoBehaviour
    {
        /// <summary>Smallest scale written: shrinking a physics collider to ~0 gives its body degenerate inertia.</summary>
        public const float MinScale = 0.1f;

        [Tooltip("Transforms to shrink; empty means this object's own transform.")]
        [SerializeField] private Transform[] targets = Array.Empty<Transform>();

        private Vector3[] startScales;
        private float age;
        private float hold;
        private float fade;

        public event Action<FadeOut> Finished;

        public bool IsFading => enabled;

        public void Begin(float holdSeconds, float fadeSeconds)
        {
            CacheStartScales();
            age = 0f;
            hold = holdSeconds;
            fade = fadeSeconds;
            enabled = true;
        }

        /// <summary>Stops without finishing and restores full size (for pool reuse).</summary>
        public void ResetScale()
        {
            enabled = false;
            if (startScales == null) return;
            for (int i = 0; i < startScales.Length; i++) Target(i).localScale = startScales[i];
        }

        private void Awake()
        {
            CacheStartScales();
            enabled = false;
        }

        private void Update()
        {
            age += Time.deltaTime;
            float scale = Mathf.Max(MinScale, DebrisFade.Scale01(age, hold, fade));
            for (int i = 0; i < startScales.Length; i++) Target(i).localScale = startScales[i] * scale;
            if (!DebrisFade.IsDone(age, hold, fade)) return;
            ResetScale();
            gameObject.SetActive(false);
            Finished?.Invoke(this);
        }

        private void CacheStartScales()
        {
            if (startScales != null) return;
            int count = Mathf.Max(1, targets.Length);
            startScales = new Vector3[count];
            for (int i = 0; i < count; i++) startScales[i] = Target(i).localScale;
        }

        private Transform Target(int i) => targets.Length == 0 ? transform : targets[i];
    }
}
