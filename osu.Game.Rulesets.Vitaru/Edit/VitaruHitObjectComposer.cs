using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Tools;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.Vitaru.Objects;

namespace osu.Game.Rulesets.Vitaru.Edit
{
    public class VitaruHitObjectComposer : HitObjectComposer
    {
        public VitaruHitObjectComposer(Ruleset ruleset) : base(ruleset) { }

        protected override RulesetContainer CreateRulesetContainer(Ruleset ruleset, WorkingBeatmap beatmap) => new VitaruEditRulesetContainer(ruleset, beatmap, true);

        protected override IReadOnlyList<ICompositionTool> CompositionTools => new ICompositionTool[]
        {
            new HitObjectCompositionTool<Pattern>(),
            new HitObjectCompositionTool<Bullet>(),
            new HitObjectCompositionTool<Laser>()
        };
    }

    public enum EditorConfiguration
    {
        Simple,
        Complex
    }
}
