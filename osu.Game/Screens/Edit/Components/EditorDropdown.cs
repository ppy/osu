// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Screens.Edit.Components
{
    public class EditorDropdown<T> : OsuDropdown<T> // Only using the new constructor
    {
        protected override DropdownHeader CreateHeader() => new OsuDropdownHeader(30);
    }
}
