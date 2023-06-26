// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Overlays.Rankings
{
    public partial class CountryFilter : CompositeDrawable, IHasCurrentValue<CountryCode>
    {
        private const int duration = 200;
        private const int height = 70;

        private readonly BindableWithCurrent<CountryCode> current = new BindableWithCurrent<CountryCode>();

        public Bindable<CountryCode> Current
        {
            get => current.Current;
            set => current.Current = value;
        }

        private readonly Box background;
        private readonly CountryPill countryPill;
        private readonly Container content;

        public CountryFilter()
        {
            RelativeSizeAxes = Axes.X;

            InternalChild = content = new Container
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
                        Margin = new MarginPadding { Left = WaveOverlayContainer.HORIZONTAL_PADDING },
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
                                Current = Current
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

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Current.BindValueChanged(onCountryChanged, true);
        }

        private void onCountryChanged(ValueChangedEvent<CountryCode> country)
        {
            if (Current.Value == CountryCode.Unknown)
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
