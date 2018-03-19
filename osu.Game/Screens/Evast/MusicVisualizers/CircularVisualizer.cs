// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using System;

namespace osu.Game.Screens.Evast.MusicVisualizers
{
    public class CircularVisualizer : MusicVisualizerContainer
    {
        protected override VisualizerBar CreateNewBar() => new DefaultBar();

        private float circleSize = 200;
        public float CircleSize
        {
            set
            {
                if (circleSize == value)
                    return;
                circleSize = value;

                if (!IsLoaded)
                    return;

                foreach (var bar in EqualizerBars)
                    bar.Position = calculateBarPosition(bar.Rotation);
            }
            get { return circleSize; }
        }

        private float degreeValue = 360;
        public float DegreeValue
        {
            set
            {
                if (degreeValue == value)
                    return;
                degreeValue = value;

                if (!IsLoaded)
                    return;

                calculateBarProperties();
            }
            get { return degreeValue; }
        }

        protected override void AddBars()
        {
            calculateBarProperties();
            base.AddBars();
        }

        private void calculateBarProperties()
        {
            float spacing = DegreeValue / BarsAmount;

            for (int i = 0; i < BarsAmount; i++)
            {
                VisualizerBar bar = EqualizerBars[i];
                bar.Origin = Anchor.BottomCentre;

                float rotationValue = i * spacing;

                bar.Rotation = rotationValue;
                bar.Position = calculateBarPosition(rotationValue);
            }
        }

        private Vector2 calculateBarPosition(float rotationValue)
        {
            float rotation = MathHelper.DegreesToRadians(rotationValue);
            float rotationCos = (float)Math.Cos(rotation);
            float rotationSin = (float)Math.Sin(rotation);
            return new Vector2(rotationSin / 2, -rotationCos / 2) * circleSize;
        }

        protected class DefaultBar : VisualizerBar
        {
            public DefaultBar()
            {
                Child = new Box
                {
                    EdgeSmoothness = Vector2.One,
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.White,
                };
            }

            public override void SetValue(float amplitudeValue, float valueMultiplier, int softness, int faloff)
            {
                var newValue = amplitudeValue * valueMultiplier;

                if (newValue <= Height)
                    return;

                this.ResizeHeightTo(newValue)
                        .Then()
                        .ResizeHeightTo(0, softness);
            }
        }
    }
}
