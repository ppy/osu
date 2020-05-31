﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK;
using osu.Framework.Bindables;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osuTK.Graphics;
using osu.Game.Rulesets;

namespace osu.Game.Overlays.BeatmapListing
{
    public class BeatmapListingSearchControl : CompositeDrawable
    {
        public Bindable<string> Query => textBox.Current;

        public Bindable<RulesetInfo> Ruleset => modeFilter.Current;

        public Bindable<SearchCategory> Category => categoryFilter.Current;

        public Bindable<SearchGenre> Genre => genreFilter.Current;

        public Bindable<SearchLanguage> Language => languageFilter.Current;

        public BeatmapSetInfo BeatmapSet
        {
            set
            {
                if (value == null || string.IsNullOrEmpty(value.OnlineInfo.Covers.Cover))
                {
                    beatmapCover.FadeOut(600, Easing.OutQuint);
                    return;
                }

                beatmapCover.BeatmapSet = value;
                beatmapCover.FadeTo(0.1f, 200, Easing.OutQuint);
            }
        }

        private readonly BeatmapSearchTextBox textBox;
        private readonly BeatmapSearchRulesetFilterRow modeFilter;
        private readonly BeatmapSearchFilterRow<SearchCategory> categoryFilter;
        private readonly BeatmapSearchFilterRow<SearchGenre> genreFilter;
        private readonly BeatmapSearchFilterRow<SearchLanguage> languageFilter;

        private readonly Box background;
        private readonly UpdateableBeatmapSetCover beatmapCover;

        public BeatmapListingSearchControl()
        {
            AutoSizeAxes = Axes.Y;
            RelativeSizeAxes = Axes.X;
            AddRangeInternal(new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    Child = beatmapCover = new UpdateableBeatmapSetCover
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0,
                    }
                },
                new Container
                {
                    AutoSizeAxes = Axes.Y,
                    RelativeSizeAxes = Axes.X,
                    Padding = new MarginPadding
                    {
                        Vertical = 20,
                        Horizontal = 40,
                    },
                    Child = new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Y,
                        RelativeSizeAxes = Axes.X,
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(0, 20),
                        Children = new Drawable[]
                        {
                            textBox = new BeatmapSearchTextBox
                            {
                                RelativeSizeAxes = Axes.X,
                            },
                            new ReverseChildIDFillFlowContainer<Drawable>
                            {
                                AutoSizeAxes = Axes.Y,
                                RelativeSizeAxes = Axes.X,
                                Direction = FillDirection.Vertical,
                                Padding = new MarginPadding { Horizontal = 10 },
                                Children = new Drawable[]
                                {
                                    modeFilter = new BeatmapSearchRulesetFilterRow(),
                                    categoryFilter = new BeatmapSearchFilterRow<SearchCategory>(@"Categories"),
                                    genreFilter = new BeatmapSearchFilterRow<SearchGenre>(@"Genre"),
                                    languageFilter = new BeatmapSearchFilterRow<SearchLanguage>(@"Language"),
                                }
                            }
                        }
                    }
                }
            });

            categoryFilter.Current.Value = SearchCategory.Leaderboard;
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            background.Colour = colourProvider.Dark6;
        }

        public void TakeFocus() => textBox.TakeFocus();

        private class BeatmapSearchTextBox : SearchTextBox
        {
            protected override Color4 SelectionColour => Color4.Gray;

            public BeatmapSearchTextBox()
            {
                PlaceholderText = @"type in keywords...";
            }
        }
    }
}
