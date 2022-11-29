// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Taiko.Skinning.Legacy
{
    internal partial class LegacyKiaiGlow : Container
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
                Scale = new Vector2(0.75f),
            };
        }

        [Resolved(CanBeNull = true)]
        private IBeatSyncProvider? beatSyncProvider { get; set; }

        [Resolved(CanBeNull = true)]
        private HealthProcessor? healthProcessor { get; set; }

        protected override void Update()
        {
            base.Update();

            if (healthProcessor != null)
                healthProcessor.NewJudgement += onNewJudgement;

            if (beatSyncProvider != null)
            {
                if (beatSyncProvider.CheckIsKiaiTime())
                    this.FadeIn(180);
                else
                    this.FadeOut(180);
            }
        }

        private void onNewJudgement(JudgementResult result)
        {
            if (!result.IsHit)
                return;

            this.ScaleTo(1.1f, 50)
                .Then().ScaleTo(1f, 50);
        }
    }
}
