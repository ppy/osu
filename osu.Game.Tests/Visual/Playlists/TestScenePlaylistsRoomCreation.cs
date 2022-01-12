// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Platform;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.OnlinePlay.Components;
using osu.Game.Screens.OnlinePlay.Playlists;
using osu.Game.Screens.Play;
using osu.Game.Tests.Beatmaps;
using osu.Game.Tests.Visual.OnlinePlay;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Playlists
{
    public class TestScenePlaylistsRoomCreation : OnlinePlayTestScene
    {
        private BeatmapManager manager;
        private RulesetStore rulesets;

        private TestPlaylistsRoomSubScreen match;

        private ILive<BeatmapSetInfo> importedBeatmap;

        [BackgroundDependencyLoader]
        private void load(GameHost host, AudioManager audio)
        {
            Dependencies.Cache(rulesets = new RulesetStore(ContextFactory));
            Dependencies.Cache(manager = new BeatmapManager(LocalStorage, ContextFactory, rulesets, null, audio, Resources, host, Beatmap.Default));
        }

        [SetUpSteps]
        public void SetupSteps()
        {
            AddStep("set room", () => SelectedRoom.Value = new Room());

            importBeatmap();

            AddStep("load match", () => LoadScreen(match = new TestPlaylistsRoomSubScreen(SelectedRoom.Value)));
            AddUntilStep("wait for load", () => match.IsCurrentScreen());
        }

        [Test]
        public void TestLoadSimpleMatch()
        {
            setupAndCreateRoom(room =>
            {
                room.Name.Value = "my awesome room";
                room.Host.Value = API.LocalUser.Value;
                room.RecentParticipants.Add(room.Host.Value);
                room.EndDate.Value = DateTimeOffset.Now.AddMinutes(5);
                room.Playlist.Add(new PlaylistItem
                {
                    Beatmap = { Value = importedBeatmap.Value.Beatmaps.First() },
                    Ruleset = { Value = new OsuRuleset().RulesetInfo }
                });
            });

            AddUntilStep("Progress details are hidden", () => match.ChildrenOfType<RoomLocalUserInfo>().FirstOrDefault()?.Parent.Alpha == 0);

            AddStep("start match", () => match.ChildrenOfType<PlaylistsReadyButton>().First().TriggerClick());
            AddUntilStep("player loader loaded", () => Stack.CurrentScreen is PlayerLoader);
        }

        [Test]
        public void TestAttemptLimitedMatch()
        {
            setupAndCreateRoom(room =>
            {
                room.Name.Value = "my awesome room";
                room.MaxAttempts.Value = 5;
                room.Host.Value = API.LocalUser.Value;
                room.RecentParticipants.Add(room.Host.Value);
                room.EndDate.Value = DateTimeOffset.Now.AddMinutes(5);
                room.Playlist.Add(new PlaylistItem
                {
                    Beatmap = { Value = importedBeatmap.Value.Beatmaps.First() },
                    Ruleset = { Value = new OsuRuleset().RulesetInfo }
                });
            });

            AddUntilStep("Progress details are visible", () => match.ChildrenOfType<RoomLocalUserInfo>().FirstOrDefault()?.Parent.Alpha == 1);
        }

        [Test]
        public void TestPlaylistItemSelectedOnCreate()
        {
            setupAndCreateRoom(room =>
            {
                room.Name.Value = "my awesome room";
                room.Host.Value = API.LocalUser.Value;
                room.Playlist.Add(new PlaylistItem
                {
                    Beatmap = { Value = importedBeatmap.Value.Beatmaps.First() },
                    Ruleset = { Value = new OsuRuleset().RulesetInfo }
                });
            });

            AddAssert("first playlist item selected", () => match.SelectedItem.Value == SelectedRoom.Value.Playlist[0]);
        }

        [Test]
        public void TestBeatmapUpdatedOnReImport()
        {
            string realHash = null;
            int realOnlineId = 0;
            int realOnlineSetId = 0;

            AddStep("store real beatmap values", () =>
            {
                realHash = importedBeatmap.Value.Beatmaps[0].MD5Hash;
                realOnlineId = importedBeatmap.Value.Beatmaps[0].OnlineID ?? -1;
                realOnlineSetId = importedBeatmap.Value.OnlineID ?? -1;
            });

            AddStep("import modified beatmap", () =>
            {
                var modifiedBeatmap = new TestBeatmap(new OsuRuleset().RulesetInfo)
                {
                    BeatmapInfo =
                    {
                        OnlineID = realOnlineId,
                        BeatmapSet =
                        {
                            OnlineID = realOnlineSetId
                        }
                    },
                };

                modifiedBeatmap.HitObjects.Clear();
                modifiedBeatmap.HitObjects.Add(new HitCircle { StartTime = 5000 });

                manager.Import(modifiedBeatmap.BeatmapInfo.BeatmapSet).WaitSafely();
            });

            // Create the room using the real beatmap values.
            setupAndCreateRoom(room =>
            {
                room.Name.Value = "my awesome room";
                room.Host.Value = API.LocalUser.Value;
                room.Playlist.Add(new PlaylistItem
                {
                    Beatmap =
                    {
                        Value = new BeatmapInfo
                        {
                            MD5Hash = realHash,
                            OnlineID = realOnlineId,
                            BeatmapSet = new BeatmapSetInfo
                            {
                                OnlineID = realOnlineSetId,
                            }
                        }
                    },
                    Ruleset = { Value = new OsuRuleset().RulesetInfo }
                });
            });

            AddAssert("match has default beatmap", () => match.Beatmap.IsDefault);

            AddStep("reimport original beatmap", () =>
            {
                var originalBeatmap = new TestBeatmap(new OsuRuleset().RulesetInfo)
                {
                    BeatmapInfo =
                    {
                        OnlineID = realOnlineId,
                        BeatmapSet =
                        {
                            OnlineID = realOnlineSetId
                        }
                    },
                };

                manager.Import(originalBeatmap.BeatmapInfo.BeatmapSet).WaitSafely();
            });

            AddUntilStep("match has correct beatmap", () => realHash == match.Beatmap.Value.BeatmapInfo.MD5Hash);
        }

        private void setupAndCreateRoom(Action<Room> room)
        {
            AddStep("setup room", () => room(SelectedRoom.Value));

            AddStep("click create button", () =>
            {
                InputManager.MoveMouseTo(this.ChildrenOfType<PlaylistsRoomSettingsOverlay.CreateRoomButton>().Single());
                InputManager.Click(MouseButton.Left);
            });
        }

        private void importBeatmap() => AddStep("import beatmap", () => importedBeatmap = manager.Import(CreateBeatmap(new OsuRuleset().RulesetInfo).BeatmapInfo.BeatmapSet).GetResultSafely());

        private class TestPlaylistsRoomSubScreen : PlaylistsRoomSubScreen
        {
            public new Bindable<PlaylistItem> SelectedItem => base.SelectedItem;

            public new Bindable<WorkingBeatmap> Beatmap => base.Beatmap;

            public TestPlaylistsRoomSubScreen(Room room)
                : base(room)
            {
            }
        }
    }
}
