using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osuTK;
using osuTK.Graphics;
using osu.Game.Overlays;
using osu.Framework.Graphics.Sprites;
using osu.Game.Overlays.Music;
using osu.Framework.Audio.Track;
using osu.Game.Screens.Play;
using osu.Game.Screens.PurePlayer.Components;
using osu.Game.Screens.Mvis.UI;
using osu.Framework.Graphics.Colour;
using osu.Framework.Bindables;

namespace osu.Game.Screens
{
    public class PurePlayerScreen : ScreenWithBeatmapBackground
    {
        private const float StandardWidth = 0.8f;

        private Track Track;
        private bool AllowCursor = true;
        private bool AllowBack = false;
        public override bool AllowBackButton => AllowBack;
        public override bool CursorVisible => AllowCursor;
        public override bool HideOverlaysOnEnter => true;

        [Resolved(CanBeNull = true)]
        private OsuGame game { get; set; }

        [Resolved]
        private MusicController musicController { get; set; }

        [Cached]
        private PlaylistOverlay playlist;

        private PurePlayer.Components.SongProgressBar progressBarContainer;
        private MusicPanelSwitchableButton loopToggleButton;
        private MusicPanelSwitchableButton togglePauseButton;
        private BindableBool TrackRunning = new BindableBool();

        public PurePlayerScreen()
        {
            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Colour4.Black.Opacity(0.5f),
                    Depth = float.MaxValue,
                },
                new FillFlowContainer()
                {
                    Name = "Base Container",
                    Direction = FillDirection.Vertical,
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Spacing = new Vector2(5),
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            Name = "Cover Container",
                            RelativeSizeAxes = Axes.Both,
                            Masking = true,
                            CornerRadius = 12.5f,
                            Height = 0.4f,
                            Width = StandardWidth,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Children = new Drawable[]
                            {
                                new BeatmapCover(),
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = ColourInfo.GradientVertical(Color4.Black.Opacity(0), Color4.Black.Opacity(0.5f)),
                                },
                                new AuthorTextFillFlow
                                {
                                    Anchor = Anchor.BottomCentre,
                                    Origin = Anchor.BottomCentre,
                                    Margin = new MarginPadding{ Bottom = 10 },
                                },
                                new MusicControllerPanel
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    d = new Container
                                    {
                                        Alpha = 0,
                                        RelativeSizeAxes = Axes.Both,
                                        Children = new Drawable[]
                                        {
                                            new Box
                                            {
                                                RelativeSizeAxes = Axes.Both,
                                                Colour = Color4.Black.Opacity(0.5f),
                                            },
                                            new FillFlowContainer
                                            {
                                                RelativeSizeAxes = Axes.Both,
                                                Direction = FillDirection.Horizontal,
                                                Children = new Drawable[]
                                                {
                                                    new Container
                                                    {
                                                        RelativeSizeAxes = Axes.Both,
                                                        Anchor = Anchor.Centre,
                                                        Origin = Anchor.Centre,
                                                        Width = 0.333f,
                                                        Children = new Drawable[]
                                                        {
                                                            new MusicPanelHoldToConfirmButton
                                                            {
                                                                TooltipText = "上一首/退出",
                                                                ButtonIcon = FontAwesome.Solid.StepBackward,
                                                                RelativeSizeAxes = Axes.Both,
                                                                Anchor = Anchor.TopCentre,
                                                                Origin = Anchor.TopCentre,
                                                                Height = 1f,
                                                                ConfirmAction = () => this.Exit(),
                                                                Action = () => musicController?.PreviousTrack(),
                                                            },
                                                        }
                                                    },
                                                    new Container
                                                    {
                                                        RelativeSizeAxes = Axes.Both,
                                                        Anchor = Anchor.Centre,
                                                        Origin = Anchor.Centre,
                                                        Width = 0.333f,
                                                        Children = new Drawable[]
                                                        {
                                                            togglePauseButton = new MusicPanelSwitchableButton
                                                            {
                                                                ActivateColor = Colour4.White,
                                                                InActivateColor = Colour4.White.Opacity(0.5f),
                                                                TooltipText = "切换暂停",
                                                                RelativeSizeAxes = Axes.Both,
                                                                Anchor = Anchor.TopCentre,
                                                                Origin = Anchor.TopCentre,
                                                                NoIcon = true,
                                                                Height = 0.5f,
                                                                ExtraDrawable = new BottomBarSongProgressInfo
                                                                {
                                                                    FontSize = 30,
                                                                    AutoSizeAxes = Axes.Both,
                                                                    Anchor = Anchor.Centre,
                                                                    Origin = Anchor.Centre,
                                                                },
                                                                Action = () => TogglePause(),
                                                            },
                                                            new GridContainer
                                                            {
                                                                Anchor = Anchor.BottomCentre,
                                                                Origin = Anchor.BottomCentre,
                                                                RelativeSizeAxes = Axes.Both,
                                                                Height = 0.5f,
                                                                RowDimensions = new[]
                                                                {
                                                                    new Dimension(),
                                                                },
                                                                Content = new[]
                                                                {
                                                                    new Drawable[]
                                                                    {
                                                                        //Left
                                                                        loopToggleButton = new MusicPanelSwitchableButton
                                                                        {
                                                                            TooltipText = "单曲循环",
                                                                            RelativeSizeAxes = Axes.Both,
                                                                            ButtonIcon = FontAwesome.Solid.Undo,
                                                                            Action = () => Beatmap.Value.Track.Looping = loopToggleButton.Value.Value,

                                                                        },
                                                                        //Right
                                                                        new MusicPanelSwitchableButton
                                                                        {
                                                                            TooltipText = "歌曲列表",
                                                                            RelativeSizeAxes = Axes.Both,
                                                                            ButtonIcon = FontAwesome.Solid.Atom,
                                                                            Action = () => playlist.ToggleVisibility(),
                                                                        }
                                                                    },
                                                                }
                                                            }
                                                        }
                                                    },
                                                    new MusicPanelButton
                                                    {
                                                        TooltipText = "下一首",
                                                        ButtonIcon = FontAwesome.Solid.StepForward,
                                                        RelativeSizeAxes = Axes.Both,
                                                        Anchor = Anchor.Centre,
                                                        Origin = Anchor.Centre,
                                                        Width = 0.333f,
                                                        Action = () => musicController?.NextTrack(),
                                                    }
                                                }
                                            },
                                        }
                                    }
                                },
                            }
                        },
                        new Container
                        {
                            Name = "Music Control Container",
                            RelativeSizeAxes = Axes.X,
                            Height = 15,
                            Width = StandardWidth,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Children = new Drawable[]
                            {
                                new CircularContainer
                                {
                                    Masking = true,
                                    RelativeSizeAxes = Axes.Both,
                                    Child = progressBarContainer = new PurePlayer.Components.SongProgressBar
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                    },
                                },
                            }
                        },
                        new Container
                        {
                            Name = "SongList Container",
                            RelativeSizeAxes = Axes.Both,
                            Width = StandardWidth,
                            Height = 0.4f,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Children = new Drawable[]
                            {
                                playlist = new PlaylistOverlay
                                {
                                    TakeFocusOnPopIn = false,
                                    NoResizeOnPopIn = true,
                                    playlist_height = 1,
                                    RelativeSizeAxes = Axes.Both,
                                },
                            }
                        },
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            AllowBack = true;
            if ( musicController != null )
                playlist.BeatmapSets.BindTo(musicController.BeatmapSets);

            progressBarContainer.progressBar.OnSeek = SeekTo;
            Beatmap.BindValueChanged(b => UpdateComponentsFromBeatmap(b.NewValue));
            loopToggleButton.Value.BindValueChanged( l => Beatmap.Value.Track.Looping = l.NewValue );
            togglePauseButton.Value.BindTo(TrackRunning);
        }

        protected override void Update()
        {
            base.Update();

            if (Track?.IsDummyDevice == false)
            {
                TrackRunning.Value = Track.IsRunning;
                progressBarContainer.progressBar.CurrentTime = Track.CurrentTime;
            }
            else
            {
                TrackRunning.Value = false;
                progressBarContainer.progressBar.CurrentTime = 0;
                progressBarContainer.progressBar.EndTime = 1;
            }
        }

        private void TogglePause()
        {
            if (Track?.IsRunning == true)
            {
                musicController?.Stop();
            }
            else
            {
                musicController?.Play();
            }
        }

        public override void OnEntering(IScreen last)
        {
            base.OnEntering(last);
            UpdateComponentsFromBeatmap(Beatmap.Value);
            this.FadeInFromZero(300);
        }

        public override bool OnExiting(IScreen next)
        {
            Beatmap.Value.Track.Looping = false;
            Track = new TrackVirtual(Beatmap.Value.Track.Length);
            this.FadeOut(300);
            return base.OnExiting(next);
        }

        private void SeekTo(double position)
        {
            musicController?.SeekTo(position);
        }

        private void UpdateComponentsFromBeatmap(WorkingBeatmap B)
        {
            Background.Beatmap = B;
            Background.BlurAmount.Value = 25;

            this.Schedule(() =>
            {
                Track = Beatmap.Value?.TrackLoaded ?? false ? Beatmap.Value.Track : null;
                progressBarContainer.progressBar.EndTime = B.Track.Length;
            });
        }
    }
}