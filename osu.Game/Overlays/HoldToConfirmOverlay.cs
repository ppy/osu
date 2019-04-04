// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.Containers;
using osuTK.Graphics;

namespace osu.Game.Overlays
{
    /// <summary>
    /// An overlay which will display a black screen that dims over a period before confirming an exit action.
    /// Action is BYO (derived class will need to call <see cref="BeginConfirm"/> and <see cref="AbortConfirm"/> from a user event).
    /// </summary>
    public abstract class HoldToConfirmOverlay : HoldToConfirmContainer
    {
        private Box overlay;

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.Both;
            AlwaysPresent = true;

            Children = new Drawable[]
            {
                overlay = new Box
                {
                    Alpha = 0,
                    Colour = Color4.Black,
                    RelativeSizeAxes = Axes.Both,
                }
            };

            Progress.ValueChanged += p => overlay.Alpha = (float)p.NewValue;
        }
    }
}
