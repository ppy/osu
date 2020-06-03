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
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online;
using osu.Game.Overlays.BeatmapListing.Panels;
using osu.Game.Overlays.BeatmapSet.Buttons;
using osu.Game.Rulesets;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.BeatmapSet
{
    public class Header : BeatmapDownloadTrackingComposite
    {
        private const float transition_duration = 200;
        private const float buttons_height = 45;
        private const float buttons_spacing = 5;

        private readonly UpdateableBeatmapSetCover cover;
        private readonly Box coverGradient;
        private readonly OsuSpriteText title, artist;
        private readonly AuthorInfo author;
        private readonly FillFlowContainer downloadButtonsContainer;
        private readonly BeatmapAvailability beatmapAvailability;
        private readonly BeatmapSetOnlineStatusPill onlineStatusPill;
        public Details Details;

        public bool DownloadButtonsVisible => downloadButtonsContainer.Any();

        public BeatmapRulesetSelector RulesetSelector => beatmapSetHeader.RulesetSelector;
        public readonly BeatmapPicker Picker;

        private readonly FavouriteButton favouriteButton;
        private readonly FillFlowContainer fadeContent;
        private readonly LoadingSpinner loading;
        private readonly BeatmapSetHeader beatmapSetHeader;

        [Cached(typeof(IBindable<RulesetInfo>))]
        private readonly Bindable<RulesetInfo> ruleset = new Bindable<RulesetInfo>();

        public Header()
        {
            ExternalLinkButton externalLink;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Masking = true;

            EdgeEffect = new EdgeEffectParameters
            {
                Colour = Color4.Black.Opacity(0.25f),
                Type = EdgeEffectType.Shadow,
                Radius = 3,
                Offset = new Vector2(0f, 1f),
            };

            InternalChild = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Children = new Drawable[]
                {
                    beatmapSetHeader = new BeatmapSetHeader
                    {
                        Ruleset = { BindTarget = ruleset },
                    },
                    new Container
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
                                                        Margin = new MarginPadding { Left = 3, Bottom = 4 }, // To better lineup with the font
                                                    },
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
                                    Details = new Details(),
                                },
                            },
                        },
                    },
                }
            };

            Picker.Beatmap.ValueChanged += b =>
            {
                Details.Beatmap = b.NewValue;
                externalLink.Link = $@"https://osu.ppy.sh/beatmapsets/{BeatmapSet.Value?.OnlineBeatmapSetID}#{b.NewValue?.Ruleset.ShortName}/{b.NewValue?.OnlineBeatmapID}";
            };
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            coverGradient.Colour = ColourInfo.GradientVertical(colourProvider.Background6.Opacity(0.3f), colourProvider.Background6.Opacity(0.8f));
            onlineStatusPill.BackgroundColour = colourProvider.Background6;

            State.BindValueChanged(_ => updateDownloadButtons());

            BeatmapSet.BindValueChanged(setInfo =>
            {
                Picker.BeatmapSet = RulesetSelector.BeatmapSet = author.BeatmapSet = beatmapAvailability.BeatmapSet = Details.BeatmapSet = setInfo.NewValue;
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

                    onlineStatusPill.FadeIn(500, Easing.OutQuint);
                    onlineStatusPill.Status = setInfo.NewValue.OnlineInfo.Status;

                    downloadButtonsContainer.FadeIn(transition_duration);
                    favouriteButton.FadeIn(transition_duration);

                    updateDownloadButtons();
                }
            }, true);
        }

        private void updateDownloadButtons()
        {
            if (BeatmapSet.Value == null) return;

            if ((BeatmapSet.Value.OnlineInfo.Availability?.DownloadDisabled ?? false) && State.Value != DownloadState.LocallyAvailable)
            {
                downloadButtonsContainer.Clear();
                return;
            }

            switch (State.Value)
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
                case DownloadState.Downloaded:
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
    }
}
