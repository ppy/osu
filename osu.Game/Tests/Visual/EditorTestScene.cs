// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Game.Rulesets;
using osu.Game.Screens.Edit;

namespace osu.Game.Tests.Visual
{
    public abstract class EditorTestScene : ScreenTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[] { typeof(Editor), typeof(EditorScreen) };

        private readonly Ruleset ruleset;

        protected EditorTestScene(Ruleset ruleset)
        {
            this.ruleset = ruleset;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Beatmap.Value = CreateWorkingBeatmap(ruleset.RulesetInfo);

            LoadScreen(new Editor());
        }
    }
}
