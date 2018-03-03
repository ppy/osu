// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using NUnit.Framework;
using osu.Game.Beatmaps.ControlPoints;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    public class TestCaseGamefield : OsuTestCase
    {
        protected override void LoadComplete()
        {
            base.LoadComplete();

            /*int time = 500;
            for (int i = 0; i < 100; i++)
            {
                  objects.Add(new HitCircle
                  {
                      StartTime = time,
                      Position = new Vector2(RNG.Next(0, (int)OsuPlayfield.BASE_SIZE.X), RNG.Next(0, (int)OsuPlayfield.BASE_SIZE.Y)),
                      Scale = RNG.NextSingle(0.5f, 1.0f),
                  });

                time += RNG.Next(50, 500);
            }*/

            var controlPointInfo = new ControlPointInfo();
            controlPointInfo.TimingPoints.Add(new TimingControlPoint
            {
                BeatLength = 200
            });

            /*WorkingBeatmap beatmap = new TestWorkingBeatmap(new Beatmap
            {
                HitObjects = objects,
                BeatmapInfo = new BeatmapInfo
                {
                    Difficulty = new BeatmapDifficulty(),
                    Ruleset = rulesets.Query<RulesetInfo>().First(),
                    Metadata = new BeatmapMetadata
                    {
                        Artist = @"Unknown",
                        Title = @"Sample Beatmap",
                        Author = @"peppy",
                    },
                },
                ControlPointInfo = controlPointInfo
            });

            AddRange(new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    //ensure we are at offset 0
                    Clock = new FramedClock(),
                    Children = new Drawable[]
                    {
                        new OsuRulesetContainer(new OsuRuleset(), beatmap, false)
                        {
                            Scale = new Vector2(0.5f),
                            Anchor = Anchor.TopLeft,
                            Origin = Anchor.TopLeft
                        },
                        new TaikoRulesetContainer(new TaikoRuleset(),beatmap, false)
                        {
                            Scale = new Vector2(0.5f),
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight
                        },
                        new CatchRulesetContainer(new CatchRuleset(),beatmap, false)
                        {
                            Scale = new Vector2(0.5f),
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft
                        },
                        new ManiaRulesetContainer(new ManiaRuleset(),beatmap, false)
                        {
                            Scale = new Vector2(0.5f),
                            Anchor = Anchor.BottomRight,
                            Origin = Anchor.BottomRight
                        }
                    }
                }
            });*/
        }
    }
}
