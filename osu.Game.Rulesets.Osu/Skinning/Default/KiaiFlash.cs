// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics.Containers;
using osu.Game.Rulesets.Osu.Configuration;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Skinning.Default
{
    public partial class KiaiFlash : BeatSyncedContainer
    {
        private const double fade_length = 80;

        private const float flash_opacity = 0.25f;

        [Resolved(canBeNull: true)]
        private OsuRulesetConfigManager? config { get; set; }

        private readonly BindableFloat kiaiFlashStrength = new BindableFloat(1f);
        private readonly Bindable<KiaiFlashFrequency> kiaiFlashFrequency = new Bindable<KiaiFlashFrequency>(KiaiFlashFrequency.EveryBeat);

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

        [BackgroundDependencyLoader]
        private void load()
        {
            kiaiFlashStrength.MinValue = 0.5f;
            kiaiFlashStrength.MaxValue = 2f;
            kiaiFlashStrength.Precision = 0.05f;

            config?.BindWith(OsuRulesetSetting.KiaiFlashStrength, kiaiFlashStrength);
            config?.BindWith(OsuRulesetSetting.KiaiFlashFrequency, kiaiFlashFrequency);
        }

        protected override void OnNewBeat(int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, ChannelAmplitudes amplitudes)
        {
            if (!effectPoint.KiaiMode)
                return;

            int interval = kiaiFlashFrequency.Value switch
            {
                KiaiFlashFrequency.EveryBeat => 1,
                KiaiFlashFrequency.EverySecondBeat => 2,
                KiaiFlashFrequency.EveryFourthBeat => 4,
                _ => 1,
            };

            if (beatIndex % interval != 0)
                return;

            float opacityMultiplier = Math.Clamp(kiaiFlashStrength.Value, 0.0f, 10.0f);

            Child
                .FadeTo(flash_opacity * opacityMultiplier, EarlyActivationMilliseconds, Easing.OutQuint)
                .Then()
                .FadeOut(Math.Max(fade_length, timingPoint.BeatLength - fade_length), Easing.OutSine);
        }
    }
}
