// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Online;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets.Osu;
using osu.Game.Scoring;
using osu.Game.Users;
using System;
using System.Collections.Generic;
using osu.Game.Screens.Ranking.Pages;

namespace osu.Game.Tests.Visual.Gameplay
{
    [TestFixture]
    public class TestSceneReplayDownloadButton : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(ReplayDownloadButton)
        };

        private TestReplayDownloadButton downloadButton;

        public TestSceneReplayDownloadButton()
        {
            createButton(true);
            AddStep(@"downloading state", () => downloadButton.SetDownloadState(DownloadState.Downloading));
            AddStep(@"locally available state", () => downloadButton.SetDownloadState(DownloadState.LocallyAvailable));
            AddStep(@"not downloaded state", () => downloadButton.SetDownloadState(DownloadState.NotDownloaded));
            createButton(false);
        }

        private void createButton(bool withReplay)
        {
            AddStep(withReplay ? @"create button with replay" : "create button without replay", () =>
            {
                Child = downloadButton = new TestReplayDownloadButton(getScoreInfo(withReplay))
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                };
            });
        }

        private ScoreInfo getScoreInfo(bool replayAvailable)
        {
            return new APILegacyScoreInfo
            {
                ID = 1,
                OnlineScoreID = 2553163309,
                Ruleset = new OsuRuleset().RulesetInfo,
                Replay = replayAvailable,
                User = new User
                {
                    Id = 39828,
                    Username = @"WubWoofWolf",
                }
            };
        }

        private class TestReplayDownloadButton : ReplayDownloadButton
        {
            public void SetDownloadState(DownloadState state) => State.Value = state;

            public TestReplayDownloadButton(ScoreInfo score)
                : base(score)
            {
            }
        }
    }
}
