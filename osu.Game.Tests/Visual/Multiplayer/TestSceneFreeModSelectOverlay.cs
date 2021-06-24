// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics.Containers;
using osu.Game.Screens.OnlinePlay;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneFreeModSelectOverlay : OsuTestScene
    {
        [SetUp]
        public void Setup() => Schedule(() =>
        {
            Child = new TestMultiplayerRoomContainer
            {
                Child = new FreeModSelectOverlay
                {
                    State = { Value = Visibility.Visible }
                }
            };
        });
    }
}
