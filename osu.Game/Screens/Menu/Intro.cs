// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
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
using osu.Game.Graphics.Containers;
using osu.Game.Screens.Backgrounds;
using OpenTK.Graphics;

namespace osu.Game.Screens.Menu
{
    public class Intro : OsuScreen
    {
        private readonly OsuLogo logo;

        private const string menu_music_beatmap_hash = "715a09144f885d746644c1983e285044";

        /// <summary>
        /// Whether we have loaded the menu previously.
        /// </summary>
        internal bool DidLoadMenu;

        private MainMenu mainMenu;
        private SampleChannel welcome;
        private SampleChannel seeya;

        internal override bool HasLocalCursorDisplayed => true;

        internal override bool ShowOverlays => false;

        protected override BackgroundScreen CreateBackground() => new BackgroundScreenEmpty();

        public Intro()
        {
            Children = new Drawable[]
            {
                new ParallaxContainer
                {
                    ParallaxAmount = 0.01f,
                    Children = new Drawable[]
                    {
                        logo = new OsuLogo
                        {
                            Alpha = 0,
                            Triangles = false,
                            Blending = BlendingMode.Additive,
                            Interactive = false,
                            Colour = Color4.DarkGray,
                            Ripple = false
                        }
                    }
                }
            };
        }

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
                var sets = beatmaps.GetAllUsableBeatmapSets(false);
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
                    beatmaps.Delete(setInfo);
                }
            }

            Beatmap.Value = beatmaps.GetWorkingBeatmap(setInfo.Beatmaps[0]);

            track = Beatmap.Value.Track;

            welcome = audio.Sample.Get(@"welcome");
            seeya = audio.Sample.Get(@"seeya");
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
                }, 2300);
            }, 600);

            logo.ScaleTo(0.4f);
            logo.FadeOut();

            logo.ScaleTo(1, 4400, Easing.OutQuint);
            logo.FadeIn(20000, Easing.OutQuint);
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

            double fadeOutTime = 2000;
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
