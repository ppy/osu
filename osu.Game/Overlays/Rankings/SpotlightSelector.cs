// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osuTK;
using System;

namespace osu.Game.Overlays.Rankings
{
    public class SpotlightSelector : Container
    {
        private readonly Box background;
        private readonly SpotlightsDropdown dropdown;
        private readonly DimmedLoadingLayer loading;

        [Resolved]
        private IAPIProvider api { get; set; }

        public readonly Bindable<APISpotlight> SelectedSpotlight = new Bindable<APISpotlight>();

        private readonly InfoCoulmn startDateColumn;
        private readonly InfoCoulmn endDateColumn;

        public SpotlightSelector()
        {
            RelativeSizeAxes = Axes.X;
            Height = 100;
            Children = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Horizontal = UserProfileOverlay.CONTENT_X_MARGIN, Vertical = 10 },
                    Children = new Drawable[]
                    {
                        dropdown = new SpotlightsDropdown
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            RelativeSizeAxes = Axes.X,
                            Current = SelectedSpotlight,
                            Depth = -float.MaxValue
                        },
                        new FillFlowContainer
                        {
                            Anchor = Anchor.BottomRight,
                            Origin = Anchor.BottomRight,
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Horizontal,
                            Spacing = new Vector2(15, 0),
                            Children = new Drawable[]
                            {
                                startDateColumn = new InfoCoulmn(@"Start Date"),
                                endDateColumn = new InfoCoulmn(@"End Date"),
                            }
                        }
                    }
                },
                loading = new DimmedLoadingLayer(),
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            background.Colour = colours.GreySeafoam;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            SelectedSpotlight.BindValueChanged(onSelectionChanged);
        }

        public void FetchSpotlights()
        {
            loading.Show();

            var request = new GetSpotlightsRequest();
            request.Success += response =>
            {
                dropdown.Items = response.Spotlights;
                loading.Hide();
            };
            api.Queue(request);
        }

        private void onSelectionChanged(ValueChangedEvent<APISpotlight> spotlight)
        {
            startDateColumn.Value = dateToString(spotlight.NewValue.StartDate);
            endDateColumn.Value = dateToString(spotlight.NewValue.EndDate);
        }

        private string dateToString(DateTimeOffset date) => $"{date.Year}-{date.Month:D2}-{date.Day:D2}";

        private class InfoCoulmn : FillFlowContainer
        {
            public string Value
            {
                set => valueText.Text = value;
            }

            private readonly OsuSpriteText valueText;

            public InfoCoulmn(string name)
            {
                AutoSizeAxes = Axes.Both;
                Direction = FillDirection.Vertical;
                Children = new Drawable[]
                {
                    new OsuSpriteText
                    {
                        Text = name,
                        Font = OsuFont.GetFont(size: 10),
                    },
                    new Container
                    {
                        AutoSizeAxes = Axes.X,
                        Height = 20,
                        Child = valueText = new OsuSpriteText
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            Font = OsuFont.GetFont(size: 18, weight: FontWeight.Light),
                        }
                    }
                };
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                valueText.Colour = colours.GreySeafoamLighter;
            }
        }

        private class SpotlightsDropdown : OsuDropdown<APISpotlight>
        {
            private DropdownMenu menu;

            protected override DropdownMenu CreateMenu() => menu = base.CreateMenu().With(m => m.MaxHeight = 400);

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                menu.BackgroundColour = colours.Gray1;
                AccentColour = colours.GreySeafoamDarker;
            }
        }
    }
}
