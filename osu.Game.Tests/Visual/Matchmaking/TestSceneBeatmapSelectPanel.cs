// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Cursor;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Screens.OnlinePlay.Matchmaking.Match.BeatmapSelect;

namespace osu.Game.Tests.Visual.Matchmaking
{
    public partial class TestSceneBeatmapSelectPanel : MatchmakingTestScene
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
                var beatmap = CreateAPIBeatmap();

                beatmap.TopTags =
                [
                    new APIBeatmapTag { TagId = 4, VoteCount = 1 },
                    new APIBeatmapTag { TagId = 2, VoteCount = 1 },
                    new APIBeatmapTag { TagId = 23, VoteCount = 5 },
                ];

                beatmap.BeatmapSet!.HasExplicitContent = true;
                beatmap.BeatmapSet!.HasVideo = true;
                beatmap.BeatmapSet!.HasStoryboard = true;
                beatmap.BeatmapSet.FeaturedInSpotlight = true;
                beatmap.BeatmapSet.TrackId = 1;
                beatmap.BeatmapSet!.RelatedTags =
                [
                    new APITag
                    {
                        Id = 2,
                        Name = "song representation/simple",
                        Description = "Accessible and straightforward map design."
                    },
                    new APITag
                    {
                        Id = 4,
                        Name = "style/clean",
                        Description = "Visually uncluttered and organised patterns, often involving few overlaps and equal visual spacing between objects."
                    },
                    new APITag
                    {
                        Id = 23,
                        Name = "aim/aim control",
                        Description = "Patterns with velocity or direction changes which strongly go against a player's natural movement pattern."
                    }
                ];

                Child = new OsuContextMenuContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = panel = new MatchmakingSelectPanelBeatmap(new MatchmakingPlaylistItem(new MultiplayerPlaylistItem(), beatmap, []))
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

            AddStep("add peppy", () => panel!.AddUser(new APIUser
            {
                Id = 2,
                Username = "peppy",
            }));

            AddToggleStep("allow selection", value => panel!.AllowSelection = value);

            AddStep("reveal beatmap", () => panel!.PresentAsChosenBeatmap(new MatchmakingPlaylistItem(new MultiplayerPlaylistItem(), CreateAPIBeatmap(), [])));
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
