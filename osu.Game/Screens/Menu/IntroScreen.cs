// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.MathUtils;
using osu.Framework.Screens;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.IO.Archives;
using osu.Game.Screens.Backgrounds;
using osu.Game.Skinning;
using osu.Game.Online.API;
using osu.Game.Users;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Menu
{
    public abstract class IntroScreen : StartupScreen
    {
        protected abstract string BeatmapHash { get; }

        protected abstract string BeatmapFile { get; }

        private readonly BindableDouble exitingVolumeFade = new BindableDouble(1);

        public const int EXIT_DELAY = 3000;

        [Resolved]
        private AudioManager audio { get; set; }

        protected SampleChannel Welcome;

        private SampleChannel seeya;

        protected Bindable<bool> MenuVoice;

        protected Bindable<bool> MenuMusic;

        protected Track Track;

        protected WorkingBeatmap IntroBeatmap;

        private LeasedBindable<WorkingBeatmap> beatmap;

        public new Bindable<WorkingBeatmap> Beatmap => beatmap;

        protected Bindable<User> User;

        protected Bindable<Skin> Skin;

        protected override BackgroundScreen CreateBackground() => new BackgroundScreenBlack();

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config, IAPIProvider api, SkinManager skinManager, BeatmapManager beatmaps, Framework.Game game)
        {
            // prevent user from changing beatmap while the intro is still runnning.
            beatmap = base.Beatmap.BeginLease(false);

            MenuVoice = config.GetBindable<bool>(OsuSetting.MenuVoice);
            MenuMusic = config.GetBindable<bool>(OsuSetting.MenuMusic);

            User = api.LocalUser.GetBoundCopy();
            Skin = skinManager.CurrentSkin.GetBoundCopy();

            Skin.BindValueChanged(_ => updateSeeya(), true);

            BeatmapSetInfo setInfo = null;

            if (!MenuMusic.Value)
            {
                var sets = beatmaps.GetAllUsableBeatmapSets();
                if (sets.Count > 0)
                    setInfo = beatmaps.QueryBeatmapSet(s => s.ID == sets[RNG.Next(0, sets.Count - 1)].ID);
            }

            if (setInfo == null)
            {
                setInfo = beatmaps.QueryBeatmapSet(b => b.Hash == BeatmapHash);

                if (setInfo == null)
                {
                    // we need to import the default menu background beatmap
                    setInfo = beatmaps.Import(new ZipArchiveReader(game.Resources.GetStream($"Tracks/{BeatmapFile}"), BeatmapFile)).Result;

                    setInfo.Protected = true;
                    beatmaps.Update(setInfo);
                }
            }

            IntroBeatmap = beatmaps.GetWorkingBeatmap(setInfo.Beatmaps[0]);
            Track = IntroBeatmap.Track;
        }

        private void updateSeeya()
        {
            if (User.Value?.IsSupporter ?? false)
                seeya = Skin.Value.GetSample(new SampleInfo("seeya")) ?? audio.Samples.Get(@"seeya");
            else
                seeya = audio.Samples.Get(@"seeya");
        }

        protected void SetWelcome()
        {
            if (User.Value?.IsSupporter ?? false)
                Welcome = Skin.Value.GetSample(new SampleInfo("welcome")) ?? audio.Samples.Get(@"welcome");
            else
                Welcome = audio.Samples.Get(@"welcome");
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
            if (MenuVoice.Value)
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
