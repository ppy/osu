// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osuTK;

namespace osu.Game.Tournament.Screens.Ladder
{
    public class ScrollableContainer : Container
    {
        protected override bool OnDragStart(DragStartEvent e) => true;

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true;

        private Vector2 target;

        private float scale = 1;

        protected override bool OnDrag(DragEvent e)
        {
            this.MoveTo(target += e.Delta, 1000, Easing.OutQuint);
            return true;
        }

        protected override bool OnScroll(ScrollEvent e)
        {
            var newScale = scale + e.ScrollDelta.Y / 15 * scale;
            this.MoveTo(target = target - e.MousePosition * (newScale - scale), 1000, Easing.OutQuint);

            this.ScaleTo(scale = newScale, 1000, Easing.OutQuint);

            return true;
        }

        protected override void Update()
        {
            base.Update();
            Invalidate();
        }
    }
}
