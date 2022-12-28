using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osuTK;
using osuTK.Graphics;

#nullable disable

namespace Mvis.Plugin.SandboxToPanel.RulesetComponents.UI
{
    public partial class TrackController : CompositeDrawable
    {
        private const float progress_height = 10;
        private const float height = 60;

        [Resolved]
        private MusicController musicController { get; set; }

        [Resolved]
        private Bindable<WorkingBeatmap> beatmap { get; set; }

        private IconButton playButton;
        private HoverableProgressBar progressBar;
        private Container beatmapSpriteHolder;

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            RelativeSizeAxes = Axes.X;
            Height = height;
            Masking = true;
            CornerRadius = 5;
            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black.Opacity(0.5f)
                },
                beatmapSpriteHolder = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.DimGray
                },
                new FillFlowContainer<IconButton>
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(5),
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Children = new[]
                    {
                        new MusicIconButton
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
                        new MusicIconButton
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Action = () => musicController.NextTrack(),
                            Icon = FontAwesome.Solid.StepForward,
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
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            musicController.TrackChanged += trackChanged;
            trackChanged(beatmap.Value);
        }

        private BeatmapSprite lastSprite;

        private void trackChanged(WorkingBeatmap beatmap, TrackChangeDirection direction = TrackChangeDirection.None)
        {
            LoadComponentAsync(new BeatmapSprite(beatmap), loaded =>
            {
                if (lastSprite != null)
                    lastSprite.FadeOut(300, Easing.OutQuint).Expire();

                beatmapSpriteHolder.Add(lastSprite = loaded);
                lastSprite.FadeInFromZero(300, Easing.OutQuint);
            });
        }

        protected override void Update()
        {
            base.Update();

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

        protected override void Dispose(bool isDisposing)
        {
            musicController.TrackChanged -= trackChanged;
            base.Dispose(isDisposing);
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

        private partial class BeatmapSprite : Sprite
        {
            private readonly WorkingBeatmap beatmap;

            public BeatmapSprite(WorkingBeatmap beatmap)
            {
                this.beatmap = beatmap;
            }

            [BackgroundDependencyLoader]
            private void load(TextureStore textures)
            {
                RelativeSizeAxes = Axes.Both;
                FillMode = FillMode.Fill;
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;
                Texture = beatmap.Background ?? textures.Get("Backgrounds/bg4");
            }
        }
    }
}
