// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.Containers;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Objects.Drawables.Pieces
{
    public class KiaiFlash : BeatSyncedContainer
    {
        public float FlashOpacity = 1f;

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

        protected override void OnNewBeat(int beatIndex, Game.Beatmaps.ControlPoints.TimingControlPoint timingPoint, Game.Beatmaps.ControlPoints.EffectControlPoint effectPoint, Framework.Audio.Track.TrackAmplitudes amplitudes)
        {
            if (!effectPoint.KiaiMode)
            {
                return;
            }

            Child
                .FadeTo(FlashOpacity, EarlyActivationMilliseconds, Easing.OutQuint)
                .Then()
                .FadeOut(timingPoint.BeatLength - 80, Easing.OutSine);
        }
    }
}