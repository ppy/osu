using osu.Framework.Input.Bindings;
using osu.Game.Rulesets;
using osu.Game.Rulesets.UI;

namespace Mvis.Plugin.SandboxToPanel.RulesetComponents
{
    public partial class SandboxInputManager : RulesetInputManager<SandboxAction>
    {
        public SandboxInputManager(RulesetInfo ruleset)
            : base(ruleset, 0, SimultaneousBindingMode.Unique)
        {
        }
    }

    public enum SandboxAction
    {
    }
}
