// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable
using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Select
{
    internal class DifficultyRangeFilterControl : CompositeDrawable
    {
        private Bindable<double> lowerStars;
        private Bindable<double> upperStars;

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
                lowerSlider = new MinimumStarsSlider
                {
                    Current = config.GetBindable<double>(OsuSetting.DisplayStarsMinimum),
                    KeyboardStep = 0.1f,
                    RelativeSizeAxes = Axes.X,
                    Y = vertical_offset,
                },
                upperSlider.Nub.CreateProxy(),
                lowerSlider.Nub.CreateProxy(),
            };

            lowerStars = config.GetBindable<double>(OsuSetting.DisplayStarsMinimum);
            upperStars = config.GetBindable<double>(OsuSetting.DisplayStarsMaximum);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            lowerStars.ValueChanged += min => upperStars.Value = Math.Max(min.NewValue + 0.1, upperStars.Value);
            upperStars.ValueChanged += max => lowerStars.Value = Math.Min(max.NewValue - 0.1, lowerStars.Value);
        }

        private class MinimumStarsSlider : StarsSlider
        {
            protected override void LoadComplete()
            {
                base.LoadComplete();

                LeftBox.Height = 6; // hide any colour bleeding from overlap

                AccentColour = BackgroundColour;
                BackgroundColour = Color4.Transparent;
            }

            public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) =>
                base.ReceivePositionalInputAt(screenSpacePos)
                && screenSpacePos.X <= Nub.ScreenSpaceDrawQuad.TopRight.X;

            public override LocalisableString TooltipText => Current.IsDefault ? UserInterfaceStrings.NoLimit : base.TooltipText;
        }

        private class MaximumStarsSlider : StarsSlider
        {
            protected override void LoadComplete()
            {
                base.LoadComplete();

                RightBox.Height = 6; // just to match the left bar height really
            }

            public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) =>
                base.ReceivePositionalInputAt(screenSpacePos)
                && screenSpacePos.X >= Nub.ScreenSpaceDrawQuad.TopLeft.X;
            public override LocalisableString TooltipText => Current.IsDefault ? UserInterfaceStrings.NoLimit : base.TooltipText;
        }

        private class StarsSlider : OsuSliderBar<double>
        {
            public override LocalisableString TooltipText => Current.Value.ToString(@"0.## stars");

            public new Nub Nub => base.Nub;

            protected override void LoadComplete()
            {
                base.LoadComplete();
                Nub.Width = Nub.HEIGHT;
                RangePadding = Nub.Width / 2;
            }
        }
    }
}
