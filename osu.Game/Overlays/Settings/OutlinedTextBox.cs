// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osuTK.Graphics;

namespace osu.Game.Overlays.Settings
{
    public partial class OutlinedTextBox : OsuTextBox
    {
        private const float border_thickness = 3;

        private Color4 borderColourFocused;
        private Color4 borderColourUnfocused;

        [BackgroundDependencyLoader]
        private void load(OsuColour colour)
        {
            borderColourUnfocused = colour.Gray4.Opacity(0.5f);
            borderColourFocused = BorderColour;

            updateBorder();
        }

        protected override void OnFocus(FocusEvent e)
        {
            base.OnFocus(e);

            updateBorder();
        }

        protected override void OnFocusLost(FocusLostEvent e)
        {
            base.OnFocusLost(e);

            updateBorder();
        }

        private void updateBorder()
        {
            BorderThickness = border_thickness;
            BorderColour = HasFocus ? borderColourFocused : borderColourUnfocused;
        }
    }
}
