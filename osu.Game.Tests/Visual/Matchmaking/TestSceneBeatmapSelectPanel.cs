// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Graphics.Cursor;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Screens.OnlinePlay.Matchmaking.Match.BeatmapSelect;
using osu.Game.Tests.Visual.Multiplayer;

namespace osu.Game.Tests.Visual.Matchmaking
{
    public partial class TestSceneBeatmapSelectPanel : MultiplayerTestScene
    {
        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Purple);

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("join room", () =>
            {
                var room = CreateDefaultRoom(MatchType.Matchmaking);
                room.Playlist = Enumerable.Range(1, 50).Select(i => new PlaylistItem(new MultiplayerPlaylistItem
                {
                    ID = i,
                    BeatmapID = 0,
                    StarRating = i / 10.0,
                })).ToArray();

                JoinRoom(room);
            });
        }

        [Test]
        public void TestBeatmapPanel()
        {
            MatchmakingSelectPanel? panel = null;

            AddStep("add panel", () =>
            {
                Child = new OsuContextMenuContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = panel = new MatchmakingSelectPanelBeatmap(new MatchmakingPlaylistItem(new MultiplayerPlaylistItem(), CreateAPIBeatmap(), []))
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

            AddToggleStep("allow selection", value => panel!.AllowSelection = value);
        }

        [Test]
        public void TestRandomPanel()
        {
            MatchmakingSelectPanelRandom? panel = null;

            AddStep("add panel", () =>
            {
                Child = new OsuContextMenuContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = panel = new MatchmakingSelectPanelRandom(new MultiplayerPlaylistItem { ID = -1 })
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    }
                };
            });

            AddToggleStep("allow selection", value => panel!.AllowSelection = value);

            AddStep("reveal beatmap", () => panel!.RevealBeatmap(CreateAPIBeatmap(), []));
        }

        [Test]
        public void TestBeatmapWithMods()
        {
            AddStep("add panel", () =>
            {
                MatchmakingSelectPanel? panel;

                Child = new OsuContextMenuContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = panel = new MatchmakingSelectPanelBeatmap(new MatchmakingPlaylistItem(new MultiplayerPlaylistItem(), CreateAPIBeatmap(), [new OsuModHardRock(), new OsuModDoubleTime()]))
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    }
                };

                panel.AddUser(new APIUser
                {
                    Id = 2,
                    Username = "peppy",
                });
            });
        }
    }
}
