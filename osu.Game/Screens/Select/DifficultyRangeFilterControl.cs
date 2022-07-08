// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
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
        private Bindable<double> lowerStars = null!;
        private Bindable<double> upperStars = null!;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            const float vertical_offset = 13;

            InternalChildren = new Drawable[]
            {
                new OsuSpriteText
                {
                    Text = "Difficulty range",
                    Font = OsuFont.GetFont(size: 14),
                },
                new MaximumStarsSlider
                {
                    Current = config.GetBindable<double>(OsuSetting.DisplayStarsMaximum),
                    KeyboardStep = 0.1f,
                    RelativeSizeAxes = Axes.X,
                    Y = vertical_offset,
                },
                new MinimumStarsSlider
                {
                    Current = config.GetBindable<double>(OsuSetting.DisplayStarsMinimum),
                    KeyboardStep = 0.1f,
                    RelativeSizeAxes = Axes.X,
                    Y = vertical_offset,
                }
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
        }

        private class StarsSlider : OsuSliderBar<double>
        {
            public override LocalisableString TooltipText => Current.IsDefault
                ? UserInterfaceStrings.NoLimit
                : Current.Value.ToString(@"0.## stars");

            protected override bool OnHover(HoverEvent e)
            {
                base.OnHover(e);
                return true; // Make sure only one nub shows hover effect at once.
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                Nub.Width = Nub.HEIGHT;
                RangePadding = Nub.Width / 2;

                OsuSpriteText currentDisplay;

                Nub.Add(currentDisplay = new OsuSpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Y = -0.5f,
                    Colour = Color4.White,
                    Font = OsuFont.Torus.With(size: 10),
                });

                Current.BindValueChanged(current =>
                {
                    currentDisplay.Text = current.NewValue != Current.Default ? current.NewValue.ToString("N1") : "∞";
                }, true);
            }
        }
    }
}
