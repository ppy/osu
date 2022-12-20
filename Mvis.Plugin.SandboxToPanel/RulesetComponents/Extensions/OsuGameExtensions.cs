using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Testing;
using osu.Game;
using osu.Game.Overlays;
using osu.Game.Rulesets;
using osu.Game.Screens;

namespace Mvis.Plugin.SandboxToPanel.RulesetComponents.Extensions
{
    public static class OsuGameExtensions
    {
        public static SandboxRuleset GetRuleset(this DependencyContainer dependencies)
        {
            var rulesets = dependencies.Get<RulesetStore>().AvailableRulesets.Select(info => info.CreateInstance());
            return (SandboxRuleset?)rulesets.FirstOrDefault(r => r is SandboxRuleset);
        }

        public static OsuScreenStack GetScreenStack(this OsuGame game) => game.ChildrenOfType<OsuScreenStack>().FirstOrDefault();

        public static SettingsOverlay GetSettingsOverlay(this OsuGame game) => game.ChildrenOfType<SettingsOverlay>().FirstOrDefault();
    }
}
