using System.Collections.Generic;
using Mvis.Plugin.SandboxToPanel.RulesetComponents.Objects;
using Mvis.Plugin.SandboxToPanel.RulesetComponents.Objects.Drawables;
using osu.Framework.Input;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI;

namespace Mvis.Plugin.SandboxToPanel.RulesetComponents.UI
{
    public partial class DrawableSandboxRuleset : DrawableRuleset<SandboxHitObject>
    {
        public DrawableSandboxRuleset(Ruleset ruleset, IBeatmap beatmap, IReadOnlyList<Mod>? mods = null)
            : base(ruleset, beatmap, mods)
        {
        }

        protected override PassThroughInputManager CreateInputManager() => new SandboxInputManager(Ruleset.RulesetInfo);

        protected override Playfield CreatePlayfield() => new SandboxPlayfield();

        public override DrawableHitObject<SandboxHitObject> CreateDrawableRepresentation(SandboxHitObject h)
        {
            switch (h)
            {
                case SandboxHitObject hitObject:
                    return new DrawableSandboxHitObject(hitObject);
            }

            return null;
        }
    }
}
