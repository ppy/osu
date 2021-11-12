// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Online;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Scoring;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Testing;
using osu.Game.Database;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.Ranking;
using osuTK.Input;
using APIUser = osu.Game.Online.API.Requests.Responses.APIUser;

namespace osu.Game.Tests.Visual.Gameplay
{
    [TestFixture]
    public class TestSceneReplayDownloadButton : OsuManualInputManagerTestScene
    {
        [Resolved]
        private RulesetStore rulesets { get; set; }

        private TestReplayDownloadButton downloadButton;

        [Test]
        public void TestDisplayStates()
        {
            AddStep(@"create button with replay", () =>
            {
                Child = downloadButton = new TestReplayDownloadButton(getScoreInfo(true))
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                };
            });

            AddUntilStep("wait for load", () => downloadButton.IsLoaded);

            AddStep(@"downloading state", () => downloadButton.SetDownloadState(DownloadState.Downloading));
            AddStep(@"locally available state", () => downloadButton.SetDownloadState(DownloadState.LocallyAvailable));
            AddStep(@"not downloaded state", () => downloadButton.SetDownloadState(DownloadState.NotDownloaded));
        }

        [Test]
        public void TestButtonWithReplayStartsDownload()
        {
            bool downloadStarted = false;
            bool downloadFinished = false;

            AddStep(@"create button with replay", () =>
            {
                downloadStarted = false;
                downloadFinished = false;

                Child = downloadButton = new TestReplayDownloadButton(getScoreInfo(true))
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                };

                downloadButton.State.BindValueChanged(state =>
                {
                    switch (state.NewValue)
                    {
                        case DownloadState.Downloading:
                            downloadStarted = true;
                            break;
                    }

                    switch (state.OldValue)
                    {
                        case DownloadState.Downloading:
                            downloadFinished = true;
                            break;
                    }
                });
            });

            AddUntilStep("wait for load", () => downloadButton.IsLoaded);

            AddAssert("state is available", () => downloadButton.State.Value == DownloadState.NotDownloaded);

            AddStep("click button", () =>
            {
                InputManager.MoveMouseTo(downloadButton);
                InputManager.Click(MouseButton.Left);
            });

            AddAssert("state entered downloading", () => downloadStarted);
            AddUntilStep("state left downloading", () => downloadFinished);
        }

        [Test]
        public void TestButtonWithoutReplay()
        {
            AddStep("create button without replay", () =>
            {
                Child = downloadButton = new TestReplayDownloadButton(getScoreInfo(false))
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                };
            });

            AddUntilStep("wait for load", () => downloadButton.IsLoaded);

            AddAssert("state is not downloaded", () => downloadButton.State.Value == DownloadState.NotDownloaded);
            AddAssert("button is not enabled", () => !downloadButton.ChildrenOfType<DownloadButton>().First().Enabled.Value);
        }

        [Resolved]
        private ScoreManager scoreManager { get; set; }

        [Test]
        public void TestScoreImportThenDelete()
        {
            ILive<ScoreInfo> imported = null;

            AddStep("create button without replay", () =>
            {
                Child = downloadButton = new TestReplayDownloadButton(getScoreInfo(false))
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                };
            });

            AddUntilStep("wait for load", () => downloadButton.IsLoaded);

            AddUntilStep("state is not downloaded", () => downloadButton.State.Value == DownloadState.NotDownloaded);

            AddStep("import score", () => imported = scoreManager.Import(getScoreInfo(true)).Result);

            AddUntilStep("state is available", () => downloadButton.State.Value == DownloadState.LocallyAvailable);

            AddStep("delete score", () => scoreManager.Delete(imported.Value));

            AddUntilStep("state is not downloaded", () => downloadButton.State.Value == DownloadState.NotDownloaded);
        }

        [Test]
        public void CreateButtonWithNoScore()
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

            AddAssert("state is not downloaded", () => downloadButton.State.Value == DownloadState.NotDownloaded);
            AddAssert("button is not enabled", () => !downloadButton.ChildrenOfType<DownloadButton>().First().Enabled.Value);
        }

        private ScoreInfo getScoreInfo(bool replayAvailable)
        {
            return new APIScoreInfo
            {
                OnlineID = 2553163309,
                RulesetID = 0,
                Beatmap = CreateAPIBeatmapSet(new OsuRuleset().RulesetInfo).Beatmaps.First(),
                HasReplay = replayAvailable,
                User = new APIUser
                {
                    Id = 39828,
                    Username = @"WubWoofWolf",
                }
            }.CreateScoreInfo(rulesets, CreateBeatmap(new OsuRuleset().RulesetInfo).BeatmapInfo);
        }

        private class TestReplayDownloadButton : ReplayDownloadButton
        {
            public void SetDownloadState(DownloadState state) => State.Value = state;

            public new Bindable<DownloadState> State => base.State;

            public TestReplayDownloadButton(ScoreInfo score)
                : base(score)
            {
            }
        }
    }
}
