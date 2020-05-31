// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;

namespace osu.Game.Rulesets.Catch.Objects.Drawables
{
    public class DrawableTinyDroplet : DrawableDroplet
    {
        public DrawableTinyDroplet(TinyDroplet h)
            : base(h)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            ScaleContainer.Scale /= 2;
        }
    }
}
