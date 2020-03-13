// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Testing;
using osu.Game.Configuration;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Tests.Visual
{
    public abstract class PlayerTestScene : RateAdjustedBeatmapTestScene
    {
        /// <summary>
        /// Whether custom test steps are provided. Custom tests should invoke <see cref="CreateTest"/> to create the test steps.
        /// </summary>
        protected virtual bool HasCustomSteps { get; } = false;

        private readonly Ruleset ruleset;

        protected TestPlayer Player;

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
        public override void SetUpSteps()
        {
            base.SetUpSteps();

            if (!HasCustomSteps)
                CreateTest(null);
        }

        protected void CreateTest(Action action)
        {
            if (action != null && !HasCustomSteps)
                throw new InvalidOperationException($"Cannot add custom test steps without {nameof(HasCustomSteps)} being set.");

            action?.Invoke();

            AddStep(ruleset.RulesetInfo.Name, LoadPlayer);
            AddUntilStep("player loaded", () => Player.IsLoaded && Player.Alpha == 1);
        }

        protected virtual bool AllowFail => false;

        protected virtual bool Autoplay => false;

        protected void LoadPlayer()
        {
            var beatmap = CreateBeatmap(ruleset.RulesetInfo);

            Beatmap.Value = CreateWorkingBeatmap(beatmap);
            Ruleset.Value = ruleset.RulesetInfo;

            SelectedMods.Value = Array.Empty<Mod>();

            if (!AllowFail)
            {
                var noFailMod = ruleset.GetAllMods().FirstOrDefault(m => m is ModNoFail);
                if (noFailMod != null)
                    SelectedMods.Value = new[] { noFailMod };
            }

            if (Autoplay)
            {
                var mod = ruleset.GetAutoplayMod();
                if (mod != null)
                    SelectedMods.Value = SelectedMods.Value.Concat(mod.Yield()).ToArray();
            }

            Player = CreatePlayer(ruleset);
            LoadScreen(Player);
        }

        protected virtual TestPlayer CreatePlayer(Ruleset ruleset) => new TestPlayer(false, false);
    }
}
