// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Models;
using osu.Game.Tournament.Screens.MapPool;

namespace osu.Game.Tournament.Tests.Screens
{
    public class TestSceneMapPoolScreen : TournamentTestScene
    {
        private MapPoolScreen screen;

        [BackgroundDependencyLoader]
        private void load()
        {
            Add(screen = new MapPoolScreen { Width = 0.7f });
        }

        [Test]
        public void TestFewMaps()
        {
            AddStep("load few maps", () =>
            {
                Ladder.CurrentMatch.Value.Round.Value.Beatmaps.Clear();

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
                Ladder.CurrentMatch.Value.Round.Value.Beatmaps.Clear();

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
                Ladder.CurrentMatch.Value.Round.Value.Beatmaps.Clear();

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
                Ladder.CurrentMatch.Value.Round.Value.Beatmaps.Clear();

                for (int i = 0; i < 11; i++)
                    addBeatmap(i > 4 ? $"M{i}" : "NM");
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
                Ladder.CurrentMatch.Value.Round.Value.Beatmaps.Clear();

                for (int i = 0; i < 12; i++)
                    addBeatmap(i > 4 ? $"M{i}" : "NM");
            });

            AddStep("reset match", () =>
            {
                Ladder.CurrentMatch.Value = new TournamentMatch();
                Ladder.CurrentMatch.Value = Ladder.Matches.First();
            });

            assertThreeWide();
        }

        private void addBeatmap(string mods = "nm")
        {
            Ladder.CurrentMatch.Value.Round.Value.Beatmaps.Add(new RoundBeatmap
            {
                Beatmap = CreateSampleBeatmap(),
                Mods = mods
            });
        }
    }
}
