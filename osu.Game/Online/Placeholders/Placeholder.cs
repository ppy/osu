// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using osu.Framework.Graphics;
using osu.Game.Graphics.Containers;

namespace osu.Game.Online.Placeholders
{
    public abstract partial class Placeholder : OsuTextFlowContainer, IEquatable<Placeholder>
    {
        protected const float TEXT_SIZE = 22;

        protected Placeholder()
            : base(cp => cp.Font = cp.Font.With(size: TEXT_SIZE))
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
