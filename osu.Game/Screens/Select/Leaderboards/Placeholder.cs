// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Screens.Select.Leaderboards
{
    public abstract class Placeholder : FillFlowContainer, IEquatable<Placeholder>
    {
        protected Placeholder()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
        }

        public virtual bool Equals(Placeholder other) => GetType() == other?.GetType();
    }
}
