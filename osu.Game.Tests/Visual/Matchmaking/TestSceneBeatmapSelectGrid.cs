// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.OnlinePlay.Matchmaking.Match.BeatmapSelect;
using osu.Game.Tests.Visual.OnlinePlay;
using osuTK;

namespace osu.Game.Tests.Visual.Matchmaking
{
    public partial class TestSceneBeatmapSelectGrid : OnlinePlayTestScene
    {
        private MatchmakingPlaylistItem[] items = null!;

        private BeatmapSelectGrid grid = null!;

        [Resolved]
        private BeatmapManager beatmapManager { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            var beatmaps = beatmapManager.GetAllUsableBeatmapSets()
                                         .SelectMany(it => it.Beatmaps)
                                         .Take(50)
                                         .ToArray();

            IEnumerable<MatchmakingPlaylistItem> playlistItems;

            if (beatmaps.Length > 0)
            {
                playlistItems = Enumerable.Range(1, 50).Select(i =>
                {
                    var beatmap = beatmaps[i % beatmaps.Length];

                    return new MatchmakingPlaylistItem(
                        new MultiplayerPlaylistItem
                        {
                            ID = i,
                            BeatmapID = beatmap.OnlineID,
                            StarRating = i / 10.0,
                        },
                        CreateAPIBeatmap(beatmap),
                        Array.Empty<Mod>()
                    );
                });
            }
            else
            {
                playlistItems = Enumerable.Range(1, 50).Select(i => new MatchmakingPlaylistItem(
                    new MultiplayerPlaylistItem
                    {
                        ID = i,
                        BeatmapID = i,
                        StarRating = i / 10.0,
                    },
                    CreateAPIBeatmap(),
                    Array.Empty<Mod>()
                ));
            }

            foreach (var item in playlistItems)
                item.Beatmap.StarRating = item.PlaylistItem.StarRating;

            items = playlistItems.ToArray();
        }

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("add grid", () => Child = grid = new BeatmapSelectGrid
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Scale = new Vector2(0.8f),
            });

            AddStep("add items", () =>
            {
                grid.AddItems(items);
            });

            AddWaitStep("wait for panels", 3);
        }

        [Test]
        public void TestBasic()
        {
            AddStep("do nothing", () =>
            {
                // test scene is weird.
            });

            AddStep("add selection 1", () => grid.ChildrenOfType<MatchmakingSelectPanel>().First().AddUser(new APIUser
            {
                Id = DummyAPIAccess.DUMMY_USER_ID,
                Username = "Maarvin",
            }));
            AddStep("add selection 2", () => grid.ChildrenOfType<MatchmakingSelectPanel>().Skip(5).First().AddUser(new APIUser
            {
                Id = 2,
                Username = "peppy",
            }));
            AddStep("add selection 3", () => grid.ChildrenOfType<MatchmakingSelectPanel>().Skip(10).First().AddUser(new APIUser
            {
                Id = 1040328,
                Username = "smoogipoo",
            }));
        }

        [Test]
        public void TestCompleteRollAnimation()
        {
            AddStep("play animation", () =>
            {
                var (candidateItems, finalItem) = pickRandomItems(5);

                grid.RollAndDisplayFinalBeatmap(candidateItems, finalItem);
            });
        }

        [Test]
        public void TestRollAnimation()
        {
            AddStep("play animation", () =>
            {
                var (candidateItems, finalItem) = pickRandomItems(5);

                grid.TransferCandidatePanelsToRollContainer(candidateItems, duration: 0);
                grid.ArrangeItemsForRollAnimation(duration: 0, stagger: 0);

                Scheduler.AddDelayed(() => grid.PlayRollAnimation(finalItem), 500);
            });
        }

        [Test]
        public void TestPresentRolledBeatmap()
        {
            AddStep("present beatmap", () =>
            {
                var (candidateItems, finalItem) = pickRandomItems(5);

                grid.TransferCandidatePanelsToRollContainer(candidateItems, duration: 0);
                grid.ArrangeItemsForRollAnimation(duration: 0, stagger: 0);
                grid.PlayRollAnimation(finalItem, duration: 0);

                Scheduler.AddDelayed(() => grid.PresentRolledBeatmap(finalItem), 500);
            });
        }

        [Test]
        public void TestPresentUnanimouslyChosenBeatmap()
        {
            AddStep("present beatmap", () =>
            {
                var (candidateItems, finalItem) = pickRandomItems(5);

                grid.TransferCandidatePanelsToRollContainer(candidateItems, duration: 0);
                grid.ArrangeItemsForRollAnimation(duration: 0, stagger: 0);
                grid.PlayRollAnimation(finalItem, duration: 0);

                Scheduler.AddDelayed(() => grid.PresentUnanimouslyChosenBeatmap(finalItem), 500);
            });
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(6)]
        [TestCase(7)]
        [TestCase(8)]
        public void TestPanelArrangement(int count)
        {
            AddStep("arrange panels", () =>
            {
                var (candidateItems, _) = pickRandomItems(count);

                grid.TransferCandidatePanelsToRollContainer(candidateItems);
                grid.Delay(BeatmapSelectGrid.ARRANGE_DELAY)
                    .Schedule(() => grid.ArrangeItemsForRollAnimation());
            });

            AddWaitStep("wait for movement", 5);

            AddStep("display roll order", () =>
            {
                var panels = grid.ChildrenOfType<MatchmakingSelectPanel>().ToArray();

                for (int i = 0; i < panels.Length; i++)
                {
                    var panel = panels[i];

                    panel.Add(new OsuSpriteText
                    {
                        Text = (i + 1).ToString(),
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Font = OsuFont.Default.With(size: 50, weight: FontWeight.SemiBold),
                    });
                }
            });
        }

        [Test]
        public void TestPresentRandomItem()
        {
            AddStep("present random item panel", () =>
            {
                grid.TransferCandidatePanelsToRollContainer(pickRandomItems(4).candidateItems.Append(-1).ToArray(), duration: 0);
                grid.ArrangeItemsForRollAnimation(duration: 0, stagger: 0);
                grid.PlayRollAnimation(-1, duration: 0);

                Scheduler.AddDelayed(() => grid.PresentUnanimouslyChosenBeatmap(-1), 500);
            });

            AddWaitStep("wait for animation", 5);

            AddStep("reveal beatmap", () => grid.RevealRandomItem(items[RNG.Next(items.Length)].PlaylistItem));
        }

        private (long[] candidateItems, long finalItem) pickRandomItems(int count)
        {
            long[] candidateItems = items.Select(it => it.ID).ToArray();
            Random.Shared.Shuffle(candidateItems);
            candidateItems = candidateItems.Take(count).ToArray();

            long finalItem = candidateItems[Random.Shared.Next(candidateItems.Length)];

            return (candidateItems, finalItem);
        }
    }
}
