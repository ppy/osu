// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable
using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osuTK;

namespace osu.Game.Screens.Select
{
    internal class DifficultyRangeFilterControl : CompositeDrawable
    {
        private Bindable<double> minStars;
        private Bindable<double> maxStars;

        private StarsSlider lowerSlider;
        private MaximumStarsSlider upperSlider;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            const float vertical_offset = 15;

            InternalChildren = new[]
            {
                new OsuSpriteText
                {
                    Text = "Difficulty range",
                    Font = OsuFont.GetFont(size: 14),
                },
                upperSlider = new MaximumStarsSlider
                {
                    Current = config.GetBindable<double>(OsuSetting.DisplayStarsMaximum),
                    KeyboardStep = 0.1f,
                    RelativeSizeAxes = Axes.X,
                    Y = vertical_offset,
                },
                lowerSlider = new StarsSlider
                {
                    Current = config.GetBindable<double>(OsuSetting.DisplayStarsMinimum),
                    KeyboardStep = 0.1f,
                    RelativeSizeAxes = Axes.X,
                    Y = vertical_offset,
                },
                lowerSlider.Nub.CreateProxy(),
                upperSlider.Nub.CreateProxy(),
            };

            lowerSlider.LeftBox.Height = 6;

            minStars = config.GetBindable<double>(OsuSetting.DisplayStarsMinimum);
            maxStars = config.GetBindable<double>(OsuSetting.DisplayStarsMaximum);

            lowerSlider.AccentColour = lowerSlider.BackgroundColour;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            minStars.ValueChanged += min => maxStars.Value = Math.Max(min.NewValue, maxStars.Value);
            maxStars.ValueChanged += max => minStars.Value = Math.Min(max.NewValue, minStars.Value);
        }

        private class MaximumStarsSlider : StarsSlider
        {
            public override LocalisableString TooltipText => Current.IsDefault ? UserInterfaceStrings.NoLimit : base.TooltipText;
        }

        private class StarsSlider : OsuSliderBar<double>
        {
            public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => Nub.ReceivePositionalInputAt(screenSpacePos);

            public override LocalisableString TooltipText => Current.Value.ToString(@"0.## stars");

            public new Nub Nub => base.Nub;
            public new Box LeftBox => base.LeftBox;
        }
    }
}
