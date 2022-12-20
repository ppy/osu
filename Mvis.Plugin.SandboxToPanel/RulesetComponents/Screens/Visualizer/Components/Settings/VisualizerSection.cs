using Mvis.Plugin.SandboxToPanel.RulesetComponents.Configuration;
using Mvis.Plugin.SandboxToPanel.RulesetComponents.UI.Settings;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Overlays.Settings;

namespace Mvis.Plugin.SandboxToPanel.RulesetComponents.Screens.Visualizer.Components.Settings
{
    public partial class VisualizerSection : SandboxSettingsSection
    {
        protected override string HeaderName => "Visualizer";

        [BackgroundDependencyLoader]
        private void load(SandboxRulesetConfigManager config)
        {
            AddRange(new Drawable[]
            {
                new SettingsEnumDropdown<VisualizerLayout>
                {
                    LabelText = "Layout type",
                    Current = config.GetBindable<VisualizerLayout>(SandboxRulesetSetting.VisualizerLayout)
                },
                new LayoutSettingsSubsection()
            });
        }
    }
}
