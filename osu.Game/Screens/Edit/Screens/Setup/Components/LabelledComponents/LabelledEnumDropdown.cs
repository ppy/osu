// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Graphics.UserInterface;

namespace osu.Game.Screens.Edit.Screens.Setup.Components.LabelledComponents
{
    // TODO: Uncomment constraint once upgraded to C# 7.3 or greater
    public class LabelledEnumDropdown<T> : LabelledDropdown<T>
        //where T : Enum
    {
        protected override OsuDropdown<T> CreateDropdown() => new SetupEnumDropdown<T>();
    }
}
