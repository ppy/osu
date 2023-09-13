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
using osu.Game.Localisation;
using osu.Game.Rulesets.Mods;
using osuTK;
using osuTK.Graphics;

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

            RightContent.Add(new Container
            {
                Width = 40,
                RelativeSizeAxes = Axes.Y,
                Margin = new MarginPadding(10),
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Child = counter = new EffectCounter
                {
                    Shear = new Vector2(-ShearedOverlayContainer.SHEAR, 0),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Current = { BindTarget = Current }
                }
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Current.BindValueChanged(e =>
            {
                if (e.NewValue > Current.Default)
                {
                    MainBackground
                        .FadeColour(colours.ForModType(ModType.DifficultyIncrease), transition_duration, Easing.OutQuint);
                    counter.FadeColour(colourProvider.Background5, transition_duration, Easing.OutQuint);
                }
                else if (e.NewValue < Current.Default)
                {
                    MainBackground
                        .FadeColour(colours.ForModType(ModType.DifficultyReduction), transition_duration, Easing.OutQuint);
                    counter.FadeColour(colourProvider.Background5, transition_duration, Easing.OutQuint);
                }
                else
                {
                    MainBackground.FadeColour(colourProvider.Background4, transition_duration, Easing.OutQuint);
                    counter.FadeColour(Colour4.White, transition_duration, Easing.OutQuint);
                }

                if (e.NewValue != Current.Default)
                    MainBackground.FlashColour(Color4.White, 500, Easing.OutQuint);

                const float move_amount = 4;
                if (e.NewValue > e.OldValue)
                    counter.MoveToY(Math.Max(-move_amount * 2, counter.Y - move_amount)).Then().MoveToY(0, transition_duration * 2, Easing.OutQuint);
                else
                    counter.MoveToY(Math.Min(move_amount * 2, counter.Y + move_amount)).Then().MoveToY(0, transition_duration * 2, Easing.OutQuint);
            }, true);

            // required to prevent the counter initially rolling up from 0 to 1
            // due to `Current.Value` having a nonstandard default value of 1.
            counter.SetCountWithoutRolling(Current.Value);
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
