// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Extensions;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Localisation;
using osu.Game.Online.Chat;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Menu;
using osu.Game.Screens.Select;
using osu.Game.Tests.Resources;

namespace osu.Game.Tests.Visual.Editing
{
    public partial class TestSceneOpenEditorTimestamp : OsuGameTestScene
    {
        protected Editor Editor => (Editor)Game.ScreenStack.CurrentScreen;
        protected EditorBeatmap EditorBeatmap => Editor.ChildrenOfType<EditorBeatmap>().Single();
        protected EditorClock EditorClock => Editor.ChildrenOfType<EditorClock>().Single();

        protected void AddStepClickLink(string timestamp, string step = "")
        {
            AddStep($"{step} {timestamp}", () =>
                Game.HandleLink(new LinkDetails(LinkAction.OpenEditorTimestamp, timestamp))
            );
        }

        protected void AddStepScreenModeTo(EditorScreenMode screenMode)
        {
            AddStep("change screen to " + screenMode, () => Editor.Mode.Value = screenMode);
        }

        protected void AssertOnScreenAt(EditorScreenMode screen, double time, string text = "stayed in")
        {
            AddAssert($"{text} {screen} at {time}", () =>
                Editor.Mode.Value == screen
                && EditorClock.CurrentTime == time
            );
        }

        protected void AssertMovedScreenTo(EditorScreenMode screen, string text = "moved to")
        {
            AddAssert($"{text} {screen}", () => Editor.Mode.Value == screen);
        }

        private bool checkSnapAndSelectCombo(double startTime, params int[] comboNumbers)
        {
            bool checkCombos = comboNumbers.Any()
                ? hasCombosInOrder(EditorBeatmap.SelectedHitObjects, comboNumbers)
                : !EditorBeatmap.SelectedHitObjects.Any();

            return EditorClock.CurrentTime == startTime
                   && EditorBeatmap.SelectedHitObjects.Count == comboNumbers.Length
                   && checkCombos;
        }

        private bool hasCombosInOrder(IEnumerable<HitObject> selected, params int[] comboNumbers)
        {
            List<HitObject> hitObjects = selected.ToList();
            if (hitObjects.Count != comboNumbers.Length)
                return false;

            return !hitObjects.Select(x => (IHasComboInformation)x)
                              .Where((combo, i) => combo.IndexInCurrentCombo + 1 != comboNumbers[i])
                              .Any();
        }

        private bool checkSnapAndSelectColumn(double startTime, IReadOnlyCollection<(int, int)> columnPairs = null)
        {
            bool checkColumns = columnPairs != null
                ? EditorBeatmap.SelectedHitObjects.All(x => columnPairs.Any(col => isNoteAt(x, col.Item1, col.Item2)))
                : !EditorBeatmap.SelectedHitObjects.Any();

            return EditorClock.CurrentTime == startTime
                   && EditorBeatmap.SelectedHitObjects.Count == (columnPairs?.Count ?? 0)
                   && checkColumns;
        }

        private bool isNoteAt(HitObject hitObject, double time, int column)
        {
            return hitObject is IHasColumn columnInfo
                   && hitObject.StartTime == time
                   && columnInfo.Column == column;
        }

        protected void SetUpEditor(RulesetInfo ruleset)
        {
            BeatmapSetInfo beatmapSet = null!;

            AddStep("Import test beatmap", () =>
                Game.BeatmapManager.Import(TestResources.GetTestBeatmapForImport()).WaitSafely()
            );
            AddStep("Retrieve beatmap", () =>
                beatmapSet = Game.BeatmapManager.QueryBeatmapSet(set => !set.Protected).AsNonNull().Value.Detach()
            );
            AddStep("Present beatmap", () => Game.PresentBeatmap(beatmapSet));
            AddUntilStep("Wait for song select", () =>
                Game.Beatmap.Value.BeatmapSetInfo.Equals(beatmapSet)
                && Game.ScreenStack.CurrentScreen is PlaySongSelect songSelect
                && songSelect.IsLoaded
            );
            AddStep("Switch ruleset", () => Game.Ruleset.Value = ruleset);
            AddStep("Open editor for ruleset", () =>
                ((PlaySongSelect)Game.ScreenStack.CurrentScreen)
                .Edit(beatmapSet.Beatmaps.Last(beatmap => beatmap.Ruleset.Name == ruleset.Name))
            );
            AddUntilStep("Wait for editor open", () => Editor.ReadyForUse);
        }

        [Test]
        public void TestErrorNotifications()
        {
            RulesetInfo rulesetInfo = new OsuRuleset().RulesetInfo;

            AddStepClickLink("00:00:000");
            AddAssert("recieved 'must be in edit'", () =>
                Game.Notifications.AllNotifications.Count(x => x.Text == EditorStrings.MustBeInEdit) == 1
            );

            AddStep("enter song select", () => Game.ChildrenOfType<ButtonSystem>().Single().OnSolo.Invoke());
            AddAssert("entered song select", () => Game.ScreenStack.CurrentScreen is PlaySongSelect);

            AddStepClickLink("00:00:000 (1)");
            AddAssert("recieved 'must be in edit'", () =>
                Game.Notifications.AllNotifications.Count(x => x.Text == EditorStrings.MustBeInEdit) == 2
            );

            SetUpEditor(rulesetInfo);
            AddAssert("is editor Osu", () => EditorBeatmap.BeatmapInfo.Ruleset.Equals(rulesetInfo));

            AddStepClickLink("00:000", "invalid link");
            AddAssert("recieved 'failed to process'", () =>
                Game.Notifications.AllNotifications.Count(x => x.Text == EditorStrings.FailedToProcessTimestamp) == 1
            );

            AddStepClickLink("00:00:00:000", "invalid link");
            AddAssert("recieved 'failed to process'", () =>
                Game.Notifications.AllNotifications.Count(x => x.Text == EditorStrings.FailedToProcessTimestamp) == 2
            );

            AddStepClickLink("00:00:000 ()", "invalid link");
            AddAssert("recieved 'failed to process'", () =>
                Game.Notifications.AllNotifications.Count(x => x.Text == EditorStrings.FailedToProcessTimestamp) == 3
            );

            AddStepClickLink("00:00:000 (-1)", "invalid link");
            AddAssert("recieved 'failed to process'", () =>
                Game.Notifications.AllNotifications.Count(x => x.Text == EditorStrings.FailedToProcessTimestamp) == 4
            );

            AddStepClickLink("50000:00:000", "too long link");
            AddAssert("recieved 'too long'", () =>
                Game.Notifications.AllNotifications.Count(x => x.Text == EditorStrings.TooLongTimestamp) == 1
            );
        }

        [Test]
        public void TestHandleCurrentScreenChanges()
        {
            const long long_link_value = 1_000 * 60 * 1_000;
            RulesetInfo rulesetInfo = new OsuRuleset().RulesetInfo;

            SetUpEditor(rulesetInfo);
            AddAssert("is editor Osu", () => EditorBeatmap.BeatmapInfo.Ruleset.Equals(rulesetInfo));

            AddStepClickLink("1000:00:000", "long link");
            AddAssert("moved to end of track", () =>
                EditorClock.CurrentTime == long_link_value
                || (EditorClock.TrackLength < long_link_value && EditorClock.CurrentTime == EditorClock.TrackLength)
            );

            AddStepScreenModeTo(EditorScreenMode.SongSetup);
            AddStepClickLink("00:00:000");
            AssertOnScreenAt(EditorScreenMode.SongSetup, 0);

            AddStepClickLink("00:05:000 (0|0)");
            AssertMovedScreenTo(EditorScreenMode.Compose);

            AddStepScreenModeTo(EditorScreenMode.Design);
            AddStepClickLink("00:10:000");
            AssertOnScreenAt(EditorScreenMode.Design, 10_000);

            AddStepClickLink("00:15:000 (1)");
            AssertMovedScreenTo(EditorScreenMode.Compose);

            AddStepScreenModeTo(EditorScreenMode.Timing);
            AddStepClickLink("00:20:000");
            AssertOnScreenAt(EditorScreenMode.Timing, 20_000);

            AddStepClickLink("00:25:000 (0,1)");
            AssertMovedScreenTo(EditorScreenMode.Compose);

            AddStepScreenModeTo(EditorScreenMode.Verify);
            AddStepClickLink("00:30:000");
            AssertOnScreenAt(EditorScreenMode.Verify, 30_000);

            AddStepClickLink("00:35:000 (0,1)");
            AssertMovedScreenTo(EditorScreenMode.Compose);

            AddStepClickLink("00:00:000");
            AssertOnScreenAt(EditorScreenMode.Compose, 0);
        }

        [Test]
        public void TestSelectionForOsu()
        {
            HitObject firstObject = null!;
            RulesetInfo rulesetInfo = new OsuRuleset().RulesetInfo;

            SetUpEditor(rulesetInfo);
            AddAssert("is editor Osu", () => EditorBeatmap.BeatmapInfo.Ruleset.Equals(rulesetInfo));

            AddStepClickLink("00:00:956 (1,2,3)");
            AddAssert("snap and select 1-2-3", () =>
            {
                firstObject = EditorBeatmap.HitObjects.First();
                return checkSnapAndSelectCombo(firstObject.StartTime, 1, 2, 3);
            });

            AddStepClickLink("00:01:450 (2,3,4,1,2)");
            AddAssert("snap and select 2-3-4-1-2", () => checkSnapAndSelectCombo(1_450, 2, 3, 4, 1, 2));

            AddStepClickLink("00:00:956 (1,1,1)");
            AddAssert("snap and select 1-1-1", () => checkSnapAndSelectCombo(firstObject.StartTime, 1, 1, 1));
        }

        [Test]
        public void TestUnusualSelectionForOsu()
        {
            HitObject firstObject = null!;
            RulesetInfo rulesetInfo = new OsuRuleset().RulesetInfo;

            SetUpEditor(rulesetInfo);
            AddAssert("is editor Osu", () => EditorBeatmap.BeatmapInfo.Ruleset.Equals(rulesetInfo));

            AddStepClickLink("00:00:000 (1,2,3)", "invalid offset");
            AddAssert("snap to next, select 1-2-3", () =>
            {
                firstObject = EditorBeatmap.HitObjects.First();
                return checkSnapAndSelectCombo(firstObject.StartTime, 1, 2, 3);
            });

            AddStepClickLink("00:00:956 (2,3,4)", "invalid offset");
            AddAssert("snap to next, select 2-3-4", () => checkSnapAndSelectCombo(firstObject.StartTime, 2, 3, 4));

            AddStepClickLink("00:00:000 (0)", "invalid offset");
            AddAssert("snap and select 1", () => checkSnapAndSelectCombo(firstObject.StartTime, 1));

            AddStepClickLink("00:00:000 (1)", "invalid offset");
            AddAssert("snap and select 1", () => checkSnapAndSelectCombo(firstObject.StartTime, 1));

            AddStepClickLink("00:00:000 (2)", "invalid offset");
            AddAssert("snap and select 1", () => checkSnapAndSelectCombo(firstObject.StartTime, 1));

            AddStepClickLink("00:00:000 (2,3)", "invalid offset");
            AddAssert("snap to 1, select 2-3", () => checkSnapAndSelectCombo(firstObject.StartTime, 2, 3));

            AddStepClickLink("00:00:956 (956|1,956|2)", "mania link");
            AddAssert("snap to next, select none", () => checkSnapAndSelectCombo(firstObject.StartTime));

            AddStepClickLink("00:00:000 (0|1)", "mania link");
            AddAssert("snap to 1, select none", () => checkSnapAndSelectCombo(firstObject.StartTime));
        }

        [Test]
        public void TestSelectionForMania()
        {
            RulesetInfo rulesetInfo = new ManiaRuleset().RulesetInfo;

            SetUpEditor(rulesetInfo);
            AddAssert("is editor Mania", () => EditorBeatmap.BeatmapInfo.Ruleset.Equals(rulesetInfo));

            AddStepClickLink("00:11:010 (11010|1,11175|5,11258|3,11340|5,11505|1)");
            AddAssert("selected group", () => checkSnapAndSelectColumn(11010, new List<(int, int)>
                { (11010, 1), (11175, 5), (11258, 3), (11340, 5), (11505, 1) }
            ));

            AddStepClickLink("00:00:956 (956|1,956|6,1285|3,1780|4)");
            AddAssert("selected ungrouped", () => checkSnapAndSelectColumn(956, new List<(int, int)>
                { (956, 1), (956, 6), (1285, 3), (1780, 4) }
            ));

            AddStepClickLink("02:36:560 (156560|1,156560|4,156560|6)");
            AddAssert("selected in row", () => checkSnapAndSelectColumn(156560, new List<(int, int)>
                { (156560, 1), (156560, 4), (156560, 6) }
            ));

            AddStepClickLink("00:35:736 (35736|3,36395|3,36725|3,37384|3)");
            AddAssert("selected in column", () => checkSnapAndSelectColumn(35736, new List<(int, int)>
                { (35736, 3), (36395, 3), (36725, 3), (37384, 3) }
            ));
        }

        [Test]
        public void TestUnusualSelectionForMania()
        {
            RulesetInfo rulesetInfo = new ManiaRuleset().RulesetInfo;

            SetUpEditor(rulesetInfo);
            AddAssert("is editor Mania", () => EditorBeatmap.BeatmapInfo.Ruleset.Equals(rulesetInfo));

            AddStepClickLink("00:00:000 (0|1)", "invalid link");
            AddAssert("snap to 1, select none", () => checkSnapAndSelectColumn(956));

            AddStepClickLink("00:00:000 (0)", "std link");
            AddAssert("snap and select 1", () => checkSnapAndSelectColumn(956, new List<(int, int)>
                { (956, 1) })
            );

            // TODO: discuss - this selects the first 2 objects on Stable, do we want that or is this fine?
            AddStepClickLink("00:00:000 (1,2)", "std link");
            AddAssert("snap to 1, select none", () => checkSnapAndSelectColumn(956));
        }
    }
}
