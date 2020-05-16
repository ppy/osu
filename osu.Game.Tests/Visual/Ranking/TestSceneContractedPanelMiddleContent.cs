// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens.Ranking;
using osu.Game.Screens.Ranking.Contracted;
using osu.Game.Tests.Beatmaps;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Tests.Visual.Ranking
{
    public class TestSceneContractedPanelMiddleContent : OsuTestScene
    {
        [Resolved]
        private RulesetStore rulesetStore { get; set; }

        [Test]
        public void TestMapWithKnownMapper()
        {
            var author = new User { Username = "mapper_name" };

            AddStep("show example score", () => showPanel(createTestBeatmap(author), createTestScore()));

            AddAssert("mapper name present", () => this.ChildrenOfType<OsuSpriteText>().Any(spriteText => spriteText.Text == "mapper_name"));
        }

        [Test]
        public void TestMapWithUnknownMapper()
        {
            AddStep("show example score", () => showPanel(createTestBeatmap(null), createTestScore()));

            AddAssert("mapped by text not present", () =>
                this.ChildrenOfType<OsuSpriteText>().All(spriteText => !containsAny(spriteText.Text, "mapped", "by")));
        }

        private void showPanel(WorkingBeatmap workingBeatmap, ScoreInfo score)
        {
            Child = new ContractedPanelMiddleContentContainer(workingBeatmap, score);
        }

        private WorkingBeatmap createTestBeatmap(User author)
        {
            var beatmap = new TestBeatmap(rulesetStore.GetRuleset(0));
            beatmap.Metadata.Author = author;
            beatmap.Metadata.Title = "Verrrrrrrrrrrrrrrrrrry looooooooooooooooooooooooong beatmap title";
            beatmap.Metadata.Artist = "Verrrrrrrrrrrrrrrrrrry looooooooooooooooooooooooong beatmap artist";

            return new TestWorkingBeatmap(beatmap);
        }

        private ScoreInfo createTestScore() => new ScoreInfo
        {
            User = new User
            {
                Id = 2,
                Username = "peppy",
                CoverUrl = "https://osu.ppy.sh/images/headers/profile-covers/c3.jpg",
            },
            Beatmap = new TestBeatmap(new OsuRuleset().RulesetInfo).BeatmapInfo,
            Mods = new Mod[] { new OsuModHardRock(), new OsuModDoubleTime() },
            TotalScore = 999999,
            Accuracy = 0.95,
            MaxCombo = 999,
            Rank = ScoreRank.S,
            Date = DateTimeOffset.Now,
            Statistics =
            {
                { HitResult.Miss, 1 },
                { HitResult.Meh, 50 },
                { HitResult.Good, 100 },
                { HitResult.Great, 300 },
            }
        };

        private bool containsAny(string text, params string[] stringsToMatch) => stringsToMatch.Any(text.Contains);

        private class ContractedPanelMiddleContentContainer : Container
        {
            [Cached]
            private Bindable<WorkingBeatmap> workingBeatmap { get; set; }

            public ContractedPanelMiddleContentContainer(WorkingBeatmap beatmap, ScoreInfo score)
            {
                workingBeatmap = new Bindable<WorkingBeatmap>(beatmap);

                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;
                Size = new Vector2(ScorePanel.CONTRACTED_WIDTH, 700);
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4Extensions.FromHex("#353535"),
                    },
                    new ContractedPanelMiddleContent(score),
                };
            }
        }
    }
}
