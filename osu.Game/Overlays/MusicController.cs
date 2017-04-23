// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input;
using osu.Framework.MathUtils;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Game.Graphics.Sprites;
using osu.Framework.Extensions;
using osu.Framework.Extensions.Color4Extensions;

namespace osu.Game.Overlays
{
    public class MusicController : FocusedOverlayContainer
    {
        private Drawable currentBackground;
        private DragBar progress;
        private Button playButton;
        private SpriteText title, artist;

        private List<BeatmapSetInfo> playList;
        private readonly List<BeatmapInfo> playHistory = new List<BeatmapInfo>();
        private int playListIndex;
        private int playHistoryIndex = -1;

        private TrackManager trackManager;
        private Bindable<WorkingBeatmap> beatmapSource;
        private Bindable<bool> preferUnicode;
        private WorkingBeatmap current;
        private BeatmapDatabase beatmaps;

        private Container dragContainer;

        private const float progress_height = 10;

        private const float bottom_black_area_height = 55;

        public MusicController()
        {
            Width = 400;
            Height = 130;

            Margin = new MarginPadding(10);
        }

        protected override bool OnDragStart(InputState state) => true;

        protected override bool OnDrag(InputState state)
        {
            Trace.Assert(state.Mouse.PositionMouseDown != null, "state.Mouse.PositionMouseDown != null");

            Vector2 change = state.Mouse.Position - state.Mouse.PositionMouseDown.Value;

            // Diminish the drag distance as we go further to simulate "rubber band" feeling.
            change *= change.Length <= 0 ? 0 : (float)Math.Pow(change.Length, 0.7f) / change.Length;

            dragContainer.MoveTo(change);
            return base.OnDrag(state);
        }

        protected override bool OnDragEnd(InputState state)
        {
            dragContainer.MoveTo(Vector2.Zero, 800, EasingTypes.OutElastic);
            return base.OnDragEnd(state);
        }

        [BackgroundDependencyLoader]
        private void load(OsuGameBase game, OsuConfigManager config, BeatmapDatabase beatmaps, OsuColour colours)
        {
            Children = new Drawable[]
            {
                dragContainer = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Masking = true,
                    CornerRadius = 5,
                    EdgeEffect = new EdgeEffect
                    {
                        Type = EdgeEffectType.Shadow,
                        Colour = Color4.Black.Opacity(40),
                        Radius = 5,
                    },
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        title = new OsuSpriteText
                        {
                            Origin = Anchor.BottomCentre,
                            Anchor = Anchor.TopCentre,
                            Position = new Vector2(0, 40),
                            TextSize = 25,
                            Colour = Color4.White,
                            Text = @"Nothing to play",
                            Font = @"Exo2.0-MediumItalic"
                        },
                        artist = new OsuSpriteText
                        {
                            Origin = Anchor.TopCentre,
                            Anchor = Anchor.TopCentre,
                            Position = new Vector2(0, 45),
                            TextSize = 15,
                            Colour = Color4.White,
                            Text = @"Nothing to play",
                            Font = @"Exo2.0-BoldItalic"
                        },
                        new Container
                        {
                            Padding = new MarginPadding { Bottom = progress_height },
                            Height = bottom_black_area_height,
                            RelativeSizeAxes = Axes.X,
                            Origin = Anchor.BottomCentre,
                            Anchor = Anchor.BottomCentre,
                            Children = new Drawable[]
                            {
                                new FillFlowContainer<Button>
                                {
                                    AutoSizeAxes = Axes.Both,
                                    Direction = FillDirection.Horizontal,
                                    Spacing = new Vector2(5),
                                    Origin = Anchor.Centre,
                                    Anchor = Anchor.Centre,
                                    Children = new[]
                                    {
                                        new Button
                                        {
                                            Action = prev,
                                            Icon = FontAwesome.fa_step_backward,
                                        },
                                        playButton = new Button
                                        {
                                            Scale = new Vector2(1.4f),
                                            IconScale = new Vector2(1.4f),
                                            Action = () =>
                                            {
                                                if (current?.Track == null) return;
                                                if (current.Track.IsRunning)
                                                    current.Track.Stop();
                                                else
                                                    current.Track.Start();
                                            },
                                            Icon = FontAwesome.fa_play_circle_o,
                                        },
                                        new Button
                                        {
                                            Action = next,
                                            Icon = FontAwesome.fa_step_forward,
                                        },
                                    }
                                },
                                new Button
                                {
                                    Origin = Anchor.Centre,
                                    Anchor = Anchor.CentreRight,
                                    Position = new Vector2(-bottom_black_area_height / 2, 0),
                                    Icon = FontAwesome.fa_bars,
                                },
                            }
                        },
                        progress = new DragBar
                        {
                            Origin = Anchor.BottomCentre,
                            Anchor = Anchor.BottomCentre,
                            Height = progress_height,
                            Colour = colours.Yellow,
                            SeekRequested = seek
                        }
                    }
                }
            };

            this.beatmaps = beatmaps;
            trackManager = game.Audio.Track;
            preferUnicode = config.GetBindable<bool>(OsuConfig.ShowUnicode);
            preferUnicode.ValueChanged += unicode => updateDisplay(current, TransformDirection.None);

            beatmapSource = game.Beatmap ?? new Bindable<WorkingBeatmap>();
            playList = beatmaps.GetAllWithChildren<BeatmapSetInfo>();

            currentBackground = new MusicControllerBackground();
            dragContainer.Add(currentBackground);
        }

        protected override void LoadComplete()
        {
            beatmapSource.ValueChanged += workingChanged;
            beatmapSource.TriggerChange();

            base.LoadComplete();
        }

        protected override void Update()
        {
            base.Update();

            if (pendingBeatmapSwitch != null)
            {
                pendingBeatmapSwitch();
                pendingBeatmapSwitch = null;
            }

            if (current?.TrackLoaded ?? false)
            {
                progress.UpdatePosition(current.Track.Length == 0 ? 0 : (float)(current.Track.CurrentTime / current.Track.Length));
                playButton.Icon = current.Track.IsRunning ? FontAwesome.fa_pause_circle_o : FontAwesome.fa_play_circle_o;

                if (current.Track.HasCompleted && !current.Track.Looping) next();
            }
            else
                playButton.Icon = FontAwesome.fa_play_circle_o;
        }

        private void workingChanged(WorkingBeatmap beatmap)
        {
            progress.IsEnabled = beatmap != null;
            if (beatmap == current) return;
            bool audioEquals = current?.BeatmapInfo?.AudioEquals(beatmap?.BeatmapInfo) ?? false;
            current = beatmap;
            updateDisplay(current, audioEquals ? TransformDirection.None : TransformDirection.Next);
            appendToHistory(current?.BeatmapInfo);
        }

        private void appendToHistory(BeatmapInfo beatmap)
        {
            if (beatmap == null) return;

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
            }).ContinueWith(task => Schedule(() => task.ThrowIfFaulted()), TaskContinuationOptions.OnlyOnFaulted);
            updateDisplay(current, isNext ? TransformDirection.Next : TransformDirection.Prev);
        }

        private Action pendingBeatmapSwitch;

        private void updateDisplay(WorkingBeatmap beatmap, TransformDirection direction)
        {
            //we might be off-screen when this update comes in.
            //rather than Scheduling, manually handle this to avoid possible memory contention.
            pendingBeatmapSwitch = () =>
            {
                Task.Run(() =>
                {
                    if (beatmap?.Beatmap == null)
                    {
                        title.Text = @"Nothing to play";
                        artist.Text = @"Nothing to play";
                    }
                    else
                    {
                        BeatmapMetadata metadata = beatmap.Beatmap.BeatmapInfo.Metadata;
                        title.Text = preferUnicode ? metadata.TitleUnicode : metadata.Title;
                        artist.Text = preferUnicode ? metadata.ArtistUnicode : metadata.Artist;
                    }
                });

                dragContainer.Add(new AsyncLoadWrapper(new MusicControllerBackground(beatmap)
                {
                    OnLoadComplete = d =>
                    {
                        switch (direction)
                        {
                            case TransformDirection.Next:
                                d.Position = new Vector2(400, 0);
                                d.MoveToX(0, 500, EasingTypes.OutCubic);
                                currentBackground.MoveToX(-400, 500, EasingTypes.OutCubic);
                                break;
                            case TransformDirection.Prev:
                                d.Position = new Vector2(-400, 0);
                                d.MoveToX(0, 500, EasingTypes.OutCubic);
                                currentBackground.MoveToX(400, 500, EasingTypes.OutCubic);
                                break;
                        }
                        currentBackground.Expire();
                        currentBackground = d;
                    }
                })
                {
                    Depth = float.MaxValue,
                });
            };
        }

        private void seek(float position)
        {
            current?.Track?.Seek(current.Track.Length * position);
            current?.Track?.Start();
        }

        private const float transition_length = 800;

        protected override void PopIn()
        {
            base.PopIn();

            FadeIn(transition_length, EasingTypes.OutQuint);
            dragContainer.ScaleTo(1, transition_length, EasingTypes.OutElastic);
        }

        protected override void PopOut()
        {
            base.PopOut();

            FadeOut(transition_length, EasingTypes.OutQuint);
            dragContainer.ScaleTo(0.9f, transition_length, EasingTypes.OutQuint);
        }

        private enum TransformDirection { None, Next, Prev }

        private class MusicControllerBackground : BufferedContainer
        {
            private readonly Sprite sprite;
            private readonly WorkingBeatmap beatmap;

            public MusicControllerBackground(WorkingBeatmap beatmap = null)
            {
                this.beatmap = beatmap;
                CacheDrawnFrameBuffer = true;
                Depth = float.MaxValue;
                RelativeSizeAxes = Axes.Both;

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
                        Height = bottom_black_area_height,
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

        private class Button : ClickableContainer
        {
            private readonly TextAwesome icon;
            private readonly Box hover;
            private readonly Container content;

            public FontAwesome Icon
            {
                get { return icon.Icon; }
                set { icon.Icon = value; }
            }

            private const float button_size = 30;
            private Color4 flashColour;

            public Vector2 IconScale
            {
                get { return icon.Scale; }
                set { icon.Scale = value; }
            }

            public Button()
            {
                AutoSizeAxes = Axes.Both;

                Origin = Anchor.Centre;
                Anchor = Anchor.Centre;

                Children = new Drawable[]
                {
                    content = new Container
                    {
                        Size = new Vector2(button_size),
                        CornerRadius = 5,
                        Masking = true,

                        Origin = Anchor.Centre,
                        Anchor = Anchor.Centre,
                        EdgeEffect = new EdgeEffect
                        {
                            Colour = Color4.Black.Opacity(0.04f),
                            Type = EdgeEffectType.Shadow,
                            Radius = 5,
                        },
                        Children = new Drawable[]
                        {
                            hover = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Alpha = 0,
                            },
                            icon = new TextAwesome
                            {
                                TextSize = 18,
                                Origin = Anchor.Centre,
                                Anchor = Anchor.Centre
                            }
                        }
                    }
                };
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                hover.Colour = colours.Yellow.Opacity(0.6f);
                flashColour = colours.Yellow;
            }

            protected override bool OnHover(InputState state)
            {
                hover.FadeIn(500, EasingTypes.OutQuint);
                return base.OnHover(state);
            }

            protected override void OnHoverLost(InputState state)
            {
                hover.FadeOut(500, EasingTypes.OutQuint);
                base.OnHoverLost(state);
            }

            protected override bool OnClick(InputState state)
            {
                hover.FlashColour(flashColour, 800, EasingTypes.OutQuint);
                return base.OnClick(state);
            }

            protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
            {
                content.ScaleTo(0.75f, 2000, EasingTypes.OutQuint);
                return base.OnMouseDown(state, args);
            }

            protected override bool OnMouseUp(InputState state, MouseUpEventArgs args)
            {
                content.ScaleTo(1, 1000, EasingTypes.OutElastic);
                return base.OnMouseUp(state, args);
            }
        }
    }
}
