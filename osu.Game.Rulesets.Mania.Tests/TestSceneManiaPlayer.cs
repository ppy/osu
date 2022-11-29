// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Mania.Tests
{
    public partial class TestSceneManiaPlayer : PlayerTestScene
    {
        protected override Ruleset CreatePlayerRuleset() => new ManiaRuleset();
    }
}
