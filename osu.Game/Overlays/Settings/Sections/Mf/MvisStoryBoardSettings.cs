// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Configuration;

namespace osu.Game.Overlays.Settings.Sections.Mf
{
    public class MvisStoryBoardSettings : SettingsSubsection
    {
        protected override string Header => "故事版";

        [BackgroundDependencyLoader]
        private void load(MConfigManager config)
        {
            Children = new Drawable[]
            {
                new SettingsCheckbox
                {
                    LabelText = "禁用故事版",
                    Current = config.GetBindable<bool>(MSetting.MvisEnableStoryboard)
                },
                new SettingsCheckbox
                {
                    LabelText = "启用故事版Proxy",
                    Current = config.GetBindable<bool>(MSetting.MvisStoryboardProxy),
                    TooltipText = "可以让故事版的Overlay显示在前景"
                }
            };
        }
    }
}
