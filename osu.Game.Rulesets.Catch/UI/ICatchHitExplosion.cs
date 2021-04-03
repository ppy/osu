// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Catch.Objects;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Catch.UI
{
    public interface ICatchHitExplosion
    {
        public Color4 ObjectColour
        {
            get;
            set;
        }

        public CatchHitObject HitObject
        {
            get;
            set;
        }
    }
}
