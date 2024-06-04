// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Screens.Play;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModNoFail : Mod, IApplicableFailOverride, IApplicableToHUD, IReadFromConfig
    {
        public override string Name => "No Fail";
        public override string Acronym => "NF";
        public override IconUsage? Icon => OsuIcon.ModNoFail;
        public override ModType Type => ModType.DifficultyReduction;
        public override LocalisableString Description => "You can't fail, no matter what.";
        public override double ScoreMultiplier => 0.5;
        public override Type[] IncompatibleMods => new[] { typeof(ModFailCondition), typeof(ModCinema) };
        public override bool Ranked => UsesDefaultConfiguration;

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
