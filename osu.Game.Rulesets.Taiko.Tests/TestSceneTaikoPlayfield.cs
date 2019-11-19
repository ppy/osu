// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.MathUtils;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Judgements;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Objects.Drawables;
using osu.Game.Rulesets.Taiko.UI;
using osu.Game.Tests.Visual;
using osuTK;

namespace osu.Game.Rulesets.Taiko.Tests
{
    [TestFixture]
    public class TestSceneTaikoPlayfield : OsuTestScene
    {
        private const double default_duration = 1000;
        private const float scroll_time = 1000;

        protected override double TimePerAction => default_duration * 2;

        private readonly Random rng = new Random(1337);
        private DrawableTaikoRuleset drawableRuleset;
        private Container playfieldContainer;

        [BackgroundDependencyLoader]
        private void load()
        {
            AddStep("Hit", () => addHitJudgement(false));
            AddStep("Strong hit", () => addStrongHitJudgement(false));
            AddStep("Kiai hit", () => addHitJudgement(true));
            AddStep("Strong kiai hit", () => addStrongHitJudgement(true));
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
            AddStep("Add centre w/ bar line", () =>
            {
                addCentreHit(false);
                addBarLine(true);
            });
            AddStep("Height test 1", () => changePlayfieldSize(1));
            AddStep("Height test 2", () => changePlayfieldSize(2));
            AddStep("Height test 3", () => changePlayfieldSize(3));
            AddStep("Height test 4", () => changePlayfieldSize(4));
            AddStep("Height test 5", () => changePlayfieldSize(5));
            AddStep("Reset height", () => changePlayfieldSize(6));

            var controlPointInfo = new ControlPointInfo();
            controlPointInfo.Add(0, new TimingControlPoint());

            WorkingBeatmap beatmap = CreateWorkingBeatmap(new Beatmap
            {
                HitObjects = new List<HitObject> { new CentreHit() },
                BeatmapInfo = new BeatmapInfo
                {
                    BaseDifficulty = new BeatmapDifficulty(),
                    Metadata = new BeatmapMetadata
                    {
                        Artist = @"Unknown",
                        Title = @"Sample Beatmap",
                        AuthorString = @"peppy",
                    },
                    Ruleset = new TaikoRuleset().RulesetInfo
                },
                ControlPointInfo = controlPointInfo
            });

            Add(playfieldContainer = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.X,
                Height = 768,
                Children = new[] { drawableRuleset = new DrawableTaikoRuleset(new TaikoRuleset(), beatmap, Array.Empty<Mod>()) }
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
            cpi.Add(0, new EffectControlPoint { KiaiMode = kiai });

            Hit hit = new Hit();
            hit.ApplyDefaults(cpi, new BeatmapDifficulty());

            var h = new DrawableTestHit(hit) { X = RNG.NextSingle(hitResult == HitResult.Good ? -0.1f : -0.05f, hitResult == HitResult.Good ? 0.1f : 0.05f) };

            ((TaikoPlayfield)drawableRuleset.Playfield).OnNewResult(h, new JudgementResult(new HitObject(), new TaikoJudgement()) { Type = hitResult });
        }

        private void addStrongHitJudgement(bool kiai)
        {
            HitResult hitResult = RNG.Next(2) == 0 ? HitResult.Good : HitResult.Great;

            var cpi = new ControlPointInfo();
            cpi.Add(0, new EffectControlPoint { KiaiMode = kiai });

            Hit hit = new Hit();
            hit.ApplyDefaults(cpi, new BeatmapDifficulty());

            var h = new DrawableTestHit(hit) { X = RNG.NextSingle(hitResult == HitResult.Good ? -0.1f : -0.05f, hitResult == HitResult.Good ? 0.1f : 0.05f) };

            ((TaikoPlayfield)drawableRuleset.Playfield).OnNewResult(h, new JudgementResult(new HitObject(), new TaikoJudgement()) { Type = hitResult });
            ((TaikoPlayfield)drawableRuleset.Playfield).OnNewResult(new TestStrongNestedHit(h), new JudgementResult(new HitObject(), new TaikoStrongJudgement()) { Type = HitResult.Great });
        }

        private void addMissJudgement()
        {
            ((TaikoPlayfield)drawableRuleset.Playfield).OnNewResult(new DrawableTestHit(new Hit()), new JudgementResult(new HitObject(), new TaikoJudgement()) { Type = HitResult.Miss });
        }

        private void addBarLine(bool major, double delay = scroll_time)
        {
            BarLine bl = new BarLine { StartTime = drawableRuleset.Playfield.Time.Current + delay };

            drawableRuleset.Playfield.Add(major ? new DrawableBarLineMajor(bl) : new DrawableBarLine(bl));
        }

        private void addSwell(double duration = default_duration)
        {
            var swell = new Swell
            {
                StartTime = drawableRuleset.Playfield.Time.Current + scroll_time,
                Duration = duration,
            };

            swell.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

            drawableRuleset.Playfield.Add(new DrawableSwell(swell));
        }

        private void addDrumRoll(bool strong, double duration = default_duration)
        {
            addBarLine(true);
            addBarLine(true, scroll_time + duration);

            var d = new DrumRoll
            {
                StartTime = drawableRuleset.Playfield.Time.Current + scroll_time,
                IsStrong = strong,
                Duration = duration,
            };

            d.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

            drawableRuleset.Playfield.Add(new DrawableDrumRoll(d));
        }

        private void addCentreHit(bool strong)
        {
            Hit h = new Hit
            {
                StartTime = drawableRuleset.Playfield.Time.Current + scroll_time,
                IsStrong = strong
            };

            h.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

            drawableRuleset.Playfield.Add(new DrawableCentreHit(h));
        }

        private void addRimHit(bool strong)
        {
            Hit h = new Hit
            {
                StartTime = drawableRuleset.Playfield.Time.Current + scroll_time,
                IsStrong = strong
            };

            h.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

            drawableRuleset.Playfield.Add(new DrawableRimHit(h));
        }

        private class TestStrongNestedHit : DrawableStrongNestedHit
        {
            public TestStrongNestedHit(DrawableHitObject mainObject)
                : base(new StrongHitObject { StartTime = mainObject.HitObject.StartTime }, mainObject)
            {
            }

            public override bool OnPressed(TaikoAction action) => false;
        }

        private class DrawableTestHit : DrawableHitObject<TaikoHitObject>
        {
            public DrawableTestHit(TaikoHitObject hitObject)
                : base(hitObject)
            {
            }
        }
    }
}
