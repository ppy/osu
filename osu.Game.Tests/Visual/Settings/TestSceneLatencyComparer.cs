// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using NUnit.Framework;
using osu.Game.Screens;

namespace osu.Game.Tests.Visual.Settings
{
    public class TestSceneLatencyComparer : ScreenTestScene
    {
        [Test]
        public void TestBasic()
        {
            AddStep("Load screen", () => LoadScreen(new LatencyComparerScreen()));
        }
    }
}
