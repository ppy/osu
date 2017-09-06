// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.MathUtils;
using osu.Framework.Timing;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Taiko.Judgements;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Objects.Drawables;
using osu.Game.Rulesets.Taiko.UI;
using OpenTK;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Beatmaps;
using osu.Desktop.Tests.Beatmaps;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Objects;

namespace osu.Desktop.Tests.Visual
{
    internal class TestCaseTaikoPlayfield : OsuTestCase
    {
        private const double default_duration = 1000;
        private const float scroll_time = 1000;

        public override string Description => "Taiko playfield";

        protected override double TimePerAction => default_duration * 2;

        private readonly Random rng = new Random(1337);
        private TaikoRulesetContainer rulesetContainer;
        private Container playfieldContainer;

        [BackgroundDependencyLoader]
        private void load(RulesetStore rulesets)
        {
            AddStep("Hit!", () => addHitJudgement(false));
            AddStep("Kiai hit", () => addHitJudgement(true));
            AddStep("Miss :(", addMissJudgement);
            AddStep("DrumRoll", () => addDrumRoll(false));
            AddStep("Strong DrumRoll", () => addDrumRoll(true));
            AddStep("Swell", () => addSwell());
            AddStep("Centre", () => addCentreHit(false));
            AddStep("Strong Centre", () => addCentreHit(true));
            AddStep("Rim", () => addRimHit(false));
            AddStep("Strong Rim", () => addRimHit(true));
            AddStep("Add bar line", () => addBarLine(false));
            AddStep("Add major bar line", () => addBarLine(true));
            AddStep("Height test 1", () => changePlayfieldSize(1));
            AddStep("Height test 2", () => changePlayfieldSize(2));
            AddStep("Height test 3", () => changePlayfieldSize(3));
            AddStep("Height test 4", () => changePlayfieldSize(4));
            AddStep("Height test 5", () => changePlayfieldSize(5));
            AddStep("Reset height", () => changePlayfieldSize(6));

            var controlPointInfo = new ControlPointInfo();
            controlPointInfo.TimingPoints.Add(new TimingControlPoint());

            WorkingBeatmap beatmap = new TestWorkingBeatmap(new Beatmap
            {
                HitObjects = new List<HitObject> { new CentreHit() },
                BeatmapInfo = new BeatmapInfo
                {
                    Difficulty = new BeatmapDifficulty(),
                    Metadata = new BeatmapMetadata
                    {
                        Artist = @"Unknown",
                        Title = @"Sample Beatmap",
                        Author = @"peppy",
                    },
                },
                ControlPointInfo = controlPointInfo
            });

            var rateAdjustClock = new StopwatchClock(true) { Rate = 1 };

            Add(playfieldContainer = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.X,
                Height = 768,
                Clock = new FramedClock(rateAdjustClock),
                Children = new[] { rulesetContainer = new TaikoRulesetContainer(rulesets.GetRuleset(1).CreateInstance(), beatmap, true) }
            });
        }

        private void changePlayfieldSize(int step)
        {
            double delay = 0;

            // Add new hits
            switch (step)
            {
                case 1:
                    addCentreHit(false);
                    break;
                case 2:
                    addCentreHit(true);
                    break;
                case 3:
                    addDrumRoll(false);
                    break;
                case 4:
                    addDrumRoll(true);
                    break;
                case 5:
                    addSwell();
                    delay = scroll_time - 100;
                    break;
            }

            // Tween playfield height
            switch (step)
            {
                default:
                    playfieldContainer.Delay(delay).ResizeTo(new Vector2(1, rng.Next(25, 400)), 500);
                    break;
                case 6:
                    playfieldContainer.Delay(delay).ResizeTo(new Vector2(1, TaikoPlayfield.DEFAULT_HEIGHT), 500);
                    break;
            }
        }

        private void addHitJudgement(bool kiai)
        {
            HitResult hitResult = RNG.Next(2) == 0 ? HitResult.Good : HitResult.Great;

            var cpi = new ControlPointInfo();
            cpi.EffectPoints.Add(new EffectControlPoint
            {
                KiaiMode = kiai
            });

            Hit hit = new Hit();
            hit.ApplyDefaults(cpi, new BeatmapDifficulty());

            var h = new DrawableTestHit(hit) { X = RNG.NextSingle(hitResult == HitResult.Good ? -0.1f : -0.05f, hitResult == HitResult.Good ? 0.1f : 0.05f) };

            rulesetContainer.Playfield.OnJudgement(h, new TaikoJudgement { Result = hitResult });

            if (RNG.Next(10) == 0)
            {
                rulesetContainer.Playfield.OnJudgement(h, new TaikoJudgement { Result = hitResult });
                rulesetContainer.Playfield.OnJudgement(h, new TaikoStrongHitJudgement());
            }
        }

        private void addMissJudgement()
        {
            rulesetContainer.Playfield.OnJudgement(new DrawableTestHit(new Hit()), new TaikoJudgement { Result = HitResult.Miss });
        }

        private void addBarLine(bool major, double delay = scroll_time)
        {
            BarLine bl = new BarLine { StartTime = rulesetContainer.Playfield.Time.Current + delay };

            rulesetContainer.Playfield.Add(major ? new DrawableBarLineMajor(bl) : new DrawableBarLine(bl));
        }

        private void addSwell(double duration = default_duration)
        {
            rulesetContainer.Playfield.Add(new DrawableSwell(new Swell
            {
                StartTime = rulesetContainer.Playfield.Time.Current + scroll_time,
                Duration = duration,
            }));
        }

        private void addDrumRoll(bool strong, double duration = default_duration)
        {
            addBarLine(true);
            addBarLine(true, scroll_time + duration);

            var d = new DrumRoll
            {
                StartTime = rulesetContainer.Playfield.Time.Current + scroll_time,
                IsStrong = strong,
                Duration = duration,
            };

            rulesetContainer.Playfield.Add(new DrawableDrumRoll(d));
        }

        private void addCentreHit(bool strong)
        {
            Hit h = new Hit
            {
                StartTime = rulesetContainer.Playfield.Time.Current + scroll_time,
                IsStrong = strong
            };

            if (strong)
                rulesetContainer.Playfield.Add(new DrawableCentreHitStrong(h));
            else
                rulesetContainer.Playfield.Add(new DrawableCentreHit(h));
        }

        private void addRimHit(bool strong)
        {
            Hit h = new Hit
            {
                StartTime = rulesetContainer.Playfield.Time.Current + scroll_time,
                IsStrong = strong
            };

            if (strong)
                rulesetContainer.Playfield.Add(new DrawableRimHitStrong(h));
            else
                rulesetContainer.Playfield.Add(new DrawableRimHit(h));
        }

        private class DrawableTestHit : DrawableHitObject<TaikoHitObject>
        {
            public DrawableTestHit(TaikoHitObject hitObject)
                : base(hitObject)
            {
            }

            protected override void UpdateState(ArmedState state)
            {
            }
        }
    }
}
