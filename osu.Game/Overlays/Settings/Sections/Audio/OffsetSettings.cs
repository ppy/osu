// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Localisation;

namespace osu.Game.Overlays.Settings.Sections.Audio
{
    public partial class OffsetSettings : SettingsSubsection
    {
        protected override LocalisableString Header => AudioSettingsStrings.OffsetHeader;

        public override IEnumerable<LocalisableString> FilterTerms => base.FilterTerms.Concat(new LocalisableString[] { "universal", "uo", "timing", "delay", "latency" });

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Children = new Drawable[]
            {
                new AudioOffsetAdjustControl
                {
                    Current = config.GetBindable<double>(OsuSetting.AudioOffset),
                },
                new SettingsButton
                {
                    Text = AudioSettingsStrings.OffsetWizard
                }
            };
        }
    }
}
