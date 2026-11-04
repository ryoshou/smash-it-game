using Game.Core;
using UnityEngine;

namespace Game.World
{
    /// <summary>
    /// On the fizzy soda: arms when the can is knocked over or reaches the floor (FizzRule), then bursts, pushing every
    /// loose body in range (props, other fizzies, the ball) and counting the can as cleared. It lives and dies with its
    /// prop, so a fuse from a finished round never bursts into the next one.
    /// </summary>
    [RequireComponent(typeof(DeskObject))]
    public sealed class FizzFuse : MonoBehaviour
    {
        private const int MaxHits = 32;
        private const float NotArmed = -1f;

        private readonly Collider[] overlap = new Collider[MaxHits];
        private readonly Rigidbody[] pushed = new Rigidbody[MaxHits];
        private DeskObject prop;
        private float armedAt = NotArmed;

        public bool IsArmed => armedAt >= 0f;

        private void Awake() => prop = GetComponent<DeskObject>();

        private void OnEnable() => armedAt = NotArmed;

        private void FixedUpdate()
        {
            var tuning = prop.Tuning;
            if (tuning == null || prop.IsBroken) return;
            if (!IsArmed)
            {
                if (FizzRule.ShouldArm(transform.up, prop.HasTouchedFloor, tuning.fizzTiltDegrees)) armedAt = Time.time;
                return;
            }
            if (!FizzRule.FuseDone(armedAt, Time.time, tuning.fizzFuseSeconds)) return;
            Push(prop.Body.worldCenterOfMass, tuning.fizzBurstRadius, tuning.fizzBurstStrength);
            prop.Burst();
        }

        private void Push(Vector3 center, float radius, float strength)
        {
            int count = Physics.OverlapSphereNonAlloc(center, radius, overlap);
            int bodies = 0;
            for (int i = 0; i < count; i++)
            {
                var body = overlap[i].attachedRigidbody;
                if (body == null || body == prop.Body || body.isKinematic || Contains(body, bodies)) continue;
                pushed[bodies++] = body;
                body.AddForce(ImpactMath.BurstImpulse(center, body.worldCenterOfMass, strength, radius), ForceMode.Impulse);
            }
            System.Array.Clear(pushed, 0, bodies);
        }

        private bool Contains(Rigidbody body, int count)
        {
            for (int i = 0; i < count; i++)
                if (pushed[i] == body) return true;
            return false;
        }
    }
}
