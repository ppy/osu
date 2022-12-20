using System;
using System.Collections.Generic;
using Mvis.Plugin.SandboxToPanel.RulesetComponents.Beatmaps;
using Mvis.Plugin.SandboxToPanel.RulesetComponents.Difficulty;
using Mvis.Plugin.SandboxToPanel.RulesetComponents.UI;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;

namespace Mvis.Plugin.SandboxToPanel.RulesetComponents
{
    public partial class SandboxRuleset : Ruleset
    {
        public static readonly string VERSION = "2022.1207.0";

        public override DrawableRuleset CreateDrawableRulesetWith(IBeatmap beatmap, IReadOnlyList<Mod>? mods = null) => new DrawableSandboxRuleset(this, beatmap, mods);

        public override IBeatmapConverter CreateBeatmapConverter(IBeatmap beatmap) => new SandboxBeatmapConverter(beatmap, this);

        //mfosu: ignore this
        //public override IRulesetConfigManager CreateConfig(SettingsStore? settings) => new SandboxRulesetConfigManager(settings, RulesetInfo);

        public override RulesetSettingsSubsection CreateSettings() => new SandboxSettingsSubsection(this);

        public override string Description => "Sandbox";

        public override string ShortName => "sandbox";

        public override string PlayingVerb => "Doing random stuff";

        public override Drawable CreateIcon() => new SandboxIcon(this);

        public override IEnumerable<Mod> GetModsFor(ModType type) => Array.Empty<Mod>();

        protected override IEnumerable<HitResult> GetValidHitResults() => new[]
        {
            HitResult.Perfect
        };

        public override DifficultyCalculator CreateDifficultyCalculator(IWorkingBeatmap beatmap) => new SandboxDifficultyCalculator(RulesetInfo, beatmap);

        private partial class SandboxIcon : Sprite
        {
            private readonly SandboxRuleset ruleset;

            public SandboxIcon(SandboxRuleset ruleset)
            {
                this.ruleset = ruleset;
            }

            [BackgroundDependencyLoader]
            private void load(GameHost host)
            {
                Texture = new TextureStore(host.Renderer, new TextureLoaderStore(ruleset.CreateResourceStore()), false).Get("Textures/ruleset");
            }
        }
    }
}
