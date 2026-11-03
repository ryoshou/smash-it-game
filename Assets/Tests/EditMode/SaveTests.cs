using Game.Meta;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Game.Tests
{
    public sealed class SaveTests
    {
        private sealed class FakeStore : ISaveStore
        {
            public string Json;
            public string Load() => Json;
            public void Save(string json) => Json = json;
        }

        [Test]
        public void RoundTrip_KeepsEveryField()
        {
            var data = new SaveData
            {
                levelIndex = 7, sound = false, music = false, haptics = false, removeAds = true, firstRunDone = true,
            };

            var read = SaveCodec.Read(SaveCodec.Write(data));

            Assert.AreEqual(SaveCodec.Version, read.version);
            Assert.AreEqual(7, read.levelIndex);
            Assert.IsFalse(read.sound);
            Assert.IsFalse(read.music);
            Assert.IsFalse(read.haptics);
            Assert.IsTrue(read.removeAds);
            Assert.IsTrue(read.firstRunDone);
        }

        [TestCase(null)]
        [TestCase("")]
        public void Read_NullOrEmpty_ReturnsDefaults(string json) => AssertDefaults(SaveCodec.Read(json));

        [Test]
        public void Read_CorruptJson_ReturnsDefaults()
        {
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("save"));

            AssertDefaults(SaveCodec.Read("{not json"));
        }

        [Test]
        public void Read_NegativeLevel_ClampsToZero() =>
            Assert.AreEqual(0, SaveCodec.Read("{\"levelIndex\":-4}").levelIndex);

        [Test]
        public void Read_HugeLevel_ClampsToMax() =>
            Assert.AreEqual(SaveCodec.MaxLevelIndex, SaveCodec.Read("{\"levelIndex\":2147483647}").levelIndex);

        [Test]
        public void Read_UnknownFields_KeepsKnownOnes() =>
            Assert.AreEqual(3, SaveCodec.Read("{\"version\":9,\"levelIndex\":3,\"future\":1}").levelIndex);

        [Test]
        public void RecordWin_AdvancesOnlyForward()
        {
            var profile = new Profile(new FakeStore());

            profile.RecordWin(4);
            profile.RecordWin(2);

            Assert.AreEqual(5, profile.Data.levelIndex);
        }

        [Test]
        public void Setters_WriteThroughToStore()
        {
            var store = new FakeStore();
            var profile = new Profile(store);

            profile.SetMusic(false);
            profile.SetSound(false);
            profile.SetHaptics(false);
            profile.SetRemoveAds(true);
            profile.MarkFirstRunDone();
            var reloaded = new Profile(store).Data;

            Assert.IsFalse(reloaded.music);
            Assert.IsFalse(reloaded.sound);
            Assert.IsFalse(reloaded.haptics);
            Assert.IsTrue(reloaded.removeAds);
            Assert.IsTrue(reloaded.firstRunDone);
        }

        [Test]
        public void ResetAll_RestoresDefaults()
        {
            var store = new FakeStore();
            var profile = new Profile(store);
            profile.RecordWin(9);
            profile.SetSound(false);

            profile.ResetAll();

            AssertDefaults(profile.Data);
            AssertDefaults(new Profile(store).Data);
        }

        [Test]
        public void PlayerPrefsStore_SavesAndLoadsUnderItsKey()
        {
            bool had = PlayerPrefs.HasKey(PlayerPrefsSaveStore.Key);
            string before = PlayerPrefs.GetString(PlayerPrefsSaveStore.Key, "");
            try
            {
                var store = new PlayerPrefsSaveStore();
                store.Save("{\"levelIndex\":12}");

                Assert.AreEqual("{\"levelIndex\":12}", store.Load());
                Assert.AreEqual(12, new Profile(store).Data.levelIndex);
            }
            finally
            {
                if (had) PlayerPrefs.SetString(PlayerPrefsSaveStore.Key, before);
                else PlayerPrefs.DeleteKey(PlayerPrefsSaveStore.Key);
            }
        }

        private static void AssertDefaults(SaveData data)
        {
            Assert.AreEqual(0, data.levelIndex);
            Assert.IsTrue(data.sound);
            Assert.IsTrue(data.music);
            Assert.IsTrue(data.haptics);
            Assert.IsFalse(data.removeAds);
            Assert.IsFalse(data.firstRunDone);
        }
    }
}
