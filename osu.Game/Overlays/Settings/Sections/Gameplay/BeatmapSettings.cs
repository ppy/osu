// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Localisation;

namespace osu.Game.Overlays.Settings.Sections.Gameplay
{
    public partial class BeatmapSettings : SettingsSubsection
    {
        protected override LocalisableString Header => GameplaySettingsStrings.BeatmapHeader;

        private readonly BindableFloat comboColourNormalisation = new BindableFloat();

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            config.BindWith(OsuSetting.ComboColourNormalisationAmount, comboColourNormalisation);

            Children = new Drawable[]
            {
                new SettingsItemV2(new FormCheckBox
                {
                    Caption = SkinSettingsStrings.BeatmapSkins,
                    Current = config.GetBindable<bool>(OsuSetting.BeatmapSkins)
                }),
                new SettingsItemV2(new FormCheckBox
                {
                    Caption = SkinSettingsStrings.BeatmapColours,
                    Current = config.GetBindable<bool>(OsuSetting.BeatmapColours)
                })
                {
                    Keywords = new[] { "combo", "override", "color" },
                },
                new SettingsItemV2(new FormCheckBox
                {
                    Caption = SkinSettingsStrings.BeatmapHitsounds,
                    Current = config.GetBindable<bool>(OsuSetting.BeatmapHitsounds)
                })
                {
                    Keywords = new[] { "samples", "override" },
                },
                new SettingsItemV2(new FormCheckBox
                {
                    Caption = GraphicsSettingsStrings.StoryboardVideo,
                    Current = config.GetBindable<bool>(OsuSetting.ShowStoryboard)
                }),
                new SettingsItemV2(new FormSliderBar<float>
                {
                    Caption = GraphicsSettingsStrings.ComboColourNormalisation,
                    Current = comboColourNormalisation,
                    DisplayAsPercentage = true,
                })
                {
                    Keywords = new[] { "color" },
                },
            };
        }
    }
}
