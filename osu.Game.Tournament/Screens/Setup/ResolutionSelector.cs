// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Tournament.Screens.Setup
{
    internal class ResolutionSelector : ActionableInfo
    {
        private const int minimum_window_height = 480;
        private const int maximum_window_height = 2160;

        public new Action<int> Action;

        private OsuNumberBox numberBox;

        protected override Drawable CreateComponent()
        {
            var drawable = base.CreateComponent();
            FlowContainer.Insert(-1, numberBox = new OsuNumberBox
            {
                Text = "1080",
                Width = 100
            });

            base.Action = () =>
            {
                if (string.IsNullOrEmpty(numberBox.Text))
                    return;

                // box contains text
                if (!int.TryParse(numberBox.Text, out int number))
                {
                    // at this point, the only reason we can arrive here is if the input number was too big to parse into an int
                    // so clamp to max allowed value
                    number = maximum_window_height;
                }
                else
                {
                    number = Math.Clamp(number, minimum_window_height, maximum_window_height);
                }

                // in case number got clamped, reset number in numberBox
                numberBox.Text = number.ToString();

                Action?.Invoke(number);
            };
            return drawable;
        }
    }
}
