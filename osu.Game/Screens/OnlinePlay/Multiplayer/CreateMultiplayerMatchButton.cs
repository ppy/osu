// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Online.Multiplayer;
using osu.Game.Screens.OnlinePlay.Match.Components;

namespace osu.Game.Screens.OnlinePlay.Multiplayer
{
    public class CreateMultiplayerMatchButton : PurpleTriangleButton
    {
        [BackgroundDependencyLoader]
        private void load(StatefulMultiplayerClient multiplayerClient)
        {
            Triangles.TriangleScale = 1.5f;

            Text = "Create room";

            ((IBindable<bool>)Enabled).BindTo(multiplayerClient.IsConnected);
        }
    }
}
