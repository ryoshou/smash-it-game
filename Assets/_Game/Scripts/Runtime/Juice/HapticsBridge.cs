#if UNITY_IOS && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif
using UnityEngine;

namespace Game.Juice
{
    /// <summary>
    /// Sends a pulse to the platform. Android: a VibrationEffect one-shot (API 26+). iOS: UIImpactFeedbackGenerator
    /// through Plugins/iOS/Haptics.mm. Editor and other platforms: nothing. Errors switch haptics off for the session
    /// (one warning) instead of throwing into gameplay.
    /// </summary>
    public static class HapticsBridge
    {
#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void GameHapticsImpact(int style, float intensity);
#endif

#if UNITY_ANDROID && !UNITY_EDITOR
        private const int DefaultAmplitude = -1; // VibrationEffect.DEFAULT_AMPLITUDE

        private static bool isReady;
        private static bool isBroken;
        private static bool hasAmplitudeControl;
        private static AndroidJavaObject vibrator;
        private static AndroidJavaClass effects;
#endif

        public static void Play(HapticKind kind, HapticPulse pulse)
        {
            if (pulse.IsNone) return;
#if UNITY_IOS && !UNITY_EDITOR
            GameHapticsImpact((int)kind, pulse.Amount01);
#elif UNITY_ANDROID && !UNITY_EDITOR
            PlayAndroid(kind, pulse);
#endif
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        private static void PlayAndroid(HapticKind kind, HapticPulse pulse)
        {
            if (!isReady) Init();
            if (isBroken)
            {
                // Also what makes Unity add android.permission.VIBRATE to the manifest.
                if (kind == HapticKind.Heavy) Handheld.Vibrate();
                return;
            }
            if (vibrator == null) return; // no vibrator: nothing to do
            try
            {
                int amplitude = hasAmplitudeControl ? pulse.AndroidAmplitude : DefaultAmplitude;
                using AndroidJavaObject effect = effects.CallStatic<AndroidJavaObject>("createOneShot", (long)pulse.Milliseconds, amplitude);
                vibrator.Call("vibrate", effect);
            }
            catch (AndroidJavaException exception)
            {
                Debug.LogWarning($"Haptics off for this session: {exception.Message}");
                isBroken = true;
            }
        }

        private static void Init()
        {
            isReady = true;
            try
            {
                using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                using AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                AndroidJavaObject service = activity.Call<AndroidJavaObject>("getSystemService", "vibrator");
                if (service == null || !service.Call<bool>("hasVibrator"))
                {
                    service?.Dispose();
                    return;
                }
                vibrator = service;
                hasAmplitudeControl = vibrator.Call<bool>("hasAmplitudeControl");
                effects = new AndroidJavaClass("android.os.VibrationEffect");
            }
            catch (AndroidJavaException exception)
            {
                Debug.LogWarning($"Haptics: no vibrator service ({exception.Message}); heavy pulses use Handheld.Vibrate.");
                isBroken = true;
            }
        }
#endif
    }
}
