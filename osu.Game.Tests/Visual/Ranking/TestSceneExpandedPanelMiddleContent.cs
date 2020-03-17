// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens.Ranking.Expanded;
using osu.Game.Screens.Ranking.Expanded.Accuracy;
using osu.Game.Screens.Ranking.Expanded.Statistics;
using osu.Game.Tests.Beatmaps;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Tests.Visual.Ranking
{
    public class TestSceneExpandedPanelMiddleContent : OsuTestScene
    {
        [Resolved]
        private BeatmapManager beatmaps { get; set; }

        private User author;

        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(ExpandedPanelMiddleContent),
            typeof(AccuracyCircle),
            typeof(AccuracyStatistic),
            typeof(ComboStatistic),
            typeof(CounterStatistic),
            typeof(StarRatingDisplay),
            typeof(StatisticDisplay),
            typeof(TotalScoreCounter)
        };

        protected override void LoadComplete()
        {
            base.LoadComplete();

            var beatmapInfo = beatmaps.QueryBeatmap(b => b.RulesetID == 0);
            if (beatmapInfo != null)
            {
                Beatmap.Value = beatmaps.GetWorkingBeatmap(beatmapInfo);
                author = Beatmap.Value.Metadata.Author;
            }
        }

        [Test]
        public void TestExampleScore()
        {
            addScoreStep(createTestScore());
        }

        [Test]
        public void TestScoreWithNullAuthor()
        {
            AddStep("set author to null", () => {
                Beatmap.Value.Metadata.Author = null;
            });
            addScoreStep(createTestScore());
            AddStep("set author to not null", () => {
                Beatmap.Value.Metadata.Author = author;
            });
        }

        private void addScoreStep(ScoreInfo score) => AddStep("add panel", () => {
            Child = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(500, 700),
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4Extensions.FromHex("#444"),
                    },
                    new ExpandedPanelMiddleContent(score)
                }
            };
        });

        private ScoreInfo createTestScore() => new ScoreInfo
        {
            User = new User
            {
                Id = 2,
                Username = "peppy",
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
    }
}
