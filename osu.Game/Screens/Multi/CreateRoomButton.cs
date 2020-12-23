// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Screens.Multi.Match.Components;
using osuTK;

namespace osu.Game.Screens.Multi
{
    public class CreateRoomButton : PurpleTriangleButton
    {
        public CreateRoomButton()
        {
            Size = new Vector2(150, Header.HEIGHT - 20);
            Margin = new MarginPadding
            {
                Top = 10,
                Right = 10 + OsuScreen.HORIZONTAL_OVERFLOW_PADDING,
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Triangles.TriangleScale = 1.5f;

            Text = "Create room";
        }
    }
}
