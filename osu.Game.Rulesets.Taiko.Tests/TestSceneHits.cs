// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Utils;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Judgements;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Objects.Drawables;
using osu.Game.Rulesets.Taiko.UI;
using osuTK;

namespace osu.Game.Rulesets.Taiko.Tests
{
    [TestFixture]
    public partial class TestSceneHits : DrawableTaikoRulesetTestScene
    {
        private const double default_duration = 3000;
        private const float scroll_time = 1000;

        protected override double TimePerAction => default_duration * 2;

        private readonly Random rng = new Random(1337);

        [Test]
        public void TestVariousHits()
        {
            AddStep("Hit", () => addHitJudgement(false));
            AddStep("Strong hit", () => addStrongHitJudgement(false));
            AddStep("Kiai hit", () => addHitJudgement(true));
            AddStep("Strong kiai hit", () => addStrongHitJudgement(true));
            AddStep("Miss :(", addMissJudgement);
            AddStep("DrumRoll", () => addDrumRoll(false));
            AddStep("Strong DrumRoll", () => addDrumRoll(true));
            AddStep("Kiai DrumRoll", () => addDrumRoll(true, kiai: true));
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
                    PlayfieldContainer.Delay(delay).ResizeTo(new Vector2(1, rng.Next(25, 400)), 500);
                    break;

                case 6:
                    PlayfieldContainer.Delay(delay).ResizeTo(new Vector2(1, DEFAULT_PLAYFIELD_CONTAINER_HEIGHT), 500);
                    break;
            }
        }

        private void addHitJudgement(bool kiai)
        {
            HitResult hitResult = RNG.Next(2) == 0 ? HitResult.Ok : HitResult.Great;

            Hit hit = new Hit { StartTime = DrawableRuleset.Playfield.Time.Current };
            var h = new DrawableTestHit(hit, kiai: kiai) { X = RNG.NextSingle(hitResult == HitResult.Ok ? -0.1f : -0.05f, hitResult == HitResult.Ok ? 0.1f : 0.05f) };

            DrawableRuleset.Playfield.Add(h);

            ((TaikoPlayfield)DrawableRuleset.Playfield).OnNewResult(h, new JudgementResult(hit, new TaikoJudgement()) { Type = hitResult });
        }

        private void addStrongHitJudgement(bool kiai)
        {
            HitResult hitResult = RNG.Next(2) == 0 ? HitResult.Ok : HitResult.Great;

            Hit hit = new Hit
            {
                StartTime = DrawableRuleset.Playfield.Time.Current,
                IsStrong = true,
                Samples = createSamples(strong: true)
            };
            var h = new DrawableTestHit(hit, kiai: kiai) { X = RNG.NextSingle(hitResult == HitResult.Ok ? -0.1f : -0.05f, hitResult == HitResult.Ok ? 0.1f : 0.05f) };

            DrawableRuleset.Playfield.Add(h);

            ((TaikoPlayfield)DrawableRuleset.Playfield).OnNewResult(h, new JudgementResult(hit, new TaikoJudgement()) { Type = hitResult });
            ((TaikoPlayfield)DrawableRuleset.Playfield).OnNewResult(h.NestedHitObjects.Single(), new JudgementResult(hit.NestedHitObjects.Single(), new TaikoStrongJudgement()) { Type = HitResult.Great });
        }

        private void addMissJudgement()
        {
            DrawableTestHit h;
            DrawableRuleset.Playfield.Add(h = new DrawableTestHit(new Hit { StartTime = DrawableRuleset.Playfield.Time.Current }, HitResult.Miss)
            {
                Alpha = 0
            });
            ((TaikoPlayfield)DrawableRuleset.Playfield).OnNewResult(h, new JudgementResult(h.HitObject, new TaikoJudgement()) { Type = HitResult.Miss });
        }

        private void addBarLine(bool major, double delay = scroll_time)
        {
            BarLine bl = new BarLine
            {
                StartTime = DrawableRuleset.Playfield.Time.Current + delay,
                Major = major
            };

            DrawableRuleset.Playfield.Add(bl);
        }

        private void addSwell(double duration = default_duration)
        {
            var swell = new Swell
            {
                StartTime = DrawableRuleset.Playfield.Time.Current + scroll_time,
                Duration = duration,
            };

            swell.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

            DrawableRuleset.Playfield.Add(new DrawableSwell(swell));
        }

        private void addDrumRoll(bool strong, double duration = default_duration, bool kiai = false)
        {
            addBarLine(true);
            addBarLine(true, scroll_time + duration);

            var d = new DrumRoll
            {
                StartTime = DrawableRuleset.Playfield.Time.Current + scroll_time,
                IsStrong = strong,
                Samples = createSamples(strong: strong),
                Duration = duration,
                TickRate = 8,
            };

            var cpi = new ControlPointInfo();
            cpi.Add(-10000, new EffectControlPoint { KiaiMode = kiai });

            d.ApplyDefaults(cpi, new BeatmapDifficulty());

            DrawableRuleset.Playfield.Add(new DrawableDrumRoll(d));
        }

        private void addCentreHit(bool strong)
        {
            Hit h = new Hit
            {
                StartTime = DrawableRuleset.Playfield.Time.Current + scroll_time,
                IsStrong = strong,
                Samples = createSamples(HitType.Centre, strong)
            };

            h.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

            DrawableRuleset.Playfield.Add(new DrawableHit(h));
        }

        private void addRimHit(bool strong)
        {
            Hit h = new Hit
            {
                StartTime = DrawableRuleset.Playfield.Time.Current + scroll_time,
                IsStrong = strong,
                Samples = createSamples(HitType.Rim, strong)
            };

            h.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

            DrawableRuleset.Playfield.Add(new DrawableHit(h));
        }

        // TODO: can be removed if a better way of handling colour/strong type and samples is developed
        private IList<HitSampleInfo> createSamples(HitType? hitType = null, bool strong = false)
        {
            var samples = new List<HitSampleInfo>();

            if (hitType == HitType.Rim)
                samples.Add(new HitSampleInfo(HitSampleInfo.HIT_CLAP));

            if (strong)
                samples.Add(new HitSampleInfo(HitSampleInfo.HIT_FINISH));

            return samples;
        }
    }
}
