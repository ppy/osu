// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Extensions;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.SelectV2.Wedge;

namespace osu.Game.Tests.Visual.SongSelectV2
{
    public partial class TestSceneLengthAndBPMStatisticPill : SongSelectComponentsTestScene
    {
        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [Test]
        public void TestValueColour()
        {
            AddStep("set pill", () => Child = new LocalLengthAndBPMStatisticPill
            {
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
            });

            AddStep("set beatmap", () =>
            {
                List<HitObject> objects = new List<HitObject>();
                for (double i = 0; i < 50000; i += 1000)
                    objects.Add(new HitCircle { StartTime = i });

                Beatmap.Value = CreateWorkingBeatmap(new Beatmap
                {
                    BeatmapInfo = new BeatmapInfo
                    {
                        Length = 83000,
                        OnlineID = 1,
                    },
                    HitObjects = objects
                });
            });

            AddStep("set double time", () => SelectedMods.Value = new[] { new OsuModDoubleTime() });

            AddAssert("length value is red", () => this.ChildrenOfType<LengthAndBPMStatisticPill.PillStatistic>().ElementAt(0).ValueColour,
                () => Is.EqualTo(colours.ForModType(ModType.DifficultyIncrease)));
            AddAssert("bpm value is red", () => this.ChildrenOfType<LengthAndBPMStatisticPill.PillStatistic>().ElementAt(1).ValueColour,
                () => Is.EqualTo(colours.ForModType(ModType.DifficultyIncrease)));

            AddStep("set half time", () => SelectedMods.Value = new[] { new OsuModHalfTime() });

            AddAssert("length value is green", () => this.ChildrenOfType<LengthAndBPMStatisticPill.PillStatistic>().ElementAt(0).ValueColour,
                () => Is.EqualTo(colours.ForModType(ModType.DifficultyReduction)));
            AddAssert("bpm value is green", () => this.ChildrenOfType<LengthAndBPMStatisticPill.PillStatistic>().ElementAt(1).ValueColour,
                () => Is.EqualTo(colours.ForModType(ModType.DifficultyReduction)));
        }

        [Test]
        public void TestLengthUpdates()
        {
            OsuModDoubleTime? doubleTime = null;

            List<HitObject> objects = new List<HitObject>();
            for (double i = 0; i < 50000; i += 1000)
                objects.Add(new HitCircle { StartTime = i });

            Beatmap beatmap = new Beatmap
            {
                HitObjects = objects,
            };

            double drain = beatmap.CalculateDrainLength();
            beatmap.BeatmapInfo.Length = drain;

            AddStep("set pill", () => Child = new LocalLengthAndBPMStatisticPill
            {
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
            });

            AddStep("set beatmap", () =>
            {
                Beatmap.Value = CreateWorkingBeatmap(beatmap);
            });

            checkDisplayedLength(drain);

            AddStep("select DT", () => SelectedMods.Value = new[] { doubleTime = new OsuModDoubleTime() });
            checkDisplayedLength(Math.Round(drain / 1.5f));

            AddStep("change DT rate", () => doubleTime!.SpeedChange.Value = 2);
            checkDisplayedLength(Math.Round(drain / 2));
        }

        private void checkDisplayedLength(double drain)
        {
            var displayedLength = drain.ToFormattedDuration();

            AddAssert($"check map drain ({displayedLength})", () => this.ChildrenOfType<LengthAndBPMStatisticPill.PillStatistic>().ElementAt(0).Value, () => Is.EqualTo(displayedLength));
        }

        [Test]
        public void TestBPMUpdates()
        {
            const double bpm = 120;
            OsuModDoubleTime? doubleTime = null;

            AddStep("set pill", () => Child = new LocalLengthAndBPMStatisticPill
            {
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
            });

            AddStep("set beatmap", () =>
            {
                Beatmap beatmap = new Beatmap();
                beatmap.ControlPointInfo.Add(0, new TimingControlPoint { BeatLength = 60 * 1000 / bpm });

                Beatmap.Value = CreateWorkingBeatmap(beatmap);
            });

            checkDisplayedBPM($"{bpm}");

            AddStep("select DT", () => SelectedMods.Value = new[] { doubleTime = new OsuModDoubleTime() });
            checkDisplayedBPM($"{bpm * 1.5f}");

            AddStep("change DT rate", () => doubleTime!.SpeedChange.Value = 2);
            checkDisplayedBPM($"{bpm * 2}");
        }

        [TestCase(120, 125, null, "120-125 (120)")]
        [TestCase(120, 120.6, null, "120-121 (120)")]
        [TestCase(120, 120.4, null, "120")]
        [TestCase(120, 120.6, "DT", "180-182 (180)")]
        [TestCase(120, 120.4, "DT", "180")]
        public void TestVaryingBPM(double commonBpm, double otherBpm, string? mod, string expectedDisplay)
        {
            AddStep("set pill", () => Child = new LocalLengthAndBPMStatisticPill
            {
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
            });

            if (mod != null)
                AddStep($"select {mod}", () => SelectedMods.Value = new[] { Ruleset.Value.CreateInstance().CreateModFromAcronym(mod) });

            AddStep("set beatmap", () =>
            {
                Beatmap beatmap = new Beatmap();
                beatmap.ControlPointInfo.Add(0, new TimingControlPoint { BeatLength = 60 * 1000 / commonBpm });
                beatmap.ControlPointInfo.Add(100, new TimingControlPoint { BeatLength = 60 * 1000 / otherBpm });
                beatmap.ControlPointInfo.Add(200, new TimingControlPoint { BeatLength = 60 * 1000 / commonBpm });

                Beatmap.Value = CreateWorkingBeatmap(beatmap);
            });

            checkDisplayedBPM(expectedDisplay);
        }

        private void checkDisplayedBPM(string target)
        {
            AddAssert($"displayed bpm is {target}", () => this.ChildrenOfType<LengthAndBPMStatisticPill.PillStatistic>().ElementAt(1).Value.ToString(), () => Is.EqualTo(target));
        }
    }
}
