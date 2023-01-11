// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics.Containers;
using osuTK.Graphics;

namespace osu.Game.Skinning
{
    public partial class LegacyKiaiFlashingDrawable : BeatSyncedContainer
    {
        public Color4 KiaiGlowColour
        {
            get => flashingDrawable.Colour;
            set => flashingDrawable.Colour = value;
        }

        private readonly Drawable flashingDrawable;

        private const float flash_opacity = 0.3f;

        public LegacyKiaiFlashingDrawable(Func<Drawable?> creationFunc)
        {
            AutoSizeAxes = Axes.Both;

            Children = new[]
            {
                (creationFunc.Invoke() ?? Empty()).With(d =>
                {
                    d.Anchor = Anchor.Centre;
                    d.Origin = Anchor.Centre;
                }),
                flashingDrawable = (creationFunc.Invoke() ?? Empty()).With(d =>
                {
                    d.Anchor = Anchor.Centre;
                    d.Origin = Anchor.Centre;
                    d.Alpha = 0;
                    d.Blending = BlendingParameters.Additive;
                })
            };
        }

        protected override void OnNewBeat(int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, ChannelAmplitudes amplitudes)
        {
            if (!effectPoint.KiaiMode)
                return;

            flashingDrawable
                .FadeTo(flash_opacity)
                .Then()
                .FadeOut(Math.Max(80, timingPoint.BeatLength - 80), Easing.OutSine);
        }
    }
}
