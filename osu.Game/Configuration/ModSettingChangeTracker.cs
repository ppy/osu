// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Configuration
{
    public class ModSettingChangeTracker : IDisposable
    {
        public Action<Mod> SettingChanged;

        private readonly List<ISettingsItem> references = new List<ISettingsItem>();

        public ModSettingChangeTracker(IEnumerable<Mod> mods)
        {
            foreach (var mod in mods)
            {
                foreach (var setting in mod.CreateSettingsControls().OfType<ISettingsItem>())
                {
                    setting.SettingChanged += () => SettingChanged?.Invoke(mod);
                    references.Add(setting);
                }
            }
        }

        public void Dispose()
        {
            SettingChanged = null;

            foreach (var r in references)
                r.Dispose();
            references.Clear();
        }
    }
}
