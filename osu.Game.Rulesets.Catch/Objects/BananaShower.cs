// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Threading;
using osu.Game.Audio;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Rulesets.Catch.Objects
{
    public class BananaShower : CatchHitObject, IHasDuration
    {
        public override bool LastInCombo => true;

        public override Judgement CreateJudgement() => new IgnoreJudgement();

        protected override void CreateNestedHitObjects(CancellationToken cancellationToken)
        {
            base.CreateNestedHitObjects(cancellationToken);
            createBananas(cancellationToken);
        }

        private void createBananas(CancellationToken cancellationToken)
        {
            double spacing = Duration;
            while (spacing > 100)
                spacing /= 2;

            if (spacing <= 0)
                return;

            double time = StartTime;
            int i = 0;

            while (time <= EndTime)
            {
                cancellationToken.ThrowIfCancellationRequested();

                AddNested(new Banana
                {
                    StartTime = time,
                    BananaIndex = i,
                    Samples = new List<HitSampleInfo> { new Banana.BananaHitSampleInfo(CreateHitSampleInfo().Volume) }
                });

                time += spacing;
                i++;
            }
        }

        public double EndTime
        {
            get => StartTime + Duration;
            set => Duration = value - StartTime;
        }

        public double Duration { get; set; }

        protected override void CopyFrom(HitObject other, IDictionary<object, object> referenceLookup)
        {
            base.CopyFrom(other, referenceLookup);

            if (other is not BananaShower bananaShower)
                throw new ArgumentException($"{nameof(other)} must be of type {nameof(BananaShower)}");

            Duration = bananaShower.Duration;
        }

        protected override HitObject CreateInstance() => new BananaShower();
    }
}
