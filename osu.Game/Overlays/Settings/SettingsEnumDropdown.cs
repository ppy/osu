// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Settings
{
    public class SettingsEnumDropdown<T> : SettingsDropdown<T>
        where T : struct, Enum
    {
        protected override OsuDropdown<T> CreateDropdown() => new DropdownControl();

        protected new class DropdownControl : OsuEnumDropdown<T>
        {
            public DropdownControl()
            {
                RelativeSizeAxes = Axes.X;
            }

            protected override DropdownMenu CreateMenu() => base.CreateMenu().With(m => m.MaxHeight = 200);
        }
    }
}
