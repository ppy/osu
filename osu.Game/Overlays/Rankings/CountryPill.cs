// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Users;
using osu.Game.Users.Drawables;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Rankings
{
    public partial class CountryPill : CompositeDrawable, IHasCurrentValue<CountryCode>
    {
        private const int duration = 200;

        private readonly BindableWithCurrent<CountryCode> current = new BindableWithCurrent<CountryCode>();

        public Bindable<CountryCode> Current
        {
            get => current.Current;
            set => current.Current = value;
        }

        private readonly Container content;
        private readonly Box background;
        private readonly UpdateableFlag flag;
        private readonly OsuSpriteText countryName;

        public CountryPill()
        {
            AutoSizeAxes = Axes.Both;

            InternalChild = content = new CircularContainer
            {
                Height = 30,
                AutoSizeDuration = duration,
                AutoSizeEasing = Easing.OutQuint,
                Masking = true,
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
                        Margin = new MarginPadding { Horizontal = 15 },
                        Direction = FillDirection.Horizontal,
                        Spacing = new Vector2(15, 0),
                        Children = new Drawable[]
                        {
                            new FillFlowContainer
                            {
                                RelativeSizeAxes = Axes.Y,
                                AutoSizeAxes = Axes.X,
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Direction = FillDirection.Horizontal,
                                Spacing = new Vector2(5, 0),
                                Children = new Drawable[]
                                {
                                    flag = new UpdateableFlag
                                    {
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        Size = new Vector2(28, 20)
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
                                Action = Current.SetDefault,
                            }
                        }
                    }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            background.Colour = colourProvider.Background5;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Current.BindValueChanged(onCountryChanged, true);
        }

        public void Expand()
        {
            content.ClearTransforms();
            content.AutoSizeAxes = Axes.X;

            this.FadeIn(duration, Easing.OutQuint);
        }

        public void Collapse()
        {
            content.ClearTransforms();
            content.AutoSizeAxes = Axes.None;
            content.ResizeWidthTo(0, duration, Easing.OutQuint);

            this.FadeOut(duration, Easing.OutQuint);
        }

        private void onCountryChanged(ValueChangedEvent<CountryCode> country)
        {
            if (Current.Value == CountryCode.Unknown)
                return;

            flag.CountryCode = country.NewValue;
            countryName.Text = country.NewValue.GetDescription();
        }

        private partial class CloseButton : OsuHoverContainer
        {
            private readonly SpriteIcon icon;

            protected override IEnumerable<Drawable> EffectTargets => new[] { icon };

            public CloseButton()
            {
                AutoSizeAxes = Axes.Both;
                Add(icon = new SpriteIcon
                {
                    Size = new Vector2(10),
                    Icon = FontAwesome.Solid.Times
                });
            }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                IdleColour = colourProvider.Light2;
                HoverColour = Color4.White;
            }
        }
    }
}
