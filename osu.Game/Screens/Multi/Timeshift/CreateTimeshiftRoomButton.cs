// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Screens.Multi.Match.Components;

namespace osu.Game.Screens.Multi.Timeshift
{
    public class CreateTimeshiftRoomButton : PurpleTriangleButton
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            Triangles.TriangleScale = 1.5f;

            Text = "Create playlist";
        }
    }
}
