// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Screens.Multi.Components;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneParticipantsList : MultiplayerTestScene
    {
        protected override bool UseOnlineAPI => true;

        public TestSceneParticipantsList()
        {
            Add(new ParticipantsList { RelativeSizeAxes = Axes.Both });
        }

        [SetUp]
        public new void Setup() => Schedule(() =>
        {
            Room.RoomID.Value = 7;
        });
    }
}
