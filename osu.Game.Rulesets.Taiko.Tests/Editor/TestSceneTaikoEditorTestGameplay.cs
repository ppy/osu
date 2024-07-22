// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Components.Timelines.Summary;
using osu.Game.Screens.Edit.GameplayTest;
using osu.Game.Storyboards;
using osu.Game.Tests.Beatmaps.IO;
using osu.Game.Tests.Visual;
using osuTK.Input;

namespace osu.Game.Rulesets.Taiko.Tests.Editor
{
    public partial class TestSceneTaikoEditorTestGameplay : EditorTestScene
    {
        protected override bool IsolateSavingFromDatabase => false;

        protected override Ruleset CreateEditorRuleset() => new TaikoRuleset();

        [Resolved]
        private OsuGameBase game { get; set; } = null!;

        [Resolved]
        private BeatmapManager beatmaps { get; set; } = null!;

        private BeatmapSetInfo importedBeatmapSet = null!;

        public override void SetUpSteps()
        {
            AddStep("import test beatmap", () => importedBeatmapSet = BeatmapImportHelper.LoadOszIntoOsu(game).GetResultSafely());
            base.SetUpSteps();
        }

        protected override WorkingBeatmap CreateWorkingBeatmap(IBeatmap beatmap, Storyboard? storyboard = null)
            => beatmaps.GetWorkingBeatmap(importedBeatmapSet.Beatmaps.First(b => b.Ruleset.OnlineID == 1));

        [Test]
        public void TestBasicGameplayTest()
        {
            AddStep("add objects", () =>
            {
                EditorBeatmap.Clear();
                EditorBeatmap.Add(new Swell { StartTime = 500, EndTime = 1500 });
                EditorBeatmap.Add(new Hit { StartTime = 3000 });
            });
            AddStep("seek to 250", () => EditorClock.Seek(250));
            AddUntilStep("wait for seek", () => EditorClock.CurrentTime, () => Is.EqualTo(250));

            AddStep("click test gameplay button", () =>
            {
                var button = Editor.ChildrenOfType<TestGameplayButton>().Single();

                InputManager.MoveMouseTo(button);
                InputManager.Click(MouseButton.Left);
            });
            AddUntilStep("save prompt shown", () => DialogOverlay.CurrentDialog is SaveRequiredPopupDialog);

            AddStep("save changes", () => DialogOverlay.CurrentDialog!.PerformOkAction());
            AddUntilStep("player pushed", () => Stack.CurrentScreen is EditorPlayer);
            AddUntilStep("wait for return to editor", () => Stack.CurrentScreen is Screens.Edit.Editor);
        }
    }
}
