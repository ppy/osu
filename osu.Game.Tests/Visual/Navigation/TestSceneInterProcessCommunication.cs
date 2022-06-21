// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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

namespace osu.Game.Tests.Visual.Navigation
{
    [TestFixture]
    [Ignore("This test cannot be run headless, as it requires the game host running the nested game to have IPC bound.")]
    public class TestSceneInterProcessCommunication : OsuGameTestScene
    {
        private HeadlessGameHost ipcSenderHost = null!;

        private OsuSchemeLinkIPCChannel osuSchemeLinkIPCReceiver = null!;
        private OsuSchemeLinkIPCChannel osuSchemeLinkIPCSender = null!;

        private const int requested_beatmap_set_id = 1;

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
            AddStep("create IPC receiver channel", () => osuSchemeLinkIPCReceiver = new OsuSchemeLinkIPCChannel(gameHost, Game));
            AddStep("create IPC sender channel", () =>
            {
                ipcSenderHost = new HeadlessGameHost(gameHost.Name, new HostOptions { BindIPC = true });
                osuSchemeLinkIPCSender = new OsuSchemeLinkIPCChannel(ipcSenderHost);
            });
        }

        [Test]
        public void TestOsuSchemeLinkIPCChannel()
        {
            AddStep("open beatmap via IPC", () => osuSchemeLinkIPCSender.HandleLinkAsync($@"osu://s/{requested_beatmap_set_id}").WaitSafely());
            AddUntilStep("beatmap overlay displayed", () => Game.ChildrenOfType<BeatmapSetOverlay>().FirstOrDefault()?.State.Value == Visibility.Visible);
            AddUntilStep("beatmap overlay showing content", () => Game.ChildrenOfType<BeatmapSetOverlay>().FirstOrDefault()?.Header.BeatmapSet.Value.OnlineID == requested_beatmap_set_id);
        }

        public override void TearDownSteps()
        {
            AddStep("dispose IPC receiver", () => osuSchemeLinkIPCReceiver.Dispose());
            AddStep("dispose IPC sender", () =>
            {
                osuSchemeLinkIPCSender.Dispose();
                ipcSenderHost.Dispose();
            });
            base.TearDownSteps();
        }
    }
}
