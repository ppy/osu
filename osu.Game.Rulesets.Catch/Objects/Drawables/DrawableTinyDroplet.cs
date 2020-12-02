// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using JetBrains.Annotations;

namespace osu.Game.Rulesets.Catch.Objects.Drawables
{
    public class DrawableTinyDroplet : DrawableDroplet
    {
        protected override float ScaleFactor => base.ScaleFactor / 2;

        public DrawableTinyDroplet()
            : this(null)
        {
        }

        public DrawableTinyDroplet([CanBeNull] TinyDroplet h)
            : base(h)
        {
        }
    }
}
