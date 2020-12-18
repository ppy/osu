// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading;
using osu.Game.Rulesets.Judgements;
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
    }
}
