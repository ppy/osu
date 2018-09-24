// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics.Containers;
using osu.Framework.Input.States;
using OpenTK;

namespace osu.Game.Tournament.Screens.Ladder
{
    public class ScrollableContainer : Container
    {
        protected override bool OnDragStart(InputState state) => true;

        public override bool ReceiveMouseInputAt(Vector2 screenSpacePos) => true;

        protected override bool OnDrag(InputState state)
        {
            Position += state.Mouse.Delta;
            return base.OnDrag(state);
        }
    }
}
