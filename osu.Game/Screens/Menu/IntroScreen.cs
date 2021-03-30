// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Utils;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.IO.Archives;
using osu.Game.Overlays;
using osu.Game.Screens.Backgrounds;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Menu
{
    public abstract class IntroScreen : StartupScreen
    {
        /// <summary>
        /// Whether we have loaded the menu previously.
        /// </summary>
        public bool DidLoadMenu { get; private set; }

        /// <summary>
        /// A hash used to find the associated beatmap if already imported.
        /// </summary>
        protected abstract string BeatmapHash { get; }

        /// <summary>
        /// A source file to use as an import source if the intro beatmap is not yet present.
        /// Should be within the "Tracks" namespace of game resources.
        /// </summary>
        protected abstract string BeatmapFile { get; }

        protected IBindable<bool> MenuVoice { get; private set; }

        protected IBindable<bool> MenuMusic { get; private set; }

        private WorkingBeatmap initialBeatmap;

        protected ITrack Track { get; private set; }

        private const int exit_delay = 3000;

        private Sample seeya;

        protected virtual string SeeyaSampleName => "Intro/seeya";

        private LeasedBindable<WorkingBeatmap> beatmap;

        private MainMenu mainMenu;

        [Resolved]
        private AudioManager audio { get; set; }

        [Resolved]
        private MusicController musicController { get; set; }

        /// <summary>
        /// Whether the <see cref="Track"/> is provided by osu! resources, rather than a user beatmap.
        /// Only valid during or after <see cref="LogoArriving"/>.
        /// </summary>
        protected bool UsingThemedIntro { get; private set; }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config, SkinManager skinManager, BeatmapManager beatmaps, Framework.Game game)
        {
            // prevent user from changing beatmap while the intro is still runnning.
            beatmap = Beatmap.BeginLease(false);

            MenuVoice = config.GetBindable<bool>(OsuSetting.MenuVoice);
            MenuMusic = config.GetBindable<bool>(OsuSetting.MenuMusic);
            seeya = audio.Samples.Get(SeeyaSampleName);

            BeatmapSetInfo setInfo = null;

            // if the user has requested not to play theme music, we should attempt to find a random beatmap from their collection.
            if (!MenuMusic.Value)
            {
                var sets = beatmaps.GetAllUsableBeatmapSets(IncludedDetails.Minimal);

                if (sets.Count > 0)
                {
                    setInfo = beatmaps.QueryBeatmapSet(s => s.ID == sets[RNG.Next(0, sets.Count - 1)].ID);
                    initialBeatmap = beatmaps.GetWorkingBeatmap(setInfo.Beatmaps[0]);
                }
            }

            // we generally want a song to be playing on startup, so use the intro music even if a user has specified not to if no other track is available.
            if (setInfo == null)
            {
                if (!loadThemedIntro())
                {
                    // if we detect that the theme track or beatmap is unavailable this is either first startup or things are in a bad state.
                    // this could happen if a user has nuked their files store. for now, reimport to repair this.
                    var import = beatmaps.Import(new ZipArchiveReader(game.Resources.GetStream($"Tracks/{BeatmapFile}"), BeatmapFile)).Result;
                    import.Protected = true;
                    beatmaps.Update(import);

                    loadThemedIntro();
                }
            }

            bool loadThemedIntro()
            {
                setInfo = beatmaps.QueryBeatmapSet(b => b.Hash == BeatmapHash);

                if (setInfo == null)
                    return false;

                return (initialBeatmap = beatmaps.GetWorkingBeatmap(setInfo.Beatmaps[0])) != null;
            }
        }

        public override void OnResuming(IScreen last)
        {
            this.FadeIn(300);

            double fadeOutTime = exit_delay;

            var track = musicController.CurrentTrack;

            // ensure the track doesn't change or loop as we are exiting.
            track.Looping = false;
            Beatmap.Disabled = true;

            // we also handle the exit transition.
            if (MenuVoice.Value)
            {
                seeya.Play();

                // if playing the outro voice, we have more time to have fun with the background track.
                // initially fade to almost silent then ramp out over the remaining time.
                const double initial_fade = 200;
                track
                    .VolumeTo(0.03f, initial_fade).Then()
                    .VolumeTo(0, fadeOutTime - initial_fade, Easing.In);
            }
            else
            {
                fadeOutTime = 500;

                // if outro voice is turned off, just do a simple fade out.
                track.VolumeTo(0, fadeOutTime, Easing.Out);
            }

            //don't want to fade out completely else we will stop running updates.
            Game.FadeTo(0.01f, fadeOutTime).OnComplete(_ => this.Exit());

            base.OnResuming(last);
        }

        public override void OnSuspending(IScreen next)
        {
            base.OnSuspending(next);
            initialBeatmap = null;
        }

        protected override BackgroundScreen CreateBackground() => new BackgroundScreenBlack();

        protected void StartTrack()
        {
            // Only start the current track if it is the menu music. A beatmap's track is started when entering the Main Menu.
            if (UsingThemedIntro)
                Track.Start();
        }

        protected override void LogoArriving(OsuLogo logo, bool resuming)
        {
            base.LogoArriving(logo, resuming);

            logo.Colour = Color4.White;
            logo.Triangles = false;
            logo.Ripple = false;

            if (!resuming)
            {
                beatmap.Value = initialBeatmap;
                Track = initialBeatmap.Track;
                UsingThemedIntro = !initialBeatmap.Track.IsDummyDevice;

                // ensure the track starts at maximum volume
                musicController.CurrentTrack.FinishTransforms();

                logo.MoveTo(new Vector2(0.5f));
                logo.ScaleTo(Vector2.One);
                logo.Hide();
            }
            else
            {
                const int quick_appear = 350;
                var initialMovementTime = logo.Alpha > 0.2f ? quick_appear : 0;

                logo.MoveTo(new Vector2(0.5f), initialMovementTime, Easing.OutQuint);

                logo
                    .ScaleTo(1, initialMovementTime, Easing.OutQuint)
                    .FadeIn(quick_appear, Easing.OutQuint)
                    .Then()
                    .RotateTo(20, exit_delay * 1.5f)
                    .FadeOut(exit_delay);
            }
        }

        protected void PrepareMenuLoad() => LoadComponentAsync(mainMenu = new MainMenu());

        protected void LoadMenu()
        {
            beatmap.Return();

            DidLoadMenu = true;
            this.Push(mainMenu);
        }
    }
}
