// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Online.API.Requests.Responses;
using osuTK.Graphics;
using osu.Game.Online.Leaderboards;
using osu.Game.Overlays;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Scoring;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.SongSelect
{
    public class TestSceneUserTopScoreContainer : OsuTestScene
    {
        [Cached]
        private readonly DialogOverlay dialogOverlay;

        public TestSceneUserTopScoreContainer()
        {
            UserTopScoreContainer<ScoreInfo> topScoreContainer;

            Add(dialogOverlay = new DialogOverlay
            {
                Depth = -1
            });

            Add(new Container
            {
                Origin = Anchor.BottomCentre,
                Anchor = Anchor.Centre,
                AutoSizeAxes = Axes.Y,
                Width = 500,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.DarkGreen,
                    },
                    topScoreContainer = new UserTopScoreContainer<ScoreInfo>(s => new LeaderboardScore(s, s.Position, false))
                    {
                        Origin = Anchor.BottomCentre,
                        Anchor = Anchor.BottomCentre,
                    }
                }
            });

            var scores = new[]
            {
                new ScoreInfo
                {
                    Position = 999,
                    Rank = ScoreRank.XH,
                    Accuracy = 1,
                    MaxCombo = 244,
                    TotalScore = 1707827,
                    Mods = new Mod[] { new OsuModHidden(), new OsuModHardRock(), },
                    Ruleset = new OsuRuleset().RulesetInfo,
                    User = new APIUser
                    {
                        Id = 6602580,
                        Username = @"waaiiru",
                        Country = new Country
                        {
                            FullName = @"Spain",
                            FlagName = @"ES",
                        },
                    },
                },
                new ScoreInfo
                {
                    Position = 110000,
                    Rank = ScoreRank.X,
                    Accuracy = 1,
                    MaxCombo = 244,
                    TotalScore = 1707827,
                    Ruleset = new OsuRuleset().RulesetInfo,
                    User = new APIUser
                    {
                        Id = 4608074,
                        Username = @"Skycries",
                        Country = new Country
                        {
                            FullName = @"Brazil",
                            FlagName = @"BR",
                        },
                    },
                },
                new ScoreInfo
                {
                    Position = 22333,
                    Rank = ScoreRank.S,
                    Accuracy = 1,
                    MaxCombo = 244,
                    TotalScore = 1707827,
                    Ruleset = new OsuRuleset().RulesetInfo,
                    User = new APIUser
                    {
                        Id = 1541390,
                        Username = @"Toukai",
                        Country = new Country
                        {
                            FullName = @"Canada",
                            FlagName = @"CA",
                        },
                    },
                }
            };

            AddStep(@"Trigger visibility", topScoreContainer.ToggleVisibility);
            AddStep(@"Add score(rank 999)", () => topScoreContainer.Score.Value = scores[0]);
            AddStep(@"Add score(rank 110000)", () => topScoreContainer.Score.Value = scores[1]);
            AddStep(@"Add score(rank 22333)", () => topScoreContainer.Score.Value = scores[2]);
            AddStep(@"Add null score", () => topScoreContainer.Score.Value = null);
        }
    }
}
