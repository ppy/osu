// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Graphics.UserInterfaceV2
{
    public class LabelledEnumDropdown<TEnum> : LabelledDropdown<TEnum>
        where TEnum : struct, Enum
    {
        protected override OsuDropdown<TEnum> CreateDropdown() => new OsuEnumDropdown<TEnum>();
    }
}
