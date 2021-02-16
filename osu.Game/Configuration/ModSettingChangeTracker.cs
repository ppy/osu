// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Configuration
{
    /// <summary>
    /// A helper class for tracking changes to the settings of a set of <see cref="Mod"/>s.
    /// </summary>
    /// <remarks>
    /// Ensure to dispose when usage is finished.
    /// </remarks>
    public class ModSettingChangeTracker : IDisposable
    {
        /// <summary>
        /// Notifies that the setting of a <see cref="Mod"/> has changed.
        /// </summary>
        public Action<Mod> SettingChanged;

        private readonly List<ISettingsItem> settings = new List<ISettingsItem>();

        /// <summary>
        /// Creates a new <see cref="ModSettingChangeTracker"/> for a set of <see cref="Mod"/>s.
        /// </summary>
        /// <param name="mods">The set of <see cref="Mod"/>s whose settings need to be tracked.</param>
        public ModSettingChangeTracker(IEnumerable<Mod> mods)
        {
            foreach (var mod in mods)
            {
                foreach (var setting in mod.CreateSettingsControls().OfType<ISettingsItem>())
                {
                    setting.SettingChanged += () => SettingChanged?.Invoke(mod);
                    settings.Add(setting);
                }
            }
        }

        public void Dispose()
        {
            SettingChanged = null;

            foreach (var r in settings)
                r.Dispose();
            settings.Clear();
        }
    }
}
