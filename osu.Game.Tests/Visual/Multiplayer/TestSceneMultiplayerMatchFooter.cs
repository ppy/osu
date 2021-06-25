// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Multiplayer.Match;
using osu.Game.Tests.Visual.OnlinePlay;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneMultiplayerMatchFooter : OnlinePlayTestScene
    {
        [SetUp]
        public new void Setup() => Schedule(() =>
        {
            SelectedRoom.Value = new Room();

            Child = new MultiplayerMatchFooter
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Height = 50
            };
        });
    }
}
