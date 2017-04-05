// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps.Samples;
using osu.Game.Modes.Objects.Drawables;
using osu.Game.Modes.Osu.Judgements;
using OpenTK;
using OpenTK.Graphics;
using System.Collections.Generic;

namespace osu.Game.Modes.Osu.Objects.Drawables
{
    public class DrawableSliderTick : DrawableOsuHitObject
    {
        private readonly SliderTick sliderTick;

        public double FadeInTime;
        public double FadeOutTime;

        public bool Tracking;

        public override bool RemoveWhenNotAlive => false;

        protected override OsuJudgement CreateJudgement() => new OsuJudgement { MaxScore = OsuScoreResult.SliderTick };

        private List<SampleChannel> samples = new List<SampleChannel>();

        public DrawableSliderTick(SliderTick sliderTick) : base(sliderTick)
        {
            this.sliderTick = sliderTick;

            Size = new Vector2(16) * sliderTick.Scale;

            Masking = true;
            CornerRadius = Size.X / 2;

            Origin = Anchor.Centre;

            BorderThickness = 2;
            BorderColour = Color4.White;

            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = AccentColour,
                    Alpha = 0.3f,
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            foreach (var bank in HitObject.SampleBanks)
                samples.Add(audio.Sample.Get($@"Gameplay/{bank.Name}-slidertick"));
        }

        protected override void PlaySamples()
        {
            samples.ForEach(s => s?.Play());
        }

        protected override void CheckJudgement(bool userTriggered)
        {
            if (Judgement.TimeOffset >= 0)
            {
                Judgement.Result = Tracking ? HitResult.Hit : HitResult.Miss;
                Judgement.Score = Tracking ? OsuScoreResult.SliderTick : OsuScoreResult.Miss;
            }
        }
        
        protected override void UpdatePreemptState()
        {
            var animIn = Math.Min(150, sliderTick.StartTime - FadeInTime);

            ScaleTo(0.5f);
            ScaleTo(1.2f, animIn);
            FadeIn(animIn);

            Delay(animIn);
            ScaleTo(1, 150, EasingTypes.Out);

            Delay(-animIn);
        }

        protected override void UpdateState(ArmedState state)
        {
            base.UpdateState(state);

            switch (state)
            {
                case ArmedState.Idle:
                    Delay(FadeOutTime - sliderTick.StartTime);
                    FadeOut();
                    break;
                case ArmedState.Miss:
                    FadeOut(160);
                    FadeColour(Color4.Red, 80);
                    break;
                case ArmedState.Hit:
                    FadeOut(120, EasingTypes.OutQuint);
                    ScaleTo(Scale * 1.5f, 120, EasingTypes.OutQuint);
                    break;
            }
        }
    }
}