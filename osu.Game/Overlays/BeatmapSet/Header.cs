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
        private readonly Container coverContainer;
        private readonly OsuSpriteText title, artist;
        private readonly Container noVideoButtons;
        private readonly FillFlowContainer videoButtons;
        private readonly AuthorInfo author;
        private readonly Container downloadButtonsContainer;
        private readonly BeatmapSetOnlineStatusPill onlineStatusPill;
        public Details Details;

        private BeatmapManager beatmaps;
        private DelayedLoadWrapper cover;

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

            cover?.FadeOut(400, Easing.Out);
            if (BeatmapSet != null)
            {
                downloadButtonsContainer.FadeIn(transition_duration);
                favouriteButton.FadeIn(transition_duration);

                noVideoButtons.FadeTo(BeatmapSet.OnlineInfo.HasVideo ? 0 : 1, transition_duration);
                videoButtons.FadeTo(BeatmapSet.OnlineInfo.HasVideo ? 1 : 0, transition_duration);

                coverContainer.Add(cover = new DelayedLoadWrapper(
                    new BeatmapSetCover(BeatmapSet)
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        FillMode = FillMode.Fill,
                        OnLoadComplete = d => d.FadeInFromZero(400, Easing.Out),
                    }, 300)
                {
                    RelativeSizeAxes = Axes.Both,
                });
            }
            else
            {
                downloadButtonsContainer.FadeOut(transition_duration);
                favouriteButton.FadeOut(transition_duration);
            }
        }

        public Header()
        {
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
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = Color4.Black,
                                },
                                coverContainer = new Container
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
                                    title = new OsuSpriteText
                                    {
                                        Font = @"Exo2.0-BoldItalic",
                                        TextSize = 37,
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
                                                        Child = new DownloadButton("Download", @"")
                                                        {
                                                            Action = () => download(false),
                                                        },
                                                    },
                                                    videoButtons = new FillFlowContainer
                                                    {
                                                        RelativeSizeAxes = Axes.Both,
                                                        Spacing = new Vector2(buttons_spacing),
                                                        Alpha = 0f,
                                                        Children = new[]
                                                        {
                                                            new DownloadButton("Download", "with Video")
                                                            {
                                                                Action = () => download(false),
                                                            },
                                                            new DownloadButton("Download", "without Video")
                                                            {
                                                                Action = () => download(true),
                                                            },
                                                        },
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
                                onlineStatusPill = new BeatmapSetOnlineStatusPill(14, new MarginPadding { Horizontal = 25, Vertical = 8 })
                                {
                                    Anchor = Anchor.TopRight,
                                    Origin = Anchor.TopRight,
                                },
                                Details = new Details(),
                            },
                        },
                    },
                },
            };

            Picker.Beatmap.ValueChanged += b => Details.Beatmap = b;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, BeatmapManager beatmaps)
        {
            tabsBg.Colour = colours.Gray3;
            this.beatmaps = beatmaps;

            beatmaps.ItemAdded += handleBeatmapAdd;

            updateDisplay();
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            if (beatmaps != null) beatmaps.ItemAdded -= handleBeatmapAdd;
        }

        private void handleBeatmapAdd(BeatmapSetInfo beatmap) => Schedule(() =>
        {
            if (beatmap.OnlineBeatmapSetID == BeatmapSet?.OnlineBeatmapSetID)
                downloadButtonsContainer.FadeOut(transition_duration);
        });

        private void download(bool noVideo)
        {
            if (beatmaps.GetExistingDownload(BeatmapSet) != null)
            {
                downloadButtonsContainer.MoveToX(-5, 50, Easing.OutSine).Then()
                       .MoveToX(5, 100, Easing.InOutSine).Then()
                       .MoveToX(-5, 100, Easing.InOutSine).Then()
                       .MoveToX(0, 50, Easing.InSine).Then();

                return;
            }

            beatmaps.Download(BeatmapSet, noVideo);
        }
    }
}
