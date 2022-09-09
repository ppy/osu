// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Extensions;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.GameplayTest;
using osu.Game.Screens.Select;
using osu.Game.Tests.Resources;

namespace osu.Game.Tests.Visual.Editing
{
    public class TestSceneEditorNavigation : OsuGameTestScene
    {
        [Test]
        public void TestEditorGameplayTestAlwaysUsesOriginalRuleset()
        {
            BeatmapSetInfo beatmapSet = null!;

            AddStep("import test beatmap", () => Game.BeatmapManager.Import(TestResources.GetTestBeatmapForImport()).WaitSafely());
            AddStep("retrieve beatmap", () => beatmapSet = Game.BeatmapManager.QueryBeatmapSet(set => !set.Protected).AsNonNull().Value.Detach());

            AddStep("present beatmap", () => Game.PresentBeatmap(beatmapSet));
            AddUntilStep("wait for song select",
                () => Game.Beatmap.Value.BeatmapSetInfo.Equals(beatmapSet)
                      && Game.ScreenStack.CurrentScreen is PlaySongSelect songSelect
                      && songSelect.IsLoaded);
            AddStep("switch ruleset", () => Game.Ruleset.Value = new ManiaRuleset().RulesetInfo);

            AddStep("open editor", () => ((PlaySongSelect)Game.ScreenStack.CurrentScreen).Edit(beatmapSet.Beatmaps.First(beatmap => beatmap.Ruleset.OnlineID == 0)));
            AddUntilStep("wait for editor open", () => Game.ScreenStack.CurrentScreen is Editor editor && editor.ReadyForUse);
            AddStep("test gameplay", () => ((Editor)Game.ScreenStack.CurrentScreen).TestGameplay());

            AddUntilStep("wait for player", () =>
            {
                // notifications may fire at almost any inopportune time and cause annoying test failures.
                // relentlessly attempt to dismiss any and all interfering overlays, which includes notifications.
                // this is theoretically not foolproof, but it's the best that can be done here.
                Game.CloseAllOverlays();
                return Game.ScreenStack.CurrentScreen is EditorPlayer editorPlayer && editorPlayer.IsLoaded;
            });

            AddAssert("current ruleset is osu!", () => Game.Ruleset.Value.Equals(new OsuRuleset().RulesetInfo));

            AddStep("exit to song select", () => Game.PerformFromScreen(_ => { }, typeof(PlaySongSelect).Yield()));
            AddUntilStep("wait for song select", () => Game.ScreenStack.CurrentScreen is PlaySongSelect);
            AddAssert("previous ruleset restored", () => Game.Ruleset.Value.Equals(new ManiaRuleset().RulesetInfo));
        }
    }
}
