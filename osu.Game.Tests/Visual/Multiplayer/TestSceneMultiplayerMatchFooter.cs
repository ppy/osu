// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Screens.OnlinePlay.Multiplayer.Match;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneMultiplayerMatchFooter : MultiplayerTestScene
    {
        [SetUp]
        public new void Setup() => Schedule(() =>
        {
            Child = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.X,
                Height = 50,
                Child = new MultiplayerMatchFooter()
            };
        });
    }
}
