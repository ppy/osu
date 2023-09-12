// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Mods
{
    public partial class VerticalAttributeDisplay : Container, IHasCurrentValue<double>
    {
        public Bindable<double> Current
        {
            get => current.Current;
            set => current.Current = value;
        }

        private readonly BindableWithCurrent<double> current = new BindableWithCurrent<double>();

        /// <summary>
        /// Text to display in the top area of the display.
        /// </summary>
        public LocalisableString Label { get; protected set; }

        public VerticalAttributeDisplay(LocalisableString label)
        {
            Label = label;

            AutoSizeAxes = Axes.X;

            Origin = Anchor.CentreLeft;
            Anchor = Anchor.CentreLeft;

            InternalChild = new FillFlowContainer
            {
                Origin = Anchor.CentreLeft,
                Anchor = Anchor.CentreLeft,
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical,
                Children = new Drawable[]
                {
                    new OsuSpriteText
                    {
                        Origin = Anchor.Centre,
                        Anchor = Anchor.Centre,
                        Text = Label,
                        Margin = new MarginPadding { Horizontal = 15 }, // to reserve space for 0.XX value
                        Font = OsuFont.Default.With(size: 20, weight: FontWeight.Bold)
                    },
                    new EffectCounter
                    {
                        Origin = Anchor.Centre,
                        Anchor = Anchor.Centre,
                        Current = { BindTarget = Current },
                    }
                }
            };
        }

        private partial class EffectCounter : RollingCounter<double>
        {
            protected override double RollingDuration => 500;

            protected override LocalisableString FormatCount(double count) => count.ToLocalisableString("0.0");

            protected override OsuSpriteText CreateSpriteText() => new OsuSpriteText
            {
                Font = OsuFont.Default.With(size: 18, weight: FontWeight.SemiBold)
            };
        }
    }
}
