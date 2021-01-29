// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Configuration;

namespace osu.Game.Overlays.Settings.Sections.Mf
{
    public class MvisStoryBoardSettings : SettingsSubsection
    {
        protected override string Header => "settings.mvis.storyboard.header";

        [BackgroundDependencyLoader]
        private void load(MConfigManager config)
        {
            Children = new Drawable[]
            {
                new SettingsCheckbox
                {
                    LabelText = "settings.mvis.storyboard.enableStoryboard",
                    Current = config.GetBindable<bool>(MSetting.MvisEnableStoryboard),
                },
                new SettingsCheckbox
                {
                    LabelText = "settings.mvis.storyboard.storyboardProxy",
                    Current = config.GetBindable<bool>(MSetting.MvisStoryboardProxy),
                }
            };
        }
    }
}
