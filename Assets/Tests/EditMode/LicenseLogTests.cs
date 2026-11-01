using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace Game.Tests
{
    public sealed class LicenseLogTests
    {
        private const string LogPath = "docs/asset-licenses.md";
        private static readonly string[] Roots = { "Assets/_Game/Art", "Assets/_Game/Audio" };
        private static readonly string[] Extensions = { ".fbx", ".glb", ".png", ".ogg", ".wav", ".mp3", ".ttf" };

        [Test]
        public void EveryArtAndAudioFile_IsListed()
        {
            var listed = new HashSet<string>(Rows().Select(row => row[0]));

            var missing = ShippedFiles().Where(path => !listed.Contains(path)).ToList();

            Assert.IsEmpty(missing, "Not in " + LogPath + ":\n" + string.Join("\n", missing));
        }

        [Test]
        public void EveryRow_HasLicense()
        {
            var unlicensed = Rows().Where(row => row.Length < 4 || row[3].Length == 0).Select(row => row[0]).ToList();

            Assert.IsEmpty(unlicensed, "Rows without a license:\n" + string.Join("\n", unlicensed));
        }

        private static IEnumerable<string> ShippedFiles() =>
            Roots.Where(Directory.Exists)
                .SelectMany(root => Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
                .Where(path => Extensions.Contains(Path.GetExtension(path).ToLowerInvariant()))
                .Select(path => path.Replace('\\', '/'))
                .OrderBy(path => path);

        /// <summary>Table rows after the header and separator, as trimmed cells (path cell without backticks).</summary>
        private static IEnumerable<string[]> Rows() =>
            File.ReadAllLines(LogPath)
                .Where(line => line.StartsWith("|"))
                .Skip(2)
                .Select(line => line.Trim().Trim('|').Split('|').Select(cell => cell.Trim().Trim('`')).ToArray());
    }
}
