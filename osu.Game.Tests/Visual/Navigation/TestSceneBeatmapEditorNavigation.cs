// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Menu;

namespace osu.Game.Tests.Visual.Navigation
{
    public partial class TestSceneBeatmapEditorNavigation : OsuGameTestScene
    {
        [Test]
        public void TestCancelNavigationToEditor()
        {
            BeatmapSetInfo[] beatmapSets = null!;

            AddStep("Fetch initial beatmaps", () => beatmapSets = allBeatmapSets());

            AddStep("Set current beatmap to default", () => Game.Beatmap.SetDefault());

            AddStep("Push editor loader", () => Game.ScreenStack.Push(new EditorLoader()));
            AddUntilStep("Wait for loader current", () => Game.ScreenStack.CurrentScreen is EditorLoader);
            AddStep("Close editor while loading", () => Game.ScreenStack.CurrentScreen.Exit());

            AddUntilStep("Wait for menu", () => Game.ScreenStack.CurrentScreen is MainMenu);
            AddAssert("Check no new beatmaps were made", () => allBeatmapSets().SequenceEqual(beatmapSets));

            BeatmapSetInfo[] allBeatmapSets() => Game.Realm.Run(realm => realm.All<BeatmapSetInfo>().Where(x => !x.DeletePending).ToArray());
        }
    }
}
