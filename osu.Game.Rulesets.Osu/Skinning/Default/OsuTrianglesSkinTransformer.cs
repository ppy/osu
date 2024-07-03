// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Rulesets.Scoring;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Osu.Skinning.Default
{
    public class OsuTrianglesSkinTransformer : SkinTransformer
    {
        public OsuTrianglesSkinTransformer(ISkin skin)
            : base(skin)
        {
        }

        public override Drawable? GetDrawableComponent(ISkinComponentLookup lookup)
        {
            switch (lookup)
            {
                case GameplaySkinComponentLookup<HitResult> resultComponent:
                    HitResult result = resultComponent.Component;

                    switch (result)
                    {
                        case HitResult.IgnoreMiss:
                        case HitResult.LargeTickMiss:
                            // use argon judgement piece for new tick misses because i don't want to design another one for triangles.
                            return new DefaultJudgementPieceSliderTickMiss(result);
                    }

                    break;
            }

            return base.GetDrawableComponent(lookup);
        }
    }
}
