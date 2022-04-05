// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Database;
using osu.Game.Rulesets.Taiko.Mods;
using osu.Game.Skinning;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Taiko.Tests
{
    public class TestSceneLegacyTaikoPlayer : PlayerTestScene
    {
        protected override Ruleset CreatePlayerRuleset() => new TaikoRuleset();

        [BackgroundDependencyLoader]
        private void load(SkinManager skins)
        {
            skins.CurrentSkinInfo.Value = DefaultLegacySkin.CreateInfo().ToLiveUnmanaged();
        }

        protected override TestPlayer CreatePlayer(Ruleset ruleset)
        {
            SelectedMods.Value = new[] { new TaikoModClassic() };
            return base.CreatePlayer(ruleset);
        }
    }
}
