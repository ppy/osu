// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps.Drawables.Cards.Buttons;
using osu.Game.Graphics;
using osu.Game.Online;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;

namespace osu.Game.Beatmaps.Drawables.Cards
{
    public class CollapsibleButtonContainer : Container
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

        public MarginPadding ButtonsPadding
        {
            get => buttons.Padding;
            set => buttons.Padding = value;
        }

        protected override Container<Drawable> Content => mainContent;

        private readonly Container background;

        private readonly Container buttonArea;
        private readonly Container<BeatmapCardIconButton> buttons;

        private readonly Container mainArea;
        private readonly Container mainContent;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        public CollapsibleButtonContainer(APIBeatmapSet beatmapSet)
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
                    // workaround for masking artifacts at the top & bottom of card,
                    // which become especially visible on downloaded beatmaps (when the icon area has a lime background).
                    Padding = new MarginPadding { Vertical = 1 },
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
                    Child = buttons = new Container<BeatmapCardIconButton>
                    {
                        RelativeSizeAxes = Axes.Both,
                        Children = new BeatmapCardIconButton[]
                        {
                            new FavouriteButton(beatmapSet)
                            {
                                Current = FavouriteState,
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre
                            },
                            new DownloadButton(beatmapSet)
                            {
                                Anchor = Anchor.BottomCentre,
                                Origin = Anchor.BottomCentre,
                                State = { BindTarget = downloadTracker.State }
                            },
                            new GoToBeatmapButton(beatmapSet)
                            {
                                Anchor = Anchor.BottomCentre,
                                Origin = Anchor.BottomCentre,
                                State = { BindTarget = downloadTracker.State }
                            }
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
            buttons.FadeTo(ShowDetails.Value ? 1 : 0, BeatmapCard.TRANSITION_DURATION, Easing.OutQuint);

            foreach (var button in buttons)
            {
                button.IdleColour = downloadTracker.State.Value != DownloadState.LocallyAvailable ? colourProvider.Light1 : colourProvider.Background3;
                button.HoverColour = downloadTracker.State.Value != DownloadState.LocallyAvailable ? colourProvider.Content1 : colourProvider.Foreground1;
            }
        }
    }
}
