using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Backgrounds;
using osuTK;
using osuTK.Graphics;
using osu.Game.Overlays;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;
using osu.Framework.Bindables;
using osu.Framework.Input.Events;
using osu.Game.Overlays.Music;
using osu.Framework.Audio.Track;
using osu.Game.Input.Bindings;
using osu.Framework.Input.Bindings;
using osu.Game.Configuration;
using osu.Game.Screens.Mvis.SideBar;
using osu.Game.Screens.Mvis;
using osu.Game.Screens.Play;
using osu.Game.Screens.PurePlayer.Components;
using osu.Framework.Graphics.Colour;
using osu.Game.Screens.Mvis.UI;

namespace osu.Game.Screens
{
    public class PurePlayerScreen : ScreenWithBeatmapBackground
    {
        private const float DURATION = 750;
        private const float StandardWidth = 0.8f;
        private static readonly Vector2 BOTTOMPANEL_SIZE = new Vector2(TwoLayerButton.SIZE_EXTENDED.X, 50);

        private Track Track;
        private bool AllowCursor = true;
        private bool AllowBack = true;
        public override bool AllowBackButton => AllowBack;
        public override bool CursorVisible => AllowCursor;

        [Resolved(CanBeNull = true)]
        private OsuGame game { get; set; }

        [Resolved]
        private MusicController musicController { get; set; }

        [Cached]
        private PlaylistOverlay playlist;

        private PurePlayer.Components.SongProgressBar progressBarContainer;

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
                            }
                        },
                        new CircularContainer
                        {
                            Name = "Progress Bar Container",
                            Masking = true,
                            RelativeSizeAxes = Axes.X,
                            Height = 7.5f,
                            Width = StandardWidth * 0.9f,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Children = new Drawable[]
                            {
                                progressBarContainer = new PurePlayer.Components.SongProgressBar
                                {
                                    RelativeSizeAxes = Axes.Both,
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
                            Padding = new MarginPadding{Bottom = 10},
                            Children = new Drawable[]
                            {
                                playlist = new PlaylistOverlay
                                {
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

            playlist.Show();
            progressBarContainer.progressBar.OnSeek = SeekTo;
            Beatmap.BindValueChanged(OnBeatmapChanged, true);
        }

        protected override void Update()
        {
            base.Update();

            Track = Beatmap.Value?.TrackLoaded ?? false ? Beatmap.Value.Track : null;
            if (Track?.IsDummyDevice == false)
            {
                //TrackRunning.Value = Track.IsRunning;
                progressBarContainer.progressBar.CurrentTime = Track.CurrentTime;
            }
            else
            {
                //TrackRunning.Value = false;
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

        private void OnBeatmapChanged(ValueChangedEvent<WorkingBeatmap> b)
        {
            //Background.Beatmap = b.NewValue;
            //Background.BlurAmount.Value = 25;

            this.Schedule(() =>
            {
                progressBarContainer.progressBar.EndTime = b.NewValue.Track.Length;
            });
        }
    }
}