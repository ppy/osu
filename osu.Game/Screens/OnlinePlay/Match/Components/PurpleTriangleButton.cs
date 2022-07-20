// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Game.Graphics.UserInterface;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay.Match.Components
{
    public class PurpleTriangleButton : TriangleButton
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            BackgroundColour = Color4Extensions.FromHex(@"593790");
            Triangles.AccentColours = new Tuple<Color4, Color4>[] {
                Tuple.Create(Color4Extensions.FromHex(@"593790"), Color4Extensions.FromHex(@"7247b6"))
            };
            // Triangles.ColourLight = Color4Extensions.FromHex(@"7247b6");
            // Triangles.ColourDark = Color4Extensions.FromHex(@"593790");
        }
    }
}
