using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
    /// <summary>Fixed import settings for shipped audio: short mono effects decompressed on load, streamed music.</summary>
    public sealed class AudioPostprocessor : AssetPostprocessor
    {
        private const string SfxRoot = "Assets/_Game/Audio/Sfx/";
        private const string MusicRoot = "Assets/_Game/Audio/Music/";
        private const float SfxQuality = 0.7f;
        private const float MusicQuality = 0.5f;

        private void OnPreprocessAudio()
        {
            var importer = (AudioImporter)assetImporter;
            if (assetPath.StartsWith(SfxRoot))
            {
                importer.forceToMono = true;
                importer.defaultSampleSettings = Settings(AudioClipLoadType.DecompressOnLoad, SfxQuality);
            }
            else if (assetPath.StartsWith(MusicRoot))
            {
                importer.forceToMono = false;
                importer.loadInBackground = true;
                importer.defaultSampleSettings = Settings(AudioClipLoadType.Streaming, MusicQuality);
            }
        }

        private static AudioImporterSampleSettings Settings(AudioClipLoadType loadType, float quality) => new AudioImporterSampleSettings
        {
            loadType = loadType,
            compressionFormat = AudioCompressionFormat.Vorbis,
            quality = quality,
            sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate,
            preloadAudioData = loadType != AudioClipLoadType.Streaming,
        };
    }
}
