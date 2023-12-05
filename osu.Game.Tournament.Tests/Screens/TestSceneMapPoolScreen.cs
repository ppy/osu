// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Framework.Testing;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Models;
using osu.Game.Tournament.Screens.MapPool;

namespace osu.Game.Tournament.Tests.Screens
{
    public partial class TestSceneMapPoolScreen : TournamentScreenTestScene
    {
        private MapPoolScreen screen = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            Add(screen = new MapPoolScreen { Width = 0.7f });
        }

        [SetUp]
        public void SetUp() => Schedule(() => Ladder.SplitMapPoolByMods.Value = true);

        [Test]
        public void TestFewMaps()
        {
            AddStep("load few maps", () =>
            {
                Ladder.CurrentMatch.Value!.Round.Value!.Beatmaps.Clear();

                for (int i = 0; i < 8; i++)
                    addBeatmap();
            });

            AddStep("reset match", () =>
            {
                Ladder.CurrentMatch.Value = new TournamentMatch();
                Ladder.CurrentMatch.Value = Ladder.Matches.First();
            });

            assertTwoWide();
        }

        [Test]
        public void TestJustEnoughMaps()
        {
            AddStep("load just enough maps", () =>
            {
                Ladder.CurrentMatch.Value!.Round.Value!.Beatmaps.Clear();

                for (int i = 0; i < 18; i++)
                    addBeatmap();
            });

            AddStep("reset match", () =>
            {
                Ladder.CurrentMatch.Value = new TournamentMatch();
                Ladder.CurrentMatch.Value = Ladder.Matches.First();
            });

            assertTwoWide();
        }

        [Test]
        public void TestManyMaps()
        {
            AddStep("load many maps", () =>
            {
                Ladder.CurrentMatch.Value!.Round.Value!.Beatmaps.Clear();

                for (int i = 0; i < 19; i++)
                    addBeatmap();
            });

            AddStep("reset match", () =>
            {
                Ladder.CurrentMatch.Value = new TournamentMatch();
                Ladder.CurrentMatch.Value = Ladder.Matches.First();
            });

            assertThreeWide();
        }

        [Test]
        public void TestJustEnoughMods()
        {
            AddStep("load many maps", () =>
            {
                Ladder.CurrentMatch.Value!.Round.Value!.Beatmaps.Clear();

                for (int i = 0; i < 11; i++)
                    addBeatmap(i > 4 ? Ruleset.Value.CreateInstance().AllMods.ElementAt(i).Acronym : "NM");
            });

            AddStep("reset match", () =>
            {
                Ladder.CurrentMatch.Value = new TournamentMatch();
                Ladder.CurrentMatch.Value = Ladder.Matches.First();
            });

            assertTwoWide();
        }

        private void assertTwoWide() =>
            AddAssert("ensure layout width is 2", () => screen.ChildrenOfType<FillFlowContainer<FillFlowContainer<TournamentBeatmapPanel>>>().First().Padding.Left > 0);

        private void assertThreeWide() =>
            AddAssert("ensure layout width is 3", () => screen.ChildrenOfType<FillFlowContainer<FillFlowContainer<TournamentBeatmapPanel>>>().First().Padding.Left == 0);

        [Test]
        public void TestManyMods()
        {
            AddStep("load many maps", () =>
            {
                Ladder.CurrentMatch.Value!.Round.Value!.Beatmaps.Clear();

                for (int i = 0; i < 12; i++)
                    addBeatmap(i > 4 ? Ruleset.Value.CreateInstance().AllMods.ElementAt(i).Acronym : "NM");
            });

            AddStep("reset match", () =>
            {
                Ladder.CurrentMatch.Value = new TournamentMatch();
                Ladder.CurrentMatch.Value = Ladder.Matches.First();
            });

            assertThreeWide();
        }

        [Test]
        public void TestSplitMapPoolByMods()
        {
            AddStep("load many maps", () =>
            {
                Ladder.CurrentMatch.Value!.Round.Value!.Beatmaps.Clear();

                for (int i = 0; i < 12; i++)
                    addBeatmap(i > 4 ? Ruleset.Value.CreateInstance().AllMods.ElementAt(i).Acronym : "NM");
            });

            AddStep("disable splitting map pool by mods", () => Ladder.SplitMapPoolByMods.Value = false);

            AddStep("reset match", () =>
            {
                Ladder.CurrentMatch.Value = new TournamentMatch();
                Ladder.CurrentMatch.Value = Ladder.Matches.First();
            });
        }

        [Test]
        public void TestSingleTeamBan()
        {
            AddStep("set ban count", () => Ladder.CurrentMatch.Value!.Round.Value!.BanCount.Value = 1);

            AddStep("load some maps", () =>
            {
                Ladder.CurrentMatch.Value!.Round.Value!.Beatmaps.Clear();

                for (int i = 0; i < 4; i++)
                    addBeatmap();
            });

            AddStep("update displayed maps", () => Ladder.SplitMapPoolByMods.Value = false);

            AddStep("perform bans", () =>
            {
                var tournamentMaps = screen.ChildrenOfType<TournamentBeatmapPanel>().ToList();

                screen.ChildrenOfType<TourneyButton>().Where(btn => btn.Text == "Red Ban").First().TriggerClick();

                PerformMapAction(tournamentMaps[0]);
                PerformMapAction(tournamentMaps[1]);
            });

            AddAssert("ensure 1 ban per team", () => Ladder.CurrentMatch.Value!.PicksBans.Count() == 2 && Ladder.CurrentMatch.Value!.PicksBans.Last().Team == TeamColour.Blue);

            AddStep("reset match", () =>
            {
                InputManager.UseParentInput = true;
                Ladder.CurrentMatch.Value = new TournamentMatch();
                Ladder.CurrentMatch.Value = Ladder.Matches.First();
                Ladder.CurrentMatch.Value.PicksBans.Clear();
            });
        }

        [Test]
        public void TestMultipleTeamBans()
        {
            AddStep("set ban count", () => Ladder.CurrentMatch.Value!.Round.Value!.BanCount.Value = 3);

            AddStep("load some maps", () =>
            {
                Ladder.CurrentMatch.Value!.Round.Value!.Beatmaps.Clear();

                for (int i = 0; i < 12; i++)
                    addBeatmap();
            });

            AddStep("update displayed maps", () => Ladder.SplitMapPoolByMods.Value = false);

            AddStep("red team ban", () =>
            {
                var tournamentMaps = screen.ChildrenOfType<TournamentBeatmapPanel>().ToList();

                screen.ChildrenOfType<TourneyButton>().Where(btn => btn.Text == "Red Ban").First().TriggerClick();

                PerformMapAction(tournamentMaps[0]);
            });

            AddAssert("ensure red team ban", () => Ladder.CurrentMatch.Value!.PicksBans.Last().Team == TeamColour.Red);

            AddStep("blue team bans", () =>
            {
                var tournamentMaps = screen.ChildrenOfType<TournamentBeatmapPanel>().ToList();

                PerformMapAction(tournamentMaps[1]);
                PerformMapAction(tournamentMaps[2]);
            });

            AddAssert("ensure blue team double ban", () => Ladder.CurrentMatch.Value!.PicksBans.Count(ban => ban.Team == TeamColour.Blue) == 2);

            AddStep("red team bans", () =>
            {
                var tournamentMaps = screen.ChildrenOfType<TournamentBeatmapPanel>().ToList();

                PerformMapAction(tournamentMaps[3]);
                PerformMapAction(tournamentMaps[4]);
            });

            AddAssert("ensure red team double ban", () => Ladder.CurrentMatch.Value!.PicksBans.Count(ban => ban.Team == TeamColour.Red) == 3);

            AddStep("blue team bans", () =>
            {
                var tournamentMaps = screen.ChildrenOfType<TournamentBeatmapPanel>().ToList();

                PerformMapAction(tournamentMaps[5]);
            });

            AddAssert("ensure blue team ban", () => Ladder.CurrentMatch.Value!.PicksBans.Last().Team == TeamColour.Blue);

            AddStep("reset match", () =>
            {
                InputManager.UseParentInput = true;
                Ladder.CurrentMatch.Value = new TournamentMatch();
                Ladder.CurrentMatch.Value = Ladder.Matches.First();
                Ladder.CurrentMatch.Value.PicksBans.Clear();
            });
        }
        private void addBeatmap(string mods = "NM")
        {
            Ladder.CurrentMatch.Value!.Round.Value!.Beatmaps.Add(new RoundBeatmap
            {
                Beatmap = CreateSampleBeatmap(),
                Mods = mods
            });
        }

        private void PerformMapAction(TournamentBeatmapPanel map)
        {
            InputManager.MoveMouseTo(map);

            InputManager.Click(osuTK.Input.MouseButton.Left);
        }
    }
}
