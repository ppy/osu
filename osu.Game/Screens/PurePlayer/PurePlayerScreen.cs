using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Graphics.UserInterface;
using osuTK;
using osuTK.Graphics;
using osu.Game.Overlays;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Bindables;
using osu.Game.Overlays.Music;
using osu.Framework.Audio.Track;
using osu.Game.Screens.Play;
using osu.Game.Screens.PurePlayer.Components;
using osu.Game.Screens.Mvis.UI;
using osu.Game.Screens.Mvis.BottomBar.Buttons;
using osu.Framework.Graphics.Colour;

namespace osu.Game.Screens
{
    public class PurePlayerScreen : ScreenWithBeatmapBackground
    {
        private const float StandardWidth = 0.8f;
        private static readonly Vector2 BOTTOMPANEL_SIZE = new Vector2(TwoLayerButton.SIZE_EXTENDED.X, 50);

        private Track Track;
        private bool AllowCursor = true;
        private bool AllowBack = false;
        public override bool AllowBackButton => AllowBack;
        public override bool CursorVisible => AllowCursor;

        [Resolved(CanBeNull = true)]
        private OsuGame game { get; set; }

        [Resolved]
        private MusicController musicController { get; set; }

        [Cached]
        private PlaylistOverlay playlist;

        private PurePlayer.Components.SongProgressBar progressBarContainer;
        private BottomBarSwitchButton loopToggleButton;

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
                                new MusicControllerPanel
                                {
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
                                                new MusicControllerButton
                                                {
                                                    ButtonIcon = FontAwesome.Solid.StepBackward,
                                                    RelativeSizeAxes = Axes.Both,
                                                    Anchor = Anchor.Centre,
                                                    Origin = Anchor.Centre,
                                                    Width = 0.333f,
                                                    Action = () => musicController?.PreviousTrack(),
                                                },
                                                new MusicControllerButton
                                                {
                                                    RelativeSizeAxes = Axes.Both,
                                                    Anchor = Anchor.Centre,
                                                    Origin = Anchor.Centre,
                                                    NoIcon = true,
                                                    Width = 0.333f,
                                                    ExtraDrawable = new BottomBarSongProgressInfo
                                                    {
                                                        FontSize = 30,
                                                        AutoSizeAxes = Axes.Both,
                                                        Anchor = Anchor.Centre,
                                                        Origin = Anchor.Centre,
                                                    },
                                                    Action = () => TogglePause(),
                                                },
                                                new MusicControllerButton
                                                {
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
                                },
                                new AuthorTextFillFlow
                                {
                                    Anchor = Anchor.BottomCentre,
                                    Origin = Anchor.BottomCentre,
                                    Margin = new MarginPadding{ Bottom = 10 },
                                },
                            }
                        },
                        new Container
                        {
                            Name = "Music Control Container",
                            RelativeSizeAxes = Axes.X,
                            Height = 30,
                            Width = StandardWidth,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Children = new Drawable[]
                            {
                                new CircularContainer
                                {
                                    Masking = true,
                                    RelativeSizeAxes = Axes.Both,
                                    Width = 0.7f,
                                    Child = progressBarContainer = new PurePlayer.Components.SongProgressBar
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                    },
                                },
                                new FillFlowContainer
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Width = 0.3f,
                                    Spacing = new Vector2(5),
                                    Anchor = Anchor.TopRight,
                                    Origin = Anchor.TopRight,
                                    Children = new Drawable[]
                                    {
                                        new BottomBarButton()
                                        {
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            ButtonIcon = FontAwesome.Solid.ArrowLeft,
                                            Action = () => this.Exit(),
                                            TooltipText = "退出",
                                        },
                                        loopToggleButton = new BottomBarSwitchButton()
                                        {
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            ButtonIcon = FontAwesome.Solid.Undo,
                                            Action = () => Beatmap.Value.Track.Looping = loopToggleButton.ToggleableValue.Value,
                                            TooltipText = "单曲循环",
                                        },
                                        new BottomBarButton()
                                        {
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            ButtonIcon = FontAwesome.Solid.User,
                                            Action = () => game?.PresentBeatmap(Beatmap.Value.BeatmapSetInfo),
                                            TooltipText = "在选歌界面中查看",
                                        },
                                        new BottomBarSwitchButton()
                                        {
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            ButtonIcon = FontAwesome.Solid.Atom,
                                            Action = () => playlist.ToggleVisibility(),
                                            TooltipText = "侧边栏",
                                        },
                                    }
                                }
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

            if ( musicController != null )
                playlist.BeatmapSets.BindTo(musicController.BeatmapSets);

            progressBarContainer.progressBar.OnSeek = SeekTo;
            Beatmap.BindValueChanged(b => UpdateComponentsFromBeatmap(b.NewValue));
            loopToggleButton.ToggleableValue.BindValueChanged( l => Beatmap.Value.Track.Looping = l.NewValue );
        }

        protected override void Update()
        {
            base.Update();

            if (Track?.IsDummyDevice == false)
            {
                progressBarContainer.progressBar.CurrentTime = Track.CurrentTime;
            }
            else
            {
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
             game?.Toolbar.Hide();
        }

        public override bool OnExiting(IScreen next)
        {
            Track = new TrackVirtual(Beatmap.Value.Track.Length);
            game?.Toolbar.Show();
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