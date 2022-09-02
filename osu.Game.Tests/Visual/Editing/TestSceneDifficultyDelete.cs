// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Storyboards;
using osu.Game.Tests.Beatmaps.IO;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Editing
{
    public class TestSceneDifficultyDelete : EditorTestScene
    {
        protected override Ruleset CreateEditorRuleset() => new OsuRuleset();
        protected override bool IsolateSavingFromDatabase => false;

        [Resolved]
        private OsuGameBase game { get; set; } = null!;

        [Resolved]
        private BeatmapManager beatmaps { get; set; } = null!;

        private BeatmapSetInfo importedBeatmapSet = null!;

        protected override WorkingBeatmap CreateWorkingBeatmap(IBeatmap beatmap, Storyboard storyboard = null!)
            => beatmaps.GetWorkingBeatmap(importedBeatmapSet.Beatmaps.First());

        public override void SetUpSteps()
        {
            AddStep("import test beatmap", () => importedBeatmapSet = BeatmapImportHelper.LoadOszIntoOsu(game, virtualTrack: true).GetResultSafely());
            base.SetUpSteps();
        }

        [Test]
        public void TestDifficultyDelete()
        {
            string lastDiff = null!;
            AddStep("remember selected difficulty", () => lastDiff = EditorBeatmap.BeatmapInfo.DifficultyName);

            AddStep("click File", () => this.ChildrenOfType<DrawableOsuMenuItem>().First().TriggerClick());
            AddStep("click Delete", () => this.ChildrenOfType<DrawableOsuMenuItem>().Single(deleteMenuItemPredicate).TriggerClick());
            AddStep("confirm", () => InputManager.Key(Key.Number2));

            AddAssert("difficulty is deleted", () =>
            {
                if (lastDiff == null!) throw new NullReferenceException();

                var newSet = beatmaps.GetWorkingBeatmap(importedBeatmapSet.Beatmaps.First(), true).BeatmapSetInfo;
                return newSet.Beatmaps.All(x => x.DifficultyName != lastDiff);
            });
        }

        private bool deleteMenuItemPredicate(DrawableOsuMenuItem item)
        {
            return item.ChildrenOfType<SpriteText>().Any(text => text.Text.ToString().StartsWith("Delete", StringComparison.Ordinal));
        }
    }
}
