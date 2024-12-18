// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Catch.Skinning.Legacy
{
    public partial class LegacyHitExplosion : CompositeDrawable, IHitExplosion
    {
        [Resolved]
        private Catcher catcher { get; set; } = null!;

        private const float catch_margin = (1 - Catcher.ALLOWED_CATCH_RANGE) / 2;

        private readonly Sprite explosion1;
        private readonly Sprite explosion2;

        public LegacyHitExplosion()
        {
            Anchor = Anchor.BottomCentre;
            Origin = Anchor.BottomCentre;
            RelativeSizeAxes = Axes.Both;
            Scale = new Vector2(0.5f);

            InternalChildren = new[]
            {
                explosion1 = new Sprite
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.CentreLeft,
                    Alpha = 0,
                    Blending = BlendingParameters.Additive,
                    Rotation = -90
                },
                explosion2 = new Sprite
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.CentreLeft,
                    Alpha = 0,
                    Blending = BlendingParameters.Additive,
                    Rotation = -90
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(SkinManager skins)
        {
            var defaultLegacySkin = skins.DefaultClassicSkin;

            // sprite names intentionally swapped to match stable member naming / ease of cross-referencing
            explosion1.Texture = defaultLegacySkin.GetTexture("scoreboard-explosion-2");
            explosion2.Texture = defaultLegacySkin.GetTexture("scoreboard-explosion-1");
        }

        public void Animate(HitExplosionEntry entry)
        {
            Colour = entry.ObjectColour;

            using (BeginAbsoluteSequence(entry.LifetimeStart))
            {
                float halfCatchWidth = catcher.CatchWidth / 2;
                float explosionOffset = Math.Clamp(entry.Position, -halfCatchWidth + catch_margin * 3, halfCatchWidth - catch_margin * 3);

                if (!(entry.HitObject is Droplet))
                {
                    float scale = Math.Clamp(entry.JudgementResult.ComboAtJudgement / 200f, 0.35f, 1.125f);

                    explosion1.Scale = new Vector2(1, 0.9f);
                    explosion1.Position = new Vector2(explosionOffset, 0);

                    explosion1.FadeOutFromOne(300);
                    explosion1.ScaleTo(new Vector2(16 * scale, 1.1f), 160, Easing.Out);
                }

                explosion2.Scale = new Vector2(0.9f, 1);
                explosion2.Position = new Vector2(explosionOffset, 0);

                explosion2.FadeOutFromOne(700);
                explosion2.ScaleTo(new Vector2(0.9f, 1.3f), 500, Easing.Out);

                this.Delay(700).FadeOutFromOne();
            }
        }
    }
}
