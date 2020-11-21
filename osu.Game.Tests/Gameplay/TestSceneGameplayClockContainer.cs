// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.Play;
using osu.Game.Tests.Visual;

namespace osu.Game.Tests.Gameplay
{
    [HeadlessTest]
    public class TestSceneGameplayClockContainer : OsuTestScene
    {
        [Test]
        public void TestStartThenElapsedTime()
        {
            GameplayClockContainer gcc = null;

            AddStep("create container", () =>
            {
                var working = CreateWorkingBeatmap(new OsuRuleset().RulesetInfo);
                working.LoadTrack();

                Add(gcc = new GameplayClockContainer(working, 0));
            });

            AddStep("start track", () => gcc.Start());
            AddUntilStep("elapsed greater than zero", () => gcc.GameplayClock.ElapsedFrameTime > 0);
        }
    }
}
