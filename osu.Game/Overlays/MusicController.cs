//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.Transformations;
using osu.Framework.Input;
using osu.Framework.MathUtils;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Framework.Graphics.Primitives;

namespace osu.Game.Overlays
{
    public class MusicController : OverlayContainer
    {
        private static readonly Vector2 start_position = new Vector2(0, 50);

        private MusicControllerBackground backgroundSprite;
        private DragBar progress;
        private TextAwesome playButton, listButton;
        private SpriteText title, artist;

        private List<BeatmapSetInfo> playList;
        private List<BeatmapInfo> playHistory = new List<BeatmapInfo>();
        private int playListIndex;
        private int playHistoryIndex = -1;

        private TrackManager trackManager;
        private Bindable<WorkingBeatmap> beatmapSource;
        private Bindable<bool> preferUnicode;
        private OsuConfigManager config;
        private WorkingBeatmap current;
        private BeatmapDatabase beatmaps;
        private BaseGame game;

        public MusicController()
        {
            Width = 400;
            Height = 130;
            CornerRadius = 5;
            EdgeEffect = new EdgeEffect
            {
                Type = EdgeEffectType.Shadow,
                Colour = Color4.Black.Opacity(40),
                Radius = 5,
            };

            Masking = true;
            Anchor = Anchor.TopRight;//placeholder
            Origin = Anchor.TopRight;
            Position = start_position;
            Margin = new MarginPadding(10);
        }

        protected override bool OnDragStart(InputState state) => true;

        protected override bool OnDrag(InputState state)
        {
            Vector2 change = (state.Mouse.Position - state.Mouse.PositionMouseDown.Value);

            // Diminish the drag distance as we go further to simulate "rubber band" feeling.
            change *= (float)Math.Pow(change.Length, 0.7f) / change.Length;

            MoveTo(start_position + change);
            return base.OnDrag(state);
        }

        protected override bool OnDragEnd(InputState state)
        {
            MoveTo(start_position, 800, EasingTypes.OutElastic);
            return base.OnDragEnd(state);
        }

        [BackgroundDependencyLoader]
        private void load(OsuGameBase osuGame, BeatmapDatabase beatmaps, AudioManager audio,
            TextureStore textures, OsuColour colours)
        {
            Children = new Drawable[]
            {
                title = new SpriteText
                {
                    Origin = Anchor.BottomCentre,
                    Anchor = Anchor.TopCentre,
                    Position = new Vector2(0, 40),
                    TextSize = 25,
                    Colour = Color4.White,
                    Text = @"Nothing to play",
                    Font = @"Exo2.0-MediumItalic"
                },
                artist = new SpriteText
                {
                    Origin = Anchor.TopCentre,
                    Anchor = Anchor.TopCentre,
                    Position = new Vector2(0, 45),
                    TextSize = 15,
                    Colour = Color4.White,
                    Text = @"Nothing to play",
                    Font = @"Exo2.0-BoldItalic"
                },
                new ClickableContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.BottomCentre,
                    Position = new Vector2(0, -30),
                    Action = () =>
                    {
                        if (current?.Track == null) return;
                        if (current.Track.IsRunning)
                            current.Track.Stop();
                        else
                            current.Track.Start();
                    },
                    Children = new Drawable[]
                    {
                        playButton = new TextAwesome
                        {
                            TextSize = 30,
                            Icon = FontAwesome.fa_play_circle_o,
                            Origin = Anchor.Centre,
                            Anchor = Anchor.Centre
                        }
                    }
                },
                new ClickableContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.BottomCentre,
                    Position = new Vector2(-30, -30),
                    Action = prev,
                    Children = new Drawable[]
                    {
                        new TextAwesome
                        {
                            TextSize = 15,
                            Icon = FontAwesome.fa_step_backward,
                            Origin = Anchor.Centre,
                            Anchor = Anchor.Centre
                        }
                    }
                },
                new ClickableContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.BottomCentre,
                    Position = new Vector2(30, -30),
                    Action = next,
                    Children = new Drawable[]
                    {
                        new TextAwesome
                        {
                            TextSize = 15,
                            Icon = FontAwesome.fa_step_forward,
                            Origin = Anchor.Centre,
                            Anchor = Anchor.Centre
                        }
                    }
                },
                new ClickableContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.BottomRight,
                    Position = new Vector2(20, -30),
                    Children = new Drawable[]
                    {
                        listButton = new TextAwesome
                        {
                            TextSize = 15,
                            Icon = FontAwesome.fa_bars,
                            Origin = Anchor.Centre,
                            Anchor = Anchor.Centre
                        }
                    }
                },
                progress = new DragBar
                {
                    Origin = Anchor.BottomCentre,
                    Anchor = Anchor.BottomCentre,
                    Height = 10,
                    Colour = colours.Yellow,
                    SeekRequested = seek
                }
            };
        
            this.beatmaps = beatmaps;
            trackManager = osuGame.Audio.Track;
            config = osuGame.Config;
            preferUnicode = osuGame.Config.GetBindable<bool>(OsuConfig.ShowUnicode);
            preferUnicode.ValueChanged += preferUnicode_changed;

            beatmapSource = osuGame.Beatmap ?? new Bindable<WorkingBeatmap>();
            playList = beatmaps.GetAllWithChildren<BeatmapSetInfo>();

            backgroundSprite = new MusicControllerBackground();
            AddInternal(backgroundSprite);
        }

        protected override void LoadComplete()
        {
            beatmapSource.ValueChanged += workingChanged;
            workingChanged();
            base.LoadComplete();
        }

        protected override void Update()
        {
            base.Update();

            if (current?.TrackLoaded ?? false)
            {

                progress.UpdatePosition((float)(current.Track.CurrentTime / current.Track.Length));
                playButton.Icon = current.Track.IsRunning ? FontAwesome.fa_pause_circle_o : FontAwesome.fa_play_circle_o;

                if (current.Track.HasCompleted && !current.Track.Looping) next();
            }
        }

        void preferUnicode_changed(object sender, EventArgs e)
        {
            updateDisplay(current, TransformDirection.None);
        }

        private void workingChanged(object sender = null, EventArgs e = null)
        {
            progress.IsEnabled = (beatmapSource.Value != null);
            if (beatmapSource.Value == current) return;
            bool audioEquals = current?.BeatmapInfo.AudioEquals(beatmapSource.Value.BeatmapInfo) ?? false;
            current = beatmapSource.Value;
            updateDisplay(current, audioEquals ? TransformDirection.None : TransformDirection.Next);
            appendToHistory(current.BeatmapInfo);
        }

        private void appendToHistory(BeatmapInfo beatmap)
        {
            if (playHistoryIndex >= 0)
            {
                if (beatmap.AudioEquals(playHistory[playHistoryIndex]))
                    return;
                if (playHistoryIndex < playHistory.Count - 1)
                    playHistory.RemoveRange(playHistoryIndex + 1, playHistory.Count - playHistoryIndex - 1);
            }
            playHistory.Insert(++playHistoryIndex, beatmap);
        }

        private void prev()
        {
            if (playHistoryIndex > 0)
                play(playHistory[--playHistoryIndex], false);
        }

        private void next()
        {
            if (playHistoryIndex < playHistory.Count - 1)
                play(playHistory[++playHistoryIndex], true);
            else
            {
                if (playList.Count == 0) return;
                if (current != null && playList.Count == 1) return;
                //shuffle
                BeatmapInfo nextToPlay;
                do
                {
                    int j = RNG.Next(playListIndex, playList.Count);
                    if (j != playListIndex)
                    {
                        BeatmapSetInfo temp = playList[playListIndex];
                        playList[playListIndex] = playList[j];
                        playList[j] = temp;
                    }

                    nextToPlay = playList[playListIndex++].Beatmaps[0];
                    if (playListIndex == playList.Count) playListIndex = 0;
                }
                while (nextToPlay.AudioEquals(current?.BeatmapInfo));

                play(nextToPlay, true);
                appendToHistory(nextToPlay);
            }
        }

        private void play(BeatmapInfo info, bool isNext)
        {
            current = beatmaps.GetWorkingBeatmap(info, current);
            Task.Run(() =>
            {
                trackManager.SetExclusive(current.Track);
                current.Track.Start();
                beatmapSource.Value = current;
            });
            updateDisplay(current, isNext ? TransformDirection.Next : TransformDirection.Prev);
        }

        protected override void PerformLoad(BaseGame game)
        {
            this.game = game;
            base.PerformLoad(game);
        }

        private void updateDisplay(WorkingBeatmap beatmap, TransformDirection direction)
        {
            Task.Run(() =>
            {
                if (beatmap?.Beatmap == null)
                    //todo: we may need to display some default text here (currently in the constructor).
                    return;

                BeatmapMetadata metadata = beatmap.Beatmap.BeatmapInfo.Metadata;
                title.Text = config.GetUnicodeString(metadata.Title, metadata.TitleUnicode);
                artist.Text = config.GetUnicodeString(metadata.Artist, metadata.ArtistUnicode);
            });

            MusicControllerBackground newBackground;

            (newBackground = new MusicControllerBackground(beatmap)).Preload(game, delegate
            {

                Add(newBackground);

                switch (direction)
                {
                    case TransformDirection.Next:
                        newBackground.Position = new Vector2(400, 0);
                        newBackground.MoveToX(0, 500, EasingTypes.OutCubic);
                        backgroundSprite.MoveToX(-400, 500, EasingTypes.OutCubic);
                        break;
                    case TransformDirection.Prev:
                        newBackground.Position = new Vector2(-400, 0);
                        newBackground.MoveToX(0, 500, EasingTypes.OutCubic);
                        backgroundSprite.MoveToX(400, 500, EasingTypes.OutCubic);
                        break;
                }

                backgroundSprite.Expire();
                backgroundSprite = newBackground;
            });
        }

        private void seek(float position)
        {
            current?.Track?.Seek(current.Track.Length * position);
            current?.Track?.Start();
        }

        protected override void Dispose(bool isDisposing)
        {
            if (preferUnicode != null)
                preferUnicode.ValueChanged -= preferUnicode_changed;
            base.Dispose(isDisposing);
        }

        protected override bool OnClick(InputState state) => true;

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args) => true;

        //placeholder for toggling
        protected override void PopIn() => FadeIn(100);

        protected override void PopOut() => FadeOut(100);

        private enum TransformDirection { None, Next, Prev }

        private class MusicControllerBackground : BufferedContainer
        {
            private Sprite sprite;
            private WorkingBeatmap beatmap;

            public MusicControllerBackground(WorkingBeatmap beatmap = null)
            {
                this.beatmap = beatmap;
                CacheDrawnFrameBuffer = true;
                RelativeSizeAxes = Axes.Both;
                Depth = float.MaxValue;

                Children = new Drawable[]
                {
                    sprite = new Sprite
                    {
                        Colour = OsuColour.Gray(150),
                        FillMode = FillMode.Fill,
                    },
                    new Box
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 50,
                        Origin = Anchor.BottomCentre,
                        Anchor = Anchor.BottomCentre,
                        Colour = Color4.Black.Opacity(0.5f)
                    }
                };
            }

            [BackgroundDependencyLoader]
            private void load(TextureStore textures)
            {
                sprite.Texture = beatmap?.Background ?? textures.Get(@"Backgrounds/bg4");
            }
        }
    }
}
