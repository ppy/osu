// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Judgements;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Catch.Skinning.Legacy
{
    public class LegacyCatchHitExplosion : CatchHitExplosion
    {
        private readonly Sprite explosion1;
        private readonly Sprite explosion2;

        [Resolved]
        private Bindable<Color4> objectColour { get; set; }

        [Resolved(Name = "CatcherWidth")]
        private Bindable<float> catcherWidth { get; set; }

        [Resolved]
        private Bindable<float> catchPosition { get; set; }

        [Resolved]
        private Bindable<JudgementResult> judgementResult { get; set; }

        [Resolved]
        private Bindable<PalpableCatchHitObject> hitObject { get; set; }

        private const float catcher_margin = (1 - Catcher.ALLOWED_CATCH_RANGE) / 2;

        public LegacyCatchHitExplosion()
        {
            Anchor = Anchor.CentreLeft;
            Origin = Anchor.BottomCentre;
            RelativeSizeAxes = Axes.Both;
            Scale = new Vector2(0.40f, 0.40f);

            InternalChildren = new Drawable[]
            {
                explosion1 = new Sprite
                {
                    Origin = Anchor.CentreLeft,
                    Blending = BlendingParameters.Additive,
                    Rotation = -90,
                },
                explosion2 = new Sprite
                {
                    Origin = Anchor.CentreLeft,
                    Blending = BlendingParameters.Additive,
                    Rotation = -90,
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(ISkinSource source)
        {
            explosion1.Texture = source.GetTexture("scoreboard-explosion-2");
            explosion2.Texture = source.GetTexture("scoreboard-explosion-1");

            objectColour.BindValueChanged(colour => explosion1.Colour = explosion2.Colour = colour.NewValue);
        }

        public override void Animate()
        {
            float catcherWidthHalf = catcherWidth.Value * 0.5f;

            float explosionOffset = Math.Clamp(catchPosition.Value, -catcherWidthHalf + catcher_margin * 3, catcherWidthHalf - catcher_margin * 3);

            if (!(hitObject.Value is Droplet))
            {
                var scale = Math.Clamp(judgementResult.Value.ComboAtJudgement / 200f, 0.35f, 1.125f);

                explosion1.Scale = new Vector2(1, 0.9f);
                explosion1.Position = new Vector2(explosionOffset, 0);

                explosion1.ScaleTo(new Vector2(20 * scale, 1.1f), 160, Easing.Out).Then().FadeOut(140);
            }

            explosion2.Scale = new Vector2(0.9f, 1f);
            explosion2.Position = new Vector2(explosionOffset, 0);

            explosion2.ScaleTo(new Vector2(0.9f, 1.3f), 500, Easing.Out).Then().FadeOut(200);

            this.FadeInFromZero().Then().Delay(700).Then().FadeOut();
        }
    }
}
