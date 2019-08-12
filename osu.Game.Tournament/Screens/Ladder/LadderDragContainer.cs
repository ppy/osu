// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Input.Events;
using osuTK;

namespace osu.Game.Tournament.Screens.Ladder
{
    public class LadderDragContainer : Container
    {
        protected override bool OnDragStart(DragStartEvent e) => true;

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true;

        private Vector2 target;

        private float scale = 1;

        protected override bool ComputeIsMaskedAway(RectangleF maskingBounds) => false;

        protected override bool OnDrag(DragEvent e)
        {
            this.MoveTo(target += e.Delta, 1000, Easing.OutQuint);
            return true;
        }

        private const float min_scale = 0.6f;
        private const float max_scale = 1.4f;

        protected override bool OnScroll(ScrollEvent e)
        {
            var newScale = MathHelper.Clamp(scale + e.ScrollDelta.Y / 15 * scale, min_scale, max_scale);

            this.MoveTo(target = target - e.MousePosition * (newScale - scale), 2000, Easing.OutQuint);
            this.ScaleTo(scale = newScale, 2000, Easing.OutQuint);

            return true;
        }
    }
}
