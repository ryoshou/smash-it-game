using NUnit.Framework;

namespace Game.Tests
{
    /// <summary>Wins write the real save now; leave the editor's PlayerPrefs as the run found them (clean).</summary>
    [SetUpFixture]
    public sealed class SaveHygiene
    {
        [OneTimeSetUp]
        public void Before() => AppTestKit.ClearSave();

        [OneTimeTearDown]
        public void After() => AppTestKit.ClearSave();
    }
}
