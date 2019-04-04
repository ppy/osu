// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Configuration;

namespace osu.Game.Overlays.Settings.Sections.Online
{
    public class WebSettings : SettingsSubsection
    {
        protected override string Header => "Web";

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Children = new Drawable[]
            {
                new SettingsCheckbox
                {
                    LabelText = "Warn about opening external links",
                    Bindable = config.GetBindable<bool>(OsuSetting.ExternalLinkWarning)
                },
            };
        }
    }
}
