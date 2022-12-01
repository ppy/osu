// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics.Containers;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Taiko.Skinning.Legacy
{
    internal partial class LegacyKiaiGlow : BeatSyncedContainer
    {
        public LegacyKiaiGlow()
        {
            AlwaysPresent = true;
            Alpha = 0;
        }

        [BackgroundDependencyLoader]
        private void load(ISkinSource skin)
        {
            Child = new Sprite
            {
                Texture = skin.GetTexture("taiko-glow"),
                Origin = Anchor.Centre,
                Anchor = Anchor.Centre,
                Scale = new Vector2(0.7f),
                Colour = new Colour4(255, 228, 0, 255),
            };
        }

        [Resolved(CanBeNull = true)]
        private HealthProcessor? healthProcessor { get; set; }

        protected override void Update()
        {
            base.Update();

            if (healthProcessor != null)
                healthProcessor.NewJudgement += onNewJudgement;
        }

        private void onNewJudgement(JudgementResult result)
        {
            if (!result.IsHit)
                return;

            this.ScaleTo(1.2f)
                .Then().ScaleTo(1f, 80, Easing.OutQuad);
        }

        protected override void OnNewBeat(int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, ChannelAmplitudes amplitudes)
        {
            if (effectPoint.KiaiMode)
                this.FadeIn(180);
            else
                this.FadeOut(180);
        }
    }
}
