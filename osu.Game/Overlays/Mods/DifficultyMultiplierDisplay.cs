// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Rulesets.Mods;
using osuTK;

namespace osu.Game.Overlays.Mods
{
    /// <summary>
    /// On the mod select overlay, this provides a local updating view of the aggregate score multiplier coming from mods.
    /// </summary>
    public partial class DifficultyMultiplierDisplay : ModFooterInformationDisplay, IHasCurrentValue<double>
    {
        public const float HEIGHT = 42;

        public Bindable<double> Current
        {
            get => current.Current;
            set => current.Current = value;
        }

        private readonly BindableWithCurrent<double> current = new BindableWithCurrent<double>();

        private const float transition_duration = 200;

        private RollingCounter<double> counter = null!;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        public DifficultyMultiplierDisplay()
        {
            Current.Default = 1d;
            Current.Value = 1d;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            LeftContent.AddRange(new Drawable[]
            {
                new OsuSpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Shear = new Vector2(-ShearedOverlayContainer.SHEAR, 0),
                    Text = DifficultyMultiplierDisplayStrings.DifficultyMultiplier,
                    Font = OsuFont.Default.With(size: 17, weight: FontWeight.SemiBold)
                }
            });

            RightContent.Add(counter = new EffectCounter
            {
                Margin = new MarginPadding(10),
                AutoSizeAxes = Axes.Y,
                Width = 40,
                Shear = new Vector2(-ShearedOverlayContainer.SHEAR, 0),
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Current = { BindTarget = Current }
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Current.BindValueChanged(e =>
            {
                var effect = calculateEffectForComparison(e.NewValue.CompareTo(Current.Default));
                setColours(effect);
            }, true);

            // required to prevent the counter initially rolling up from 0 to 1
            // due to `Current.Value` having a nonstandard default value of 1.
            counter.SetCountWithoutRolling(Current.Value);
        }

        /// <summary>
        /// Fades colours of text and its background according to displayed value.
        /// </summary>
        /// <param name="effect">Effect of the value.</param>
        private void setColours(ModEffect effect)
        {
            switch (effect)
            {
                case ModEffect.NotChanged:
                    MainBackground.FadeColour(colourProvider.Background4, transition_duration, Easing.OutQuint);
                    counter.FadeColour(Colour4.White, transition_duration, Easing.OutQuint);
                    break;

                case ModEffect.DifficultyReduction:
                    MainBackground.FadeColour(colours.ForModType(ModType.DifficultyReduction), transition_duration, Easing.OutQuint);
                    counter.FadeColour(colourProvider.Background5, transition_duration, Easing.OutQuint);
                    break;

                case ModEffect.DifficultyIncrease:
                    MainBackground.FadeColour(colours.ForModType(ModType.DifficultyIncrease), transition_duration, Easing.OutQuint);
                    counter.FadeColour(colourProvider.Background5, transition_duration, Easing.OutQuint);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(effect));
            }
        }

        /// <summary>
        /// Converts signed integer into <see cref="ModEffect"/>. Negative values are counted as difficulty reduction, positive as increase.
        /// </summary>
        /// <param name="comparison">Value to convert. Will arrive from comparison between <see cref="Current"/> bindable once it changes and it's <see cref="Bindable{T}.Default"/>.</param>
        /// <returns>Effect of the value.</returns>
        private static ModEffect calculateEffectForComparison(int comparison)
        {
            if (comparison == 0)
                return ModEffect.NotChanged;
            if (comparison < 0)
                return ModEffect.DifficultyReduction;

            return ModEffect.DifficultyIncrease;
        }

        protected enum ModEffect
        {
            NotChanged,
            DifficultyReduction,
            DifficultyIncrease
        }

        private partial class EffectCounter : RollingCounter<double>
        {
            protected override double RollingDuration => 500;

            protected override LocalisableString FormatCount(double count) => count.ToLocalisableString(@"0.00x");

            protected override OsuSpriteText CreateSpriteText() => new OsuSpriteText
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Font = OsuFont.Default.With(size: 17, weight: FontWeight.SemiBold)
            };
        }
    }
}
