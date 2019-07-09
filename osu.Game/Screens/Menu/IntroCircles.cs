// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Audio.Sample;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Screens;
using osu.Framework.Graphics;
using osu.Framework.MathUtils;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.IO.Archives;

namespace osu.Game.Screens.Menu
{
    public class IntroCircles : IntroScreen
    {
        private const string menu_music_beatmap_hash = "3c8b1fcc9434dbb29e2fb613d3b9eada9d7bb6c125ceb32396c3b53437280c83";

        private SampleChannel welcome;

        private Bindable<bool> menuMusic;

        private Track track;

        private WorkingBeatmap introBeatmap;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config, BeatmapManager beatmaps, Framework.Game game, ISampleStore samples)
        {
            menuMusic = config.GetBindable<bool>(OsuSetting.MenuMusic);

            BeatmapSetInfo setInfo = null;

            if (!menuMusic.Value)
            {
                var sets = beatmaps.GetAllUsableBeatmapSets();
                if (sets.Count > 0)
                    setInfo = beatmaps.QueryBeatmapSet(s => s.ID == sets[RNG.Next(0, sets.Count - 1)].ID);
            }

            if (setInfo == null)
            {
                setInfo = beatmaps.QueryBeatmapSet(b => b.Hash == menu_music_beatmap_hash);

                if (setInfo == null)
                {
                    // we need to import the default menu background beatmap
                    setInfo = beatmaps.Import(new ZipArchiveReader(game.Resources.GetStream(@"Tracks/circles.osz"), "circles.osz")).Result;

                    setInfo.Protected = true;
                    beatmaps.Update(setInfo);
                }
            }

            introBeatmap = beatmaps.GetWorkingBeatmap(setInfo.Beatmaps[0]);
            track = introBeatmap.Track;

            if (config.Get<bool>(OsuSetting.MenuVoice))
                welcome = samples.Get(@"welcome");
        }

        private const double delay_step_one = 2300;
        private const double delay_step_two = 600;

        protected override void LogoArriving(OsuLogo logo, bool resuming)
        {
            base.LogoArriving(logo, resuming);

            if (!resuming)
            {
                Beatmap.Value = introBeatmap;
                introBeatmap = null;

                welcome?.Play();

                Scheduler.AddDelayed(delegate
                {
                    // Only start the current track if it is the menu music. A beatmap's track is started when entering the Main Manu.
                    if (menuMusic.Value)
                    {
                        track.Restart();
                        track = null;
                    }

                    PrepareMenuLoad();

                    Scheduler.AddDelayed(LoadMenu, delay_step_one);
                }, delay_step_two);

                logo.ScaleTo(1);
                logo.FadeIn();
                logo.PlayIntro();
            }
        }

        public override void OnSuspending(IScreen next)
        {
            track = null;

            this.FadeOut(300);
            base.OnSuspending(next);
        }
    }
}
