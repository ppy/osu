// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Components;

namespace osu.Game.Tests.Visual.RankedPlay
{
    public partial class TestSceneRankedPlayScoreCounter : OsuTestScene
    {
        [Test]
        public void TestSimpleIncrementDecrement()
        {
            RankedPlayScoreCounter counter = null!;

            AddStep("add counter", () => Child = counter = new RankedPlayScoreCounter
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Font = OsuFont.Default.With(size: 72),
            });

            AddStep("set value to 10_000", () => counter.Value = 10_000);
            AddWaitStep("wait for animations", 3);

            AddStep("set value to 100_000", () => counter.Value = 100_000);
            AddWaitStep("wait for animations", 3);

            AddStep("set value to 1_000_000", () => counter.Value = 1_000_000);
            AddWaitStep("wait for animations", 3);

            AddStep("set value to 100_000", () => counter.Value = 100_000);
            AddWaitStep("wait for animations", 3);

            AddStep("set value to 10_000", () => counter.Value = 10_000);
            AddWaitStep("wait for animations", 3);

            AddStep("set value to 0", () => counter.Value = 0);
            AddWaitStep("wait for animations", 3);
        }

        [Test]
        public void TestDigitBoundaryIncrementDecrement()
        {
            RankedPlayScoreCounter counter = null!;

            AddStep("add counter", () => Child = counter = new RankedPlayScoreCounter
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Font = OsuFont.Default.With(size: 72),
            });

            AddStep("set value to 99_999", () => counter.Value = 99_999);
            AddWaitStep("wait for animations", 3);

            AddStep("set value to 100_000", () => counter.Value = 100_000);
            AddWaitStep("wait for animations", 3);

            AddStep("set value to 99_999", () => counter.Value = 99_999);
            AddWaitStep("wait for animations", 3);
        }
    }
}
