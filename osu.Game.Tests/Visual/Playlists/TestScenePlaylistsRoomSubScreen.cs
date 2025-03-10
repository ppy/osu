// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Catch.Mods;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Taiko;
using osu.Game.Rulesets.Taiko.Mods;
using osu.Game.Screens;
using osu.Game.Screens.OnlinePlay;
using osu.Game.Screens.OnlinePlay.Playlists;
using osu.Game.Tests.Resources;
using osu.Game.Tests.Visual.OnlinePlay;

namespace osu.Game.Tests.Visual.Playlists
{
    public partial class TestScenePlaylistsRoomSubScreen : OnlinePlayTestScene
    {
        private BeatmapManager beatmaps = null!;
        private BeatmapSetInfo importedSet = null!;

        [BackgroundDependencyLoader]
        private void load(GameHost host, AudioManager audio)
        {
            BeatmapStore beatmapStore;

            Dependencies.Cache(new RealmRulesetStore(Realm));
            Dependencies.Cache(beatmaps = new BeatmapManager(LocalStorage, Realm, API, audio, Resources, host, Beatmap.Default));
            Dependencies.CacheAs(beatmapStore = new RealmDetachedBeatmapStore());
            Dependencies.Cache(Realm);

            Add(beatmapStore);

            importedSet = beatmaps.Import(new BeatmapSetInfo
            {
                OnlineID = TestResources.GetNextTestID(),
                Hash = new MemoryStream(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString())).ComputeMD5Hash(),
                DateAdded = DateTimeOffset.UtcNow,
                Beatmaps =
                {
                    new BeatmapInfo
                    {
                        OnlineID = 1,
                        DifficultyName = "Osu 1",
                        Hash = Guid.NewGuid().ToString().ComputeMD5Hash(),
                        MD5Hash = Guid.NewGuid().ToString().ComputeMD5Hash(),
                        Ruleset = new OsuRuleset().RulesetInfo,
                        Metadata =
                        {
                            Artist = "Some Artist",
                            Title = "Some Song",
                            Author = { Username = "Some Guy" },
                        },
                    },
                    new BeatmapInfo
                    {
                        OnlineID = 2,
                        DifficultyName = "Osu 2",
                        Hash = Guid.NewGuid().ToString().ComputeMD5Hash(),
                        MD5Hash = Guid.NewGuid().ToString().ComputeMD5Hash(),
                        Ruleset = new OsuRuleset().RulesetInfo,
                        Metadata =
                        {
                            Artist = "Some Artist",
                            Title = "Some Song",
                            Author = { Username = "Some Guy" },
                        },
                    },
                    new BeatmapInfo
                    {
                        OnlineID = 3,
                        DifficultyName = "Taiko 1",
                        Hash = Guid.NewGuid().ToString().ComputeMD5Hash(),
                        MD5Hash = Guid.NewGuid().ToString().ComputeMD5Hash(),
                        Ruleset = new TaikoRuleset().RulesetInfo,
                        Metadata =
                        {
                            Artist = "Some Artist",
                            Title = "Some Song",
                            Author = { Username = "Some Guy" },
                        },
                    },
                    new BeatmapInfo
                    {
                        OnlineID = 4,
                        DifficultyName = "Taiko 2",
                        Hash = Guid.NewGuid().ToString().ComputeMD5Hash(),
                        MD5Hash = Guid.NewGuid().ToString().ComputeMD5Hash(),
                        Ruleset = new TaikoRuleset().RulesetInfo,
                        Metadata =
                        {
                            Artist = "Some Artist",
                            Title = "Some Song",
                            Author = { Username = "Some Guy" },
                        },
                    }
                }
            })!.PerformRead(s => s.Detach());
        }

        /// <summary>
        /// Tests that the beatmap and ruleset are adjusted to follow the selected item.
        /// </summary>
        [Test]
        public void TestBeatmapAndRuleset_FollowSelection()
        {
            Room room = null!;

            AddStep("add room", () =>
            {
                room = new Room
                {
                    RoomID = 1,
                    Playlist =
                    [
                        // osu! beatmap
                        new PlaylistItem(importedSet.Beatmaps[0])
                        {
                            RulesetID = new OsuRuleset().RulesetInfo.OnlineID,
                            Freestyle = true
                        },
                        // osu! beatmap converted played in taiko
                        new PlaylistItem(importedSet.Beatmaps[1])
                        {
                            RulesetID = new TaikoRuleset().RulesetInfo.OnlineID,
                            Freestyle = true
                        }
                    ]
                };

                API.Perform(new CreateRoomRequest(room));
            });

            TestPlaylistsRoomSubScreen screen = null!;
            AddStep("load screen", () => LoadScreen(new TestPlaylistsScreen(screen = new TestPlaylistsRoomSubScreen(room))));
            AddUntilStep("wait for load", () => screen.IsLoaded);

            AddStep("select first item", () => screen.SelectedItem.Value = room.Playlist[0]);
            AddUntilStep("first beatmap selected", () => Beatmap.Value.BeatmapInfo.Equals(importedSet.Beatmaps[0]));
            AddUntilStep("osu ruleset selected", () => Ruleset.Value.Equals(new OsuRuleset().RulesetInfo));

            AddStep("select second item", () => screen.SelectedItem.Value = room.Playlist[1]);
            AddUntilStep("second beatmap selected", () => Beatmap.Value.BeatmapInfo.Equals(importedSet.Beatmaps[1]));
            AddUntilStep("taiko ruleset selected", () => Ruleset.Value.Equals(new TaikoRuleset().RulesetInfo));
        }

        /// <summary>
        /// Tests that the beatmap style is reset when the selected item is changed.
        /// </summary>
        [Test]
        public void TestBeatmapStyle_Reset_OnSelection()
        {
            Room room = null!;

            AddStep("add room", () =>
            {
                room = new Room
                {
                    RoomID = 1,
                    Playlist =
                    [
                        new PlaylistItem(importedSet.Beatmaps[0])
                        {
                            RulesetID = new OsuRuleset().RulesetInfo.OnlineID,
                            Freestyle = true
                        },
                        new PlaylistItem(importedSet.Beatmaps[0])
                        {
                            RulesetID = new OsuRuleset().RulesetInfo.OnlineID,
                            Freestyle = true
                        },
                    ]
                };

                API.Perform(new CreateRoomRequest(room));
            });

            TestPlaylistsRoomSubScreen screen = null!;
            AddStep("load screen", () => LoadScreen(new TestPlaylistsScreen(screen = new TestPlaylistsRoomSubScreen(room))));
            AddUntilStep("wait for load", () => screen.IsLoaded);

            AddStep("set user beatmap style", () => screen.UserBeatmap.Value = importedSet.Beatmaps[1]);
            AddUntilStep("user beatmap selected", () => Beatmap.Value.BeatmapInfo.Equals(importedSet.Beatmaps[1]));

            AddStep("select second item", () => screen.SelectedItem.Value = room.Playlist[1]);
            AddUntilStep("user beatmap style reset", () => screen.UserBeatmap.Value == null);
            AddUntilStep("second beatmap selected", () => Beatmap.Value.BeatmapInfo.Equals(importedSet.Beatmaps[0]));
        }

        /// <summary>
        /// Tests that the ruleset style is reset when the selected item is changed and it's no longer valid.
        /// </summary>
        [Test]
        public void TestRulesetStyle_Reset_OnSelection_IfNotValid()
        {
            Room room = null!;

            AddStep("add room", () =>
            {
                room = new Room
                {
                    RoomID = 1,
                    Playlist =
                    [
                        new PlaylistItem(importedSet.Beatmaps[0])
                        {
                            RulesetID = new OsuRuleset().RulesetInfo.OnlineID,
                            Freestyle = true
                        },
                        new PlaylistItem(importedSet.Beatmaps[0])
                        {
                            RulesetID = new TaikoRuleset().RulesetInfo.OnlineID,
                            Freestyle = true
                        },
                    ]
                };

                API.Perform(new CreateRoomRequest(room));
            });

            TestPlaylistsRoomSubScreen screen = null!;
            AddStep("load screen", () => LoadScreen(new TestPlaylistsScreen(screen = new TestPlaylistsRoomSubScreen(room))));
            AddUntilStep("wait for load", () => screen.IsLoaded);

            AddStep("set user ruleset style", () => screen.UserRuleset.Value = new ManiaRuleset().RulesetInfo);
            AddUntilStep("user ruleset selected", () => Ruleset.Value.Equals(new ManiaRuleset().RulesetInfo));

            AddStep("select second item", () => screen.SelectedItem.Value = room.Playlist[1]);
            AddUntilStep("user ruleset style reset", () => screen.UserRuleset.Value == null);
            AddUntilStep("second ruleset selected", () => Ruleset.Value.Equals(new TaikoRuleset().RulesetInfo));
        }

        /// <summary>
        /// Tests that the ruleset style is preserved when the selected item is changed and the ruleset is still valid.
        /// </summary>
        [Test]
        public void TestRulesetStyle_Preserved_OnSelection_IfStillValid()
        {
            Room room = null!;

            AddStep("add room", () =>
            {
                room = new Room
                {
                    RoomID = 1,
                    Playlist =
                    [
                        new PlaylistItem(importedSet.Beatmaps[0])
                        {
                            RulesetID = new OsuRuleset().RulesetInfo.OnlineID,
                            Freestyle = true
                        },
                        new PlaylistItem(importedSet.Beatmaps[0])
                        {
                            RulesetID = new OsuRuleset().RulesetInfo.OnlineID,
                            Freestyle = true
                        },
                    ]
                };

                API.Perform(new CreateRoomRequest(room));
            });

            TestPlaylistsRoomSubScreen screen = null!;
            AddStep("load screen", () => LoadScreen(new TestPlaylistsScreen(screen = new TestPlaylistsRoomSubScreen(room))));
            AddUntilStep("wait for load", () => screen.IsLoaded);

            AddStep("set user ruleset style", () => screen.UserRuleset.Value = new ManiaRuleset().RulesetInfo);
            AddUntilStep("user ruleset selected", () => Ruleset.Value.Equals(new ManiaRuleset().RulesetInfo));

            AddStep("select second item", () => screen.SelectedItem.Value = room.Playlist[1]);
            AddUntilStep("user ruleset style preserved", () => screen.UserRuleset.Value!.Equals(new ManiaRuleset().RulesetInfo));
            AddUntilStep("user ruleset selected", () => Ruleset.Value.Equals(new ManiaRuleset().RulesetInfo));
        }

        /// <summary>
        /// Tests that mod style is reset when the selected item is changed to another with an inconvertible ruleset.
        /// No user style is assumed.
        /// </summary>
        [Test]
        public void TestModsReset_OnSelection_DifferentRuleset_NoUserStyle()
        {
            Room room = null!;

            AddStep("add room", () =>
            {
                room = new Room
                {
                    RoomID = 1,
                    Playlist =
                    [
                        new PlaylistItem(importedSet.Beatmaps[0])
                        {
                            RulesetID = new OsuRuleset().RulesetInfo.OnlineID,
                            Freestyle = true
                        },
                        new PlaylistItem(importedSet.Beatmaps[0])
                        {
                            RulesetID = new TaikoRuleset().RulesetInfo.OnlineID,
                            Freestyle = true
                        },
                    ]
                };

                API.Perform(new CreateRoomRequest(room));
            });

            TestPlaylistsRoomSubScreen screen = null!;
            AddStep("load screen", () => LoadScreen(new TestPlaylistsScreen(screen = new TestPlaylistsRoomSubScreen(room))));
            AddUntilStep("wait for load", () => screen.IsLoaded);

            AddStep("set user mods", () => screen.UserMods.Value = [new OsuModDoubleTime()]);
            AddUntilStep("user mods selected", () => SelectedMods.Value.OfType<OsuModDoubleTime>().Any());

            AddStep("select second item", () => screen.SelectedItem.Value = room.Playlist[1]);
            AddUntilStep("user mod style reset", () => !screen.UserMods.Value.Any());
            AddUntilStep("mods reset", () => !SelectedMods.Value.Any());
        }

        /// <summary>
        /// Tests that mod style is preserved when the selected item is changed to another with the same ruleset.
        /// No user style is assumed.
        /// </summary>
        [Test]
        public void TestModsPreserved_OnSelection_SameRuleset_NoUserStyle()
        {
            Room room = null!;

            AddStep("add room", () =>
            {
                room = new Room
                {
                    RoomID = 1,
                    Playlist =
                    [
                        new PlaylistItem(importedSet.Beatmaps[0])
                        {
                            RulesetID = new OsuRuleset().RulesetInfo.OnlineID,
                            Freestyle = true
                        },
                        new PlaylistItem(importedSet.Beatmaps[0])
                        {
                            RulesetID = new OsuRuleset().RulesetInfo.OnlineID,
                            Freestyle = true
                        },
                    ]
                };

                API.Perform(new CreateRoomRequest(room));
            });

            TestPlaylistsRoomSubScreen screen = null!;
            AddStep("load screen", () => LoadScreen(new TestPlaylistsScreen(screen = new TestPlaylistsRoomSubScreen(room))));
            AddUntilStep("wait for load", () => screen.IsLoaded);

            AddStep("set user mods", () => screen.UserMods.Value = [new OsuModDoubleTime()]);
            AddUntilStep("user mods selected", () => SelectedMods.Value.OfType<OsuModDoubleTime>().Any());

            AddStep("select second item", () => screen.SelectedItem.Value = room.Playlist[1]);
            AddUntilStep("user mod style preserved", () => screen.UserMods.Value.OfType<OsuModDoubleTime>().Any());
            AddUntilStep("mods preserved", () => SelectedMods.Value.OfType<OsuModDoubleTime>().Any());
        }

        /// <summary>
        /// Tests that mod style is reset when the selected item is changed to another with an inconvertible ruleset.
        /// A user beatmap/ruleset style is assumed.
        /// </summary>
        [Test]
        public void TestModsReset_OnSelection_DifferentRuleset_WithUserStyle()
        {
            Room room = null!;

            AddStep("add room", () =>
            {
                room = new Room
                {
                    RoomID = 1,
                    Playlist =
                    [
                        new PlaylistItem(importedSet.Beatmaps[0])
                        {
                            RulesetID = new OsuRuleset().RulesetInfo.OnlineID,
                            Freestyle = true
                        },
                        new PlaylistItem(importedSet.Beatmaps[0])
                        {
                            RulesetID = new TaikoRuleset().RulesetInfo.OnlineID,
                            Freestyle = true
                        },
                    ]
                };

                API.Perform(new CreateRoomRequest(room));
            });

            TestPlaylistsRoomSubScreen screen = null!;
            AddStep("load screen", () => LoadScreen(new TestPlaylistsScreen(screen = new TestPlaylistsRoomSubScreen(room))));
            AddUntilStep("wait for load", () => screen.IsLoaded);

            AddStep("set user ruleset", () => screen.UserRuleset.Value = new CatchRuleset().RulesetInfo);
            AddUntilStep("user ruleset selected", () => Ruleset.Value.Equals(new CatchRuleset().RulesetInfo));
            AddStep("set user mods", () => screen.UserMods.Value = [new CatchModDoubleTime()]);
            AddUntilStep("user mods selected", () => SelectedMods.Value.OfType<CatchModDoubleTime>().Any());

            AddStep("select second item", () => screen.SelectedItem.Value = room.Playlist[1]);
            AddUntilStep("user mod style reset", () => !screen.UserMods.Value.Any());
            AddUntilStep("mods reset", () => !SelectedMods.Value.Any());
        }

        /// <summary>
        /// Tests that mod style is preserved when the selected item is changed to another with the same ruleset.
        /// A user beatmap/ruleset style is assumed.
        /// </summary>
        [Test]
        public void TestModsPreserved_OnSelection_SameRuleset_WithStyle()
        {
            Room room = null!;

            AddStep("add room", () =>
            {
                room = new Room
                {
                    RoomID = 1,
                    Playlist =
                    [
                        new PlaylistItem(importedSet.Beatmaps[0])
                        {
                            RulesetID = new OsuRuleset().RulesetInfo.OnlineID,
                            Freestyle = true
                        },
                        new PlaylistItem(importedSet.Beatmaps[0])
                        {
                            RulesetID = new TaikoRuleset().RulesetInfo.OnlineID,
                            Freestyle = true
                        },
                    ]
                };

                API.Perform(new CreateRoomRequest(room));
            });

            TestPlaylistsRoomSubScreen screen = null!;
            AddStep("load screen", () => LoadScreen(new TestPlaylistsScreen(screen = new TestPlaylistsRoomSubScreen(room))));
            AddUntilStep("wait for load", () => screen.IsLoaded);

            AddStep("set user ruleset", () => screen.UserRuleset.Value = new TaikoRuleset().RulesetInfo);
            AddUntilStep("user ruleset selected", () => Ruleset.Value.Equals(new TaikoRuleset().RulesetInfo));
            AddStep("set user mods", () => screen.UserMods.Value = [new TaikoModDoubleTime()]);
            AddUntilStep("user mods selected", () => SelectedMods.Value.OfType<TaikoModDoubleTime>().Any());

            AddStep("select second item", () => screen.SelectedItem.Value = room.Playlist[1]);
            AddUntilStep("user mod style preserved", () => screen.UserMods.Value.OfType<TaikoModDoubleTime>().Any());
            AddUntilStep("mods preserved", () => SelectedMods.Value.OfType<TaikoModDoubleTime>().Any());
        }

        /// <summary>
        /// Tests that the mod style is revalidated when the ruleset style is changed.
        /// </summary>
        [Test]
        public void TestModsValidated_OnRulesetStyleChanged()
        {
            Room room = null!;

            AddStep("add room", () =>
            {
                room = new Room
                {
                    RoomID = 1,
                    Playlist =
                    [
                        new PlaylistItem(importedSet.Beatmaps[0])
                        {
                            RulesetID = new OsuRuleset().RulesetInfo.OnlineID,
                            Freestyle = true
                        },
                    ]
                };

                API.Perform(new CreateRoomRequest(room));
            });

            TestPlaylistsRoomSubScreen screen = null!;
            AddStep("load screen", () => LoadScreen(new TestPlaylistsScreen(screen = new TestPlaylistsRoomSubScreen(room))));
            AddUntilStep("wait for load", () => screen.IsLoaded);

            AddStep("set user mods", () => screen.UserMods.Value = [new OsuModDoubleTime()]);
            AddUntilStep("user mods selected", () => SelectedMods.Value.OfType<OsuModDoubleTime>().Any());

            AddStep("set user ruleset", () => screen.UserRuleset.Value = new TaikoRuleset().RulesetInfo);
            AddUntilStep("user ruleset selected", () => Ruleset.Value.Equals(new TaikoRuleset().RulesetInfo));
            AddUntilStep("user mods reset", () => !screen.UserMods.Value.Any());
            AddUntilStep("mods reset", () => !SelectedMods.Value.Any());
        }

        /// <summary>
        /// Tests that the beatmap and ruleset style are reset when the selected item is changed to one without freestyle,
        /// and that the mod selection is re-validated against the item's allowed mods.
        /// </summary>
        [Test]
        public void TestUserStyle_Reset_OnFreestyleDisabled()
        {
            Room room = null!;

            AddStep("add room", () =>
            {
                room = new Room
                {
                    RoomID = 1,
                    Playlist =
                    [
                        new PlaylistItem(importedSet.Beatmaps[0])
                        {
                            RulesetID = new OsuRuleset().RulesetInfo.OnlineID,
                            Freestyle = true
                        },
                        new PlaylistItem(importedSet.Beatmaps[0])
                        {
                            RulesetID = new OsuRuleset().RulesetInfo.OnlineID,
                            AllowedMods = [new APIMod(new OsuModDoubleTime())]
                        },
                    ]
                };

                API.Perform(new CreateRoomRequest(room));
            });

            TestPlaylistsRoomSubScreen screen = null!;
            AddStep("load screen", () => LoadScreen(new TestPlaylistsScreen(screen = new TestPlaylistsRoomSubScreen(room))));
            AddUntilStep("wait for load", () => screen.IsLoaded);

            // Set beatmap + ruleset, reset by selecting second playlist item
            AddStep("set user beatmap/ruleset style", () =>
            {
                screen.UserBeatmap.Value = importedSet.Beatmaps[1];
                screen.UserRuleset.Value = new TaikoRuleset().RulesetInfo;
            });
            AddUntilStep("beatmap/ruleset set", () => Beatmap.Value.BeatmapInfo.Equals(importedSet.Beatmaps[1]) && Ruleset.Value.Equals(new TaikoRuleset().RulesetInfo));
            AddStep("select second playlist item", () => screen.SelectedItem.Value = room.Playlist[1]);
            AddUntilStep("user style reset", () => screen.UserBeatmap.Value == null && screen.UserRuleset.Value == null);
            AddUntilStep("beatmap/ruleset set", () => Beatmap.Value.BeatmapInfo.Equals(importedSet.Beatmaps[0]) && Ruleset.Value.Equals(new OsuRuleset().RulesetInfo));

            AddStep("select first playlist item", () => screen.SelectedItem.Value = room.Playlist[0]);

            // Set mods (DT+HR), validate by selecting second playlist item where only DT is allowed.
            AddStep("set user mods style", () => screen.UserMods.Value = [new OsuModDoubleTime(), new OsuModHardRock()]);
            AddUntilStep("mods set", () => SelectedMods.Value.OfType<OsuModDoubleTime>().Any() && SelectedMods.Value.OfType<OsuModHardRock>().Any());
            AddStep("select second playlist item", () => screen.SelectedItem.Value = room.Playlist[1]);
            AddUntilStep("user mods validated", () => screen.UserMods.Value.Count == 1 && screen.UserMods.Value.OfType<OsuModDoubleTime>().Any());
            AddUntilStep("mods set", () => SelectedMods.Value.Count == 1 && SelectedMods.Value.OfType<OsuModDoubleTime>().Any());
        }

        private partial class TestPlaylistsScreen : OsuScreen
        {
            public TestPlaylistsScreen(PlaylistsRoomSubScreen screen)
            {
                OnlinePlaySubScreenStack stack;

                InternalChildren = new Drawable[]
                {
                    stack = new OnlinePlaySubScreenStack
                    {
                        RelativeSizeAxes = Axes.Both
                    },
                    new BackButton
                    {
                        Anchor = Anchor.BottomLeft,
                        Origin = Anchor.BottomLeft,
                        State = { Value = Visibility.Visible },
                        Action = () =>
                        {
                            if (stack.CurrentScreen is not PlaylistsRoomSubScreen)
                                stack.Exit();
                        }
                    }
                };

                stack.Push(screen);
            }
        }

        private partial class TestPlaylistsRoomSubScreen : PlaylistsRoomSubScreen
        {
            public new Bindable<PlaylistItem?> SelectedItem => base.SelectedItem;
            public new Bindable<BeatmapInfo?> UserBeatmap => base.UserBeatmap;
            public new Bindable<RulesetInfo?> UserRuleset => base.UserRuleset;
            public new Bindable<IReadOnlyList<Mod>> UserMods => base.UserMods;

            public TestPlaylistsRoomSubScreen(Room room)
                : base(room)
            {
            }
        }
    }
}
