// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Configuration;
using osu.Game.Rulesets.Osu.Configuration;
using osu.Game.Screens.Play.PlayerSettings;

namespace osu.Game.Rulesets.Osu.UI
{
    public partial class ReplayShowCoords : PlayerSettingsGroup
    {
        private readonly OsuRulesetConfigManager config;

        // BindableNumber<float> creates a slider, but SettingsNumberBox only accepts an int. That's why a textBox needs to be used.

        [SettingSource("X", "Current player X")]
        public Bindable<string> ReplayPlayerX { get; } = new Bindable<string>
        {
            Default = string.Empty,
            // Disabled = true //It will throw an exception if the value changes, even if it is changed in the code.
        };

        [SettingSource("Y", "Current player Y")]
        public Bindable<string> ReplayPlayerY { get; } = new Bindable<string>
        {
            Default = string.Empty,
        };

        public ReplayShowCoords(OsuRulesetConfigManager config)
            : base("Player Coords")
        {
            this.config = config;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AddRange(this.CreateSettingsControls());

            config.BindWith(OsuRulesetSetting.ReplayPlayerX, ReplayPlayerX);
            config.BindWith(OsuRulesetSetting.ReplayPlayerY, ReplayPlayerY);
        }
    }
}
