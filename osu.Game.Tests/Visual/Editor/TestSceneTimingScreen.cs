// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.Edit.Timing;

namespace osu.Game.Tests.Visual.Editor
{
    [TestFixture]
    public class TestSceneTimingScreen : EditorClockTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(ControlPointTable),
            typeof(ControlPointSettings),
            typeof(Section<>),
            typeof(TimingSection),
            typeof(EffectSection),
            typeof(SampleSection),
            typeof(DifficultySection),
            typeof(RowAttribute)
        };

        [BackgroundDependencyLoader]
        private void load()
        {
            Beatmap.Value = CreateWorkingBeatmap(new OsuRuleset().RulesetInfo);
            Child = new TimingScreen();
        }
    }
}
