// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osuTK;
using System;
using System.Collections.Generic;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Online.API.Requests;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Localisation;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Overlays.Rankings
{
    public partial class SpotlightSelector : CompositeDrawable, IHasCurrentValue<APISpotlight>
    {
        private readonly BindableWithCurrent<APISpotlight> current = new BindableWithCurrent<APISpotlight>();
        public readonly Bindable<RankingsSortCriteria> Sort = new Bindable<RankingsSortCriteria>();

        public Bindable<APISpotlight> Current
        {
            get => current.Current;
            set => current.Current = value;
        }

        public IEnumerable<APISpotlight> Spotlights
        {
            get => dropdown.Items;
            set => dropdown.Items = value;
        }

        private readonly Box background;
        private readonly RankingSelectorDropdown<APISpotlight> dropdown;
        private readonly InfoColumn startDateColumn;
        private readonly InfoColumn endDateColumn;
        private readonly InfoColumn mapCountColumn;
        private readonly InfoColumn participantsColumn;

        public SpotlightSelector()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            InternalChildren = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                },
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding { Horizontal = WaveOverlayContainer.HORIZONTAL_PADDING },
                    Child = new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Children = new Drawable[]
                        {
                            new Container
                            {
                                Margin = new MarginPadding { Vertical = 20 },
                                Padding = new MarginPadding { Vertical = 20 },
                                RelativeSizeAxes = Axes.X,
                                Height = 40,
                                Depth = -float.MaxValue,
                                Child = dropdown = new RankingSelectorDropdown<APISpotlight>
                                {
                                    RelativeSizeAxes = Axes.X,
                                    Current = Current
                                }
                            },
                            new Container
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Children = new Drawable[]
                                {
                                    new FillFlowContainer
                                    {
                                        AutoSizeAxes = Axes.Both,
                                        Direction = FillDirection.Horizontal,
                                        Spacing = new Vector2(10, 0),
                                        Margin = new MarginPadding { Bottom = 5 },
                                        Children = new Drawable[]
                                        {
                                            startDateColumn = new InfoColumn(RankingsStrings.SpotlightStartDate),
                                            endDateColumn = new InfoColumn(RankingsStrings.SpotlightEndDate),
                                            mapCountColumn = new InfoColumn(RankingsStrings.SpotlightMapCount),
                                            participantsColumn = new InfoColumn(RankingsStrings.SpotlightParticipants)
                                        }
                                    },
                                    new RankingsSortTabControl
                                    {
                                        Anchor = Anchor.CentreRight,
                                        Origin = Anchor.CentreRight,
                                        Current = Sort
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            background.Colour = colourProvider.Dark3;
        }

        public void ShowInfo(GetSpotlightRankingsResponse response)
        {
            startDateColumn.Value = dateToString(response.Spotlight.StartDate);
            endDateColumn.Value = dateToString(response.Spotlight.EndDate);
            mapCountColumn.Value = response.BeatmapSets.Count.ToLocalisableString(@"N0");
            participantsColumn.Value = response.Spotlight.Participants?.ToLocalisableString(@"N0") ?? default;
        }

        private LocalisableString dateToString(DateTimeOffset date) => date.ToLocalisableString(@"yyyy-MM-dd");

        private partial class InfoColumn : FillFlowContainer
        {
            public LocalisableString Value
            {
                set => valueText.Text = value;
            }

            private readonly OsuSpriteText valueText;

            public InfoColumn(LocalisableString name)
            {
                AutoSizeAxes = Axes.Both;
                Direction = FillDirection.Vertical;
                Padding = new MarginPadding { Vertical = 15 };
                Children = new Drawable[]
                {
                    new OsuSpriteText
                    {
                        Text = name,
                        Font = OsuFont.GetFont(size: 10, weight: FontWeight.Regular),
                    },
                    new Container
                    {
                        AutoSizeAxes = Axes.X,
                        Height = 25,
                        Child = valueText = new OsuSpriteText
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Font = OsuFont.GetFont(size: 20, weight: FontWeight.Light),
                        }
                    }
                };
            }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                valueText.Colour = colourProvider.Content2;
            }
        }
    }
}
