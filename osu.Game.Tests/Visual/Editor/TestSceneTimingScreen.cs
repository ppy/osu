// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.Edit.Timing;

namespace osu.Game.Tests.Visual.Editor
{
    [TestFixture]
    public class TestSceneTimingScreen : EditorClockTestScene
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            Beatmap.Value = CreateWorkingBeatmap(new OsuRuleset().RulesetInfo);
            Child = new TimingScreen();
        }
    }
}
