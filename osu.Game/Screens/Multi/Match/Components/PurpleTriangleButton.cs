// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Screens.Multi.Match.Components
{
    public class PurpleTriangleButton : TriangleButton
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            BackgroundColour = Color4Extensions.FromHex(@"593790");
            Triangles.ColourLight = Color4Extensions.FromHex(@"7247b6");
            Triangles.ColourDark = Color4Extensions.FromHex(@"593790");
        }
    }
}
