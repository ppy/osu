// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Models;

namespace osu.Game.Tournament.Tests.Components
{
    [TestFixture]
    public partial class TestSceneSongBar : TournamentTestScene
    {
        private SongBar songBar = null!;
        private TournamentBeatmap ladderBeatmap = null!;

        [SetUpSteps]
        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("setup picks bans", () =>
            {
                ladderBeatmap = CreateSampleBeatmap();
                Ladder.CurrentMatch.Value!.PicksBans.Add(new BeatmapChoice
                {
                    BeatmapID = ladderBeatmap.OnlineID,
                    Team = TeamColour.Red,
                    Type = ChoiceType.Pick,
                });
            });

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
                beatmap.OnlineID = ladderBeatmap.OnlineID;

                songBar.Beatmap = new TournamentBeatmap(beatmap);
            });

            AddStep("set mods to HR", () => songBar.Mods = LegacyMods.HardRock);
            AddStep("set mods to DT", () => songBar.Mods = LegacyMods.DoubleTime);
            AddStep("unset mods", () => songBar.Mods = LegacyMods.None);

            AddToggleStep("toggle expanded", expanded => songBar.Expanded = expanded);

            AddStep("set null beatmap", () => songBar.Beatmap = null);
        }
    }
}
