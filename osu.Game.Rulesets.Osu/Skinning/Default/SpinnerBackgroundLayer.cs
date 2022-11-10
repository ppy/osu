// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;

namespace osu.Game.Rulesets.Osu.Skinning.Default
{
    public class SpinnerBackgroundLayer : SpinnerFill
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            Disc.Alpha = 0;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
        }
    }
}
