// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Menu;
using osu.Game.Screens.Select;
using osu.Game.Tests.Resources;

namespace osu.Game.Tests.Visual.Editing
{
    public partial class TestSceneOpenEditorTimestamp : OsuGameTestScene
    {
        private Editor editor => (Editor)Game.ScreenStack.CurrentScreen;
        private EditorBeatmap editorBeatmap => editor.ChildrenOfType<EditorBeatmap>().Single();
        private EditorClock editorClock => editor.ChildrenOfType<EditorClock>().Single();

        private void addStepClickLink(string timestamp, string step = "", bool waitForSeek = true)
        {
            AddStep($"{step} {timestamp}", () =>
                Game.HandleLink(new LinkDetails(LinkAction.OpenEditorTimestamp, timestamp))
            );

            if (waitForSeek)
                AddUntilStep("wait for seek", () => editorClock.SeekingOrStopped.Value);
        }

        private void addStepScreenModeTo(EditorScreenMode screenMode)
        {
            AddStep("change screen to " + screenMode, () => editor.Mode.Value = screenMode);
        }

        private void assertOnScreenAt(EditorScreenMode screen, double time, string text = "stayed in")
        {
            AddAssert($"{text} {screen} at {time}", () =>
                editor.Mode.Value == screen
                && editorClock.CurrentTime == time
            );
        }

        private void assertMovedScreenTo(EditorScreenMode screen, string text = "moved to")
        {
            AddAssert($"{text} {screen}", () => editor.Mode.Value == screen);
        }

        private void setUpEditor(RulesetInfo ruleset)
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
            AddUntilStep("Wait for editor open", () => editor.ReadyForUse);
        }

        [Test]
        public void TestErrorNotifications()
        {
            RulesetInfo rulesetInfo = new OsuRuleset().RulesetInfo;

            addStepClickLink("00:00:000", waitForSeek: false);
            AddAssert("recieved 'must be in edit'", () =>
                Game.Notifications.AllNotifications.Count(x => x.Text == EditorStrings.MustBeInEdit) == 1
            );

            AddStep("enter song select", () => Game.ChildrenOfType<ButtonSystem>().Single().OnSolo.Invoke());
            AddAssert("entered song select", () => Game.ScreenStack.CurrentScreen is PlaySongSelect);

            addStepClickLink("00:00:000 (1)", waitForSeek: false);
            AddAssert("recieved 'must be in edit'", () =>
                Game.Notifications.AllNotifications.Count(x => x.Text == EditorStrings.MustBeInEdit) == 2
            );

            setUpEditor(rulesetInfo);
            AddAssert("is editor Osu", () => editorBeatmap.BeatmapInfo.Ruleset.Equals(rulesetInfo));

            addStepClickLink("00:000", "invalid link", waitForSeek: false);
            AddAssert("recieved 'failed to process'", () =>
                Game.Notifications.AllNotifications.Count(x => x.Text == EditorStrings.FailedToProcessTimestamp) == 1
            );

            addStepClickLink("50000:00:000", "too long link", waitForSeek: false);
            AddAssert("recieved 'too long'", () =>
                Game.Notifications.AllNotifications.Count(x => x.Text == EditorStrings.TooLongTimestamp) == 1
            );
        }

        [Test]
        public void TestHandleCurrentScreenChanges()
        {
            const long long_link_value = 1_000 * 60 * 1_000;
            RulesetInfo rulesetInfo = new OsuRuleset().RulesetInfo;

            setUpEditor(rulesetInfo);
            AddAssert("is editor Osu", () => editorBeatmap.BeatmapInfo.Ruleset.Equals(rulesetInfo));

            addStepClickLink("1000:00:000", "long link");
            AddAssert("moved to end of track", () =>
                editorClock.CurrentTime == long_link_value
                || (editorClock.TrackLength < long_link_value && editorClock.CurrentTime == editorClock.TrackLength)
            );

            addStepScreenModeTo(EditorScreenMode.SongSetup);
            addStepClickLink("00:00:000");
            assertOnScreenAt(EditorScreenMode.SongSetup, 0);

            addStepClickLink("00:05:000 (0|0)");
            assertMovedScreenTo(EditorScreenMode.Compose);

            addStepScreenModeTo(EditorScreenMode.Design);
            addStepClickLink("00:10:000");
            assertOnScreenAt(EditorScreenMode.Design, 10_000);

            addStepClickLink("00:15:000 (1)");
            assertMovedScreenTo(EditorScreenMode.Compose);

            addStepScreenModeTo(EditorScreenMode.Timing);
            addStepClickLink("00:20:000");
            assertOnScreenAt(EditorScreenMode.Timing, 20_000);

            addStepClickLink("00:25:000 (0,1)");
            assertMovedScreenTo(EditorScreenMode.Compose);

            addStepScreenModeTo(EditorScreenMode.Verify);
            addStepClickLink("00:30:000");
            assertOnScreenAt(EditorScreenMode.Verify, 30_000);

            addStepClickLink("00:35:000 (0,1)");
            assertMovedScreenTo(EditorScreenMode.Compose);

            addStepClickLink("00:00:000");
            assertOnScreenAt(EditorScreenMode.Compose, 0);
        }
    }
}
