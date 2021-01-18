// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online;
using osu.Game.Online.API;
using osu.Game.Overlays.BeatmapListing.Panels;
using osu.Game.Overlays.BeatmapSet.Buttons;
using osu.Game.Rulesets;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.BeatmapSet
{
    public class BeatmapSetHeader : OverlayHeader
    {
        private const float transition_duration = 200;
        private const float buttons_height = 45;
        private const float buttons_spacing = 5;

        public readonly Bindable<BeatmapSetInfo> BeatmapSet = new Bindable<BeatmapSetInfo>();

        public bool DownloadButtonsVisible => downloadButtonsContainer.Any();

        public BeatmapPicker Picker { get; private set; }

        public BeatmapRulesetSelector RulesetSelector { get; private set; }

        private IBindable<DownloadState> state => downloadTracker.State;

        [Cached(typeof(IBindable<RulesetInfo>))]
        private readonly Bindable<RulesetInfo> ruleset = new Bindable<RulesetInfo>();

        [Resolved]
        private IAPIProvider api { get; set; }

        private readonly DownloadTracker downloadTracker;
        private OsuSpriteText title, artist;
        private AuthorInfo author;
        private ExplicitContentBeatmapPill explicitContentPill;
        private FillFlowContainer downloadButtonsContainer;
        private BeatmapAvailability beatmapAvailability;
        private BeatmapSetOnlineStatusPill onlineStatusPill;
        private ExternalLinkButton externalLink;
        private UpdateableBeatmapSetCover cover;
        private Box coverGradient;
        private FillFlowContainer fadeContent;
        private FavouriteButton favouriteButton;
        private LoadingSpinner loading;
        private Details details;

        public BeatmapSetHeader()
        {
            Masking = true;

            EdgeEffect = new EdgeEffectParameters
            {
                Colour = Color4.Black.Opacity(0.25f),
                Type = EdgeEffectType.Shadow,
                Radius = 3,
                Offset = new Vector2(0f, 1f),
            };

            AddInternal(downloadTracker = new DownloadTracker
            {
                BeatmapSet = { BindTarget = BeatmapSet }
            });
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            Picker.Beatmap.ValueChanged += b =>
            {
                details.Beatmap = b.NewValue;
                externalLink.Link = $@"{api.WebsiteRootUrl}/beatmapsets/{BeatmapSet.Value?.OnlineBeatmapSetID}#{b.NewValue?.Ruleset.ShortName}/{b.NewValue?.OnlineBeatmapID}";
            };

            coverGradient.Colour = ColourInfo.GradientVertical(colourProvider.Background6.Opacity(0.3f), colourProvider.Background6.Opacity(0.8f));
            onlineStatusPill.BackgroundColour = colourProvider.Background6;

            state.BindValueChanged(_ => updateDownloadButtons());

            BeatmapSet.BindValueChanged(setInfo =>
            {
                Picker.BeatmapSet = RulesetSelector.BeatmapSet = author.BeatmapSet = beatmapAvailability.BeatmapSet = details.BeatmapSet = setInfo.NewValue;
                cover.BeatmapSet = setInfo.NewValue;

                if (setInfo.NewValue == null)
                {
                    onlineStatusPill.FadeTo(0.5f, 500, Easing.OutQuint);
                    fadeContent.Hide();

                    loading.Show();

                    downloadButtonsContainer.FadeOut(transition_duration);
                    favouriteButton.FadeOut(transition_duration);
                }
                else
                {
                    fadeContent.FadeIn(500, Easing.OutQuint);

                    loading.Hide();

                    title.Text = setInfo.NewValue.Metadata.Title ?? string.Empty;
                    artist.Text = setInfo.NewValue.Metadata.Artist ?? string.Empty;

                    explicitContentPill.Alpha = setInfo.NewValue.OnlineInfo.HasExplicitContent ? 1 : 0;

                    onlineStatusPill.FadeIn(500, Easing.OutQuint);
                    onlineStatusPill.Status = setInfo.NewValue.OnlineInfo.Status;

                    downloadButtonsContainer.FadeIn(transition_duration);
                    favouriteButton.FadeIn(transition_duration);

                    updateDownloadButtons();
                }
            }, true);
        }

        protected override Drawable CreateContent() => new Container
        {
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            Children = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        cover = new UpdateableBeatmapSetCover
                        {
                            RelativeSizeAxes = Axes.Both,
                            Masking = true,
                        },
                        coverGradient = new Box
                        {
                            RelativeSizeAxes = Axes.Both
                        },
                    },
                },
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding
                    {
                        Vertical = BeatmapSetOverlay.Y_PADDING,
                        Left = BeatmapSetOverlay.X_PADDING,
                        Right = BeatmapSetOverlay.X_PADDING + BeatmapSetOverlay.RIGHT_WIDTH,
                    },
                    Children = new Drawable[]
                    {
                        fadeContent = new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Vertical,
                            Children = new Drawable[]
                            {
                                new Container
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Child = Picker = new BeatmapPicker(),
                                },
                                new FillFlowContainer
                                {
                                    Direction = FillDirection.Horizontal,
                                    AutoSizeAxes = Axes.Both,
                                    Margin = new MarginPadding { Top = 15 },
                                    Children = new Drawable[]
                                    {
                                        title = new OsuSpriteText
                                        {
                                            Font = OsuFont.GetFont(size: 30, weight: FontWeight.SemiBold, italics: true)
                                        },
                                        externalLink = new ExternalLinkButton
                                        {
                                            Anchor = Anchor.BottomLeft,
                                            Origin = Anchor.BottomLeft,
                                            Margin = new MarginPadding { Left = 5, Bottom = 4 }, // To better lineup with the font
                                        },
                                        explicitContentPill = new ExplicitContentBeatmapPill
                                        {
                                            Alpha = 0f,
                                            Anchor = Anchor.BottomLeft,
                                            Origin = Anchor.BottomLeft,
                                            Margin = new MarginPadding { Left = 10, Bottom = 4 },
                                        }
                                    }
                                },
                                artist = new OsuSpriteText
                                {
                                    Font = OsuFont.GetFont(size: 20, weight: FontWeight.Medium, italics: true),
                                    Margin = new MarginPadding { Bottom = 20 }
                                },
                                new Container
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Child = author = new AuthorInfo(),
                                },
                                beatmapAvailability = new BeatmapAvailability(),
                                new Container
                                {
                                    RelativeSizeAxes = Axes.X,
                                    Height = buttons_height,
                                    Margin = new MarginPadding { Top = 10 },
                                    Children = new Drawable[]
                                    {
                                        favouriteButton = new FavouriteButton
                                        {
                                            BeatmapSet = { BindTarget = BeatmapSet }
                                        },
                                        downloadButtonsContainer = new FillFlowContainer
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Padding = new MarginPadding { Left = buttons_height + buttons_spacing },
                                            Spacing = new Vector2(buttons_spacing),
                                        },
                                    },
                                },
                            },
                        },
                    }
                },
                loading = new LoadingSpinner
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Scale = new Vector2(1.5f),
                },
                new FillFlowContainer
                {
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                    AutoSizeAxes = Axes.Both,
                    Margin = new MarginPadding { Top = BeatmapSetOverlay.Y_PADDING, Right = BeatmapSetOverlay.X_PADDING },
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(10),
                    Children = new Drawable[]
                    {
                        onlineStatusPill = new BeatmapSetOnlineStatusPill
                        {
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                            TextSize = 14,
                            TextPadding = new MarginPadding { Horizontal = 35, Vertical = 10 }
                        },
                        details = new Details(),
                    },
                },
            }
        };

        private void updateDownloadButtons()
        {
            if (BeatmapSet.Value == null) return;

            if ((BeatmapSet.Value.OnlineInfo.Availability?.DownloadDisabled ?? false) && state.Value != DownloadState.LocallyAvailable)
            {
                downloadButtonsContainer.Clear();
                return;
            }

            switch (state.Value)
            {
                case DownloadState.LocallyAvailable:
                    // temporary for UX until new design is implemented.
                    downloadButtonsContainer.Child = new BeatmapPanelDownloadButton(BeatmapSet.Value)
                    {
                        Width = 50,
                        RelativeSizeAxes = Axes.Y,
                        SelectedBeatmap = { BindTarget = Picker.Beatmap }
                    };
                    break;

                case DownloadState.Downloading:
                case DownloadState.Importing:
                    // temporary to avoid showing two buttons for maps with novideo. will be fixed in new beatmap overlay design.
                    downloadButtonsContainer.Child = new HeaderDownloadButton(BeatmapSet.Value);
                    break;

                default:
                    downloadButtonsContainer.Child = new HeaderDownloadButton(BeatmapSet.Value);
                    if (BeatmapSet.Value.OnlineInfo.HasVideo)
                        downloadButtonsContainer.Add(new HeaderDownloadButton(BeatmapSet.Value, true));
                    break;
            }
        }

        private class DownloadTracker : BeatmapDownloadTrackingComposite
        {
            public new Bindable<DownloadState> State => base.State;
        }

        protected override OverlayTitle CreateTitle() => new BeatmapHeaderTitle();

        protected override Drawable CreateTitleContent() => RulesetSelector = new BeatmapRulesetSelector
        {
            Current = ruleset
        };

        private class BeatmapHeaderTitle : OverlayTitle
        {
            public BeatmapHeaderTitle()
            {
                Title = "beatmap info";
                IconTexture = "Icons/Hexacons/beatmap";
            }
        }
    }
}
