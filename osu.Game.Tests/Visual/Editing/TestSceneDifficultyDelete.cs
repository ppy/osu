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
    public partial class TestSceneDifficultyDelete : EditorTestScene
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
        public void TestDeleteDifficulties()
        {
            Guid deletedDifficultyID = Guid.Empty;
            int countBeforeDeletion = 0;
            string beatmapSetHashBefore = string.Empty;

            for (int i = 0; i < 12; i++)
            {
                // Will be reloaded after each deletion.
                AddUntilStep("wait for editor to load", () => Editor?.ReadyForUse == true);

                AddStep("store selected difficulty", () =>
                {
                    deletedDifficultyID = EditorBeatmap.BeatmapInfo.ID;
                    countBeforeDeletion = Beatmap.Value.BeatmapSetInfo.Beatmaps.Count;
                    beatmapSetHashBefore = Beatmap.Value.BeatmapSetInfo.Hash;
                });

                AddStep("click File", () => this.ChildrenOfType<DrawableOsuMenuItem>().First().TriggerClick());

                if (i == 11)
                {
                    // last difficulty shouldn't be able to be deleted.
                    AddAssert("Delete menu item disabled", () => getDeleteMenuItem().Item.Action.Disabled);
                }
                else
                {
                    AddStep("click delete", () => getDeleteMenuItem().TriggerClick());
                    AddUntilStep("wait for dialog", () => DialogOverlay.CurrentDialog != null);
                    AddStep("confirm", () => InputManager.Key(Key.Number1));

                    AddAssert($"difficulty {i} is unattached from set",
                        () => Beatmap.Value.BeatmapSetInfo.Beatmaps.Select(b => b.ID), () => Does.Not.Contain(deletedDifficultyID));
                    AddAssert("beatmap set difficulty count decreased by one",
                        () => Beatmap.Value.BeatmapSetInfo.Beatmaps.Count, () => Is.EqualTo(countBeforeDeletion - 1));
                    AddAssert("set hash changed", () => Beatmap.Value.BeatmapSetInfo.Hash, () => Is.Not.EqualTo(beatmapSetHashBefore));
                    AddAssert($"difficulty {i} is deleted from realm",
                        () => Realm.Run(r => r.Find<BeatmapInfo>(deletedDifficultyID)), () => Is.Null);
                }
            }
        }

        private DrawableOsuMenuItem getDeleteMenuItem() => this.ChildrenOfType<DrawableOsuMenuItem>()
                                                               .Single(item => item.ChildrenOfType<SpriteText>().Any(text => text.Text.ToString().StartsWith("Delete", StringComparison.Ordinal)));
    }
}
