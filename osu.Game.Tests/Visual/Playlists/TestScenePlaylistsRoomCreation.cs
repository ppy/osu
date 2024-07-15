// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Platform;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osu.Game.Overlays.Dialog;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Menu;
using osu.Game.Screens.OnlinePlay.Components;
using osu.Game.Screens.OnlinePlay.Match.Components;
using osu.Game.Screens.OnlinePlay.Playlists;
using osu.Game.Screens.Play;
using osu.Game.Tests.Beatmaps;
using osu.Game.Tests.Visual.OnlinePlay;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Playlists
{
    public partial class TestScenePlaylistsRoomCreation : OnlinePlayTestScene
    {
        private BeatmapManager manager;

        private TestPlaylistsRoomSubScreen match;

        private BeatmapSetInfo importedBeatmap;

        [BackgroundDependencyLoader]
        private void load(GameHost host, AudioManager audio)
        {
            Dependencies.Cache(new RealmRulesetStore(Realm));
            Dependencies.Cache(manager = new BeatmapManager(LocalStorage, Realm, API, audio, Resources, host, Beatmap.Default));
            Dependencies.Cache(Realm);
        }

        [SetUpSteps]
        public void SetupSteps()
        {
            AddStep("set room", () => SelectedRoom!.Value = new Room());

            importBeatmap();

            AddStep("load match", () => LoadScreen(match = new TestPlaylistsRoomSubScreen(SelectedRoom!.Value)));
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
                room.Playlist.Add(new PlaylistItem(importedBeatmap.Beatmaps.First())
                {
                    RulesetID = new OsuRuleset().RulesetInfo.OnlineID
                });
            });

            AddUntilStep("Progress details are hidden", () => match.ChildrenOfType<RoomLocalUserInfo>().FirstOrDefault()?.Parent!.Alpha == 0);

            AddUntilStep("Leaderboard shows two aggregate scores", () => match.ChildrenOfType<MatchLeaderboardScore>().Count(s => s.ScoreText.Text != "0") == 2);

            ClickButtonWhenEnabled<PlaylistsReadyButton>();
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
                room.Playlist.Add(new PlaylistItem(importedBeatmap.Beatmaps.First())
                {
                    RulesetID = new OsuRuleset().RulesetInfo.OnlineID
                });
            });

            AddUntilStep("Progress details are visible", () => match.ChildrenOfType<RoomLocalUserInfo>().FirstOrDefault()?.Parent!.Alpha == 1);
        }

        [Test]
        public void TestPlaylistItemSelectedOnCreate()
        {
            setupAndCreateRoom(room =>
            {
                room.Name.Value = "my awesome room";
                room.Host.Value = API.LocalUser.Value;
                room.Playlist.Add(new PlaylistItem(importedBeatmap.Beatmaps.First())
                {
                    RulesetID = new OsuRuleset().RulesetInfo.OnlineID
                });
            });

            AddAssert("first playlist item selected", () => match.SelectedItem.Value == SelectedRoom!.Value.Playlist[0]);
        }

        [Test]
        public void TestBeatmapUpdatedOnReImport()
        {
            string realHash = null;
            int realOnlineId = 0;
            int realOnlineSetId = 0;

            AddStep("store real beatmap values", () =>
            {
                realHash = importedBeatmap.Beatmaps[0].MD5Hash;
                realOnlineId = importedBeatmap.Beatmaps[0].OnlineID;
                realOnlineSetId = importedBeatmap.OnlineID;
            });

            AddStep("import modified beatmap", () =>
            {
                var modifiedBeatmap = new TestBeatmap(new OsuRuleset().RulesetInfo)
                {
                    BeatmapInfo =
                    {
                        OnlineID = realOnlineId,
                        Metadata = new BeatmapMetadata(),
                        BeatmapSet =
                        {
                            OnlineID = realOnlineSetId
                        }
                    },
                };

                modifiedBeatmap.HitObjects.Clear();
                modifiedBeatmap.HitObjects.Add(new HitCircle { StartTime = 5000 });

                Debug.Assert(modifiedBeatmap.BeatmapInfo.BeatmapSet != null);

                manager.Import(modifiedBeatmap.BeatmapInfo.BeatmapSet);
            });

            // Create the room using the real beatmap values.
            setupAndCreateRoom(room =>
            {
                room.Name.Value = "my awesome room";
                room.Host.Value = API.LocalUser.Value;
                room.Playlist.Add(new PlaylistItem(new BeatmapInfo
                {
                    MD5Hash = realHash,
                    OnlineID = realOnlineId,
                    Metadata = new BeatmapMetadata(),
                    BeatmapSet = new BeatmapSetInfo
                    {
                        OnlineID = realOnlineSetId,
                    }
                })
                {
                    RulesetID = new OsuRuleset().RulesetInfo.OnlineID
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

                Debug.Assert(originalBeatmap.BeatmapInfo.BeatmapSet != null);

                manager.Import(originalBeatmap.BeatmapInfo.BeatmapSet);
            });

            AddUntilStep("match has correct beatmap", () => realHash == match.Beatmap.Value.BeatmapInfo.MD5Hash);
        }

        private void setupAndCreateRoom(Action<Room> room)
        {
            AddStep("setup room", () => room(SelectedRoom!.Value));

            AddStep("click create button", () =>
            {
                InputManager.MoveMouseTo(this.ChildrenOfType<PlaylistsRoomSettingsOverlay.CreateRoomButton>().Single());
                InputManager.Click(MouseButton.Left);
            });
        }

        private void importBeatmap() => AddStep("import beatmap", () =>
        {
            var beatmap = CreateBeatmap(new OsuRuleset().RulesetInfo);

            Debug.Assert(beatmap.BeatmapInfo.BeatmapSet != null);

            importedBeatmap = manager.Import(beatmap.BeatmapInfo.BeatmapSet)?.Value.Detach();
        });

        private partial class TestPlaylistsRoomSubScreen : PlaylistsRoomSubScreen
        {
            public new Bindable<PlaylistItem> SelectedItem => base.SelectedItem;

            public new Bindable<WorkingBeatmap> Beatmap => base.Beatmap;

            [Resolved(canBeNull: true)]
            [CanBeNull]
            private IDialogOverlay dialogOverlay { get; set; }

            public TestPlaylistsRoomSubScreen(Room room)
                : base(room)
            {
            }

            public override bool OnExiting(ScreenExitEvent e)
            {
                // For testing purposes allow the screen to exit without confirming on second attempt.
                if (!ExitConfirmed && dialogOverlay?.CurrentDialog is ConfirmDiscardChangesDialog confirmDialog)
                {
                    confirmDialog.PerformAction<PopupDialogDangerousButton>();
                    return true;
                }

                return base.OnExiting(e);
            }
        }
    }
}
