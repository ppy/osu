// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Screens;
using osu.Framework.Utils;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Database;
using osu.Game.Localisation;
using osu.Game.Online.API;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using osu.Game.Rulesets;
using osu.Game.Screens.Backgrounds;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;
using Realms;

namespace osu.Game.Screens.Menu
{
    public abstract partial class IntroScreen : StartupScreen
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

        private SkinnableSound skinnableSeeya;
        private ISample seeya;

        protected virtual string SeeyaSampleName => "Intro/seeya";

        protected override bool PlayExitSound => false;

        private LeasedBindable<WorkingBeatmap> beatmap;

        private OsuScreen nextScreen;

        [Resolved]
        private AudioManager audio { get; set; }

        [Resolved]
        private MusicController musicController { get; set; }

        [CanBeNull]
        private readonly Func<OsuScreen> createNextScreen;

        [Resolved]
        private RulesetStore rulesets { get; set; }

        /// <summary>
        /// Whether the <see cref="Track"/> is provided by osu! resources, rather than a user beatmap.
        /// Only valid during or after <see cref="LogoArriving"/>.
        /// </summary>
        protected bool UsingThemedIntro { get; private set; }

        protected override BackgroundScreen CreateBackground() => new BackgroundScreenDefault(false)
        {
            Colour = Color4.Black
        };

        protected IntroScreen([CanBeNull] Func<MainMenu> createNextScreen = null)
        {
            this.createNextScreen = createNextScreen;
        }

        [Resolved]
        private BeatmapManager beatmaps { get; set; }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config, Framework.Game game, RealmAccess realm, IAPIProvider api)
        {
            // prevent user from changing beatmap while the intro is still running.
            beatmap = Beatmap.BeginLease(false);

            musicController.AllowTrackControl.Value = false;

            MenuVoice = config.GetBindable<bool>(OsuSetting.MenuVoice);
            MenuMusic = config.GetBindable<bool>(OsuSetting.MenuMusic);

            if (api.LocalUser.Value.IsSupporter)
                AddInternal(skinnableSeeya = new SkinnableSound(new SampleInfo(SeeyaSampleName)));
            else
                seeya = audio.Samples.Get(SeeyaSampleName);

            // if the user has requested not to play theme music, we should attempt to find a random beatmap from their collection.
            if (!MenuMusic.Value)
            {
                realm.Run(r =>
                {
                    var usableBeatmapSets = r.All<BeatmapSetInfo>().Where(s => !s.DeletePending && !s.Protected).AsRealmCollection();

                    int setCount = usableBeatmapSets.Count;

                    if (setCount > 0)
                    {
                        var found = usableBeatmapSets[RNG.Next(0, setCount - 1)].Beatmaps.FirstOrDefault();

                        if (found != null)
                            initialBeatmap = beatmaps.GetWorkingBeatmap(found);
                    }
                });
            }

            // we generally want a song to be playing on startup, so use the intro music even if a user has specified not to if no other track is available.
            if (initialBeatmap == null)
            {
                // Intro beatmaps are generally made using the osu! ruleset.
                // It might not be present in test projects for other rulesets.
                bool osuRulesetPresent = rulesets.GetRuleset(0) != null;

                if (!loadThemedIntro() && osuRulesetPresent)
                {
                    // if we detect that the theme track or beatmap is unavailable this is either first startup or things are in a bad state.
                    // this could happen if a user has nuked their files store. for now, reimport to repair this.
                    var import = beatmaps.Import(new ImportTask(game.Resources.GetStream($"Tracks/{BeatmapFile}"), BeatmapFile)).GetResultSafely();

                    import?.PerformWrite(b => b.Protected = true);

                    loadThemedIntro();
                }
            }

            bool loadThemedIntro()
            {
                var setInfo = beatmaps.QueryBeatmapSet(b => b.Protected && b.Hash == BeatmapHash);

                if (setInfo == null)
                    return false;

                setInfo.PerformRead(s =>
                {
                    if (s.Beatmaps.Count == 0)
                        return;

                    initialBeatmap = beatmaps.GetWorkingBeatmap(s.Beatmaps.First());
                });

                return UsingThemedIntro = initialBeatmap != null;
            }
        }

        public override void OnEntering(ScreenTransitionEvent e)
        {
            base.OnEntering(e);
            ensureEventuallyArrivingAtMenu();
        }

        [Resolved]
        private INotificationOverlay notifications { get; set; }

        private void ensureEventuallyArrivingAtMenu()
        {
            // This intends to handle the case where an intro may get stuck.
            // Historically, this could happen if the host system's audio device is in a state it can't
            // play audio, causing a clock to never elapse time and the intro to never end.
            //
            // This safety measure gives the user a chance to fix the problem from the settings menu.
            Scheduler.AddDelayed(() =>
            {
                if (DidLoadMenu)
                    return;

                PrepareMenuLoad();
                LoadMenu();

                if (!Debugger.IsAttached)
                {
                    notifications.Post(new SimpleErrorNotification
                    {
                        Text = NotificationsStrings.AudioPlaybackIssue
                    });
                }
            }, 5000);
        }

        public override void OnResuming(ScreenTransitionEvent e)
        {
            this.FadeIn(300);

            ApplyToBackground(b => b.FadeColour(Color4.Black, 100));

            double fadeOutTime = exit_delay;

            var track = musicController.CurrentTrack;

            // ensure the track doesn't change or loop as we are exiting.
            track.Looping = false;
            Beatmap.Disabled = true;

            // we also handle the exit transition.
            if (MenuVoice.Value)
            {
                if (skinnableSeeya != null)
                {
                    // resuming a screen (i.e. calling OnResume) happens before the screen itself becomes alive,
                    // therefore skinnable samples may not be updated yet with the recently selected skin.
                    // schedule after children to ensure skinnable samples have processed skin changes before playing.
                    ScheduleAfterChildren(() => skinnableSeeya.Play());
                }
                else
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

            base.OnResuming(e);
        }

        private bool backgroundFaded;

        protected void FadeInBackground(float duration = 0)
        {
            ApplyToBackground(b => b.FadeColour(Color4.White, duration));
            backgroundFaded = true;
        }

        public override void OnSuspending(ScreenTransitionEvent e)
        {
            base.OnSuspending(e);
            initialBeatmap = null;

            if (!backgroundFaded)
                FadeInBackground(200);
        }

        protected void StartTrack()
        {
            var drawableTrack = musicController.CurrentTrack;

            if (!UsingThemedIntro)
            {
                initialBeatmap?.PrepareTrackForPreview(false, -2600);

                drawableTrack.VolumeTo(0);
                drawableTrack.Restart();
                drawableTrack.VolumeTo(1, 2600, Easing.InCubic);
            }
            else
            {
                drawableTrack.Restart();
            }
        }

        protected override void LogoArriving(OsuLogo logo, bool resuming)
        {
            base.LogoArriving(logo, resuming);

            logo.Colour = Color4.White;
            logo.Triangles = false;
            logo.Ripple = false;

            if (!resuming)
            {
                // generally this can never be null
                // an exception is running ruleset tests, where the osu! ruleset may not be present (causing importing the intro to fail).
                if (initialBeatmap != null)
                    beatmap.Value = initialBeatmap;
                Track = beatmap.Value.Track;

                // ensure the track starts at maximum volume
                musicController.CurrentTrack.FinishTransforms();

                logo.MoveTo(new Vector2(0.5f));
                logo.ScaleTo(Vector2.One);
                logo.Hide();
            }
            else
            {
                const int quick_appear = 350;
                int initialMovementTime = logo.Alpha > 0.2f ? quick_appear : 0;

                logo.MoveTo(new Vector2(0.5f), initialMovementTime, Easing.OutQuint);

                logo
                    .ScaleTo(1, initialMovementTime, Easing.OutQuint)
                    .FadeIn(quick_appear, Easing.OutQuint)
                    .Then()
                    .RotateTo(20, exit_delay * 1.5f)
                    .FadeOut(exit_delay);
            }
        }

        protected void PrepareMenuLoad()
        {
            if (nextScreen != null)
                return;

            nextScreen = createNextScreen?.Invoke();

            if (nextScreen != null)
                LoadComponentAsync(nextScreen);
        }

        protected void LoadMenu()
        {
            if (DidLoadMenu)
                return;

            beatmap.Return();

            DidLoadMenu = true;
            if (nextScreen != null)
                this.Push(nextScreen);
        }
    }
}
