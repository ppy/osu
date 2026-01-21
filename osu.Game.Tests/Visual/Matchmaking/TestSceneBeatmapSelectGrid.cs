// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Matchmaking.Match.BeatmapSelect;
using osu.Game.Tests.Visual.OnlinePlay;
using osuTK;

namespace osu.Game.Tests.Visual.Matchmaking
{
    public partial class TestSceneBeatmapSelectGrid : OnlinePlayTestScene
    {
        private MultiplayerPlaylistItem[] items = null!;

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

            if (beatmaps.Length > 0)
            {
                items = Enumerable.Range(1, 50).Select(i => new MultiplayerPlaylistItem
                {
                    ID = i,
                    BeatmapID = beatmaps[i % beatmaps.Length].OnlineID,
                    StarRating = i / 10.0,
                }).ToArray();
            }
            else
            {
                items = Enumerable.Range(1, 50).Select(i => new MultiplayerPlaylistItem
                {
                    ID = i,
                    BeatmapID = i,
                    StarRating = i / 10.0,
                }).ToArray();
            }
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
                foreach (var item in items)
                    grid.AddItem(item);
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

            AddStep("add selection 1", () => grid.ChildrenOfType<BeatmapSelectPanel>().First().AddUser(new APIUser
            {
                Id = DummyAPIAccess.DUMMY_USER_ID,
                Username = "Maarvin",
            }));
            AddStep("add selection 2", () => grid.ChildrenOfType<BeatmapSelectPanel>().Skip(5).First().AddUser(new APIUser
            {
                Id = 2,
                Username = "peppy",
            }));
            AddStep("add selection 3", () => grid.ChildrenOfType<BeatmapSelectPanel>().Skip(10).First().AddUser(new APIUser
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
                var panels = grid.ChildrenOfType<BeatmapSelectPanel>().ToArray();

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
