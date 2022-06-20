// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Linq;
using System.Collections.Generic;
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
                switch (ho)
                {
                    case DrumRoll drumRoll:
                        return new ClassicDrumRoll(drumRoll);

                    case Swell swell:
                        return new ClassicSwell(swell);

                    default:
                        return ho;
                }
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

            protected override List<TaikoHitObject> CreateTicks(CancellationToken cancellationToken)
            {
                List<TaikoHitObject> oldTicks = base.CreateTicks(cancellationToken);

                List<TaikoHitObject> newTicks = oldTicks.Select(oldTick =>
                {
                    if (oldTick is DrumRollTick drumRollTick)
                    {
                        return new ClassicDrumRollTick(drumRollTick);
                    }

                    return oldTick;
                }).ToList();

                return newTicks;
            }
        }

        private class ClassicDrumRollTick : DrumRollTick
        {
            public override Judgement CreateJudgement() => new TaikoClassicDrumRollTickJudgement();

            public ClassicDrumRollTick(DrumRollTick original)
            {
                StartTime = original.StartTime;
                Samples = original.Samples;
                FirstTick = original.FirstTick;
                TickSpacing = original.TickSpacing;
            }
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
            public override HitResult MaxResult => HitResult.IgnoreHit;
        }

        private class TaikoClassicDrumRollTickJudgement : TaikoDrumRollTickJudgement
        {
            public override HitResult MaxResult => HitResult.SmallBonus;
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

                ApplyResult(r => r.Type = HitResult.IgnoreHit);
            }
        }

        private class ClassicDrawableSwell : DrawableSwell
        {
            public override bool DisplayResult => false;

            protected override HitResult OkResult => HitResult.SmallBonus;
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
