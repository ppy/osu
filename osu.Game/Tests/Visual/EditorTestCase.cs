// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Game.Rulesets;
using osu.Game.Screens.Edit;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Tests.Visual
{
    public abstract class EditorTestCase : ScreenTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[] { typeof(Editor), typeof(EditorScreen) };

        private readonly Ruleset ruleset;

        protected EditorTestCase(Ruleset ruleset)
        {
            this.ruleset = ruleset;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Beatmap.Value = new TestWorkingBeatmap(ruleset.RulesetInfo, null);

            LoadScreen(new Editor());
        }
    }
}
