using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Bindings;
using osu.Game.Rulesets;
using osu.Game.Rulesets.UI;
using Symcol.Rulesets.Core.VectorVideos;

namespace Symcol.Rulesets.Core
{
    public class SymcolInputManager<T> : RulesetInputManager<T>
        where T : struct
    {
        protected virtual bool VectorVideo => false;

        public SymcolInputManager(RulesetInfo ruleset, int variant, SimultaneousBindingMode unique) : base(ruleset, variant, unique)
        {
            Child = new VectorVideo();
        }
    }
}
