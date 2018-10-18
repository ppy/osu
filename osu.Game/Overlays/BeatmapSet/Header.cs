// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

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
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Overlays.BeatmapSet
{
    public class Header : Container
    {
        private const float transition_duration = 200;
        private const float tabs_height = 50;
        private const float buttons_height = 45;
        private const float buttons_spacing = 5;

        private readonly Box tabsBg;
        private readonly UpdateableBeatmapSetCover cover;
        private readonly OsuSpriteText title, artist;
        private readonly Container noVideoButtons;
        private readonly FillFlowContainer videoButtons;
        private readonly AuthorInfo author;
        private readonly Container downloadButtonsContainer;
        private readonly BeatmapSetOnlineStatusPill onlineStatusPill;
        public Details Details;

        public readonly BeatmapPicker Picker;

        private BeatmapSetInfo beatmapSet;
        private readonly FavouriteButton favouriteButton;

        public BeatmapSetInfo BeatmapSet
        {
            get { return beatmapSet; }
            set
            {
                if (value == beatmapSet) return;
                beatmapSet = value;

                Picker.BeatmapSet = author.BeatmapSet = Details.BeatmapSet = BeatmapSet;

                updateDisplay();
            }
        }

        private void updateDisplay()
        {
            title.Text = BeatmapSet?.Metadata.Title ?? string.Empty;
            artist.Text = BeatmapSet?.Metadata.Artist ?? string.Empty;
            onlineStatusPill.Status = BeatmapSet?.OnlineInfo.Status ?? BeatmapSetOnlineStatus.None;
            cover.BeatmapSet = BeatmapSet;

            if (BeatmapSet != null)
            {
                downloadButtonsContainer.FadeIn(transition_duration);
                favouriteButton.FadeIn(transition_duration);

                if (BeatmapSet.OnlineInfo.HasVideo)
                {
                    videoButtons.Children = new[]
                    {
                        new DownloadButton(BeatmapSet),
                        new DownloadButton(BeatmapSet, true),
                    };

                    videoButtons.FadeIn(transition_duration);
                    noVideoButtons.FadeOut(transition_duration);
                }
                else
                {
                    noVideoButtons.Child = new DownloadButton(BeatmapSet);

                    noVideoButtons.FadeIn(transition_duration);
                    videoButtons.FadeOut(transition_duration);
                }
            }
            else
            {
                downloadButtonsContainer.FadeOut(transition_duration);
                favouriteButton.FadeOut(transition_duration);
            }
        }

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
            Children = new Drawable[]
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
                                                Font = @"Exo2.0-BoldItalic",
                                                TextSize = 37,
                                            },
                                            externalLink = new ExternalLinkButton
                                            {
                                                Anchor = Anchor.BottomLeft,
                                                Origin = Anchor.BottomLeft,
                                                Margin = new MarginPadding { Left = 3, Bottom = 4 }, //To better lineup with the font
                                            },
                                        }
                                    },
                                    artist = new OsuSpriteText
                                    {
                                        Font = @"Exo2.0-SemiBoldItalic",
                                        TextSize = 25,
                                    },
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
                                            downloadButtonsContainer = new Container
                                            {
                                                RelativeSizeAxes = Axes.Both,
                                                Padding = new MarginPadding { Left = buttons_height + buttons_spacing },
                                                Children = new Drawable[]
                                                {
                                                    noVideoButtons = new Container
                                                    {
                                                        RelativeSizeAxes = Axes.Both,
                                                        Alpha = 0f,
                                                    },
                                                    videoButtons = new FillFlowContainer
                                                    {
                                                        RelativeSizeAxes = Axes.Both,
                                                        Spacing = new Vector2(buttons_spacing),
                                                        Alpha = 0f,
                                                    },
                                                },
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

            Picker.Beatmap.ValueChanged += b => Details.Beatmap = b;
            Picker.Beatmap.ValueChanged += b => externalLink.Link = $@"https://osu.ppy.sh/beatmapsets/{BeatmapSet?.OnlineBeatmapSetID}#{b?.Ruleset.ShortName}/{b?.OnlineBeatmapID}";
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            tabsBg.Colour = colours.Gray3;
            updateDisplay();
        }
    }
}
