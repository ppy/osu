// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Online;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Scoring;
using osu.Game.Users;
using osu.Framework.Allocation;
using osu.Game.Rulesets;
using osu.Game.Screens.Ranking;

namespace osu.Game.Tests.Visual.Gameplay
{
    [TestFixture]
    public class TestSceneReplayDownloadButton : OsuTestScene
    {
        [Resolved]
        private RulesetStore rulesets { get; set; }

        private TestReplayDownloadButton downloadButton;

        public TestSceneReplayDownloadButton()
        {
            createButton(true);
            AddStep(@"downloading state", () => downloadButton.SetDownloadState(DownloadState.Downloading));
            AddStep(@"locally available state", () => downloadButton.SetDownloadState(DownloadState.LocallyAvailable));
            AddStep(@"not downloaded state", () => downloadButton.SetDownloadState(DownloadState.NotDownloaded));
            createButton(false);
            createButtonNoScore();
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

            AddUntilStep("wait for load", () => downloadButton.IsLoaded);
        }

        private void createButtonNoScore()
        {
            AddStep("create button with null score", () =>
            {
                Child = downloadButton = new TestReplayDownloadButton(null)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                };
            });

            AddUntilStep("wait for load", () => downloadButton.IsLoaded);
        }

        private ScoreInfo getScoreInfo(bool replayAvailable)
        {
            return new APILegacyScoreInfo
            {
                OnlineScoreID = 2553163309,
                OnlineRulesetID = 0,
                Replay = replayAvailable,
                User = new User
                {
                    Id = 39828,
                    Username = @"WubWoofWolf",
                }
            }.CreateScoreInfo(rulesets);
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
