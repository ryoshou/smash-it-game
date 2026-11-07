using Game.Core;
using UnityEngine;

namespace Game.World
{
    /// <summary>
    /// Food's break: it squashes flat in one piece where it landed (no pieces: food is not glass or china), holds, then
    /// shrinks away and switches off. The stain under it is the juice's job (PropFeedback).
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public sealed class Squash : MonoBehaviour
    {
        /// <summary>Smallest scale written, so a shrinking prop never reaches a degenerate zero scale.</summary>
        private const float MinScale = 0.01f;

        [Tooltip("The prop's upright height at scale 1, so the squashed prop sits on the ground under it.")]
        [SerializeField] private float height;

        private Rigidbody body;
        private Vector3 startScale;
        private Vector3 ground;
        private float age;
        private float flatten;
        private float squashSeconds;
        private float holdSeconds;
        private float fadeSeconds;

        public bool IsSquashing => enabled;

        public void Begin(Vector3 groundPoint, ShotTuning t)
        {
            // Out of the simulation: a squashed prop is scenery, not something to hit.
            body.isKinematic = true;
            body.detectCollisions = false;
            body.interpolation = RigidbodyInterpolation.None;
            startScale = transform.localScale;
            ground = groundPoint;
            transform.rotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
            flatten = t.squashFlatten;
            squashSeconds = t.squashSeconds;
            holdSeconds = t.debrisHoldSeconds;
            fadeSeconds = t.debrisFadeSeconds;
            age = 0f;
            enabled = true;
            Apply();
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            enabled = false;
        }

        private void Update()
        {
            age += Time.deltaTime;
            if (SquashMath.IsDone(age, squashSeconds, holdSeconds, fadeSeconds))
            {
                enabled = false;
                gameObject.SetActive(false);
                return;
            }
            Apply();
        }

        private void Apply()
        {
            var scale = SquashMath.Scale(age, flatten, squashSeconds, holdSeconds, fadeSeconds);
            float across = Mathf.Max(scale.x, MinScale);
            float up = Mathf.Max(scale.y, MinScale);
            transform.localScale = new Vector3(startScale.x * across, startScale.y * up, startScale.z * across);
            transform.position = ground + Vector3.up * (height * startScale.y * up / 2f);
        }
    }
}
