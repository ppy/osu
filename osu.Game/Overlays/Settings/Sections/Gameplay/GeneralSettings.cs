// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Localisation;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Overlays.Settings.Sections.Gameplay
{
    public partial class GeneralSettings : SettingsSubsection
    {
        protected override LocalisableString Header => CommonStrings.General;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Children = new Drawable[]
            {
                new SettingsItemV2(new FormEnumDropdown<ScoringMode>
                {
                    Caption = GameplaySettingsStrings.ScoreDisplayMode,
                    Current = config.GetBindable<ScoringMode>(OsuSetting.ScoreDisplayMode),
                })
                {
                    Keywords = new[] { "scoring" },
                    ApplyClassicDefault = c => ((IHasCurrentValue<ScoringMode>)c).Current.Value = ScoringMode.Classic,
                },
                new SettingsItemV2(new FormCheckBox
                {
                    Caption = GraphicsSettingsStrings.HitLighting,
                    Current = config.GetBindable<bool>(OsuSetting.HitLighting)
                }),
                new SettingsItemV2(new FormCheckBox
                {
                    Caption = GameplaySettingsStrings.StarFountains,
                    Current = config.GetBindable<bool>(OsuSetting.StarFountains)
                }),
            };
        }
    }
}
