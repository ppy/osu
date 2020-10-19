// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables.Pieces;

namespace osu.Game.Rulesets.Osu.Objects.Drawables
{
    public class SpinnerBackgroundLayer : SpinnerFill
    {
        [BackgroundDependencyLoader]
        private void load(OsuColour colours, DrawableHitObject drawableHitObject)
        {
            Disc.Alpha = 0;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
        }
    }
}
