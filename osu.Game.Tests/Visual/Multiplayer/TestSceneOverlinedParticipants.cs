// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Online.Multiplayer;
using osu.Game.Screens.Multi.Components;
using osuTK;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneOverlinedParticipants : MultiplayerTestScene
    {
        protected override bool UseOnlineAPI => true;

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            Room = new Room { RoomID = { Value = 7 } };
        });

        [Test]
        public void TestHorizontalLayout()
        {
            AddStep("create component", () =>
            {
                Child = new ParticipantsDisplay(Direction.Horizontal)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Width = 500,
                };
            });
        }

        [Test]
        public void TestVerticalLayout()
        {
            AddStep("create component", () =>
            {
                Child = new ParticipantsDisplay(Direction.Vertical)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(500)
                };
            });
        }
    }
}
