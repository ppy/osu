// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Online;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets.Osu;
using osu.Game.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Users;
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

        private TestReplayDownloadButton downloadButton;

        public TestSceneReplayDownloadButton()
        {
            createButton();
            AddStep(@"downloading state", () => downloadButton.SetDownloadState(DownloadState.Downloading));
            AddStep(@"locally available state", () => downloadButton.SetDownloadState(DownloadState.LocallyAvailable));
            AddStep(@"not downloaded state", () => downloadButton.SetDownloadState(DownloadState.NotDownloaded));
        }

        private void createButton()
        {
            AddStep(@"create button", () => Child = downloadButton = new TestReplayDownloadButton(getScoreInfo())
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
