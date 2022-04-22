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
        /// Apply the default values of a setting item, if the setting item specifies a "classic" default via <see cref="SettingsItem{T}.ApplyClassicDefault"/>.
        /// </summary>
        /// <param name="useClassicDefault">Whether to apply the classic value. If <c>false</c>, the standard default is applied.</param>
        void ApplyClassicDefault(bool useClassicDefault);
    }
}
