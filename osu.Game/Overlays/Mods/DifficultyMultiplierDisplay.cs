// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osuTK;
using osu.Game.Localisation;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Overlays.Mods
{
    public sealed class DifficultyMultiplierDisplay : ModsEffectDisplay
    {
        protected override LocalisableString Label => DifficultyMultiplierDisplayStrings.DifficultyMultiplier;

        protected override string CounterFormat => @"N2";

        [Resolved]
        private IBindable<IReadOnlyList<Mod>> mods { get; set; } = null!;

        private ModSettingChangeTracker? settingChangeTracker;

        public DifficultyMultiplierDisplay()
        {
            Current.Default = 1d;
            Current.Value = 1d;
            Add(new SpriteIcon
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                Icon = FontAwesome.Solid.Times,
                Size = new Vector2(7),
                Margin = new MarginPadding { Top = 1 }
            });
        }

        protected override void LoadComplete()
        {
            mods.BindValueChanged(e =>
            {
                settingChangeTracker?.Dispose();

                updateMultiplier();

                settingChangeTracker = new ModSettingChangeTracker(e.NewValue);
                settingChangeTracker.SettingChanged += _ => updateMultiplier();
            }, true);
            base.LoadComplete();

            // required to prevent the counter initially rolling up from 0 to 1
            // due to `Current.Value` having a nonstandard default value of 1.
            Counter.SetCountWithoutRolling(Current.Value);
        }

        private void updateMultiplier()
        {
            double multiplier = 1.0;

            foreach (var mod in mods.Value)
                multiplier *= mod.ScoreMultiplier;

            Current.Value = multiplier;
        }
    }
}
