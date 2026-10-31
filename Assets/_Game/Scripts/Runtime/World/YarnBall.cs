using Game.Core;
using UnityEngine;

namespace Game.World
{
    /// <summary>The fired ball. Adds the extra impact punch and a one-time burst on its first prop contact.</summary>
    [RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
    public sealed class YarnBall : MonoBehaviour
    {
        private const int OverlapSlots = 32;

        private readonly Collider[] overlap = new Collider[OverlapSlots];
        private Rigidbody body;
        private ShotTuning tuning;
        private GameEvents events;
        private Vector3 preHitVelocity;
        private bool hasHitProp;

        public bool IsActive { get; private set; }
        public float Age { get; private set; }

        public void Launch(Vector3 origin, Vector3 velocity, ShotTuning t, GameEvents events)
        {
            tuning = t;
            this.events = events;
            Age = 0f;
            hasHitProp = false;
            IsActive = true;
            transform.SetPositionAndRotation(origin, Quaternion.identity);
            transform.localScale = Vector3.one * (t.ballRadius * 2f);
            gameObject.SetActive(true);

            body.mass = t.ballMass;
            body.position = origin;
            body.rotation = Quaternion.identity;
            body.linearVelocity = velocity;
            body.angularVelocity = Vector3.zero;
            preHitVelocity = velocity;
            body.WakeUp();
        }

        public void Despawn()
        {
            IsActive = false;
            gameObject.SetActive(false);
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            body.linearDamping = 0f;
            body.angularDamping = 0f;
            body.interpolation = RigidbodyInterpolation.Interpolate;
        }

        private void FixedUpdate()
        {
            if (!IsActive) return;
            Age += Time.fixedDeltaTime;
            preHitVelocity = body.linearVelocity;
            if (BallLife.ShouldDespawn(Age, preHitVelocity.magnitude, body.position.y, tuning)) Despawn();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!IsActive || collision.rigidbody == null) return;
            if (!collision.rigidbody.TryGetComponent(out DeskObject prop)) return;

            var point = collision.GetContact(0).point;
            var boost = ImpactMath.BoostImpulse(preHitVelocity, tuning.ballMass, tuning.impactSpeedMultiplier);
            prop.Body.AddForceAtPosition(boost, point, ForceMode.Impulse);

            bool first = !hasHitProp;
            hasHitProp = true;
            if (first) Burst(point);
            events?.RaiseImpact(point, collision.relativeVelocity.magnitude, first);
        }

        private void Burst(Vector3 center)
        {
            int count = Physics.OverlapSphereNonAlloc(center, tuning.burstRadius, overlap);
            for (int i = 0; i < count; i++)
            {
                var hit = overlap[i].attachedRigidbody;
                if (hit == null || !hit.TryGetComponent(out DeskObject _)) continue;
                var impulse = ImpactMath.BurstImpulse(center, hit.worldCenterOfMass, tuning.burstStrength, tuning.burstRadius);
                hit.AddForce(impulse, ForceMode.Impulse);
            }
        }
    }
}
