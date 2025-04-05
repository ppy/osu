// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Configuration;
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

        private readonly BindableBool kiaiFlashing = new BindableBool();

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

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            config.BindWith(OsuSetting.KiaiFlashing, kiaiFlashing);
        }

        protected override void OnNewBeat(int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, ChannelAmplitudes amplitudes)
        {
            if (!effectPoint.KiaiMode || !kiaiFlashing.Value)
                return;

            flashingDrawable
                .FadeTo(flash_opacity)
                .Then()
                .FadeOut(Math.Max(80, timingPoint.BeatLength - 80), Easing.OutSine);
        }
    }
}
