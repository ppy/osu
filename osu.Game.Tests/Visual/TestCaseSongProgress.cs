// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.MathUtils;
using osu.Framework.Timing;
using osu.Game.Rulesets.Objects;
using osu.Game.Screens.Play;
using osu.Game.Beatmaps;
using osu.Game.Tests.Beatmaps;
using osu.Game.Rulesets.Taiko;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    public class TestCaseSongProgress : OsuTestCase
    {
        private readonly SongProgress progress;
        private readonly SongProgressGraph graph;

        private readonly StopwatchClock clock;

        public TestCaseSongProgress()
        {
            clock = new StopwatchClock(true);

            Add(progress = new SongProgress
            {
                RelativeSizeAxes = Axes.X,
                AudioClock = new StopwatchClock(true),
                Anchor = Anchor.BottomLeft,
                Origin = Anchor.BottomLeft,
            });

            Add(graph = new SongProgressGraph
            {
                RelativeSizeAxes = Axes.X,
                Height = 200,
                Anchor = Anchor.TopLeft,
                Origin = Anchor.TopLeft,
            });

            AddStep("Toggle Bar", () => progress.AllowSeeking = !progress.AllowSeeking);
            AddWaitStep(5);
            AddStep("Toggle Bar", () => progress.AllowSeeking = !progress.AllowSeeking);
            AddWaitStep(2);
            AddRepeatStep("New Values", displayNewValues, 5);

            displayNewValues();
        }

        private void displayNewValues()
        {
            List<HitObject> objects = new List<HitObject>();
            for (double i = 0; i < 5000; i += RNG.NextDouble() * 10 + i / 1000)
                objects.Add(new HitObject { StartTime = i });

            var ruleset = new TaikoRuleset();

            WorkingBeatmap beatmap = new TestWorkingBeatmap(new Beatmap
            {
                HitObjects = objects,
                BeatmapInfo = new BeatmapInfo
                {
                    BaseDifficulty = new BeatmapDifficulty(),
                    Metadata = new BeatmapMetadata
                    {
                        Artist = @"Unknown",
                        Title = @"Sample Beatmap",
                        AuthorString = @"peppy",
                    },
                    Ruleset = new TaikoRuleset().RulesetInfo,
                },
            });

            progress.AudioClock = clock;
            graph.AudioClock = clock;
            progress.Strains = ruleset.CreateDifficultyCalculator(beatmap).DifficultySectionRating();
            graph.Strains = ruleset.CreateDifficultyCalculator(beatmap).DifficultySectionRating();
            progress.StrainStep = ruleset.CreateDifficultyCalculator(beatmap).StrainStep();
            graph.StrainStep = ruleset.CreateDifficultyCalculator(beatmap).StrainStep();
            if (ruleset.LegacyID == 0)
                progress.StrainStep = 1;
                graph.StrainStep = 1;
            progress.Objects = objects;
            graph.Objects = objects;
            progress.OnSeek = pos => clock.Seek(pos);
        }
    }
}
