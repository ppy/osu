// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK.Graphics;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Scoring;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Screens.Select.Leaderboards;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.SongSelect
{
    public class TestSceneUserTopScoreContainer : OsuTestScene
    {
        public TestSceneUserTopScoreContainer()
        {
            UserTopScoreContainer topScoreContainer;

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
                    topScoreContainer = new UserTopScoreContainer
                    {
                        Origin = Anchor.BottomCentre,
                        Anchor = Anchor.BottomCentre,
                    }
                }
            });

            var scores = new[]
            {
                new APILegacyUserTopScoreInfo
                {
                    Position = 999,
                    Score = new APILegacyScoreInfo
                    {
                        Rank = ScoreRank.XH,
                        Accuracy = 1,
                        MaxCombo = 244,
                        TotalScore = 1707827,
                        Mods = new Mod[] { new OsuModHidden(), new OsuModHardRock(), },
                        User = new User
                        {
                            Id = 6602580,
                            Username = @"waaiiru",
                            Country = new Country
                            {
                                FullName = @"Spain",
                                FlagName = @"ES",
                            },
                        },
                    }
                },
                new APILegacyUserTopScoreInfo
                {
                    Position = 110000,
                    Score = new APILegacyScoreInfo
                    {
                        Rank = ScoreRank.X,
                        Accuracy = 1,
                        MaxCombo = 244,
                        TotalScore = 1707827,
                        User = new User
                        {
                            Id = 4608074,
                            Username = @"Skycries",
                            Country = new Country
                            {
                                FullName = @"Brazil",
                                FlagName = @"BR",
                            },
                        },
                    }
                },
                new APILegacyUserTopScoreInfo
                {
                    Position = 22333,
                    Score = new APILegacyScoreInfo
                    {
                        Rank = ScoreRank.S,
                        Accuracy = 1,
                        MaxCombo = 244,
                        TotalScore = 1707827,
                        User = new User
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
