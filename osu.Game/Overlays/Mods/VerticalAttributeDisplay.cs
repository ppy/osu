// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osuTK.Graphics;

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

        public Bindable<Ruleset.RateAdjustType> RateChangeType = new Bindable<Ruleset.RateAdjustType>(Ruleset.RateAdjustType.NotChanged);

        /// <summary>
        /// Text to display in the top area of the display.
        /// </summary>
        public LocalisableString Label { get; protected set; }

        private EffectCounter counter;
        private OsuSpriteText text;

        [Resolved]
        private OsuColour colours { get; set; } = null!;
        private void updateTextColor()
        {
            Color4 newColor;
            switch (RateChangeType.Value)
            {
                case Ruleset.RateAdjustType.NotChanged:
                    newColor = Color4.White;
                    break;

                case Ruleset.RateAdjustType.DifficultyReduction:
                    newColor = colours.ForModType(ModType.DifficultyReduction);
                    break;

                case Ruleset.RateAdjustType.DifficultyIncrease:
                    newColor = colours.ForModType(ModType.DifficultyIncrease);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(RateChangeType.Value));
            }

            text.Colour = newColor;
            counter.Colour = newColor;
        }

        public VerticalAttributeDisplay(LocalisableString label)
        {
            Label = label;

            AutoSizeAxes = Axes.X;

            Origin = Anchor.CentreLeft;
            Anchor = Anchor.CentreLeft;

            RateChangeType.BindValueChanged(_ => updateTextColor());

            InternalChild = new FillFlowContainer
            {
                Origin = Anchor.CentreLeft,
                Anchor = Anchor.CentreLeft,
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical,
                Children = new Drawable[]
                {
                    text = new OsuSpriteText
                    {
                        Origin = Anchor.Centre,
                        Anchor = Anchor.Centre,
                        Text = Label,
                        Margin = new MarginPadding { Horizontal = 15 }, // to reserve space for 0.XX value
                        Font = OsuFont.Default.With(size: 20, weight: FontWeight.Bold)
                    },
                    counter = new EffectCounter
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

            protected override LocalisableString FormatCount(double count) => count.ToLocalisableString("0.0#");

            protected override OsuSpriteText CreateSpriteText() => new OsuSpriteText
            {
                Font = OsuFont.Default.With(size: 18, weight: FontWeight.SemiBold)
            };
        }
    }
}
