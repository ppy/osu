// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Overlays.Settings
{
    /// <summary>
    /// A non-generic interface for <see cref="SettingsItem{T}"/>s.
    /// </summary>
    public interface ISettingsItem : IExpandable, IDisposable
    {
        /// <summary>
        /// Invoked when the setting value has changed.
        /// </summary>
        event Action SettingChanged;
    }
}
