// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Linq;
using System.Threading;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Beatmaps;
using osu.Game.Rulesets.Taiko.Judgements;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Objects.Drawables;
using osu.Game.Rulesets.Taiko.UI;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Taiko.Mods
{
    public class TaikoModClassic : ModClassic, IApplicableToDrawableRuleset<TaikoHitObject>, IUpdatableByPlayfield, IApplicableAfterBeatmapConversion
    {
        private DrawableTaikoRuleset drawableTaikoRuleset;

        public void ApplyToDrawableRuleset(DrawableRuleset<TaikoHitObject> drawableRuleset)
        {
            drawableTaikoRuleset = (DrawableTaikoRuleset)drawableRuleset;
            drawableTaikoRuleset.LockPlayfieldAspect.Value = false;

            drawableTaikoRuleset.Playfield.RegisterPool<ClassicDrumRoll, ClassicDrawableDrumRoll>(5);
            drawableTaikoRuleset.Playfield.RegisterPool<ClassicDrumRollTick, DrawableDrumRollTick>(100);
            drawableTaikoRuleset.Playfield.RegisterPool<ClassicSwell, ClassicDrawableSwell>(5);
        }

        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            var taikoBeatmap = (TaikoBeatmap)beatmap;

            if (taikoBeatmap.HitObjects.Count == 0) return;

            var hitObjects = taikoBeatmap.HitObjects.Select(ho =>
            {
                if (ho is DrumRoll drumRoll)
                {
                    var newDrumRoll = new ClassicDrumRoll(drumRoll);
                    return newDrumRoll;
                }

                if (ho is Swell swell)
                {
                    var newSwell = new ClassicSwell(swell);
                    return newSwell;
                }

                return ho;
            }).ToList();

            taikoBeatmap.HitObjects = hitObjects;
        }

        private class ClassicDrumRoll : DrumRoll
        {
            public ClassicDrumRoll(DrumRoll original)
            {
                StartTime = original.StartTime;
                Samples = original.Samples;
                EndTime = original.EndTime;
                Duration = original.Duration;
                TickRate = original.TickRate;
                RequiredGoodHits = original.RequiredGoodHits;
                RequiredGreatHits = original.RequiredGreatHits;
            }

            public override Judgement CreateJudgement() => new TaikoClassicDrumRollJudgement();

            protected override void CreateTicks(CancellationToken cancellationToken)
            {
                if (TickSpacing == 0)
                    return;

                bool first = true;

                for (double t = StartTime; t < EndTime + TickSpacing / 2; t += TickSpacing)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    AddNested(new ClassicDrumRollTick
                    {
                        FirstTick = first,
                        TickSpacing = TickSpacing,
                        StartTime = t,
                        IsStrong = IsStrong
                    });

                    first = false;
                }
            }
        }

        private class ClassicDrumRollTick : DrumRollTick
        {
            public override Judgement CreateJudgement() => new TaikoClassicDrumRollTickJudgement();
        }

        private class ClassicSwell : Swell
        {
            public ClassicSwell(Swell original)
            {
                StartTime = original.StartTime;
                Samples = original.Samples;
                EndTime = original.EndTime;
                Duration = original.Duration;
                RequiredHits = original.RequiredHits;
            }

            public override Judgement CreateJudgement() => new TaikoClassicSwellJudgement();
        }

        private class TaikoClassicDrumRollJudgement : TaikoDrumRollJudgement
        {
            public override HitResult MaxResult => HitResult.LargeBonus;
        }

        private class TaikoClassicDrumRollTickJudgement : TaikoDrumRollTickJudgement
        {
            public override HitResult MaxResult => HitResult.LargeBonus;
        }

        private class TaikoClassicSwellJudgement : TaikoSwellJudgement
        {
            public override HitResult MaxResult => HitResult.LargeBonus;
        }

        private class ClassicDrawableDrumRoll : DrawableDrumRoll
        {
            public override bool DisplayResult => false;

            protected override void CheckForResult(bool userTriggered, double timeOffset)
            {
                if (userTriggered)
                    return;

                if (timeOffset < 0)
                    return;

                ApplyResult(r => r.Type = HitResult.IgnoreMiss);
            }
        }

        private class ClassicDrawableSwell : DrawableSwell
        {
            public override bool DisplayResult => false;

            protected override void CheckForResult(bool userTriggered, double timeOffset)
            {
                if (userTriggered)
                {
                    DrawableSwellTick nextTick = null;

                    foreach (var t in Ticks)
                    {
                        if (!t.Result.HasResult)
                        {
                            nextTick = t;
                            break;
                        }
                    }

                    nextTick?.TriggerResult(true);

                    int numHits = Ticks.Count(r => r.IsHit);

                    AnimateCompletion(numHits);

                    if (numHits == HitObject.RequiredHits)
                        ApplyResult(r => r.Type = HitResult.LargeBonus);
                }
                else
                {
                    if (timeOffset < 0)
                        return;

                    int numHits = 0;

                    foreach (var tick in Ticks)
                    {
                        if (tick.IsHit)
                        {
                            numHits++;
                            continue;
                        }

                        if (!tick.Result.HasResult)
                            tick.TriggerResult(false);
                    }

                    ApplyResult(r => r.Type = numHits > HitObject.RequiredHits / 2 ? HitResult.SmallBonus : HitResult.IgnoreMiss);
                }
            }
        }

        public void Update(Playfield playfield)
        {
            // Classic taiko scrolls at a constant 100px per 1000ms. More notes become visible as the playfield is lengthened.
            const float scroll_rate = 10;

            // Since the time range will depend on a positional value, it is referenced to the x480 pixel space.
            float ratio = drawableTaikoRuleset.DrawHeight / 480;

            drawableTaikoRuleset.TimeRange.Value = (playfield.HitObjectContainer.DrawWidth / ratio) * scroll_rate;
        }
    }
}
