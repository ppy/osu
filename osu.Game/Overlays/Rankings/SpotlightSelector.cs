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
using osu.Game.Online.API.Requests.Responses;
using osuTK;
using System;
using System.Collections.Generic;
using osu.Framework.Graphics.UserInterface;

namespace osu.Game.Overlays.Rankings
{
    public class SpotlightSelector : CompositeDrawable, IHasCurrentValue<APISpotlight>
    {
        private readonly Box background;
        private readonly SpotlightsDropdown dropdown;

        private readonly BindableWithCurrent<APISpotlight> current = new BindableWithCurrent<APISpotlight>();

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

        private readonly InfoColumn startDateColumn;
        private readonly InfoColumn endDateColumn;

        public SpotlightSelector()
        {
            RelativeSizeAxes = Axes.X;
            Height = 100;

            InternalChildren = new Drawable[]
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
                            Current = Current,
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
                                startDateColumn = new InfoColumn(@"Start Date"),
                                endDateColumn = new InfoColumn(@"End Date"),
                            }
                        }
                    }
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            background.Colour = colourProvider.Dark3;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Current.BindValueChanged(onCurrentChanged);
        }

        private void onCurrentChanged(ValueChangedEvent<APISpotlight> spotlight)
        {
            startDateColumn.Value = dateToString(spotlight.NewValue.StartDate);
            endDateColumn.Value = dateToString(spotlight.NewValue.EndDate);
        }

        private string dateToString(DateTimeOffset date) => date.ToString("yyyy-MM-dd");

        private class InfoColumn : FillFlowContainer
        {
            public string Value
            {
                set => valueText.Text = value;
            }

            private readonly OsuSpriteText valueText;

            public InfoColumn(string name)
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
            private void load(OverlayColourProvider colourProvider)
            {
                valueText.Colour = colourProvider.Content2;
            }
        }

        private class SpotlightsDropdown : OsuDropdown<APISpotlight>
        {
            private DropdownMenu menu;

            protected override DropdownMenu CreateMenu() => menu = base.CreateMenu().With(m => m.MaxHeight = 400);

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                menu.BackgroundColour = colourProvider.Background5;
                AccentColour = colourProvider.Background6;
            }
        }
    }
}
