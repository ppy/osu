using System;
using osu.Framework.Graphics;
using osuTK;

namespace Mvis.Plugin.RulesetPanel.Objects.MusicVisualizers
{
    public class MusicCircularVisualizer : MusicBarsVisualizer
    {
        private float circleSize = 200;

        public float CircleSize
        {
            get => circleSize;
            set
            {
                circleSize = value;

                if (!IsLoaded)
                    return;

                foreach (var bar in EqualizerBars)
                    bar.Position = calculateBarPosition(bar.Rotation);
            }
        }

        private float degreeValue = 360;

        public float DegreeValue
        {
            get => degreeValue;
            set
            {
                degreeValue = value;

                if (!IsLoaded)
                    return;

                setBarProperties();
            }
        }

        protected override void AddBars()
        {
            setBarProperties();
            base.AddBars();
        }

        private void setBarProperties()
        {
            float spacing = DegreeValue / BarsCount;

            for (int i = 0; i < BarsCount; i++)
            {
                var bar = EqualizerBars[i];
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
    }
}
