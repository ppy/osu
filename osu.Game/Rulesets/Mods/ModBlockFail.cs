// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Configuration;
using osu.Game.Screens.Play;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModBlockFail : Mod, IApplicableFailOverride, IApplicableToHUD, IReadFromConfig
    {
        private Bindable<bool> hideHealthBar = new Bindable<bool>();

        /// <summary>
        /// We never fail, 'yo.
        /// </summary>
        public bool AllowFail => false;

        public void ReadFromConfig(OsuConfigManager config)
        {
            hideHealthBar = config.GetBindable<bool>(OsuSetting.HideHealthBar);
        }

        public void ApplyToHUD(HUDOverlay overlay)
        {
            if (hideHealthBar.Value)
                overlay.HealthDisplay.Hide();
        }
    }
}
