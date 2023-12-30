// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Rulesets.Scoring;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Osu.Skinning.Argon
{
    public class OsuArgonSkinTransformer : ArgonSkinTransformer
    {
        public OsuArgonSkinTransformer(ISkin skin)
            : base(skin)
        {
        }

        public override Drawable? GetDrawableComponent(ISkinComponentLookup lookup)
        {
            switch (lookup)
            {
                case GameplaySkinComponentLookup<HitResult> resultComponent:
                    // This should eventually be moved to a skin setting, when supported.
                    if (Skin is ArgonProSkin && (resultComponent.Component == HitResult.Great || resultComponent.Component == HitResult.Perfect))
                        return Drawable.Empty();

                    return new ArgonJudgementPiece(resultComponent.Component);

                case OsuSkinComponentLookup osuComponent:
                    // TODO: Once everything is finalised, consider throwing UnsupportedSkinComponentException on missing entries.
                    switch (osuComponent.Component)
                    {
                        case OsuSkinComponents.HitCircle:
                            return new ArgonMainCirclePiece(true);

                        case OsuSkinComponents.SliderHeadHitCircle:
                            return new ArgonMainCirclePiece(false);

                        case OsuSkinComponents.SliderBody:
                            return new ArgonSliderBody();

                        case OsuSkinComponents.SliderBall:
                            return new ArgonSliderBall();

                        case OsuSkinComponents.SliderFollowCircle:
                            return new ArgonFollowCircle();

                        case OsuSkinComponents.SliderScorePoint:
                            return new ArgonSliderScorePoint();

                        case OsuSkinComponents.SpinnerBody:
                            return new ArgonSpinner();

                        case OsuSkinComponents.ReverseArrow:
                            return new ArgonReverseArrow();

                        case OsuSkinComponents.FollowPoint:
                            return new ArgonFollowPoint();

                        case OsuSkinComponents.Cursor:
                            return new ArgonCursor();

                        case OsuSkinComponents.CursorTrail:
                            return new ArgonCursorTrail();
                    }

                    break;
            }

            return base.GetDrawableComponent(lookup);
        }
    }
}
