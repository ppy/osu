// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Graphics;
using osu.Game.Graphics.Containers;

namespace osu.Game.Screens.Select.Leaderboards
{
    public abstract class Placeholder : OsuTextFlowContainer, IEquatable<Placeholder>
    {
        protected const float TEXT_SIZE = 22;

        public override bool HandlePositionalInput => true;

        protected Placeholder()
            : base(cp => cp.TextSize = TEXT_SIZE)
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            TextAnchor = Anchor.TopCentre;

            Padding = new MarginPadding(20);

            AutoSizeAxes = Axes.Y;
            RelativeSizeAxes = Axes.X;
        }

        public virtual bool Equals(Placeholder other) => GetType() == other?.GetType();
    }
}
