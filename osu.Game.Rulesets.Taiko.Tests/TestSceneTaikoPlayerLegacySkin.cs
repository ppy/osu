// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Taiko.Mods;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Taiko.Tests
{
    public partial class TestSceneTaikoPlayerLegacySkin : LegacySkinPlayerTestScene
    {
        protected override Ruleset CreatePlayerRuleset() => new TaikoRuleset();

        protected override TestPlayer CreatePlayer(Ruleset ruleset)
        {
            SelectedMods.Value = new[] { new TaikoModClassic() };
            return base.CreatePlayer(ruleset);
        }
    }
}
