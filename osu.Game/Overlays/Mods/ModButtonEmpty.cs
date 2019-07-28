// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Overlays.Mods
{
    /// <summary>
    /// A mod button used exclusively for providing an empty space the size of a mod button.
    /// </summary>
    public class ModButtonEmpty : Container
    {
        public ModButtonEmpty()
        {
            Size = new Vector2(100f);
            AlwaysPresent = true;
        }
    }
}
