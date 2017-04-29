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
using osu.Framework.Extensions;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using System.Linq;

namespace osu.Game.Overlays
{
    public class MusicController : FocusedOverlayContainer
    {
        private const float player_height = 130;
        private const float playlist_height = 510;
        private Drawable currentBackground;
        private DragBar progress;
        private Button playButton;
        private SpriteText title, artist;
        private ClickableContainer playlistButton;
        private PlaylistController playlist;
        private Color4 activeColour;

        private List<BeatmapSetInfo> playList;
        private int playListIndex = -1;

        private TrackManager trackManager;
        private Bindable<WorkingBeatmap> beatmapSource;
        private WorkingBeatmap current;
        private BeatmapDatabase beatmaps;
        private LocalisationEngine localisation;

        private Container dragContainer;
        private Container playerContainer;

        private const float progress_height = 10;

        private const float bottom_black_area_height = 55;

        public MusicController()
        {
            Width = 400;
            Height = player_height + playlist_height;

            Margin = new MarginPadding(10);
        }

        protected override bool InternalContains(Vector2 screenSpacePos) => playlist.State == Visibility.Visible ? dragContainer.Contains(screenSpacePos) : playerContainer.Contains(screenSpacePos);

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
        private void load(OsuGameBase game, BeatmapDatabase beatmaps, OsuColour colours, LocalisationEngine localisation)
        {
            activeColour = colours.Yellow;

            Children = new Drawable[]
            {
                dragContainer = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        playlist = new PlaylistController
                        {
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding { Top = player_height + 10 },
                            //todo: this is the logic I expect, but maybe not others
                            OnSelect = (set, index) =>
                            {
                                if (set.ID == (current?.BeatmapSetInfo?.ID ?? -1))
                                    current?.Track?.Seek(0);

                                playListIndex = index;
                                play(set.Beatmaps[0], true);
                            },
                        },
                        playerContainer = new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = player_height,
                            Masking = true,
                            CornerRadius = 5,
                            EdgeEffect = new EdgeEffect
                            {
                                Type = EdgeEffectType.Shadow,
                                Colour = Color4.Black.Opacity(40),
                                Radius = 5,
                            },
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
                                                        if (current?.Track == null)
                                                        {
                                                            if (playList.Count > 0)
                                                                play(playList.First().Beatmaps[0], true);
                                                            else
                                                                return;
                                                        }
                                                        if (current?.Track?.IsRunning ?? false)
                                                            current?.Track?.Stop();
                                                        else
                                                            current?.Track?.Start();
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
                                        playlistButton = new Button
                                        {
                                            Origin = Anchor.Centre,
                                            Anchor = Anchor.CentreRight,
                                            Position = new Vector2(-bottom_black_area_height / 2, 0),
                                            Icon = FontAwesome.fa_bars,
                                            Action = () => playlist.ToggleVisibility(),
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
                            },
                        },
                    }
                }
            };

            this.beatmaps = beatmaps;
            trackManager = game.Audio.Track;
            this.localisation = localisation;

            beatmapSource = game.Beatmap ?? new Bindable<WorkingBeatmap>();

            currentBackground = new MusicControllerBackground();
            playerContainer.Add(currentBackground);
            playlist.StateChanged += (c, s) => playlistButton.FadeColour(s == Visibility.Visible? activeColour : Color4.White, transition_length, EasingTypes.OutQuint);
        }

        protected override void LoadComplete()
        {
            beatmapSource.ValueChanged += workingChanged;
            beatmapSource.TriggerChange();
            playList = playlist.List.ToList();

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
        }

        private void prev()
        {
            if (playList.Count == 0) return;
            if (current != null && playList.Count == 1) return;

            int n = playListIndex - 1;
            if (n < 0)
                n = playList.Count - 1;

            play(playList[n].Beatmaps[0], false);
            playListIndex = n;
        }

        private void next()
        {
            if (playList.Count == 0) return;
            if (current != null && playList.Count == 1) return;

            int n = playListIndex + 1;
            if (n >= playList.Count)
                n = 0;

            play(playList[n].Beatmaps[0], true);
            playListIndex = n;
        }

        private void play(BeatmapInfo info, bool isNext)
        {
            current = beatmaps.GetWorkingBeatmap(info, current);
            Task.Run(() =>
            {
                trackManager.SetExclusive(current.Track);
                current.Track.Start();
                beatmapSource.Value = current;
            }).ContinueWith(task => Schedule(task.ThrowIfFaulted), TaskContinuationOptions.OnlyOnFaulted);
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
                    if (beatmap?.Beatmap == null) //this is not needed if a placeholder exists
                    {
                        title.Current = null;
                        title.Text = @"Nothing to play";

                        artist.Current = null;
                        artist.Text = @"Nothing to play";
                    }
                    else
                    {
                        BeatmapMetadata metadata = beatmap.Beatmap.BeatmapInfo.Metadata;
                        title.Current = localisation.GetUnicodePreference(metadata.TitleUnicode, metadata.Title);
                        artist.Current = localisation.GetUnicodePreference(metadata.ArtistUnicode, metadata.Artist);
                        playlist.Current = beatmap.BeatmapSetInfo;
                    }
                });

                playerContainer.Add(new AsyncLoadWrapper(new MusicControllerBackground(beatmap)
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
