// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.Edit.Compose;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Tests.Visual.Editor
{
    [TestFixture]
    public class TestCaseEditorCompose : EditorClockTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[] { typeof(ComposeScreen) };

        [BackgroundDependencyLoader]
        private void load()
        {
            Beatmap.Value = new TestWorkingBeatmap(new OsuRuleset().RulesetInfo, Clock);
            Child = new ComposeScreen();
        }
    }
}
