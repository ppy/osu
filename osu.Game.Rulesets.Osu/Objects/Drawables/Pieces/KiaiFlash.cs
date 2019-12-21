// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;

namespace osu.Game.Rulesets.Osu.Objects.Drawables.Pieces
{
    public class KiaiFlash : BeatSyncedContainer
    {
        public Drawable FlashComponent { get; set; }

        public float Intensity { get; set; }

        public KiaiFlash()
        {
            Blending = BlendingParameters.Additive;

            Child = FlashComponent = new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = OsuColour.Gray(1f),
                Alpha = 0f,
            };
        }

        protected new double EarlyActivationMilliseconds = 80;

        protected override void OnNewBeat(int beatIndex, Game.Beatmaps.ControlPoints.TimingControlPoint timingPoint, Game.Beatmaps.ControlPoints.EffectControlPoint effectPoint, Framework.Audio.Track.TrackAmplitudes amplitudes)
        {
            if (effectPoint.KiaiMode)
            {
                FlashComponent
                    .FadeTo(Intensity, EarlyActivationMilliseconds, Easing.OutQuint)
                    .Then()
                    .FadeOut(timingPoint.BeatLength - 80, Easing.OutSine);
            }
        }
    }
}
