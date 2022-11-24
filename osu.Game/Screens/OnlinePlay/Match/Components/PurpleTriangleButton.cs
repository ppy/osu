// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Screens.OnlinePlay.Match.Components
{
    public class PurpleTriangleButton : TriangleButton
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            BackgroundColour = Color4Extensions.FromHex(@"593790");
        }
    }
}
