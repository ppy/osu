// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Input.Events;

namespace osu.Game.Overlays.Volume
{
    /// <summary>
    /// Allows adjusting volume via mouse scroll while holding alt, regardless of the current game state.
    /// </summary>
    public partial class GlobalAltScrollAdjustsVolume : GlobalScrollAdjustsVolume
    {
        protected override bool OnScroll(ScrollEvent e)
        {
            if (!e.AltPressed || e.ControlPressed || e.ShiftPressed)
                return false;

            return base.OnScroll(e);
        }
    }
}
