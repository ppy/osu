// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
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

        [BackgroundDependencyLoader]
        private void load()
        {
            OsuConfigManager manager;
            Dependencies.Cache(manager = new OsuConfigManager(LocalStorage));
            manager.GetBindable<double>(OsuSetting.DimLevel).Value = 1.0;
        }

        [SetUpSteps]
        public virtual void SetUpSteps()
        {
            AddStep(ruleset.RulesetInfo.Name, loadPlayer);
            AddUntilStep("player loaded", () => Player.IsLoaded && Player.Alpha == 1);
        }

        protected virtual bool AllowFail => false;

        private void loadPlayer()
        {
            var beatmap = CreateBeatmap(ruleset.RulesetInfo);

            Beatmap.Value = CreateWorkingBeatmap(beatmap);

            if (!AllowFail)
                Mods.Value = new[] { ruleset.GetAllMods().First(m => m is ModNoFail) };

            Player = CreatePlayer(ruleset);
            LoadScreen(Player);
        }

        protected virtual Player CreatePlayer(Ruleset ruleset) => new TestPlayer(false, false);
    }
}
