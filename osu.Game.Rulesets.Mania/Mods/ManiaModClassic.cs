// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Mania.Mods
{
    public class ManiaModClassic : ModClassic, IApplicableToHitObject
    {
        [SettingSource("Legacy hit windows", "Uses half-integer legacy hit windows.")]
        public Bindable<bool> LegacyHitWindows { get; } = new BindableBool(true);

        public void ApplyToHitObject(HitObject hitObject)
        {
            switch (hitObject)
            {
                case Note note:
                    if (LegacyHitWindows.Value)
                        note.HitWindows.SetLegacy(true);
                    break;
            }
        }
    }
}
