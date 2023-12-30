// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Rulesets.Scoring;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Taiko.Skinning.Argon
{
    public class TaikoArgonSkinTransformer : ArgonSkinTransformer
    {
        public TaikoArgonSkinTransformer(ISkin skin)
            : base(skin)
        {
        }

        public override Drawable? GetDrawableComponent(ISkinComponentLookup lookup)
        {
            switch (lookup)
            {
                case GameplaySkinComponentLookup<HitResult> resultComponent:
                    // This should eventually be moved to a skin setting, when supported.
                    if (Skin is ArgonProSkin && resultComponent.Component >= HitResult.Great)
                        return Drawable.Empty();

                    return new ArgonJudgementPiece(resultComponent.Component);

                case TaikoSkinComponentLookup taikoComponent:
                    // TODO: Once everything is finalised, consider throwing UnsupportedSkinComponentException on missing entries.
                    switch (taikoComponent.Component)
                    {
                        case TaikoSkinComponents.CentreHit:
                            return new ArgonCentreCirclePiece();

                        case TaikoSkinComponents.RimHit:
                            return new ArgonRimCirclePiece();

                        case TaikoSkinComponents.PlayfieldBackgroundLeft:
                            return new ArgonPlayfieldBackgroundLeft();

                        case TaikoSkinComponents.PlayfieldBackgroundRight:
                            return new ArgonPlayfieldBackgroundRight();

                        case TaikoSkinComponents.InputDrum:
                            return new ArgonInputDrum();

                        case TaikoSkinComponents.HitTarget:
                            return new ArgonHitTarget();

                        case TaikoSkinComponents.BarLine:
                            return new ArgonBarLine();

                        case TaikoSkinComponents.DrumRollBody:
                            return new ArgonElongatedCirclePiece();

                        case TaikoSkinComponents.DrumRollTick:
                            return new ArgonTickPiece();

                        case TaikoSkinComponents.TaikoExplosionKiai:
                            // the drawable needs to expire as soon as possible to avoid accumulating empty drawables on the playfield.
                            return Drawable.Empty().With(d => d.Expire());

                        case TaikoSkinComponents.DrumSamplePlayer:
                            return new ArgonDrumSamplePlayer();

                        case TaikoSkinComponents.TaikoExplosionGreat:
                        case TaikoSkinComponents.TaikoExplosionMiss:
                        case TaikoSkinComponents.TaikoExplosionOk:
                            return new ArgonHitExplosion(taikoComponent.Component);

                        case TaikoSkinComponents.Swell:
                            return new ArgonSwellCirclePiece();
                    }

                    break;
            }

            return base.GetDrawableComponent(lookup);
        }
    }
}
