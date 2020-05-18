// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Screens.Multi.Match.Components;
using osuTK;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneMatchLeaderboardChatDisplay : MultiplayerTestScene
    {
        protected override bool UseOnlineAPI => true;

        public TestSceneMatchLeaderboardChatDisplay()
        {
            Room.RoomID.Value = 7;

            Add(new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(500),
                Child = new LeaderboardChatDisplay
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                }
            });
        }
    }
}
