// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using JetBrains.Annotations;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Sprites;
using osu.Game.Models;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Scoring;
using osu.Game.Screens.Ranking;
using osu.Game.Screens.Ranking.Expanded;
using osu.Game.Tests.Beatmaps;
using osu.Game.Tests.Resources;
using osuTK;

namespace osu.Game.Tests.Visual.Ranking
{
    public class TestSceneExpandedPanelMiddleContent : OsuTestScene
    {
        [Resolved]
        private RulesetStore rulesetStore { get; set; }

        [Test]
        public void TestMapWithKnownMapper()
        {
            var author = new RealmUser { Username = "mapper_name" };

            AddStep("show example score", () => showPanel(TestResources.CreateTestScoreInfo(createTestBeatmap(author))));
        }

        [Test]
        public void TestExcessMods()
        {
            AddStep("show excess mods score", () =>
            {
                var author = new RealmUser { Username = "mapper_name" };

                var score = TestResources.CreateTestScoreInfo(createTestBeatmap(author));
                score.Mods = score.BeatmapInfo.Ruleset.CreateInstance().CreateAllMods().ToArray();

                showPanel(score);
            });

            AddAssert("mapper name present", () => this.ChildrenOfType<OsuSpriteText>().Any(spriteText => spriteText.Current.Value == "mapper_name"));
        }

        [Test]
        public void TestMapWithUnknownMapper()
        {
            AddStep("show example score", () => showPanel(TestResources.CreateTestScoreInfo(createTestBeatmap(new RealmUser()))));

            AddAssert("mapped by text not present", () =>
                this.ChildrenOfType<OsuSpriteText>().All(spriteText => !containsAny(spriteText.Text.ToString(), "mapped", "by")));

            AddAssert("play time displayed", () => this.ChildrenOfType<ExpandedPanelMiddleContent.PlayedOnText>().Any());
        }

        [Test]
        public void TestWithDefaultDate()
        {
            AddStep("show autoplay score", () =>
            {
                var ruleset = new OsuRuleset();

                var mods = new Mod[] { ruleset.GetAutoplayMod() };
                var beatmap = createTestBeatmap(new RealmUser());

                var score = TestResources.CreateTestScoreInfo(beatmap);

                score.Mods = mods;
                score.Date = default;

                showPanel(score);
            });

            AddAssert("play time not displayed", () => !this.ChildrenOfType<ExpandedPanelMiddleContent.PlayedOnText>().Any());
        }

        private void showPanel(ScoreInfo score) =>
            Child = new ExpandedPanelMiddleContentContainer(score);

        private BeatmapInfo createTestBeatmap([NotNull] RealmUser author)
        {
            var beatmap = new TestBeatmap(rulesetStore.GetRuleset(0)).BeatmapInfo;

            beatmap.Metadata.Author = author;
            beatmap.Metadata.Title = "Verrrrrrrrrrrrrrrrrrry looooooooooooooooooooooooong beatmap title";
            beatmap.Metadata.Artist = "Verrrrrrrrrrrrrrrrrrry looooooooooooooooooooooooong beatmap artist";

            return beatmap;
        }

        private bool containsAny(string text, params string[] stringsToMatch) => stringsToMatch.Any(text.Contains);

        private class ExpandedPanelMiddleContentContainer : Container
        {
            public ExpandedPanelMiddleContentContainer(ScoreInfo score)
            {
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;
                Size = new Vector2(ScorePanel.EXPANDED_WIDTH, 700);
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4Extensions.FromHex("#444"),
                    },
                    new ExpandedPanelMiddleContent(score)
                };
            }
        }
    }
}
