// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.MathUtils;
using osu.Game.Rulesets.Objects.Types;
using OpenTK.Graphics;

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
                AddNested(new Banana
                {
                    Samples = Samples,
                    ComboColour = getNextComboColour(),
                    StartTime = i,
                    X = RNG.NextSingle()
                });
        }

        private Color4 getNextComboColour()
        {
            switch (RNG.Next(0, 3))
            {
                default:
                    return new Color4(255, 240, 0, 255);
                case 1:
                    return new Color4(255, 192, 0, 255);
                case 2:
                    return new Color4(214, 221, 28, 255);
            }
        }

        public double EndTime => StartTime + Duration;

        public double Duration { get; set; }

        public class Banana : Fruit
        {
            public override FruitVisualRepresentation VisualRepresentation => FruitVisualRepresentation.Banana;
        }
    }
}
