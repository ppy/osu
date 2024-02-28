// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Localisation;

namespace osu.Game.Overlays.Settings.Sections.Gameplay
{
    public partial class ModsSettings : SettingsSubsection
    {
        protected override LocalisableString Header => GameplaySettingsStrings.ModsHeader;

        public override IEnumerable<LocalisableString> FilterTerms => base.FilterTerms.Concat(new LocalisableString[] { "mod" });

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Children = new[]
            {
                new SettingsCheckbox
                {
                    LabelText = GameplaySettingsStrings.IncreaseFirstObjectVisibility,
                    Current = config.GetBindable<bool>(OsuSetting.IncreaseFirstObjectVisibility),
                    Keywords = new[] { @"approach", @"circle", @"hidden" },
                },
            };
        }
    }
}
