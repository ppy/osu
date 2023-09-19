// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Resources.Localisation.Web;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Beatmaps.Drawables.Cards
{
    public partial class CollapsibleButtonContainerSlim : Container
    {
        public Bindable<bool> ShowDetails = new Bindable<bool>();
        public Bindable<BeatmapSetFavouriteState> FavouriteState = new Bindable<BeatmapSetFavouriteState>();

        private readonly BeatmapDownloadTracker downloadTracker;

        private float buttonsExpandedWidth;

        public float ButtonsExpandedWidth
        {
            get => buttonsExpandedWidth;
            set
            {
                buttonsExpandedWidth = value;
                buttonArea.Width = value;
                if (IsLoaded)
                    updateState();
            }
        }

        private float buttonsCollapsedWidth;

        public float ButtonsCollapsedWidth
        {
            get => buttonsCollapsedWidth;
            set
            {
                buttonsCollapsedWidth = value;
                if (IsLoaded)
                    updateState();
            }
        }

        protected override Container<Drawable> Content => mainContent;

        private readonly Container background;

        private readonly OsuClickableContainer buttonArea;

        private readonly Container mainArea;
        private readonly Container mainContent;

        private const int icon_size = 12;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        public CollapsibleButtonContainerSlim(APIBeatmapSet beatmapSet)
        {
            downloadTracker = new BeatmapDownloadTracker(beatmapSet);

            RelativeSizeAxes = Axes.Y;
            Masking = true;
            CornerRadius = BeatmapCard.CORNER_RADIUS;

            InternalChildren = new Drawable[]
            {
                downloadTracker,
                background = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    Child = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Colour4.White
                    },
                },
                buttonArea = new ButtonArea(beatmapSet)
                {
                    Name = @"Right (button) area",
                    State = { BindTarget = downloadTracker.State }
                },
                mainArea = new Container
                {
                    Name = @"Main content",
                    RelativeSizeAxes = Axes.Y,
                    CornerRadius = BeatmapCard.CORNER_RADIUS,
                    Masking = true,
                    Children = new Drawable[]
                    {
                        new BeatmapCardContentBackground(beatmapSet)
                        {
                            RelativeSizeAxes = Axes.Both,
                            Dimmed = { BindTarget = ShowDetails }
                        },
                        mainContent = new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding
                            {
                                Horizontal = 10,
                                Vertical = 4
                            },
                        }
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            downloadTracker.State.BindValueChanged(_ => updateState());
            ShowDetails.BindValueChanged(_ => updateState(), true);
            FinishTransforms(true);
        }

        private void updateState()
        {
            float targetWidth = Width - (ShowDetails.Value ? ButtonsExpandedWidth : ButtonsCollapsedWidth);

            mainArea.ResizeWidthTo(targetWidth, BeatmapCard.TRANSITION_DURATION, Easing.OutQuint);
            background.FadeColour(downloadTracker.State.Value == DownloadState.LocallyAvailable ? colours.Lime0 : colourProvider.Background3, BeatmapCard.TRANSITION_DURATION, Easing.OutQuint);
            buttonArea.FadeTo(ShowDetails.Value ? 1 : 0, BeatmapCard.TRANSITION_DURATION, Easing.OutQuint);
        }

        private partial class ButtonArea : OsuClickableContainer
        {
            public Bindable<DownloadState> State { get; } = new Bindable<DownloadState>();

            private readonly APIBeatmapSet beatmapSet;

            private Box hoverLayer = null!;
            private SpriteIcon downloadIcon = null!;
            private LoadingSpinner spinner = null!;
            private SpriteIcon goToBeatmapIcon = null!;

            private Bindable<bool> preferNoVideo = null!;

            [Resolved]
            private OverlayColourProvider colourProvider { get; set; } = null!;

            [Resolved]
            private BeatmapModelDownloader beatmaps { get; set; } = null!;

            [Resolved]
            private OsuGame? game { get; set; }

            public ButtonArea(APIBeatmapSet beatmapSet)
            {
                this.beatmapSet = beatmapSet;
            }

            [BackgroundDependencyLoader]
            private void load(OsuConfigManager config)
            {
                RelativeSizeAxes = Axes.Y;
                Origin = Anchor.TopRight;
                Anchor = Anchor.TopRight;
                Child = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding { Left = -BeatmapCard.CORNER_RADIUS },
                            Child = hoverLayer = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = Colour4.White.Opacity(0.1f),
                                Blending = BlendingParameters.Additive
                            }
                        },
                        downloadIcon = new SpriteIcon
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Size = new Vector2(icon_size),
                            Icon = FontAwesome.Solid.Download
                        },
                        spinner = new LoadingSpinner
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Size = new Vector2(icon_size)
                        },
                        goToBeatmapIcon = new SpriteIcon
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Size = new Vector2(icon_size),
                            Icon = FontAwesome.Solid.AngleDoubleRight
                        },
                    }
                };

                preferNoVideo = config.GetBindable<bool>(OsuSetting.PreferNoVideo);
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                State.BindValueChanged(_ => updateState(), true);
                FinishTransforms(true);
            }

            protected override bool OnHover(HoverEvent e)
            {
                updateState();
                return base.OnHover(e);
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                updateState();
                base.OnHoverLost(e);
            }

            private void updateState()
            {
                hoverLayer.FadeTo(IsHovered ? 1 : 0, BeatmapCard.TRANSITION_DURATION, Easing.OutQuint);

                downloadIcon.FadeTo(State.Value == DownloadState.NotDownloaded ? 1 : 0, BeatmapCard.TRANSITION_DURATION, Easing.OutQuint);
                downloadIcon.FadeColour(IsHovered ? colourProvider.Content1 : colourProvider.Light1, BeatmapCard.TRANSITION_DURATION, Easing.OutQuint);

                spinner.FadeTo(State.Value == DownloadState.Downloading || State.Value == DownloadState.Importing ? 1 : 0,
                    BeatmapCard.TRANSITION_DURATION, Easing.OutQuint);
                spinner.FadeColour(IsHovered ? colourProvider.Content1 : colourProvider.Light1, BeatmapCard.TRANSITION_DURATION, Easing.OutQuint);

                goToBeatmapIcon.FadeTo(State.Value == DownloadState.LocallyAvailable ? 1 : 0, BeatmapCard.TRANSITION_DURATION, Easing.OutQuint);
                goToBeatmapIcon.FadeColour(IsHovered ? colourProvider.Foreground1 : colourProvider.Background3, BeatmapCard.TRANSITION_DURATION, Easing.OutQuint);

                switch (State.Value)
                {
                    case DownloadState.NotDownloaded:
                        Action = () => beatmaps.Download(beatmapSet, preferNoVideo.Value);
                        break;

                    case DownloadState.LocallyAvailable:
                        Action = () => game?.PresentBeatmap(beatmapSet);
                        break;

                    default:
                        Action = null;
                        break;
                }

                if (beatmapSet.Availability.DownloadDisabled)
                {
                    Enabled.Value = false;
                    TooltipText = BeatmapsetsStrings.AvailabilityDisabled;
                    return;
                }

                if (State.Value == DownloadState.NotDownloaded)
                {
                    if (!beatmapSet.HasVideo)
                        TooltipText = BeatmapsetsStrings.PanelDownloadAll;
                    else
                        TooltipText = preferNoVideo.Value ? BeatmapsetsStrings.PanelDownloadNoVideo : BeatmapsetsStrings.PanelDownloadVideo;
                }
                else
                {
                    TooltipText = default;
                }
            }
        }
    }
}
