// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Graphics.Cursor;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osu.Game.Screens.OnlinePlay.Matchmaking.Match.BeatmapSelect;
using osu.Game.Tests.Visual.Multiplayer;

namespace osu.Game.Tests.Visual.Matchmaking
{
    public partial class TestSceneBeatmapSelectPanel : MultiplayerTestScene
    {
        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Purple);

        [Test]
        public void TestBeatmapPanel()
        {
            BeatmapSelectPanel? panel = null;

            AddStep("add panel", () =>
            {
                Child = new OsuContextMenuContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = panel = new BeatmapSelectPanel(new MultiplayerPlaylistItem())
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    }
                };
            });

            AddStep("add maarvin", () => panel!.AddUser(new APIUser
            {
                Id = DummyAPIAccess.DUMMY_USER_ID,
                Username = "Maarvin",
            }));
            AddStep("add peppy", () => panel!.AddUser(new APIUser
            {
                Id = 2,
                Username = "peppy",
            }));
            AddStep("add smogipoo", () => panel!.AddUser(new APIUser
            {
                Id = 1040328,
                Username = "smoogipoo",
            }));
            AddStep("remove smogipoo", () => panel!.RemoveUser(new APIUser { Id = 1040328 }));
            AddStep("remove peppy", () => panel!.RemoveUser(new APIUser { Id = 2 }));
            AddStep("remove maarvin", () => panel!.RemoveUser(new APIUser { Id = 6411631 }));

            AddToggleStep("allow selection", value =>
            {
                if (panel != null)
                    panel.AllowSelection = value;
            });
        }

        [Test]
        public void TestFailedBeatmapLookup()
        {
            AddStep("setup request handle", () =>
            {
                var api = (DummyAPIAccess)API;
                var handler = api.HandleRequest;
                api.HandleRequest = req =>
                {
                    switch (req)
                    {
                        case GetBeatmapRequest:
                        case GetBeatmapsRequest:
                            req.TriggerFailure(new InvalidOperationException());
                            return false;

                        default:
                            return handler?.Invoke(req) ?? false;
                    }
                };
            });

            AddStep("add panel", () =>
            {
                Child = new OsuContextMenuContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = new BeatmapSelectPanel(new MultiplayerPlaylistItem())
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    }
                };
            });
        }
    }
}
