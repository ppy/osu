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
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Scoring;
using osu.Game.Screens.Ranking;
using osu.Game.Screens.Ranking.Expanded;
using osu.Game.Tests.Beatmaps;
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
            var author = new APIUser { Username = "mapper_name" };

            AddStep("show example score", () => showPanel(new TestScoreInfo(new OsuRuleset().RulesetInfo)
            {
                BeatmapInfo = createTestBeatmap(author)
            }));
        }

        [Test]
        public void TestExcessMods()
        {
            var author = new APIUser { Username = "mapper_name" };

            AddStep("show excess mods score", () => showPanel(new TestScoreInfo(new OsuRuleset().RulesetInfo, true)
            {
                BeatmapInfo = createTestBeatmap(author)
            }));

            AddAssert("mapper name present", () => this.ChildrenOfType<OsuSpriteText>().Any(spriteText => spriteText.Current.Value == "mapper_name"));
        }

        [Test]
        public void TestMapWithUnknownMapper()
        {
            AddStep("show example score", () => showPanel(new TestScoreInfo(new OsuRuleset().RulesetInfo)
            {
                BeatmapInfo = createTestBeatmap(new APIUser())
            }));

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
                var beatmap = createTestBeatmap(new APIUser());

                showPanel(new TestScoreInfo(ruleset.RulesetInfo)
                {
                    Mods = mods,
                    BeatmapInfo = beatmap,
                    Date = default,
                });
            });

            AddAssert("play time not displayed", () => !this.ChildrenOfType<ExpandedPanelMiddleContent.PlayedOnText>().Any());
        }

        private void showPanel(ScoreInfo score) =>
            Child = new ExpandedPanelMiddleContentContainer(score);

        private BeatmapInfo createTestBeatmap([NotNull] APIUser author)
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
