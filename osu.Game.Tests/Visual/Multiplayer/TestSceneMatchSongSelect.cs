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
using osu.Framework.Extensions;
using osu.Framework.Platform;
using osu.Framework.Screens;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Online.Multiplayer;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.Multi.Components;
using osu.Game.Screens.Select;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneMatchSongSelect : MultiplayerTestScene
    {
        [Resolved]
        private BeatmapManager beatmapManager { get; set; }

        private BeatmapManager manager;

        private RulesetStore rulesets;

        private TestMatchSongSelect songSelect;

        [BackgroundDependencyLoader]
        private void load(GameHost host, AudioManager audio)
        {
            Dependencies.Cache(rulesets = new RulesetStore(ContextFactory));
            Dependencies.Cache(manager = new BeatmapManager(LocalStorage, ContextFactory, rulesets, null, audio, host, Beatmap.Default));

            var beatmaps = new List<BeatmapInfo>();

            for (int i = 0; i < 6; i++)
            {
                int beatmapId = 10 * 10 + i;

                int length = RNG.Next(30000, 200000);
                double bpm = RNG.NextSingle(80, 200);

                beatmaps.Add(new BeatmapInfo
                {
                    Ruleset = new OsuRuleset().RulesetInfo,
                    OnlineBeatmapID = beatmapId,
                    Path = "normal.osu",
                    Version = $"{beatmapId} (length {TimeSpan.FromMilliseconds(length):m\\:ss}, bpm {bpm:0.#})",
                    Length = length,
                    BPM = bpm,
                    BaseDifficulty = new BeatmapDifficulty
                    {
                        OverallDifficulty = 3.5f,
                    },
                });
            }

            manager.Import(new BeatmapSetInfo
            {
                OnlineBeatmapSetID = 10,
                Hash = new MemoryStream(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString())).ComputeMD5Hash(),
                Metadata = new BeatmapMetadata
                {
                    // Create random metadata, then we can check if sorting works based on these
                    Artist = "Some Artist " + RNG.Next(0, 9),
                    Title = $"Some Song (set id 10), max bpm {beatmaps.Max(b => b.BPM):0.#})",
                    AuthorString = "Some Guy " + RNG.Next(0, 9),
                },
                Beatmaps = beatmaps,
                DateAdded = DateTimeOffset.UtcNow,
            }).Wait();
        }

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("reset", () =>
            {
                Ruleset.Value = new OsuRuleset().RulesetInfo;
                Beatmap.SetDefault();
            });

            AddStep("create song select", () => LoadScreen(songSelect = new TestMatchSongSelect()));
            AddUntilStep("wait for present", () => songSelect.IsCurrentScreen());
        }

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            Room = new Room();
        });

        [Test]
        public void TestItemAddedIfEmptyOnStart()
        {
            AddStep("finalise selection", () => songSelect.FinaliseSelection());
            AddAssert("playlist has 1 item", () => Room.Playlist.Count == 1);
        }

        [Test]
        public void TestItemAddedWhenCreateNewItemClicked()
        {
            AddStep("create new item", () => songSelect.BeatmapDetails.CreateNewItem());
            AddAssert("playlist has 1 item", () => Room.Playlist.Count == 1);
        }

        [Test]
        public void TestItemNotAddedIfExistingOnStart()
        {
            AddStep("create new item", () => songSelect.BeatmapDetails.CreateNewItem());
            AddStep("finalise selection", () => songSelect.FinaliseSelection());
            AddAssert("playlist has 1 item", () => Room.Playlist.Count == 1);
        }

        [Test]
        public void TestAddSameItemMultipleTimes()
        {
            AddStep("create new item", () => songSelect.BeatmapDetails.CreateNewItem());
            AddStep("create new item", () => songSelect.BeatmapDetails.CreateNewItem());
            AddAssert("playlist has 2 items", () => Room.Playlist.Count == 2);
        }

        [Test]
        public void TestAddItemAfterRearrangement()
        {
            AddStep("create new item", () => songSelect.BeatmapDetails.CreateNewItem());
            AddStep("create new item", () => songSelect.BeatmapDetails.CreateNewItem());
            AddStep("rearrange", () =>
            {
                var item = Room.Playlist[0];
                Room.Playlist.RemoveAt(0);
                Room.Playlist.Add(item);
            });

            AddStep("create new item", () => songSelect.BeatmapDetails.CreateNewItem());
            AddAssert("new item has id 2", () => Room.Playlist.Last().ID == 2);
        }

        private class TestMatchSongSelect : MatchSongSelect
        {
            public new MatchBeatmapDetailArea BeatmapDetails => (MatchBeatmapDetailArea)base.BeatmapDetails;
        }
    }
}
