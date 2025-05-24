// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Input.Events;

namespace osu.Game.Screens.SelectV2
{
    /// <summary>
    /// A component inserted in the wedge hierarchy to block scroll components from reaching the carousel.
    /// </summary>
    public partial class WedgeScrollBlockerComponent : Component
    {
        public WedgeScrollBlockerComponent()
        {
            RelativeSizeAxes = Axes.Both;
        }

        // we want to block plain scrolls on the left side so that they don't scroll the carousel,
        // but also we *don't* want to handle scrolls when they're combined with keyboard modifiers
        // as those will usually correspond to other interactions like adjusting volume.
        protected override bool OnScroll(ScrollEvent e) => !e.ControlPressed && !e.AltPressed && !e.ShiftPressed && !e.SuperPressed;
    }
}
