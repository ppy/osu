// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Screens;
using osu.Framework.Graphics;
using osu.Framework.Allocation;
using osu.Game.Configuration;
using osu.Framework.Bindables;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Screens.Backgrounds;
using osuTK.Graphics;

#nullable disable

namespace osu.Game.Screens.Menu
{
    public partial class IntroSkipped : IntroScreen
    {
        protected override string BeatmapHash => "3c8b1fcc9434dbb29e2fb613d3b9eada9d7bb6c125ceb32396c3b53437280c83";

        protected override string BeatmapFile => "circles.osz";

        private Color4 backgroundColor;

        protected override BackgroundScreen CreateBackground() => new BackgroundScreenPureColor(backgroundColor);

        protected IBindable<bool> LoadDirectToSongSelect { get; private set; }

        [Resolved(CanBeNull = true)]
        private OsuGame game { get; set; }

        public IntroSkipped(Func<MainMenu> createScreen)
            : base(createScreen)
        {
        }

        [BackgroundDependencyLoader]
        private void load(MConfigManager config)
        {
            LoadDirectToSongSelect = config.GetBindable<bool>(MSetting.IntroLoadDirectToSongSelect);
            backgroundColor = config.GetCustomLoaderColor();
        }

        protected override void LogoArriving(OsuLogo logo, bool resuming)
        {
            base.LogoArriving(logo, resuming);

            if (!resuming)
            {
                Scheduler.AddDelayed(delegate
                {
                    StartTrack();

                    PrepareMenuLoad();

                    Scheduler.AddDelayed(LoadMenu, 0);
                }, 0);

                logo.ScaleTo(0).FadeOut();

                if (!LoadDirectToSongSelect.Value)
                {
                    logo.ScaleTo(1, 300, Easing.OutQuint);
                    logo.FadeIn(300);
                }
            }
        }

        public override void OnSuspending(ScreenTransitionEvent e)
        {
            this.FadeOut(300);
            base.OnSuspending(e);
        }

        [Resolved]
        private BeatmapManager beatmapManager { get; set; }

        protected override void LoadMenu()
        {
            base.LoadMenu();

            if (LoadDirectToSongSelect.Value)
            {
                var beatmapSets = beatmapManager.GetAllUsableBeatmapSets();
                game?.PresentBeatmap(beatmapSets[RNG.Next(beatmapSets.Count)]);
            }
        }
    }
}
