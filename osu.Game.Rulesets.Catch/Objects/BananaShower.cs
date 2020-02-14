// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Rulesets.Catch.Objects
{
    public class BananaShower : CatchHitObject, IHasEndTime
    {
        public override FruitVisualRepresentation VisualRepresentation => FruitVisualRepresentation.Banana;

        public override bool LastInCombo => true;

        protected override void CreateNestedHitObjects()
        {
            base.CreateNestedHitObjects();
            createBananas();
        }

        private void createBananas()
        {
            double spacing = Duration;
            while (spacing > 100)
                spacing /= 2;

            if (spacing <= 0)
                return;

            for (double i = StartTime; i <= EndTime; i += spacing)
            {
                AddNested(new Banana
                {
                    Samples = Samples,
                    StartTime = i
                });
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
