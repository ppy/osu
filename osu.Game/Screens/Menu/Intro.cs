﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Audio.Track;
using osu.Framework.Configuration;
using osu.Framework.Screens;
using osu.Framework.Graphics;
using osu.Framework.MathUtils;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.IO;
using osu.Game.Configuration;
using osu.Game.Screens.Backgrounds;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Screens.Menu
{
    public class Intro : OsuScreen
    {
        private const string menu_music_beatmap_hash = "3c8b1fcc9434dbb29e2fb613d3b9eada9d7bb6c125ceb32396c3b53437280c83";

        /// <summary>
        /// Whether we have loaded the menu previously.
        /// </summary>
        public bool DidLoadMenu;

        private MainMenu mainMenu;
        private SampleChannel welcome;
        private SampleChannel seeya;

        public override bool HasLocalCursorDisplayed => true;

        public override bool ShowOverlays => false;

        protected override BackgroundScreen CreateBackground() => new BackgroundScreenEmpty();

        private Bindable<bool> menuVoice;
        private Bindable<bool> menuMusic;
        private Track track;

        [BackgroundDependencyLoader]
        private void load(AudioManager audio, OsuConfigManager config, BeatmapManager beatmaps, Framework.Game game)
        {
            menuVoice = config.GetBindable<bool>(OsuSetting.MenuVoice);
            menuMusic = config.GetBindable<bool>(OsuSetting.MenuMusic);

            BeatmapSetInfo setInfo = null;

            if (!menuMusic)
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
                    setInfo = beatmaps.Import(new OszArchiveReader(game.Resources.GetStream(@"Tracks/circles.osz")));
                    setInfo.Protected = true;
                }
            }

            Beatmap.Value = beatmaps.GetWorkingBeatmap(setInfo.Beatmaps[0]);

            track = Beatmap.Value.Track;

            welcome = audio.Sample.Get(@"welcome");
            seeya = audio.Sample.Get(@"seeya");

            if (setInfo.Protected)
                beatmaps.Delete(setInfo);
        }

        protected override void OnEntering(Screen last)
        {
            base.OnEntering(last);

            if (menuVoice)
                welcome.Play();

            Scheduler.AddDelayed(delegate
            {
                // Only start the current track if it is the menu music. A beatmap's track is started when entering the Main Manu.
                if (menuMusic)
                    track.Start();

                LoadComponentAsync(mainMenu = new MainMenu());

                Scheduler.AddDelayed(delegate
                {
                    DidLoadMenu = true;
                    Push(mainMenu);
                }, delay_step_one);
            }, delay_step_two);
        }

        private const double delay_step_one = 2300;
        private const double delay_step_two = 600;

        public const int EXIT_DELAY = 3000;

        protected override void LogoArriving(OsuLogo logo, bool resuming)
        {
            base.LogoArriving(logo, resuming);

            logo.RelativePositionAxes = Axes.Both;
            logo.Colour = Color4.White;
            logo.Ripple = false;

            const int quick_appear = 350;

            int initialMovementTime = logo.Alpha > 0.2f ? quick_appear : 0;

            logo.MoveTo(new Vector2(0.5f), initialMovementTime, Easing.OutQuint);

            if (!resuming)
            {
                logo.ScaleTo(1);
                logo.FadeIn();
                logo.PlayIntro();
            }
            else
            {
                logo.Triangles = false;

                logo
                    .ScaleTo(1, initialMovementTime, Easing.OutQuint)
                    .FadeIn(quick_appear, Easing.OutQuint)
                    .Then()
                    .RotateTo(20, EXIT_DELAY * 1.5f)
                    .FadeOut(EXIT_DELAY);
            }
        }

        protected override void OnSuspending(Screen next)
        {
            Content.FadeOut(300);
            base.OnSuspending(next);
        }

        protected override bool OnExiting(Screen next)
        {
            //cancel exiting if we haven't loaded the menu yet.
            return !DidLoadMenu;
        }

        protected override void OnResuming(Screen last)
        {
            if (!(last is MainMenu))
                Content.FadeIn(300);

            double fadeOutTime = EXIT_DELAY;
            //we also handle the exit transition.
            if (menuVoice)
                seeya.Play();
            else
                fadeOutTime = 500;

            Scheduler.AddDelayed(Exit, fadeOutTime);

            //don't want to fade out completely else we will stop running updates and shit will hit the fan.
            Game.FadeTo(0.01f, fadeOutTime);

            base.OnResuming(last);
        }
    }
}
