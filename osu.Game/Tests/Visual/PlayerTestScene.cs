// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Testing;
using osu.Game.Configuration;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Play;

namespace osu.Game.Tests.Visual
{
    public abstract class PlayerTestScene : RateAdjustedBeatmapTestScene
    {
        private readonly Ruleset ruleset;

        protected Player Player;

        protected PlayerTestScene(Ruleset ruleset)
        {
            this.ruleset = ruleset;
        }

        protected OsuConfigManager LocalConfig;

        [BackgroundDependencyLoader]
        private void load()
        {
            Dependencies.Cache(LocalConfig = new OsuConfigManager(LocalStorage));
            LocalConfig.GetBindable<double>(OsuSetting.DimLevel).Value = 1.0;
        }

        [SetUpSteps]
        public virtual void SetUpSteps()
        {
            AddStep(ruleset.RulesetInfo.Name, loadPlayer);
            AddUntilStep("player loaded", () => Player.IsLoaded && Player.Alpha == 1);
        }

        protected virtual bool AllowFail => false;

        protected virtual bool Autoplay => false;

        private void loadPlayer()
        {
            var beatmap = CreateBeatmap(ruleset.RulesetInfo);

            Beatmap.Value = CreateWorkingBeatmap(beatmap);

            if (!AllowFail)
            {
                var noFailMod = ruleset.GetAllMods().FirstOrDefault(m => m is ModNoFail);
                if (noFailMod != null)
                    Mods.Value = new[] { noFailMod };
            }

            if (Autoplay)
            {
                var mod = ruleset.GetAutoplayMod();
                if (mod != null)
                    Mods.Value = Mods.Value.Concat(mod.Yield()).ToArray();
            }

            Player = CreatePlayer(ruleset);
            LoadScreen(Player);
        }

        protected virtual Player CreatePlayer(Ruleset ruleset) => new TestPlayer(false, false);
    }
}
