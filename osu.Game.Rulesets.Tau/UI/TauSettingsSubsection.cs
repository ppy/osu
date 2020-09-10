using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Tau.Configuration;

namespace osu.Game.Rulesets.Tau.UI
{
    public class TauSettingsSubsection : RulesetSettingsSubsection
    {
        protected override string Header => "tau";

        public TauSettingsSubsection(Ruleset ruleset)
            : base(ruleset)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            var config = (TauRulesetConfigManager)Config;

            if (config == null)
                return;

            Children = new Drawable[]
            {
                new SettingsCheckbox
                {
                    LabelText = "在歌曲高潮时显示频谱",
                    Bindable = config.GetBindable<bool>(TauRulesetSettings.ShowVisualizer)
                },
                new SettingsSlider<float>
                {
                    LabelText = "圆盘暗化",
                    Bindable = config.GetBindable<float>(TauRulesetSettings.PlayfieldDim),
                    KeyboardStep = 0.01f,
                    DisplayAsPercentage = true
                },
                new SettingsSlider<float>
                {
                    LabelText = "物件大小",
                    Bindable = config.GetBindable<float>(TauRulesetSettings.BeatSize),
                    KeyboardStep = 1f
                }
            };
        }
    }
}
