// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Tests.Visual;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Models;

namespace osu.Game.Tournament.Tests.Components
{
    [TestFixture]
    public partial class TestSceneSongBar : OsuTestScene
    {
        [Cached]
        private readonly LadderInfo ladder = new LadderInfo();

        private SongBar songBar = null!;

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("create bar", () => Child = songBar = new SongBar
            {
                RelativeSizeAxes = Axes.X,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre
            });
            AddUntilStep("wait for loaded", () => songBar.IsLoaded);
        }

        [Test]
        public void TestSongBar()
        {
            AddStep("set beatmap", () =>
            {
                var beatmap = CreateAPIBeatmap(Ruleset.Value);
                beatmap.CircleSize = 3.4f;
                beatmap.ApproachRate = 6.8f;
                beatmap.OverallDifficulty = 5.5f;
                beatmap.StarRating = 4.56f;
                beatmap.Length = 123456;
                beatmap.BPM = 133;

                songBar.Beatmap = new TournamentBeatmap(beatmap);
            });
            AddStep("set mods to HR", () => songBar.Mods = LegacyMods.HardRock);
            AddStep("set mods to DT", () => songBar.Mods = LegacyMods.DoubleTime);
            AddStep("unset mods", () => songBar.Mods = LegacyMods.None);
        }
    }
}
