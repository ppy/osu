// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Catch.Skinning.Legacy
{
    public partial class LegacyCatcherOld : LegacyCatcher
    {
        public LegacyCatcherOld()
        {
            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(ISkinSource skin)
        {
            InternalChild = skin.GetAnimation(@"fruit-ryuuta", true, true, true) ?? Empty();
        }
    }
}
