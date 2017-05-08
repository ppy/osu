// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Overlays.Mods
{
    /// <summary>
    /// A mod button used exclusively for providing an empty space the size of a mod button.
    /// </summary>
    public class ModButtonEmpty : Container
    {
        public virtual Mod SelectedMod => null;

        public ModButtonEmpty()
        {
            Size = new Vector2(100f);
            AlwaysPresent = true;
        }
    }
}
