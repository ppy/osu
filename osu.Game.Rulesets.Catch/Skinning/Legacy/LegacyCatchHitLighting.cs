// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Catch.Skinning.Legacy
{
    public class LegacyCatchHitLighting : Container, ICatchHitExplosion
    {
        protected override void LoadComplete()
        {
            base.LoadComplete();

            // Stub
        }

        public Color4 ObjectColour { get; set; }
        public CatchHitObject HitObject { get; set; }
    }
}
