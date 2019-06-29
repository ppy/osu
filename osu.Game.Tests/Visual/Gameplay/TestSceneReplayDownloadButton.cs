using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets.Osu;
using osu.Game.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Users;
using osuTK;
using System;
using System.Collections.Generic;

namespace osu.Game.Tests.Visual.Gameplay
{
    [TestFixture]
    public class TestSceneReplayDownloadButton : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(ReplayDownloadButton)
        };

        private ReplayDownloadButton downloadButton;

        public TestSceneReplayDownloadButton()
        {
            Add(new ReplayDownloadButton(getScoreInfo())
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            });
        }

        private ScoreInfo getScoreInfo()
        {
            return new APILegacyScoreInfo
            {
                ID = 1,
                OnlineScoreID = 2553163309,
                Ruleset = new OsuRuleset().RulesetInfo,
                Replay = true,
                User = new User
                {
                    Id = 39828,
                    Username = @"WubWoofWolf",
                }
            };
        }
    }
}
