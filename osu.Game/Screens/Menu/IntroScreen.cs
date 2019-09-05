// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Screens.Backgrounds;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Menu
{
    public abstract class IntroScreen : StartupScreen
    {
        private readonly BindableDouble exitingVolumeFade = new BindableDouble(1);

        public const int EXIT_DELAY = 3000;

        [Resolved]
        private AudioManager audio { get; set; }

        private SampleChannel seeya;

        private Bindable<bool> menuVoice;

        private LeasedBindable<WorkingBeatmap> beatmap;

        public new Bindable<WorkingBeatmap> Beatmap => beatmap;

        protected override BackgroundScreen CreateBackground() => new BackgroundScreenBlack();

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config, BeatmapManager beatmaps, Framework.Game game)
        {
            // prevent user from changing beatmap while the intro is still runnning.
            beatmap = base.Beatmap.BeginLease(false);

            menuVoice = config.GetBindable<bool>(OsuSetting.MenuVoice);
            seeya = audio.Samples.Get(@"seeya");
        }

        /// <summary>
        /// Whether we have loaded the menu previously.
        /// </summary>
        public bool DidLoadMenu { get; private set; }

        public override bool OnExiting(IScreen next)
        {
            //cancel exiting if we haven't loaded the menu yet.
            return !DidLoadMenu;
        }

        public override void OnResuming(IScreen last)
        {
            this.FadeIn(300);

            double fadeOutTime = EXIT_DELAY;
            //we also handle the exit transition.
            if (menuVoice.Value)
                seeya.Play();
            else
                fadeOutTime = 500;

            audio.AddAdjustment(AdjustableProperty.Volume, exitingVolumeFade);
            this.TransformBindableTo(exitingVolumeFade, 0, fadeOutTime).OnComplete(_ => this.Exit());

            //don't want to fade out completely else we will stop running updates.
            Game.FadeTo(0.01f, fadeOutTime);

            base.OnResuming(last);
        }

        protected override void LogoArriving(OsuLogo logo, bool resuming)
        {
            base.LogoArriving(logo, resuming);

            logo.Colour = Color4.White;
            logo.Triangles = false;
            logo.Ripple = false;

            if (!resuming)
            {
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
                    .RotateTo(20, EXIT_DELAY * 1.5f)
                    .FadeOut(EXIT_DELAY);
            }
        }

        private MainMenu mainMenu;

        protected void PrepareMenuLoad()
        {
            LoadComponentAsync(mainMenu = new MainMenu());
        }

        protected void LoadMenu()
        {
            beatmap.Return();

            DidLoadMenu = true;
            this.Push(mainMenu);
        }
    }
}
