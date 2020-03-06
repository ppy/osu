// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Osu.Tests
{
    public class TestSceneOsuFlashlight : TestSceneOsuPlayer
    {
        protected override TestPlayer CreatePlayer(Ruleset ruleset)
        {
            SelectedMods.Value = new Mod[] { new OsuModAutoplay(), new OsuModFlashlight(), };

            return base.CreatePlayer(ruleset);
        }
    }
}
