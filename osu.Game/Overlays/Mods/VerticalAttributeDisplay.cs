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
using osu.Framework.Utils;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
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

        public Bindable<ModEffect> AdjustType = new Bindable<ModEffect>();

        /// <summary>
        /// Text to display in the top area of the display.
        /// </summary>
        public LocalisableString Label { get; protected set; }

        private readonly EffectCounter counter;
        private readonly OsuSpriteText text;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        private void updateTextColor()
        {
            Color4 newColor;

            switch (AdjustType.Value)
            {
                case ModEffect.NotChanged:
                    newColor = Color4.White;
                    break;

                case ModEffect.DifficultyReduction:
                    newColor = colours.ForModType(ModType.DifficultyReduction);
                    break;

                case ModEffect.DifficultyIncrease:
                    newColor = colours.ForModType(ModType.DifficultyIncrease);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(AdjustType.Value));
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

            AdjustType.BindValueChanged(_ => updateTextColor());

            InternalChild = new FillFlowContainer
            {
                Origin = Anchor.CentreLeft,
                Anchor = Anchor.CentreLeft,
                AutoSizeAxes = Axes.Y,
                Width = 50,
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

        public static ModEffect CalculateEffect(double oldValue, double newValue)
        {
            if (Precision.AlmostEquals(newValue, oldValue, 0.01))
                return ModEffect.NotChanged;
            if (newValue < oldValue)
                return ModEffect.DifficultyReduction;

            return ModEffect.DifficultyIncrease;
        }

        public enum ModEffect
        {
            NotChanged,
            DifficultyReduction,
            DifficultyIncrease,
        }

        private partial class EffectCounter : RollingCounter<double>
        {
            protected override double RollingDuration => 250;

            protected override LocalisableString FormatCount(double count) => count.ToLocalisableString("0.0#");

            protected override OsuSpriteText CreateSpriteText() => new OsuSpriteText
            {
                Font = OsuFont.Default.With(size: 18, weight: FontWeight.SemiBold)
            };
        }
    }
}
