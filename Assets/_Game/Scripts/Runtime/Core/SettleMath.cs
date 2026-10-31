using UnityEngine;

namespace Game.Core
{
    /// <summary>Whether a body still counts as moving for the "wait until things settle" check.</summary>
    public static class SettleMath
    {
        public static bool IsMoving(Vector3 linear, Vector3 angular, float linThreshold, float angThreshold) =>
            linear.sqrMagnitude > linThreshold * linThreshold || angular.sqrMagnitude > angThreshold * angThreshold;
    }
}
