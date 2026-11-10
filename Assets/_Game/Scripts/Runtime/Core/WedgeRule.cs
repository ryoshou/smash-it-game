using UnityEngine;

namespace Game.Core
{
    /// <summary>
    /// The owner's call: nothing stays stuck between the desks. A prop pushed off the main desk's back edge can lean on the
    /// back desk's top and never fall. Nothing there can hold a prop up but such a wedge, so one resting with its centre
    /// over the gap is stuck and gets let through.
    /// </summary>
    public static class WedgeRule
    {
        /// <summary>Whether the centre is over the gap: past the main desk's back edge, short of the back desk's front.</summary>
        public static bool IsOverGap(Vector3 centre, float mainDeskBackZ, float backDeskFrontZ) =>
            centre.z > mainDeskBackZ && centre.z < backDeskFrontZ;

        public static bool IsStuck(float restedOverGapSeconds, float stuckSeconds) => restedOverGapSeconds >= stuckSeconds;
    }
}
