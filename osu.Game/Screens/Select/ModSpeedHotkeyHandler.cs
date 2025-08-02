// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Utils;
using osu.Game.Configuration;
using osu.Game.Input;
using osu.Game.Overlays;
using osu.Game.Overlays.OSD;
using osu.Game.Rulesets.Mods;
using osu.Game.Utils;

namespace osu.Game.Screens.Select
{
    public partial class ModSpeedHotkeyHandler : Component
    {
        [Resolved]
        private Bindable<IReadOnlyList<Mod>> selectedMods { get; set; } = null!;

        [Resolved]
        private RealmKeyBindingStore keyBindingStore { get; set; } = null!;

        [Resolved]
        private OnScreenDisplay? onScreenDisplay { get; set; }

        private ModRateAdjust? lastActiveRateAdjustMod;
        private ModSettingChangeTracker? settingChangeTracker;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            selectedMods.BindValueChanged(val =>
            {
                storeLastActiveRateAdjustMod();

                settingChangeTracker?.Dispose();
                settingChangeTracker = new ModSettingChangeTracker(val.NewValue);
                settingChangeTracker.SettingChanged += _ => storeLastActiveRateAdjustMod();
            }, true);
        }

        private void storeLastActiveRateAdjustMod()
        {
            lastActiveRateAdjustMod = (ModRateAdjust?)selectedMods.Value.OfType<ModRateAdjust>().SingleOrDefault()?.DeepClone() ?? lastActiveRateAdjustMod;
        }

        public bool ChangeSpeed(double delta, IEnumerable<Mod> availableMods)
        {
            double targetSpeed = (selectedMods.Value.OfType<ModRateAdjust>().SingleOrDefault()?.SpeedChange.Value ?? 1) + delta;

            if (Precision.AlmostEquals(targetSpeed, 1, 0.005))
            {
                selectedMods.Value = selectedMods.Value.Where(m => m is not ModRateAdjust).ToList();
                onScreenDisplay?.Display(new SpeedChangeToast(keyBindingStore, targetSpeed));
                return true;
            }

            ModRateAdjust? targetMod;

            if (lastActiveRateAdjustMod is ModDaycore || lastActiveRateAdjustMod is ModNightcore)
            {
                targetMod = targetSpeed < 1
                    ? availableMods.OfType<ModDaycore>().SingleOrDefault()
                    : availableMods.OfType<ModNightcore>().SingleOrDefault();
            }
            else
            {
                targetMod = targetSpeed < 1
                    ? availableMods.OfType<ModHalfTime>().SingleOrDefault()
                    : availableMods.OfType<ModDoubleTime>().SingleOrDefault();
            }

            if (targetMod == null)
                return false;

            // preserve other settings from latest rate adjust mod instance seen
            if (lastActiveRateAdjustMod != null)
            {
                foreach (var (_, sourceProperty) in lastActiveRateAdjustMod.GetSettingsSourceProperties())
                {
                    if (sourceProperty.Name == nameof(ModRateAdjust.SpeedChange))
                        continue;

                    var targetProperty = targetMod.GetType().GetProperty(sourceProperty.Name);

                    if (targetProperty == null)
                        continue;

                    var targetBindable = (IBindable)targetProperty.GetValue(targetMod)!;
                    var sourceBindable = (IBindable)sourceProperty.GetValue(lastActiveRateAdjustMod)!;

                    if (targetBindable.GetType() != sourceBindable.GetType())
                        continue;

                    lastActiveRateAdjustMod.CopyAdjustedSetting(targetBindable, sourceBindable);
                }
            }

            targetMod.SpeedChange.Value = targetSpeed;

            var intendedMods = selectedMods.Value.Where(m => m is not ModRateAdjust).Append(targetMod).ToList();

            if (!ModUtils.CheckCompatibleSet(intendedMods))
                return false;

            selectedMods.Value = intendedMods;
            onScreenDisplay?.Display(new SpeedChangeToast(keyBindingStore, targetMod.SpeedChange.Value));
            return true;
        }
    }
}
