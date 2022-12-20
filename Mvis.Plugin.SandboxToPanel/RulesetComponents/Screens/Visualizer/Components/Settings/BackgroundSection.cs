using Mvis.Plugin.SandboxToPanel.RulesetComponents.Configuration;
using Mvis.Plugin.SandboxToPanel.RulesetComponents.UI.Settings;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Configuration;
using osu.Game.Overlays.Settings;

namespace Mvis.Plugin.SandboxToPanel.RulesetComponents.Screens.Visualizer.Components.Settings
{
    public partial class BackgroundSection : SandboxSettingsSection
    {
        protected override string HeaderName => "Background";

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config, SandboxRulesetConfigManager rulesetConfig)
        {
            AddRange(new Drawable[]
            {
                new SettingsCheckbox
                {
                    LabelText = "Show storyboard (if available)",
                    Current = rulesetConfig.GetBindable<bool>(SandboxRulesetSetting.ShowStoryboard)
                },
                new SettingsCheckbox
                {
                    LabelText = "Show particles",
                    Current = rulesetConfig.GetBindable<bool>(SandboxRulesetSetting.ShowParticles)
                },
                new ParticleSettings(),
                new SettingsSlider<double>
                {
                    LabelText = "Background dim",
                    Current = config.GetBindable<double>(OsuSetting.DimLevel),
                    KeyboardStep = 0.01f,
                    DisplayAsPercentage = true
                },
                new SettingsSlider<double>
                {
                    LabelText = "Background blur",
                    Current = config.GetBindable<double>(OsuSetting.BlurLevel),
                    KeyboardStep = 0.01f,
                    DisplayAsPercentage = true
                }
            });
        }
    }
}
