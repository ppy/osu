// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics.Containers;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Skinning.Default
{
    public partial class KiaiFlash : BeatSyncedContainer
    {
        private const double fade_length = 80;

        private const float flash_opacity = 0.25f;

        public KiaiFlash()
        {
            EarlyActivationMilliseconds = 80;
            Blending = BlendingParameters.Additive;

            Child = new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = Color4.White,
                Alpha = 0f,
            };
        }

        protected override void OnNewBeat(int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, ChannelAmplitudes amplitudes)
        {
            if (!effectPoint.KiaiMode)
                return;

            double beatLength = timingPoint.BeatLength; // bpm = 60000 / beatLength
            double flashingPeriod;

            if (beatLength <= 120) // >=500 bpm, flash every 4 beats
            {
                if (beatIndex % 4 != 0)
                {
                    return;
                }

                flashingPeriod = beatLength * 4;
            }
            else if (beatLength <= 240) // >=250 bpm, flash every 2 beats
            {
                if (beatIndex % 2 != 0)
                {
                    return;
                }

                flashingPeriod = beatLength * 2;
            }
            else // <250 bpm, flash every beat
            {
                flashingPeriod = beatLength;
            }

            Child
                .FadeTo(flash_opacity, EarlyActivationMilliseconds, Easing.OutQuint)
                .Then()
                .FadeOut(Math.Max(fade_length, flashingPeriod - fade_length), Easing.OutSine);
        }
    }
}
