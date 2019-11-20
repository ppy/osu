// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Game.Configuration;

namespace osu.Game.Overlays.Settings.Sections.Graphics
{
    public class RendererSettings : SettingsSubsection
    {
        protected override string Header => "Renderer";

        public override IEnumerable<string> FilterTerms => base.FilterTerms.Concat(new[] { "FPS" });

        [BackgroundDependencyLoader]
        private void load(FrameworkConfigManager config, OsuConfigManager osuConfig)
        {
            // NOTE: Compatability mode omitted
            Children = new Drawable[]
            {
                // TODO: this needs to be a custom dropdown at some point
                new SettingsEnumDropdown<FrameSync>
                {
                    LabelText = "Frame limiter",
                    Bindable = config.GetBindable<FrameSync>(FrameworkSetting.FrameSync)
                },
                new SettingsCheckbox
                {
                    LabelText = "Show FPS",
                    Bindable = osuConfig.GetBindable<bool>(OsuSetting.ShowFpsDisplay)
                },
            };
        }
    }
}
