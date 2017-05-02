// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.MathUtils;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Database;
using osu.Game.Graphics.Containers;
using osu.Game.Screens.Backgrounds;
using osu.Game.Screens.Charts;
using osu.Game.Screens.Direct;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Multiplayer;
using OpenTK;
using osu.Game.Screens.Select;
using osu.Game.Screens.Tournament;
using osu.Framework.Input;
using OpenTK.Input;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Screens.Menu
{
    public class MainMenu : OsuScreen
    {
        private readonly ButtonSystem buttons;

        internal override bool ShowOverlays => buttons.State != MenuState.Initial;

        private readonly BackgroundScreen background;
        private Screen songSelect;

        protected override BackgroundScreen CreateBackground() => background;

        public MainMenu()
        {
            background = new BackgroundScreenDefault();

            Children = new Drawable[]
            {
                new ParallaxContainer
                {
                    ParallaxAmount = 0.01f,
                    Children = new Drawable[]
                    {
                        buttons = new ButtonSystem
                        {
                            OnChart = delegate { Push(new ChartListing()); },
                            OnDirect = delegate { Push(new OnlineListing()); },
                            OnEdit = delegate { Push(new Editor()); },
                            OnSolo = delegate { Push(consumeSongSelect()); },
                            OnMulti = delegate { Push(new Lobby()); },
                            OnExit = delegate { Exit(); },
                        }
                    }
                }
            };
        }

        private Bindable<bool> menuMusic;
        private TrackManager trackManager;
        private WorkingBeatmap song;

        [BackgroundDependencyLoader]
        private void load(OsuGame game, OsuConfigManager config, BeatmapDatabase beatmaps)
        {
            menuMusic = config.GetBindable<bool>(OsuConfig.MenuMusic);
            LoadComponentAsync(background);

            if (!menuMusic)
            {
                trackManager = game.Audio.Track;
                List<BeatmapSetInfo> choosableBeatmapSets = beatmaps.Query<BeatmapSetInfo>().ToList();
                if (choosableBeatmapSets.Count > 0)
                {
                    song = beatmaps.GetWorkingBeatmap(beatmaps.GetWithChildren<BeatmapSetInfo>(choosableBeatmapSets[RNG.Next(0, choosableBeatmapSets.Count - 1)].ID).Beatmaps[0]);
                    Beatmap = song;
                }
            }

            buttons.OnSettings = game.ToggleOptions;

            preloadSongSelect();
        }

        private void preloadSongSelect()
        {
            if (songSelect == null)
                LoadComponentAsync(songSelect = new PlaySongSelect());
        }

        private Screen consumeSongSelect()
        {
            var s = songSelect;
            songSelect = null;
            return s;
        }

        protected override void OnEntering(Screen last)
        {
            base.OnEntering(last);
            buttons.FadeInFromZero(500);
            if (last is Intro && song != null)
            {
                Task.Run(() =>
                {
                    trackManager.SetExclusive(song.Track);
                    song.Track.Seek(song.Beatmap.Metadata.PreviewTime);
                    if (song.Beatmap.Metadata.PreviewTime == -1)
                        song.Track.Seek(song.Track.Length * 0.4f);
                    song.Track.Start();
                });
            }
        }

        protected override void OnSuspending(Screen next)
        {
            base.OnSuspending(next);

            const float length = 400;

            buttons.State = MenuState.EnteringMode;

            Content.FadeOut(length, EasingTypes.InSine);
            Content.MoveTo(new Vector2(-800, 0), length, EasingTypes.InSine);
        }

        protected override void OnResuming(Screen last)
        {
            base.OnResuming(last);

            //we may have consumed our preloaded instance, so let's make another.
            preloadSongSelect();

            const float length = 300;

            buttons.State = MenuState.TopLevel;

            Content.FadeIn(length, EasingTypes.OutQuint);
            Content.MoveTo(new Vector2(0, 0), length, EasingTypes.OutQuint);
        }

        protected override bool OnExiting(Screen next)
        {
            buttons.State = MenuState.Exit;
            Content.FadeOut(3000);
            return base.OnExiting(next);
        }

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            if (!args.Repeat && state.Keyboard.ControlPressed && state.Keyboard.ShiftPressed && args.Key == Key.D)
            {
                Push(new Drawings());
                return true;
            }

            return base.OnKeyDown(state, args);
        }
    }
}
