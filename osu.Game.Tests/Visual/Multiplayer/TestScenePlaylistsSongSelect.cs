// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Platform;
using osu.Framework.Screens;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Screens.OnlinePlay.Components;
using osu.Game.Screens.OnlinePlay.Playlists;
using osu.Game.Tests.Resources;
using osu.Game.Tests.Visual.OnlinePlay;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public partial class TestScenePlaylistsSongSelect : OnlinePlayTestScene
    {
        private BeatmapManager manager = null!;
        private TestPlaylistsSongSelect songSelect = null!;
        private Room room = null!;

        [BackgroundDependencyLoader]
        private void load(GameHost host, AudioManager audio)
        {
            BeatmapStore beatmapStore;

            Dependencies.Cache(new RealmRulesetStore(Realm));
            Dependencies.Cache(manager = new BeatmapManager(LocalStorage, Realm, null, audio, Resources, host, Beatmap.Default));
            Dependencies.CacheAs(beatmapStore = new RealmDetachedBeatmapStore());
            Dependencies.Cache(Realm);

            var beatmapSet = TestResources.CreateTestBeatmapSetInfo();

            manager.Import(beatmapSet);

            Add(beatmapStore);
        }

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("reset", () =>
            {
                room = new Room();
                Ruleset.Value = new OsuRuleset().RulesetInfo;
                Beatmap.SetDefault();
                SelectedMods.Value = Array.Empty<Mod>();
            });

            AddStep("create song select", () => LoadScreen(songSelect = new TestPlaylistsSongSelect(room)));
            AddUntilStep("wait for present", () => songSelect.IsCurrentScreen() && songSelect.BeatmapSetsLoaded);
        }

        [Test]
        public void TestItemAddedIfEmptyOnStart()
        {
            AddStep("finalise selection", () => songSelect.FinaliseSelection());
            AddAssert("playlist has 1 item", () => room.Playlist.Count == 1);
        }

        [Test]
        public void TestItemAddedWhenCreateNewItemClicked()
        {
            AddStep("create new item", () => songSelect.BeatmapDetails.CreateNewItem!());
            AddAssert("playlist has 1 item", () => room.Playlist.Count == 1);
        }

        [Test]
        public void TestItemNotAddedIfExistingOnStart()
        {
            AddStep("create new item", () => songSelect.BeatmapDetails.CreateNewItem!());
            AddStep("finalise selection", () => songSelect.FinaliseSelection());
            AddAssert("playlist has 1 item", () => room.Playlist.Count == 1);
        }

        [Test]
        public void TestAddSameItemMultipleTimes()
        {
            AddStep("create new item", () => songSelect.BeatmapDetails.CreateNewItem!());
            AddStep("create new item", () => songSelect.BeatmapDetails.CreateNewItem!());
            AddAssert("playlist has 2 items", () => room.Playlist.Count == 2);
        }

        [Test]
        public void TestAddItemAfterRearrangement()
        {
            AddStep("create new item", () => songSelect.BeatmapDetails.CreateNewItem!());
            AddStep("create new item", () => songSelect.BeatmapDetails.CreateNewItem!());
            AddStep("rearrange", () => room.Playlist = room.Playlist.Skip(1).Append(room.Playlist[0]).ToArray());

            AddStep("create new item", () => songSelect.BeatmapDetails.CreateNewItem!());
            AddAssert("new item has id 2", () => room.Playlist.Last().ID == 2);
        }

        /// <summary>
        /// Tests that the same <see cref="Mod"/> instances are not shared between two playlist items.
        /// </summary>
        [Test]
        public void TestNewItemHasNewModInstances()
        {
            AddStep("set dt mod", () => SelectedMods.Value = new[] { new OsuModDoubleTime() });
            AddStep("create item", () => songSelect.BeatmapDetails.CreateNewItem!());
            AddStep("change mod rate", () => ((OsuModDoubleTime)SelectedMods.Value[0]).SpeedChange.Value = 2);
            AddStep("create item", () => songSelect.BeatmapDetails.CreateNewItem!());

            AddAssert("item 1 has rate 1.5", () =>
            {
                var mod = (OsuModDoubleTime)room.Playlist.First().RequiredMods[0].ToMod(new OsuRuleset());
                return Precision.AlmostEquals(1.5, mod.SpeedChange.Value);
            });

            AddAssert("item 2 has rate 2", () =>
            {
                var mod = (OsuModDoubleTime)room.Playlist.Last().RequiredMods[0].ToMod(new OsuRuleset());
                return Precision.AlmostEquals(2, mod.SpeedChange.Value);
            });
        }

        /// <summary>
        /// Tests that the global mod instances are not retained by the rooms, as global mod instances are retained and re-used by the mod select overlay.
        /// </summary>
        [Test]
        public void TestGlobalModInstancesNotRetained()
        {
            OsuModDoubleTime mod = null!;

            AddStep("set dt mod and store", () =>
            {
                SelectedMods.Value = new[] { new OsuModDoubleTime() };

                // Mod select overlay replaces our mod.
                mod = (OsuModDoubleTime)SelectedMods.Value[0];
            });

            AddStep("create item", () => songSelect.BeatmapDetails.CreateNewItem!());

            AddStep("change stored mod rate", () => mod.SpeedChange.Value = 2);
            AddAssert("item has rate 1.5", () =>
            {
                var m = (OsuModDoubleTime)room.Playlist.First().RequiredMods[0].ToMod(new OsuRuleset());
                return Precision.AlmostEquals(1.5, m.SpeedChange.Value);
            });
        }

        private partial class TestPlaylistsSongSelect : PlaylistsSongSelect
        {
            public new MatchBeatmapDetailArea BeatmapDetails => (MatchBeatmapDetailArea)base.BeatmapDetails;

            public TestPlaylistsSongSelect(Room room)
                : base(room)
            {
            }
        }
    }
}
