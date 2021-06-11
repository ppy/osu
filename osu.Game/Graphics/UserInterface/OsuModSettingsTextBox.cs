// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics.Colour;
using osu.Framework.Input.Events;

namespace osu.Game.Graphics.UserInterface
{
    public class OsuModSettingsTextBox : OsuTextBox
    {
        private const float border_thickness = 3;

        private SRGBColour borderColourFocused;
        private SRGBColour borderColourUnfocused;

        [BackgroundDependencyLoader]
        private void load(OsuColour colour)
        {
            borderColourUnfocused = colour.Gray4.Opacity(0.5f);
            borderColourFocused = BorderColour;

            BorderThickness = border_thickness;
            BorderColour = borderColourUnfocused;
        }

        protected override void OnFocus(FocusEvent e)
        {
            base.OnFocus(e);

            BorderThickness = border_thickness;
            BorderColour = borderColourFocused;
        }

        protected override void OnFocusLost(FocusLostEvent e)
        {
            base.OnFocusLost(e);

            BorderThickness = border_thickness;
            BorderColour = borderColourUnfocused;
        }
    }
}
