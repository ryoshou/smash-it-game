using System;

namespace Game.Meta
{
    /// <summary>
    /// Which screen the app is on (spec §7): splash, a first-run consent step, home, playing, paused. Illegal moves
    /// return false and change nothing, so a stray tap or a late callback can't skip a screen.
    /// </summary>
    public sealed class AppFlow
    {
        private readonly bool firstRun;

        public AppFlow(bool firstRun) => this.firstRun = firstRun;

        public AppScreen Screen { get; private set; } = AppScreen.Splash;
        public event Action<AppScreen> Changed;

        public bool SplashDone() => Move(AppScreen.Splash, firstRun ? AppScreen.Consent : AppScreen.Home);
        public bool ConsentDone() => Move(AppScreen.Consent, AppScreen.Home);
        public bool Play() => Move(AppScreen.Home, AppScreen.Playing);
        public bool Pause() => Move(AppScreen.Playing, AppScreen.Paused);
        public bool Resume() => Move(AppScreen.Paused, AppScreen.Playing);

        public bool GoHome() =>
            (Screen == AppScreen.Playing || Screen == AppScreen.Paused) && Move(Screen, AppScreen.Home);

        private bool Move(AppScreen from, AppScreen to)
        {
            if (Screen != from) return false;
            Screen = to;
            Changed?.Invoke(to);
            return true;
        }
    }
}
