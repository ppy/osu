// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Testing;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Edit;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Compose.Components.Timeline;

namespace osu.Game.Tests.Visual
{
    public abstract class EditorTestScene : ScreenTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[] { typeof(Editor), typeof(EditorScreen) };

        protected Editor Editor { get; private set; }

        private readonly Ruleset ruleset;

        protected EditorTestScene(Ruleset ruleset)
        {
            this.ruleset = ruleset;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Beatmap.Value = CreateWorkingBeatmap(ruleset.RulesetInfo);
        }

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("load editor", () => LoadScreen(Editor = new Editor()));
            AddUntilStep("wait for editor to load", () => Editor.ChildrenOfType<HitObjectComposer>().FirstOrDefault()?.IsLoaded == true
                                                          && Editor.ChildrenOfType<TimelineArea>().FirstOrDefault()?.IsLoaded == true);
        }
    }
}
