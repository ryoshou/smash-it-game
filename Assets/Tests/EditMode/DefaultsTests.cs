using Game.Core;
using Game.Levels;
using Game.Meta;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    /// <summary>Fresh tuning and strings carry their designed defaults (the shipped assets are built from them).</summary>
    public sealed class DefaultsTests
    {
        [Test]
        public void ShotTuning_Default_HasTheFizzValues()
        {
            var tuning = ShotTuning.CreateDefault();
            try
            {
                Assert.AreEqual(55f, tuning.fizzTiltDegrees);
                Assert.AreEqual(0.25f, tuning.fizzFuseSeconds);
                Assert.AreEqual(0.7f, tuning.fizzBurstRadius);
                Assert.AreEqual(2.5f, tuning.fizzBurstStrength);
            }
            finally
            {
                Object.DestroyImmediate(tuning);
            }
        }

        [Test]
        public void ShotTuning_Clone_CopiesEveryValue()
        {
            var tuning = ShotTuning.CreateDefault();
            tuning.fizzBurstStrength = 9f;
            var clone = tuning.Clone();
            try
            {
                Assert.AreNotSame(tuning, clone);
                Assert.AreEqual(JsonUtility.ToJson(tuning), JsonUtility.ToJson(clone));
            }
            finally
            {
                Object.DestroyImmediate(tuning);
                Object.DestroyImmediate(clone);
            }
        }

        [Test]
        public void UiStrings_PropName_FallsBackToTheKindName()
        {
            var strings = ScriptableObject.CreateInstance<UiStrings>();
            try
            {
                Assert.AreEqual("FizzySoda", strings.PropName(PropKind.FizzySoda));
                strings.propNames = new[] { "Keycap" };
                Assert.AreEqual("Keycap", strings.PropName(PropKind.Keycap1u));
                Assert.AreEqual("Level {0}", strings.levelFormat);
            }
            finally
            {
                Object.DestroyImmediate(strings);
            }
        }
    }
}
