// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics.Containers;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Game.IPC;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using osu.Game.Tests.Resources;

namespace osu.Game.Tests.Visual.Navigation
{
    [TestFixture]
    [Ignore("This test cannot be run headless, as it requires the game host running the nested game to have IPC bound.")]
    public partial class TestSceneInterProcessCommunication : OsuGameTestScene
    {
        private HeadlessGameHost ipcSenderHost = null!;

        private OsuInstanceIPCChannel osuInstanceIPCSender = null!;

        private const int requested_beatmap_set_id = 1;

        private readonly string cwd = Environment.CurrentDirectory;

        protected override TestOsuGame CreateTestGame() => new IpcGame(LocalStorage, API, cwd);

        [Resolved]
        private GameHost gameHost { get; set; } = null!;

        public override void SetUpSteps()
        {
            base.SetUpSteps();
            AddStep("set up request handling", () =>
            {
                ((DummyAPIAccess)API).HandleRequest = request =>
                {
                    switch (request)
                    {
                        case GetBeatmapSetRequest gbr:

                            var apiBeatmapSet = CreateAPIBeatmapSet();
                            apiBeatmapSet.OnlineID = requested_beatmap_set_id;
                            apiBeatmapSet.Beatmaps = apiBeatmapSet.Beatmaps.Append(new APIBeatmap
                            {
                                DifficultyName = "Target difficulty",
                                OnlineID = 75,
                            }).ToArray();
                            gbr.TriggerSuccess(apiBeatmapSet);
                            return true;
                    }

                    return false;
                };
            });
            AddStep("create IPC sender channels", () =>
            {
                ipcSenderHost = new HeadlessGameHost(gameHost.Name, new HostOptions { BindIPC = true });
                osuInstanceIPCSender = new OsuInstanceIPCChannel(ipcSenderHost);
            });
        }

        [Test]
        public void TestOsuSchemeLinkIPCChannel()
        {
            AddStep("open beatmap via IPC", () => osuInstanceIPCSender.SendAsync(cwd, new[] { $@"osu://s/{requested_beatmap_set_id}" }).WaitSafely());
            AddUntilStep("beatmap overlay displayed", () => Game.ChildrenOfType<BeatmapSetOverlay>().FirstOrDefault()?.State.Value == Visibility.Visible);
            AddUntilStep("beatmap overlay showing content", () => Game.ChildrenOfType<BeatmapSetOverlay>().FirstOrDefault()?.Header.BeatmapSet.Value.OnlineID == requested_beatmap_set_id);
        }

        [Test]
        public void TestArchiveImportLinkIPCChannel()
        {
            string? beatmapFilepath = null;

            AddStep("import beatmap via IPC", () => osuInstanceIPCSender.SendAsync(cwd, new[] { beatmapFilepath = TestResources.GetQuickTestBeatmapForImport() }).WaitSafely());
            AddUntilStep("import complete notification was presented", () => Game.Notifications.ChildrenOfType<ProgressCompletionNotification>().Count(), () => Is.EqualTo(1));
            AddAssert("original file deleted", () => File.Exists(beatmapFilepath), () => Is.False);
        }

        public override void TearDownSteps()
        {
            AddStep("dispose IPC senders", () =>
            {
                osuInstanceIPCSender.Dispose();
                ipcSenderHost.Dispose();
            });
            base.TearDownSteps();
        }

        private partial class IpcGame : TestOsuGame
        {
            private OsuInstanceIPCChannel? osuInstanceIPCChannel;

            public IpcGame(Storage storage, IAPIProvider api, string cwd, string[]? args = null)
                : base(storage, api, cwd, args)
            {
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                osuInstanceIPCChannel = new OsuInstanceIPCChannel(Host);
            }

            protected override void Dispose(bool isDisposing)
            {
                base.Dispose(isDisposing);
                osuInstanceIPCChannel?.Dispose();
            }
        }
    }
}
