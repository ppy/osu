// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.States;
using OpenTK;

namespace osu.Game.Tournament.Screens.Ladder
{
    public class ScrollableContainer : Container
    {
        protected override bool OnDragStart(InputState state) => true;

        public override bool ReceiveMouseInputAt(Vector2 screenSpacePos) => true;

        private Vector2 target;

        private float scale = 1;

        protected override bool OnDrag(InputState state)
        {
            this.MoveTo(target += state.Mouse.Delta, 1000, Easing.OutQuint);

            return base.OnDrag(state);
        }

        protected override bool OnScroll(InputState state)
        {
            this.ScaleTo(scale += state.Mouse.ScrollDelta.Y / 15, 1000, Easing.OutQuint);

            return base.OnScroll(state);
        }
    }
}
