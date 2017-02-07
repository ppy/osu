// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Graphics;
using osu.Framework.Input;

namespace osu.Game.Input
{
    public class GlobalHotkeys : Drawable
    {
        public Func<InputState, KeyDownEventArgs, bool> Handler;

        public override bool HandleInput => true;

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            return Handler(state, args);
        }
    }
}