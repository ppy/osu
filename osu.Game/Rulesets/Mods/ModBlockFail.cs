// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Configuration;
using osu.Game.Screens.Play;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModBlockFail : Mod, IApplicableFailOverride, IApplicableToHUD, IReadFromConfig
    {
        private readonly Bindable<bool> showHealthBar = new Bindable<bool>();

        /// <summary>
        /// We never fail, 'yo.
        /// </summary>
        public bool PerformFail() => false;

        public bool RestartOnFail => false;

        public void ReadFromConfig(OsuConfigManager config)
        {
            config.BindWith(OsuSetting.ShowHealthDisplayWhenCantFail, showHealthBar);
        }

        public void ApplyToHUD(HUDOverlay overlay)
        {
            overlay.ShowHealthBar.BindTo(showHealthBar);
        }
    }
}
