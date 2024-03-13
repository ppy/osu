// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;

namespace osu.Game.Overlays.Settings
{
    public interface ISettingsItem : IDrawable, IDisposable
    {
        event Action SettingChanged;

        /// <summary>
        /// Whether this setting has a classic default (ie. a different default which better aligns with osu-stable expectations).
        /// </summary>
        bool HasClassicDefault { get; }

        /// <summary>
        /// Apply the classic default value of the associated setting. Will throw if <see cref="HasClassicDefault"/> is <c>false</c>.
        /// </summary>
        void ApplyClassicDefault();

        /// <summary>
        /// Apply the default value of the associated setting.
        /// </summary>
        void ApplyDefault();
    }
}
