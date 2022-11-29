// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK;

namespace osu.Game.Rulesets.Catch.Skinning.Default
{
    public partial class BananaPulpFormation : PulpFormation
    {
        public BananaPulpFormation()
        {
            AddPulp(new Vector2(0, -0.3f), new Vector2(SMALL_PULP));
            AddPulp(new Vector2(0, 0.05f), new Vector2(LARGE_PULP_4 * 0.8f, LARGE_PULP_4 * 2.5f));
        }
    }
}
