// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Extensions.Color4Extensions;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Screens.Edit.Screens.Compose;
using System;

namespace osu.Game.Rulesets.Mania.Edit
{
    public class EditSnapLine : ManiaHitObject
    {
        /// <summary>
        /// The control point which this snap line is part of.
        /// </summary>
        public TimingControlPoint ControlPoint;

        /// <summary>
        /// The beat snap divisor that determines the attributes of this snap line.
        /// </summary>
        public BindableBeatDivisor BeatDivisor;

        /// <summary>
        /// The colour of this snap line.
        /// </summary>
        public Color4 Colour;

        private int beatIndex;

        /// <summary>
        /// The index of the beat which this snap line represents within the control point.
        /// This is relative to the snap divisor that is currently used.
        /// </summary>
        public int BeatIndex
        {
            get => beatIndex;
            set
            {
                beatIndex = value;
                // TODO: In the case that the new beat snap divisors PR is accepted, adjust those
                // The new beat snap divisors are:
                // 1/5, 1/7, 1/9, 1/11, 1/18, 1/24, 1/32
                // The current implementation would have to take 1/5, 1/7 and 1/11 into account
                // Legend:
                // W - White
                // R - Red
                // B - Blue
                // M - Magenta
                // Possible suggestions:
                // 1/5:  W M M R M
                // 1/7:  W M M R M R M
                // 1/11: W M M B M M R M M B M
                // Maybe a feature to also customise the colours?
                int t = BeatDivisor.Value % 3 == 0 ? 3 : 1;
                int i = beatIndex / t;
                int b = BeatDivisor.Value / t;
                int d = i * Math.Max(8, b) / b % Math.Max(8, b);
                if (d == 0)
                    Colour = Color4.White;
                else if (d % 4 == 0)
                    Colour = Color4.Red;
                else if (d % 2 == 0)
                    Colour = new Color4(0, 96, 192, 255); // More eye-friendly
                else
                    Colour = Color4.Yellow;
                // In case the divisor refers to triplets and the index is within the split areas, override the previous calculation
                // Bad expression, but you get the idea
                if (t == 3 && beatIndex % 3 > 0)
                    Colour = Color4.Magenta;
            }
        }
    }
}
