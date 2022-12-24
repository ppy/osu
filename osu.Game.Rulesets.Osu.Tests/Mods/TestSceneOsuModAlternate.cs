// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Timing;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Replays;
using osu.Game.Rulesets.Replays;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests.Mods
{
    public partial class TestSceneOsuModAlternate : OsuModTestScene
    {
        [Test]
        public void TestInputAlternating() => CreateModTest(new ModTestData
        {
            Mod = new OsuModAlternate(),
            PassCondition = () => Player.ScoreProcessor.Combo.Value == 4,
            Autoplay = false,
            Beatmap = new Beatmap
            {
                HitObjects = new List<HitObject>
                {
                    new HitCircle
                    {
                        StartTime = 500,
                        Position = new Vector2(100),
                    },
                    new HitCircle
                    {
                        StartTime = 1000,
                        Position = new Vector2(200, 100),
                    },
                    new HitCircle
                    {
                        StartTime = 1500,
                        Position = new Vector2(300, 100),
                    },
                    new HitCircle
                    {
                        StartTime = 2000,
                        Position = new Vector2(400, 100),
                    },
                },
            },
            ReplayFrames = new List<ReplayFrame>
            {
                new OsuReplayFrame(500, new Vector2(100), OsuAction.LeftButton),
                new OsuReplayFrame(501, new Vector2(100)),
                new OsuReplayFrame(1000, new Vector2(200, 100), OsuAction.RightButton),
                new OsuReplayFrame(1001, new Vector2(200, 100)),
                new OsuReplayFrame(1500, new Vector2(300, 100), OsuAction.LeftButton),
                new OsuReplayFrame(1501, new Vector2(300, 100)),
                new OsuReplayFrame(2000, new Vector2(400, 100), OsuAction.RightButton),
                new OsuReplayFrame(2001, new Vector2(400, 100)),
            }
        });

        [Test]
        public void TestInputSingular() => CreateModTest(new ModTestData
        {
            Mod = new OsuModAlternate(),
            PassCondition = () => Player.ScoreProcessor.Combo.Value == 0 && Player.ScoreProcessor.HighestCombo.Value == 1,
            Autoplay = false,
            Beatmap = new Beatmap
            {
                HitObjects = new List<HitObject>
                {
                    new HitCircle
                    {
                        StartTime = 500,
                        Position = new Vector2(100),
                    },
                    new HitCircle
                    {
                        StartTime = 1000,
                        Position = new Vector2(200, 100),
                    },
                },
            },
            ReplayFrames = new List<ReplayFrame>
            {
                new OsuReplayFrame(500, new Vector2(100), OsuAction.LeftButton),
                new OsuReplayFrame(501, new Vector2(100)),
                new OsuReplayFrame(1000, new Vector2(200, 100), OsuAction.LeftButton),
            }
        });

        /// <summary>
        /// Ensures alternation is reset before the first hitobject after intro.
        /// </summary>
        [Test]
        public void TestInputSingularAtIntro() => CreateModTest(new ModTestData
        {
            Mod = new OsuModAlternate(),
            PassCondition = () => Player.ScoreProcessor.Combo.Value == 1,
            Autoplay = false,
            Beatmap = new Beatmap
            {
                HitObjects = new List<HitObject>
                {
                    new HitCircle
                    {
                        StartTime = 1000,
                        Position = new Vector2(100),
                    },
                },
            },
            ReplayFrames = new List<ReplayFrame>
            {
                // first press during intro.
                new OsuReplayFrame(500, new Vector2(200), OsuAction.LeftButton),
                new OsuReplayFrame(501, new Vector2(200)),
                // press same key at hitobject and ensure it has been hit.
                new OsuReplayFrame(1000, new Vector2(100), OsuAction.LeftButton),
            }
        });

        /// <summary>
        /// Ensures alternation is reset before the first hitobject after a break.
        /// </summary>
        [Test]
        public void TestInputSingularWithBreak([Values] bool pressBeforeSecondObject) => CreateModTest(new ModTestData
        {
            Mod = new OsuModAlternate(),
            PassCondition = () => Player.ScoreProcessor.Combo.Value == 0 && Player.ScoreProcessor.HighestCombo.Value == 2,
            Autoplay = false,
            Beatmap = new Beatmap
            {
                Breaks = new List<BreakPeriod>
                {
                    new BreakPeriod(500, 2000),
                },
                HitObjects = new List<HitObject>
                {
                    new HitCircle
                    {
                        StartTime = 500,
                        Position = new Vector2(100),
                    },
                    new HitCircle
                    {
                        StartTime = 2500,
                        Position = new Vector2(500, 100),
                    },
                    new HitCircle
                    {
                        StartTime = 3000,
                        Position = new Vector2(500, 100),
                    },
                }
            },
            ReplayFrames = new ReplayFrame[]
            {
                // first press to start alternate lock.
                new OsuReplayFrame(450, new Vector2(100), OsuAction.LeftButton),
                new OsuReplayFrame(451, new Vector2(100)),
                // press same key at second hitobject and ensure it has been hit.
                new OsuReplayFrame(2450, new Vector2(500, 100), OsuAction.LeftButton),
                new OsuReplayFrame(2451, new Vector2(500, 100)),
                // press same key at third hitobject and ensure it has been missed.
                new OsuReplayFrame(2950, new Vector2(500, 100), OsuAction.LeftButton),
                new OsuReplayFrame(2951, new Vector2(500, 100)),
            }.Concat(!pressBeforeSecondObject
                ? Enumerable.Empty<ReplayFrame>()
                : new ReplayFrame[]
                {
                    // press same key after break but before hit object.
                    new OsuReplayFrame(2250, new Vector2(300, 100), OsuAction.LeftButton),
                    new OsuReplayFrame(2251, new Vector2(300, 100)),
                }
            ).ToList()
        });
    }
}
