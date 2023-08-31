// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Replays;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Scoring;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests.Mods
{
    public partial class TestSceneOsuModStrictTracking : OsuModTestScene
    {
        [Test]
        public void TestSlider() => CreateModTest(new ModTestData
        {
            Mod = new OsuModStrictTracking(),
            Autoplay = true,
            Beatmap = new Beatmap
            {
                HitObjects = new List<HitObject>
                {
                    new Slider
                    {
                        StartTime = 500,
                        Position = new Vector2(256, 192),
                        Path = new SliderPath(new[]
                        {
                            new PathControlPoint(),
                            new PathControlPoint(new Vector2(0, 64f))
                        }),
                        RepeatCount = 0,
                        SliderVelocity = 1
                    }
                }
            },
            PassCondition = () => Player.HealthProcessor.HasCompleted.Value && Player.ScoreProcessor.Accuracy.Value == 1f
        });

        [Test]
        public void TestSliderBreak() => CreateModTest(new ModTestData
        {
            Mod = new OsuModStrictTracking(),
            Autoplay = false,
            Beatmap = new Beatmap
            {
                HitObjects = new List<HitObject>
                {
                    new Slider
                    {
                        StartTime = 500,
                        Position = new Vector2(128, 96),
                        Path = new SliderPath(new[]
                        {
                            new PathControlPoint(),
                            new PathControlPoint(new Vector2(128f, 96f)),
                            new PathControlPoint(new Vector2(256f, 0f)),
                        }),
                        RepeatCount = 0,
                        SliderVelocity = 3
                    }
                }
            },
            PassCondition = () => Player.HealthProcessor.HasCompleted.Value && Player.ScoreProcessor.HitEvents.Any(e => e.Result == HitResult.Miss),
            ReplayFrames = new List<ReplayFrame>
            {
                new OsuReplayFrame(500, new Vector2(128, 96), OsuAction.LeftButton),
                new OsuReplayFrame(900, new Vector2(128, 96)),
            }
        });

        [Test]
        public void TestSliderRewinding()
        {
            var beatmap = new Beatmap();
            int x = 128;
            int y = 96;

            for (int i = 500; i <= 10000; i += 500)
            {
                x += 128;
                y += 96;

                if (x > 384)
                {
                    x = 128;
                }

                if (y > 298)
                {
                    y = 96;
                }

                beatmap.HitObjects.Add(new Slider
                {
                    StartTime = i,
                    Position = new Vector2(x, y),
                    Path = new SliderPath(new[]
                    {
                        new PathControlPoint(),
                        new PathControlPoint(new Vector2(0, 64f))
                    }),
                    RepeatCount = 0,
                    SliderVelocity = 10
                });
            }

            double minAccuracy = 1f;

            CreateModTest(new ModTestData
            {
                Mod = new OsuModStrictTracking(),
                Autoplay = true,
                Beatmap = beatmap,
                PassCondition = () => minAccuracy == 1f
            });

            // Subscribing to event because accuracy changes too quickly and AddAssert doesn't have time to process the value when it is below 100%
            AddStep("subscribing to event", () => Player.ScoreProcessor.Accuracy.ValueChanged += testAccuracy =>
            {
                minAccuracy = Math.Min(minAccuracy, testAccuracy.NewValue);
            });
            AddUntilStep("wait for track to start running", () => Beatmap.Value.Track.IsRunning);

            addSeekStep(10000, true);
            addSeekStep(0, false);
            AddAssert("no slider missed", () => minAccuracy == 1f);

            addSeekStep(10000, true);
            addSeekStep(4635, false);
            AddAssert("no slider missed", () => minAccuracy == 1f);

            addSeekStep(10000, true);
            addSeekStep(8754, false);
            AddAssert("no slider missed", () => minAccuracy == 1f);

            addSeekStep(10000, true);
            addSeekStep(5536, false);
            AddAssert("no slider missed", () => minAccuracy == 1f);

            addSeekStep(20000, true);
            addSeekStep(1234, false);
            AddAssert("no slider missed", () => minAccuracy == 1f);
        }

        private void addSeekStep(double time, bool wait)
        {
            AddStep($"seek to {time}", () => Beatmap.Value.Track.Seek(time));

            if (wait)
            {
                AddUntilStep("wait for seek to finish", () => Precision.AlmostBigger(Player.DrawableRuleset.FrameStableClock.CurrentTime, time, 100));
            }
        }
    }
}
