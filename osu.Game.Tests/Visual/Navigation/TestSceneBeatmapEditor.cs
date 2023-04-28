// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using System.Threading.Tasks;
using DeepEqual.Syntax;
using NUnit.Framework;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Menu;

namespace osu.Game.Tests.Visual.Navigation
{
    public partial class TestSceneBeatmapEditor : OsuGameTestScene
    {
        [Test]
        public void TestCancelNavigationToEditor()
        {
            BeatmapSetInfo[] beatmapSets = Array.Empty<BeatmapSetInfo>();

            AddStep("Timestamp current beatmapsets", () =>
            {
                Game.Realm.Run(realm =>
                {
                    beatmapSets = realm.All<BeatmapSetInfo>().Where(x => !x.DeletePending).ToArray();
                });
            });

            AddStep("Open editor and close it while loading", () =>
            {
                var task = Task.Run(async () =>
                {
                    await Task.Delay(100);
                    Game.ScreenStack.CurrentScreen.Exit();
                });

                Game.ScreenStack.Push(new EditorLoader());
            });

            AddUntilStep("wait for editor", () => Game.ScreenStack.CurrentScreen is MainMenu);

            BeatmapSetInfo[] currentSetInfos = Array.Empty<BeatmapSetInfo>();

            AddStep("Get current beatmaps", () =>
            {
                Game.Realm.Run(realm =>
                {
                    currentSetInfos = realm.All<BeatmapSetInfo>().Where(x => !x.DeletePending).ToArray();
                });
            });

            AddAssert("dummy beatmap didn't appear", () => currentSetInfos.IsDeepEqual(beatmapSets));
        }
    }
}
