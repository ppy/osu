// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Online;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Osu;
using osu.Game.Scoring;
using osu.Game.Screens.Ranking;
using osu.Game.Tests.Resources;
using osuTK.Input;
using APIUser = osu.Game.Online.API.Requests.Responses.APIUser;

namespace osu.Game.Tests.Visual.Gameplay
{
    [TestFixture]
    public partial class TestSceneReplayDownloadButton : OsuManualInputManagerTestScene
    {
        private const long online_score_id = 2553163309;

        private TestReplayDownloadButton downloadButton;

        [Resolved]
        private BeatmapManager beatmapManager { get; set; }

        [Resolved]
        private ScoreManager scoreManager { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            beatmapManager.Import(TestResources.GetQuickTestBeatmapForImport()).WaitSafely();
        }

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("delete previous imports", () =>
            {
                scoreManager.Delete(s => s.OnlineID == online_score_id);
            });
        }

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

            checkState(DownloadState.NotDownloaded);

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

            checkState(DownloadState.NotDownloaded);
            AddAssert("button is not enabled", () => !downloadButton.ChildrenOfType<DownloadButton>().First().Enabled.Value);
        }

        [Test]
        public void TestLocallyAvailableWithoutReplay()
        {
            Live<ScoreInfo> imported = null;

            AddStep("import score", () => imported = scoreManager.Import(getScoreInfo(false, false)));

            AddStep("create button without replay", () =>
            {
                Child = downloadButton = new TestReplayDownloadButton(imported.Value)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                };
            });

            AddUntilStep("wait for load", () => downloadButton.IsLoaded);

            checkState(DownloadState.NotDownloaded);
            AddAssert("button is not enabled", () => !downloadButton.ChildrenOfType<DownloadButton>().First().Enabled.Value);
        }

        [Test]
        public void TestScoreImportThenDelete()
        {
            Live<ScoreInfo> imported = null;

            AddStep("create button without replay", () =>
            {
                Child = downloadButton = new TestReplayDownloadButton(getScoreInfo(false))
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                };
            });

            AddUntilStep("wait for load", () => downloadButton.IsLoaded);
            checkState(DownloadState.NotDownloaded);

            AddStep("import score", () => imported = scoreManager.Import(getScoreInfo(true)));

            checkState(DownloadState.LocallyAvailable);
            AddAssert("button is enabled", () => downloadButton.ChildrenOfType<DownloadButton>().First().Enabled.Value);

            AddStep("delete score", () => scoreManager.Delete(imported.Value));

            checkState(DownloadState.NotDownloaded);
            AddAssert("button is not enabled", () => !downloadButton.ChildrenOfType<DownloadButton>().First().Enabled.Value);
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

            checkState(DownloadState.Unknown);
            AddAssert("button is not enabled", () => !downloadButton.ChildrenOfType<DownloadButton>().First().Enabled.Value);
        }

        private void checkState(DownloadState expectedState) =>
            AddUntilStep($"state is {expectedState}", () => downloadButton.State.Value, () => Is.EqualTo(expectedState));

        private ScoreInfo getScoreInfo(bool replayAvailable, bool hasOnlineId = true) => new ScoreInfo
        {
            OnlineID = hasOnlineId ? online_score_id : 0,
            Ruleset = new OsuRuleset().RulesetInfo,
            BeatmapInfo = beatmapManager.GetAllUsableBeatmapSets().First().Beatmaps.First(),
            HasOnlineReplay = replayAvailable,
            User = new APIUser
            {
                Id = 39828,
                Username = @"WubWoofWolf",
            }
        };

        private partial class TestReplayDownloadButton : ReplayDownloadButton
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
