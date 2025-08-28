// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Localisation;
using osu.Framework.Utils;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Mods;
using osuTK.Graphics;

namespace osu.Game.Overlays.Mods
{
    public partial class VerticalAttributeDisplay : Container, IHasCustomTooltip<RulesetBeatmapAttribute?>
    {
        private readonly BindableWithCurrent<double> current = new BindableWithCurrent<double>();

        /// <summary>
        /// Text to display in the top area of the display.
        /// </summary>
        public LocalisableString Label
        {
            get => text.Text;
            set => text.Text = value;
        }

        private readonly EffectCounter counter;
        private readonly OsuSpriteText text;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        public VerticalAttributeDisplay()
        {
            AutoSizeAxes = Axes.X;
            RelativeSizeAxes = Axes.Y;

            Origin = Anchor.CentreLeft;
            Anchor = Anchor.CentreLeft;

            InternalChild = new FillFlowContainer
            {
                Origin = Anchor.CentreLeft,
                Anchor = Anchor.CentreLeft,
                RelativeSizeAxes = Axes.Y,
                Width = 42,
                Direction = FillDirection.Vertical,
                Children = new Drawable[]
                {
                    text = new OsuSpriteText
                    {
                        Origin = Anchor.Centre,
                        Anchor = Anchor.Centre,
                        Font = OsuFont.Default.With(size: 20, weight: FontWeight.Bold)
                    },
                    counter = new EffectCounter
                    {
                        Origin = Anchor.Centre,
                        Anchor = Anchor.Centre,
                        Current = { BindTarget = current },
                    }
                }
            };
        }

        public void SetAttribute(RulesetBeatmapAttribute? attribute)
        {
            if (attribute != null)
            {
                text.Text = attribute.Acronym;
                current.Value = attribute.AdjustedValue;
                var effect = calculateEffect(attribute.OriginalValue, attribute.AdjustedValue);
                updateTextColor(effect);
                Alpha = 1;
            }
            else
                Alpha = 0;

            TooltipContent = attribute;
        }

        private static ModEffect calculateEffect(double oldValue, double newValue)
        {
            if (Precision.AlmostEquals(newValue, oldValue, 0.01))
                return ModEffect.NotChanged;
            if (newValue < oldValue)
                return ModEffect.DifficultyReduction;

            return ModEffect.DifficultyIncrease;
        }

        private void updateTextColor(ModEffect effect)
        {
            Color4 newColor;

            switch (effect)
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
                    throw new ArgumentOutOfRangeException(nameof(effect), effect, null);
            }

            text.Colour = newColor;
            counter.Colour = newColor;
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

        public ITooltip<RulesetBeatmapAttribute?> GetCustomTooltip() => new BeatmapAttributeTooltip();
        public RulesetBeatmapAttribute? TooltipContent { get; set; }
    }
}
