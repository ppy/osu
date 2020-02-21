// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Screens.Multi.Components;
using osuTK;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneOverlinedParticipants : MultiplayerTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(OverlinedParticipants),
            typeof(OverlinedDisplay),
            typeof(ParticipantsList)
        };

        protected override bool UseOnlineAPI => true;

        public TestSceneOverlinedParticipants()
        {
            Room.RoomID.Value = 7;
        }

        [Test]
        public void TestHorizontalLayout()
        {
            AddStep("create component", () =>
            {
                Child = new OverlinedParticipants(Direction.Horizontal)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Width = 500,
                    AutoSizeAxes = Axes.Y,
                };
            });
        }

        [Test]
        public void TestVerticalLayout()
        {
            AddStep("create component", () =>
            {
                Child = new OverlinedParticipants(Direction.Vertical)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(500)
                };
            });
        }
    }
}
