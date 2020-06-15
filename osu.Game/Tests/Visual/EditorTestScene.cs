// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using JetBrains.Annotations;
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
        protected Editor Editor { get; private set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            Beatmap.Value = CreateWorkingBeatmap(Ruleset.Value);
        }

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("load editor", () => LoadScreen(Editor = CreateEditor()));
            AddUntilStep("wait for editor to load", () => Editor.ChildrenOfType<HitObjectComposer>().FirstOrDefault()?.IsLoaded == true
                                                          && Editor.ChildrenOfType<TimelineArea>().FirstOrDefault()?.IsLoaded == true);
        }

        /// <summary>
        /// Creates the ruleset for providing a corresponding beatmap to load the editor on.
        /// </summary>
        [NotNull]
        protected abstract Ruleset CreateEditorRuleset();

        protected sealed override Ruleset CreateRuleset() => CreateEditorRuleset();

        protected virtual Editor CreateEditor() => new Editor();
    }
}
