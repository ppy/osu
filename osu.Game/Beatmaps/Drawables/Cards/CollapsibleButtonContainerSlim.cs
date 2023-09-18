// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Resources.Localisation.Web;
using osuTK;

namespace osu.Game.Beatmaps.Drawables.Cards
{
    public partial class CollapsibleButtonContainerSlim : OsuClickableContainer
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

        private readonly APIBeatmapSet beatmapSet;

        private readonly Container background;

        private readonly Container buttonArea;

        private readonly Container mainArea;
        private readonly Container mainContent;

        private readonly Container icons;
        private readonly SpriteIcon downloadIcon;
        private readonly LoadingSpinner spinner;
        private readonly SpriteIcon goToBeatmapIcon;

        private const int icon_size = 12;

        private Bindable<bool> preferNoVideo = null!;

        [Resolved]
        private BeatmapModelDownloader beatmaps { get; set; } = null!;

        [Resolved]
        private OsuGame? game { get; set; }

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        public CollapsibleButtonContainerSlim(APIBeatmapSet beatmapSet)
        {
            this.beatmapSet = beatmapSet;

            downloadTracker = new BeatmapDownloadTracker(beatmapSet);

            RelativeSizeAxes = Axes.Y;
            Masking = true;
            CornerRadius = BeatmapCard.CORNER_RADIUS;

            base.Content.AddRange(new Drawable[]
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
                buttonArea = new Container
                {
                    Name = @"Right (button) area",
                    RelativeSizeAxes = Axes.Y,
                    Origin = Anchor.TopRight,
                    Anchor = Anchor.TopRight,
                    Child = icons = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Children = new Drawable[]
                        {
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
                    }
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
            });
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            preferNoVideo = config.GetBindable<bool>(OsuSetting.PreferNoVideo);

            downloadIcon.Colour = spinner.Colour = colourProvider.Content1;
            goToBeatmapIcon.Colour = colourProvider.Foreground1;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            preferNoVideo.BindValueChanged(_ => updateState());
            downloadTracker.State.BindValueChanged(_ => updateState());
            ShowDetails.BindValueChanged(_ => updateState(), true);
            FinishTransforms(true);
        }

        private void updateState()
        {
            float targetWidth = Width - (ShowDetails.Value ? ButtonsExpandedWidth : ButtonsCollapsedWidth);

            mainArea.ResizeWidthTo(targetWidth, BeatmapCard.TRANSITION_DURATION, Easing.OutQuint);

            var backgroundColour = downloadTracker.State.Value == DownloadState.LocallyAvailable ? colours.Lime0 : colourProvider.Background3;
            if (ShowDetails.Value)
                backgroundColour = backgroundColour.Lighten(0.2f);

            background.FadeColour(backgroundColour, BeatmapCard.TRANSITION_DURATION, Easing.OutQuint);
            icons.FadeTo(ShowDetails.Value ? 1 : 0, BeatmapCard.TRANSITION_DURATION, Easing.OutQuint);

            if (beatmapSet.Availability.DownloadDisabled)
            {
                Enabled.Value = false;
                TooltipText = BeatmapsetsStrings.AvailabilityDisabled;
                return;
            }

            switch (downloadTracker.State.Value)
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

            downloadIcon.FadeTo(downloadTracker.State.Value == DownloadState.NotDownloaded ? 1 : 0, BeatmapCard.TRANSITION_DURATION, Easing.OutQuint);
            spinner.FadeTo(downloadTracker.State.Value == DownloadState.Downloading || downloadTracker.State.Value == DownloadState.Importing ? 1 : 0,
                BeatmapCard.TRANSITION_DURATION, Easing.OutQuint);
            goToBeatmapIcon.FadeTo(downloadTracker.State.Value == DownloadState.LocallyAvailable ? 1 : 0, BeatmapCard.TRANSITION_DURATION, Easing.OutQuint);

            if (downloadTracker.State.Value == DownloadState.NotDownloaded)
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
