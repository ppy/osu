// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Input.Bindings;

namespace osu.Game.Overlays.Volume
{
    /// <summary>
    /// Add to a container or screen to make scrolling anywhere in the container cause the global game volume to be adjusted.
    /// </summary>
    /// <remarks>
    /// This is generally expected behaviour in many locations in osu!stable.
    /// </remarks>
    public partial class GlobalScrollAdjustsVolume : Container
    {
        [Resolved]
        private VolumeOverlay? volumeOverlay { get; set; }

        private bool needsAltPressed = false;

        public GlobalScrollAdjustsVolume()
        {
            RelativeSizeAxes = Axes.Both;
        }

        public GlobalScrollAdjustsVolume(bool needsAltPressed)
        {
            RelativeSizeAxes = Axes.Both;
            this.needsAltPressed = needsAltPressed;
        }

        protected override bool OnScroll(ScrollEvent e)
        {
            if (e.ScrollDelta.Y == 0)
                return false;

            if (needsAltPressed && !e.AltPressed)
                return false;

            // forward any unhandled mouse scroll events to the volume control.
            return volumeOverlay?.Adjust(GlobalAction.IncreaseVolume, e.ScrollDelta.Y, e.IsPrecise) ?? false;
        }
    }
}
