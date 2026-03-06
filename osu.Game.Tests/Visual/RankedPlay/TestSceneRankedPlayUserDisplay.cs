// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay;
using osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Components;
using osu.Game.Tests.Visual.Multiplayer;
using osuTK;

namespace osu.Game.Tests.Visual.RankedPlay
{
    public partial class TestSceneRankedPlayUserDisplay : MultiplayerTestScene
    {
        private readonly BindableInt health = new BindableInt
        {
            MaxValue = 1_000_000,
            MinValue = 0,
            Value = 1_000_000,
        };

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("add display", () => Child = new RankedPlayUserDisplay(2, Anchor.BottomLeft, RankedPlayColourScheme.Blue)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(256, 72),
                Health = { BindTarget = health }
            });
        }

        [Test]
        public void TesUserDisplay()
        {
            AddStep("blue color scheme", () => Child = new RankedPlayUserDisplay(2, Anchor.BottomLeft, RankedPlayColourScheme.Blue)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(256, 72),
                Health = { BindTarget = health }
            });

            AddStep("red color scheme", () => Child = new RankedPlayUserDisplay(2, Anchor.BottomLeft, RankedPlayColourScheme.Red)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(256, 72),
                Health = { BindTarget = health }
            });

            AddSliderStep("health", 0, 1_000_000, 1_000_000, value => health.Value = value);
        }
    }
}
