// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Overlays.Rankings
{
    public class CountryFilter : Container
    {
        private const int duration = 200;
        private const int height = 50;

        public readonly Bindable<Country> Country = new Bindable<Country>();

        private readonly Box background;
        private readonly CountryPill countryPill;
        private readonly Container content;

        public CountryFilter()
        {
            RelativeSizeAxes = Axes.X;
            Child = content = new Container
            {
                RelativeSizeAxes = Axes.X,
                Height = height,
                Alpha = 0,
                Children = new Drawable[]
                {
                    background = new Box
                    {
                        RelativeSizeAxes = Axes.Both
                    },
                    new FillFlowContainer
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Direction = FillDirection.Horizontal,
                        Spacing = new Vector2(10, 0),
                        Margin = new MarginPadding { Left = UserProfileOverlay.CONTENT_X_MARGIN },
                        Children = new Drawable[]
                        {
                            new OsuSpriteText
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Text = @"filtered by country:",
                                Font = OsuFont.GetFont(size: 14)
                            },
                            countryPill = new CountryPill
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Alpha = 0,
                                Country = { BindTarget = Country }
                            }
                        }
                    }
                }
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
            Country.BindValueChanged(onCountryChanged, true);
        }

        private void onCountryChanged(ValueChangedEvent<Country> country)
        {
            if (country.NewValue == null)
            {
                countryPill.Collapse();
                this.ResizeHeightTo(0, duration, Easing.OutQuint);
                content.FadeOut(duration, Easing.OutQuint);
                return;
            }

            this.ResizeHeightTo(height, duration, Easing.OutQuint);
            content.FadeIn(duration, Easing.OutQuint);
            countryPill.Expand();
        }
    }
}
