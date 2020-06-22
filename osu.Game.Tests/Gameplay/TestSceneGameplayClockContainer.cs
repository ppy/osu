// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Rulesets.Mods;
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

            AddStep("create container", () => Add(gcc = new GameplayClockContainer(CreateWorkingBeatmap(new OsuRuleset().RulesetInfo), Array.Empty<Mod>(), 0)));
            AddStep("start track", () => gcc.Start());
            AddUntilStep("elapsed greater than zero", () => gcc.GameplayClock.ElapsedFrameTime > 0);
        }
    }
}
