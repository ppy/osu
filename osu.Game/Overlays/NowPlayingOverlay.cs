// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Overlays.Music;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays
{
    public partial class NowPlayingOverlay : OsuFocusedOverlayContainer, INamedOverlayComponent
    {
        public IconUsage Icon => OsuIcon.Music;
        public LocalisableString Title => NowPlayingStrings.HeaderTitle;
        public LocalisableString Description => NowPlayingStrings.HeaderDescription;

        private const float player_height = 130;
        private const float transition_length = 800;
        private const float progress_height = 10;
        private const float bottom_black_area_height = 55;
        private const float margin = 10;

        private Drawable background = null!;
        private ProgressBar progressBar = null!;

        private IconButton prevButton = null!;
        private IconButton playButton = null!;
        private IconButton nextButton = null!;
        private IconButton playlistButton = null!;

        private SpriteText title = null!, artist = null!;

        private PlaylistOverlay? playlist;

        private Container dragContainer = null!;
        private Container playerContainer = null!;
        private Container playlistContainer = null!;

        protected override double PopInOutSampleBalance => OsuGameBase.SFX_STEREO_STRENGTH * 0.75f;

        [Resolved]
        private MusicController musicController { get; set; } = null!;

        [Resolved]
        private Bindable<WorkingBeatmap> beatmap { get; set; } = null!;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        private Bindable<bool> allowTrackControl = null!;

        public NowPlayingOverlay()
        {
            Width = 400;
            Margin = new MarginPadding(margin);
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Children = new Drawable[]
            {
                dragContainer = new DragContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.X,
                    Children = new Drawable[]
                    {
                        playerContainer = new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = player_height,
                            Masking = true,
                            CornerRadius = 5,
                            EdgeEffect = new EdgeEffectParameters
                            {
                                Type = EdgeEffectType.Shadow,
                                Colour = Color4.Black.Opacity(40),
                                Radius = 5,
                            },
                            Children = new[]
                            {
                                background = Empty(),
                                title = new OsuSpriteText
                                {
                                    Origin = Anchor.BottomCentre,
                                    Anchor = Anchor.TopCentre,
                                    Position = new Vector2(0, 40),
                                    Font = OsuFont.GetFont(size: 25, italics: true),
                                    Colour = Color4.White,
                                    Text = @"Nothing to play",
                                },
                                artist = new OsuSpriteText
                                {
                                    Origin = Anchor.TopCentre,
                                    Anchor = Anchor.TopCentre,
                                    Position = new Vector2(0, 45),
                                    Font = OsuFont.GetFont(size: 15, weight: FontWeight.Bold, italics: true),
                                    Colour = Color4.White,
                                    Text = @"Nothing to play",
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
                                        new FillFlowContainer<IconButton>
                                        {
                                            AutoSizeAxes = Axes.Both,
                                            Direction = FillDirection.Horizontal,
                                            Spacing = new Vector2(5),
                                            Origin = Anchor.Centre,
                                            Anchor = Anchor.Centre,
                                            Children = new[]
                                            {
                                                prevButton = new MusicIconButton
                                                {
                                                    Anchor = Anchor.Centre,
                                                    Origin = Anchor.Centre,
                                                    Action = () => musicController.PreviousTrack(),
                                                    Icon = FontAwesome.Solid.StepBackward,
                                                },
                                                playButton = new MusicIconButton
                                                {
                                                    Anchor = Anchor.Centre,
                                                    Origin = Anchor.Centre,
                                                    Scale = new Vector2(1.4f),
                                                    IconScale = new Vector2(1.4f),
                                                    Action = () => musicController.TogglePause(),
                                                    Icon = FontAwesome.Regular.PlayCircle,
                                                },
                                                nextButton = new MusicIconButton
                                                {
                                                    Anchor = Anchor.Centre,
                                                    Origin = Anchor.Centre,
                                                    Action = () => musicController.NextTrack(),
                                                    Icon = FontAwesome.Solid.StepForward,
                                                },
                                            }
                                        },
                                        playlistButton = new MusicIconButton
                                        {
                                            Origin = Anchor.Centre,
                                            Anchor = Anchor.CentreRight,
                                            Position = new Vector2(-bottom_black_area_height / 2, 0),
                                            Icon = FontAwesome.Solid.Bars,
                                            Action = togglePlaylist
                                        },
                                    }
                                },
                                progressBar = new HoverableProgressBar
                                {
                                    Origin = Anchor.BottomCentre,
                                    Anchor = Anchor.BottomCentre,
                                    Height = progress_height / 2,
                                    FillColour = colours.Yellow,
                                    BackgroundColour = colours.YellowDarker.Opacity(0.5f),
                                    OnSeek = musicController.SeekTo
                                }
                            },
                        },
                        playlistContainer = new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            Y = player_height + margin,
                        }
                    }
                },
            };
        }

        private void togglePlaylist()
        {
            if (playlist == null)
            {
                LoadComponentAsync(playlist = new PlaylistOverlay
                {
                    RelativeSizeAxes = Axes.Both,
                }, _ =>
                {
                    playlistContainer.Add(playlist);

                    playlist.State.BindValueChanged(s => playlistButton.FadeColour(s.NewValue == Visibility.Visible ? colours.Yellow : Color4.White, 200, Easing.OutQuint), true);

                    togglePlaylist();
                });

                return;
            }

            if (!beatmap.Disabled)
                playlist.ToggleVisibility();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            beatmap.BindDisabledChanged(_ => Scheduler.AddOnce(updateEnabledStates));

            allowTrackControl = musicController.AllowTrackControl.GetBoundCopy();
            allowTrackControl.BindValueChanged(_ => Scheduler.AddOnce(updateEnabledStates), true);

            musicController.TrackChanged += trackChanged;
            trackChanged(beatmap.Value);
        }

        protected override void PopIn()
        {
            this.FadeIn(transition_length, Easing.OutQuint);
            dragContainer.ScaleTo(1, transition_length, Easing.OutElastic);
        }

        protected override void PopOut()
        {
            base.PopOut();

            this.FadeOut(transition_length, Easing.OutQuint);
            dragContainer.ScaleTo(0.9f, transition_length, Easing.OutQuint);
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            playlistContainer.Height = MathF.Min(Parent!.DrawHeight - margin * 3 - player_height, PlaylistOverlay.PLAYLIST_HEIGHT);

            float height = player_height;

            if (playlist != null)
            {
                height += playlist.DrawHeight;
                if (playlist.State.Value == Visibility.Visible)
                    height += margin;
            }

            Height = dragContainer.Height = height;
        }

        protected override void Update()
        {
            base.Update();

            if (pendingBeatmapSwitch != null)
            {
                pendingBeatmapSwitch();
                pendingBeatmapSwitch = null;
            }

            var track = musicController.CurrentTrack;

            if (!track.IsDummyDevice)
            {
                progressBar.EndTime = track.Length;
                progressBar.CurrentTime = track.CurrentTime;

                playButton.Icon = track.IsRunning ? FontAwesome.Regular.PauseCircle : FontAwesome.Regular.PlayCircle;
            }
            else
            {
                progressBar.CurrentTime = 0;
                progressBar.EndTime = 1;
                playButton.Icon = FontAwesome.Regular.PlayCircle;
            }
        }

        private Action? pendingBeatmapSwitch;

        private CancellationTokenSource? backgroundLoadCancellation;

        private WorkingBeatmap? currentBeatmap;

        private void trackChanged(WorkingBeatmap beatmap, TrackChangeDirection direction = TrackChangeDirection.None)
        {
            currentBeatmap = beatmap;

            // avoid using scheduler as our scheduler may not be run for a long time, holding references to beatmaps.
            pendingBeatmapSwitch = delegate
            {
                BeatmapMetadata metadata = beatmap.Metadata;

                title.Text = new RomanisableString(metadata.TitleUnicode, metadata.Title);
                artist.Text = new RomanisableString(metadata.ArtistUnicode, metadata.Artist);

                backgroundLoadCancellation?.Cancel();

                LoadComponentAsync(new Background(beatmap) { Depth = float.MaxValue }, newBackground =>
                {
                    if (beatmap != currentBeatmap)
                    {
                        newBackground.Dispose();
                        return;
                    }

                    switch (direction)
                    {
                        case TrackChangeDirection.Next:
                            newBackground.Position = new Vector2(400, 0);
                            newBackground.MoveToX(0, 500, Easing.OutCubic);
                            background.MoveToX(-400, 500, Easing.OutCubic);
                            break;

                        case TrackChangeDirection.Prev:
                            newBackground.Position = new Vector2(-400, 0);
                            newBackground.MoveToX(0, 500, Easing.OutCubic);
                            background.MoveToX(400, 500, Easing.OutCubic);
                            break;
                    }

                    background.Expire();
                    background = newBackground;

                    playerContainer.Add(newBackground);
                }, (backgroundLoadCancellation = new CancellationTokenSource()).Token);
            };
        }

        private void updateEnabledStates()
        {
            bool beatmapDisabled = beatmap.Disabled;
            bool trackControlDisabled = !musicController.AllowTrackControl.Value;

            if (beatmapDisabled || trackControlDisabled)
                playlist?.Hide();

            prevButton.Enabled.Value = !beatmapDisabled && !trackControlDisabled;
            nextButton.Enabled.Value = !beatmapDisabled && !trackControlDisabled;
            playlistButton.Enabled.Value = !beatmapDisabled && !trackControlDisabled;
            playButton.Enabled.Value = !trackControlDisabled;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (musicController.IsNotNull())
                musicController.TrackChanged -= trackChanged;
        }

        private partial class MusicIconButton : IconButton
        {
            public MusicIconButton()
            {
                AutoSizeAxes = Axes.Both;
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                HoverColour = colours.YellowDark.Opacity(0.6f);
                FlashColour = colours.Yellow;
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                // works with AutoSizeAxes above to make buttons autosize with the scale animation.
                Content.AutoSizeAxes = Axes.None;
                Content.Size = new Vector2(DEFAULT_BUTTON_SIZE);
            }
        }

        private partial class Background : BufferedContainer
        {
            private readonly Sprite sprite;
            private readonly WorkingBeatmap beatmap;

            public Background(WorkingBeatmap beatmap)
                : base(cachedFrameBuffer: true)
            {
                this.beatmap = beatmap;

                Depth = float.MaxValue;
                RelativeSizeAxes = Axes.Both;

                Children = new Drawable[]
                {
                    sprite = new Sprite
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = OsuColour.Gray(150),
                        FillMode = FillMode.Fill,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
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
            private void load(LargeTextureStore textures)
            {
                sprite.Texture = beatmap.GetBackground() ?? textures.Get(@"Backgrounds/bg4");
            }
        }

        private partial class DragContainer : Container
        {
            protected override bool OnDragStart(DragStartEvent e)
            {
                return true;
            }

            protected override void OnDrag(DragEvent e)
            {
                Vector2 change = e.MousePosition - e.MouseDownPosition;

                // Diminish the drag distance as we go further to simulate "rubber band" feeling.
                change *= change.Length <= 0 ? 0 : MathF.Pow(change.Length, 0.7f) / change.Length;

                this.MoveTo(change);
            }

            protected override void OnDragEnd(DragEndEvent e)
            {
                this.MoveTo(Vector2.Zero, 800, Easing.OutElastic);
                base.OnDragEnd(e);
            }
        }

        private partial class HoverableProgressBar : ProgressBar
        {
            public HoverableProgressBar()
                : base(true)
            {
            }

            protected override bool OnHover(HoverEvent e)
            {
                this.ResizeHeightTo(progress_height, 500, Easing.OutQuint);
                return base.OnHover(e);
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                this.ResizeHeightTo(progress_height / 2, 500, Easing.OutQuint);
                base.OnHoverLost(e);
            }
        }
    }
}
