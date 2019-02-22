// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.BeatmapSet.Buttons;
using osu.Game.Overlays.Direct;
using osuTK;
using osuTK.Graphics;
using DownloadButton = osu.Game.Overlays.BeatmapSet.Buttons.DownloadButton;

namespace osu.Game.Overlays.BeatmapSet
{
    public class Header : DownloadTrackingComposite
    {
        private const float transition_duration = 200;
        private const float tabs_height = 50;
        private const float buttons_height = 45;
        private const float buttons_spacing = 5;

        private readonly Box tabsBg;
        private readonly UpdateableBeatmapSetCover cover;
        private readonly OsuSpriteText title, artist;
        private readonly AuthorInfo author;
        private readonly FillFlowContainer downloadButtonsContainer;
        private readonly BeatmapSetOnlineStatusPill onlineStatusPill;
        public Details Details;

        public readonly BeatmapPicker Picker;

        private readonly FavouriteButton favouriteButton;

        public Header()
        {
            ExternalLinkButton externalLink;

            RelativeSizeAxes = Axes.X;
            Height = 400;
            Masking = true;

            EdgeEffect = new EdgeEffectParameters
            {
                Colour = Color4.Black.Opacity(0.25f),
                Type = EdgeEffectType.Shadow,
                Radius = 3,
                Offset = new Vector2(0f, 1f),
            };

            InternalChildren = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Height = tabs_height,
                    Children = new[]
                    {
                        tabsBg = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                        },
                    },
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Top = tabs_height },
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
                                },
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = ColourInfo.GradientVertical(Color4.Black.Opacity(0.3f), Color4.Black.Opacity(0.8f)),
                                },
                            },
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding { Top = 20, Bottom = 30, Horizontal = BeatmapSetOverlay.X_PADDING },
                            Child = new FillFlowContainer
                            {
                                RelativeSizeAxes = Axes.Both,
                                Direction = FillDirection.Vertical,
                                Children = new Drawable[]
                                {
                                    new Container
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        Height = 113,
                                        Child = Picker = new BeatmapPicker(),
                                    },
                                    new FillFlowContainer
                                    {
                                        Direction = FillDirection.Horizontal,
                                        AutoSizeAxes = Axes.Both,
                                        Children = new Drawable[]
                                        {
                                            title = new OsuSpriteText
                                            {
                                                Font = OsuFont.GetFont(size: 37, weight: FontWeight.Bold, italics: true)
                                            },
                                            externalLink = new ExternalLinkButton
                                            {
                                                Anchor = Anchor.BottomLeft,
                                                Origin = Anchor.BottomLeft,
                                                Margin = new MarginPadding { Left = 3, Bottom = 4 }, //To better lineup with the font
                                            },
                                        }
                                    },
                                    artist = new OsuSpriteText { Font = OsuFont.GetFont(size: 25, weight: FontWeight.SemiBold, italics: true) },
                                    new Container
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Margin = new MarginPadding { Top = 20 },
                                        Child = author = new AuthorInfo(),
                                    },
                                    new Container
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        Height = buttons_height,
                                        Margin = new MarginPadding { Top = 10 },
                                        Children = new Drawable[]
                                        {
                                            favouriteButton = new FavouriteButton(),
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
                        },
                        new FillFlowContainer
                        {
                            Anchor = Anchor.BottomRight,
                            Origin = Anchor.BottomRight,
                            AutoSizeAxes = Axes.Both,
                            Margin = new MarginPadding { Right = BeatmapSetOverlay.X_PADDING },
                            Direction = FillDirection.Vertical,
                            Spacing = new Vector2(10),
                            Children = new Drawable[]
                            {
                                onlineStatusPill = new BeatmapSetOnlineStatusPill
                                {
                                    Anchor = Anchor.TopRight,
                                    Origin = Anchor.TopRight,
                                    TextSize = 14,
                                    TextPadding = new MarginPadding { Horizontal = 25, Vertical = 8 }
                                },
                                Details = new Details(),
                            },
                        },
                    },
                },
            };

            Picker.Beatmap.ValueChanged += b => Details.Beatmap = b.NewValue;
            Picker.Beatmap.ValueChanged += b => externalLink.Link = $@"https://osu.ppy.sh/beatmapsets/{BeatmapSet.Value?.OnlineBeatmapSetID}#{b.NewValue?.Ruleset.ShortName}/{b.NewValue?.OnlineBeatmapID}";
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            tabsBg.Colour = colours.Gray3;

            State.BindValueChanged(_ => updateDownloadButtons());

            BeatmapSet.BindValueChanged(setInfo =>
            {
                Picker.BeatmapSet = author.BeatmapSet = Details.BeatmapSet = setInfo.NewValue;

                title.Text = setInfo.NewValue?.Metadata.Title ?? string.Empty;
                artist.Text = setInfo.NewValue?.Metadata.Artist ?? string.Empty;
                onlineStatusPill.Status = setInfo.NewValue?.OnlineInfo.Status ?? BeatmapSetOnlineStatus.None;
                cover.BeatmapSet = setInfo.NewValue;

                if (setInfo.NewValue != null)
                {
                    downloadButtonsContainer.FadeIn(transition_duration);
                    favouriteButton.FadeIn(transition_duration);
                }
                else
                {
                    downloadButtonsContainer.FadeOut(transition_duration);
                    favouriteButton.FadeOut(transition_duration);
                }

                updateDownloadButtons();
            }, true);
        }

        private void updateDownloadButtons()
        {
            if (BeatmapSet.Value == null) return;

            switch (State.Value)
            {
                case DownloadState.LocallyAvailable:
                    // temporary for UX until new design is implemented.
                    downloadButtonsContainer.Child = new osu.Game.Overlays.Direct.DownloadButton(BeatmapSet.Value)
                    {
                        Width = 50,
                        RelativeSizeAxes = Axes.Y
                    };
                    break;
                case DownloadState.Downloading:
                case DownloadState.Downloaded:
                    // temporary to avoid showing two buttons for maps with novideo. will be fixed in new beatmap overlay design.
                    downloadButtonsContainer.Child = new DownloadButton(BeatmapSet.Value);
                    break;
                default:
                    downloadButtonsContainer.Child = new DownloadButton(BeatmapSet.Value);
                    if (BeatmapSet.Value.OnlineInfo.HasVideo)
                        downloadButtonsContainer.Add(new DownloadButton(BeatmapSet.Value, true));
                    break;
            }
        }
    }
}
