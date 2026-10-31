using Game.Core;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    public sealed class TrajectoryMathTests
    {
        private const float Tolerance = 1e-5f;

        [Test]
        public void Sample_FirstDot_MatchesOneSemiImplicitStep()
        {
            var dots = new Vector3[1];

            TrajectoryMath.Sample(Vector3.zero, new Vector3(0f, 0f, 10f), new Vector3(0f, -10f, 0f), 0.02f, 1, dots);

            Assert.AreEqual(0f, dots[0].x, Tolerance);
            Assert.AreEqual(-0.004f, dots[0].y, Tolerance);
            Assert.AreEqual(0.2f, dots[0].z, Tolerance);
        }

        [Test]
        public void Sample_FillsAllDots_AndZAdvancesMonotonically()
        {
            var sentinel = new Vector3(float.NaN, float.NaN, float.NaN);
            var dots = new Vector3[8];
            for (int i = 0; i < dots.Length; i++) dots[i] = sentinel;

            TrajectoryMath.Sample(Vector3.zero, new Vector3(0f, 3f, 8f), new Vector3(0f, -9.81f, 0f), 0.02f, 2, dots);

            float lastZ = 0f;
            foreach (var dot in dots)
            {
                Assert.IsFalse(float.IsNaN(dot.z));
                Assert.Greater(dot.z, lastZ);
                lastZ = dot.z;
            }
        }

        [Test]
        public void StepsPerDot_Defaults()
        {
            Assert.AreEqual(1, TrajectoryMath.StepsPerDot(0.2f, 8, 0.02f));
            Assert.AreEqual(6, TrajectoryMath.StepsPerDot(1.0f, 8, 0.02f));
            Assert.AreEqual(1, TrajectoryMath.StepsPerDot(0.01f, 8, 0.02f));
        }
    }
}
