// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Users;
using osu.Game.Users.Drawables;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Rankings
{
    public class CountryPill : CircularContainer
    {
        private const int duration = 200;

        private readonly Box background;
        private readonly UpdateableFlag flag;
        private readonly OsuSpriteText countryName;

        public readonly Bindable<Country> Country = new Bindable<Country>();

        public CountryPill()
        {
            AutoSizeDuration = duration;
            AutoSizeEasing = Easing.OutQuint;
            Height = 25;
            Masking = true;
            Children = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both
                },
                new FillFlowContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Y,
                    AutoSizeAxes = Axes.X,
                    Margin = new MarginPadding { Horizontal = 10 },
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(8, 0),
                    Children = new Drawable[]
                    {
                        new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.Y,
                            AutoSizeAxes = Axes.X,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Direction = FillDirection.Horizontal,
                            Spacing = new Vector2(3, 0),
                            Children = new Drawable[]
                            {
                                flag = new UpdateableFlag
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Size = new Vector2(22, 15)
                                },
                                countryName = new OsuSpriteText
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Font = OsuFont.GetFont(size: 14)
                                }
                            }
                        },
                        new CloseButton
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Action = () => Country.Value = null
                        }
                    }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            background.Colour = colours.GreySeafoamDarker;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Country.BindValueChanged(onCountryChanged, true);
        }

        public void Expand()
        {
            ClearTransforms();
            AutoSizeAxes = Axes.X;
            this.FadeIn(duration, Easing.OutQuint);
        }

        public void Collapse()
        {
            ClearTransforms();
            AutoSizeAxes = Axes.None;
            this.ResizeWidthTo(0, duration, Easing.OutQuint);
            this.FadeOut(duration, Easing.OutQuint);
        }

        private void onCountryChanged(ValueChangedEvent<Country> country)
        {
            if (country.NewValue == null)
                return;

            flag.Country = country.NewValue;
            countryName.Text = country.NewValue.FullName;
        }

        private class CloseButton : OsuHoverContainer
        {
            private readonly SpriteIcon icon;

            protected override IEnumerable<Drawable> EffectTargets => new[] { icon };

            public CloseButton()
            {
                AutoSizeAxes = Axes.Both;
                Add(icon = new SpriteIcon
                {
                    Size = new Vector2(8),
                    Icon = FontAwesome.Solid.Times
                });
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                IdleColour = colours.GreySeafoamLighter;
                HoverColour = Color4.White;
            }
        }
    }
}
